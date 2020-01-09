using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSScriptIntellisense.Interop;
using Intellisense.Common;
using Kbg.NppPluginNET.PluginInfrastructure;
using UltraSharp.Cecil;
using static Kbg.NppPluginNET.PluginInfrastructure.Win32;

namespace CSScriptIntellisense
{
    static public partial class Plugin
    {
        public static string PluginName
        {
            get
            {
                if (Bootstrapper.IsInConflictWithCSScriptNpp())
                    return "C# Intellisence (disabled)";
                else
                    return "C# Intellisence";
            }
        }

        static internal string currentFile = null;
        static int currentFileCssHash = -1;
        static DateTime currentFileTimestamp;
        static List<string> parsedFiles = new List<string>();

        static bool SingleFileMode = false;
        static internal bool Enabled = false;
        static public Func<bool> SuppressCodeTolltips = () => false;

        static MemberInfoPopupManager memberInfoPopup;

        static internal void CommandMenuInit()
        {
            int cmdIndex = 0;
            CommandMenuInit(ref cmdIndex,
                (index, name, handler, shortcut) =>
                {
                    SetCommand(index, name, handler, shortcut);
                });
        }

        //this method will also be called from the parent plugin
        static public void CommandMenuInit(ref int cmdIndex, SetMenuCommand setCommand)
        {
            //System.Diagnostics.Debug.Assert(false);

            bool standaloneSetup = (cmdIndex == 0);

            Plugin.Enabled = Bootstrapper.Init(standaloneSetup);

            if (Plugin.Enabled)
            {
                Dispatcher.Init();
                KeyInterceptor.Instance.Install();

                Task.Factory.StartNew(SimpleCodeCompletion.Init);

                //'_' prefix in the shortcutName means "plugin action shortcut" as opposite to "plugin key interceptor action"
                setCommand(cmdIndex++, "Show auto-complete list", ShowSuggestionList, "_ShowAutoComplete:Ctrl+Space");
                setCommand(cmdIndex++, "Insert Code Snippet", ShowSnippetsList, "_InsertCodeSnippet:Ctrl+Shift+Space");
                setCommand(cmdIndex++, "Auto-add missing 'usings'", AddAllMissingUsings, "_AddAllMissingUsings:Alt+U");
                setCommand(cmdIndex++, "Add missing 'using'", AddMissingUsing, "_AddMissingUsings:Ctrl+OemPeriod");
                setCommand(cmdIndex++, "Re-analyze current document", Reparse, null);
                if (!Config.Instance.DisableMethodInfo)
                    setCommand(cmdIndex++, "Show Method Info", ShowMethodInfo, "_ShowMethodInfo:F6");
                setCommand(cmdIndex++, "Format Document", FormatDocument, "_FormatDocument:Ctrl+F8");
                setCommand(cmdIndex++, "Rename...   (Ctrl+R,R)", RenameMemberAtCaret, null);
                setCommand(cmdIndex++, "Go To Definition", GoToDefinition, "_GoToDefinition:F12");
                setCommand(cmdIndex++, "Find All References", FindAllReferences, "_FindAllReferences:Shift+F12");
                setCommand(cmdIndex++, "---", null, null);
                setCommand(cmdIndex++, "Settings", ShowConfig, null);
                setCommand(cmdIndex++, "Manage Code Snippets", Snippets.EditSnippetsConfig, null);
                setCommand(cmdIndex++, "---", null, null);
#if DEBUG
                //setCommand(cmdIndex++, "Test", Test, true, false, true, Keys.L);
#endif
                if (standaloneSetup)
                    setCommand(cmdIndex++, "About", ShowAboutBox, null);

                memberInfoPopup = new MemberInfoPopupManager(ShowQuickInfo);

                //NPP already intercepts these shortcuts so we need to hook keyboard messages
                IEnumerable<Keys> keysToIntercept = BindInternalShortcuts();

                foreach (var key in keysToIntercept)
                    KeyInterceptor.Instance.Add(key);

                KeyInterceptor.Instance.Add(Keys.Up);
                KeyInterceptor.Instance.Add(Keys.Down);
                KeyInterceptor.Instance.Add(Keys.Right);
                KeyInterceptor.Instance.Add(Keys.Left);
                KeyInterceptor.Instance.Add(Keys.Tab);
                KeyInterceptor.Instance.Add(Keys.R);
                KeyInterceptor.Instance.Add(Keys.Return);
                KeyInterceptor.Instance.Add(Keys.Escape);
                KeyInterceptor.Instance.Add(Keys.Z);
                KeyInterceptor.Instance.KeyDown += Instance_KeyDown;
            }
            else
                PluginBase.SetCommand(cmdIndex++, "About", ShowAboutBox);
        }

        static bool TriggerCodeSnippetInsertion()
        {
            if (!Config.Instance.CodeSnippetsEnabled)
                return false;

            Debug.WriteLine("------------------ TRIGGER called");

            var document = Npp.GetCurrentDocument();
            Point point;
            string token = document.GetWordAtCursor(out point);

            if (Snippets.Contains(token))
            {
                Dispatcher.Schedule(10, () =>
                    InsertCodeSnippet(token, point));
                return true;
            }
            return false;
        }

        static void Instance_KeyDown(Keys key, int repeatCount, ref bool handled)
        {
            bool isScriptDoc = Npp.Editor.IsCurrentDocScriptFile();
            var document = Npp.GetCurrentDocument();

            if (isScriptDoc)
            {
                if (key == Keys.Up || key == Keys.Down || key == Keys.Right || key == Keys.Left || key == Keys.Tab || key == Keys.Return || key == Keys.Escape)
                {
                    if (autocompleteForm != null && autocompleteForm.Visible)
                    {
                        handled = autocompleteForm.OnKeyDown(key);
                    }

                    if (memberInfoPopup != null && memberInfoPopup.IsShowing)
                    {
                        // memberInfoPopup handles its own keyboard hook events
                    }

                    if (namespaceMenu != null && namespaceMenu.Visible)
                    {
                        namespaceMenu.OnKeyDown(key);
                        if (key != Keys.Right)//right only processed in autocompleteForm
                            handled = true;
                    }

                    return;
                }
            }

            if (Config.Instance.CodeSnippetsEnabled && !IsShowingInteractivePopup)
            {
                if ((key == Keys.Tab || key == Keys.Escape || key == Keys.Return) && isScriptDoc)
                {
                    Modifiers modifiers = KeyInterceptor.GetModifiers();

                    if (!modifiers.IsCtrl && !modifiers.IsAlt && !modifiers.IsShift)
                    {
                        if (currentSnippetContext == null)
                        {
                            //no snippet insertion in progress
                            if (key == Keys.Tab)
                            {
                                if (TriggerCodeSnippetInsertion())
                                    handled = true;
                            }
                        }
                        else
                        {
                            //there is a snippet insertion in progress
                            if (key == Keys.Tab)
                            {
                                if (!Snippets.NavigateToNextParam(currentSnippetContext))
                                    currentSnippetContext = null;
                                else
                                    handled = true;
                            }
                            else if (key == Keys.Escape || key == Keys.Return)
                            {
                                Snippets.FinalizeCurrent();
                                if (key == Keys.Return)
                                    handled = true;
                                currentSnippetContext = null;
                            }
                        }
                    }
                }
            }

            if (key == Keys.Z && Config.Instance.PostFormattingUndoCaretReset && isScriptDoc && SourceCodeFormatter.CaretBeforeLastFormatting != -1)
            {
                Modifiers modifiers = KeyInterceptor.GetModifiers();
                if (modifiers.IsCtrl && !modifiers.IsShift && !modifiers.IsAlt)
                {
                    if (document.CanUndo())
                    {
                        //Native NPP undo moves caret to the end of text unconditionally if the text was reset completely (e.g. CodeFormatting).
                        //Thus manual resetting is required after UNDO. Though NPP doesn't have undo notification so we use shortcut for this.
                        handled = true;
                        document.Undo();

                        int newCurrentPos = SourceCodeFormatter.CaretBeforeLastFormatting;
                        SourceCodeFormatter.CaretBeforeLastFormatting = -1;

                        document.MoveCaretTo(newCurrentPos);
                        document.SetFirstVisibleLine(document.LineFromPosition(newCurrentPos) - SourceCodeFormatter.TopScrollOffsetBeforeLastFormatting);
                    }
                }
            }

            if (Config.Instance.InterceptCtrlSpace && isScriptDoc)
            {
                foreach (var shortcut in internalShortcuts.Keys)
                    if ((byte)key == shortcut._key)
                    {
                        Modifiers modifiers = KeyInterceptor.GetModifiers();

                        if (modifiers.IsCtrl == shortcut.IsCtrl && modifiers.IsShift == shortcut.IsShift && modifiers.IsAlt == shortcut.IsAlt)
                        {
                            handled = true;
                            var handler = internalShortcuts[shortcut];
                            Dispatcher.Schedule(10, () => InvokeShortcutHandler(handler.Item2));

                            break;
                        }
                    }
            }

            if (key == Keys.R && isScriptDoc)
            {
                Modifiers modifiers = KeyInterceptor.GetModifiers();

                if (modifiers.IsCtrl)
                {
                    handled = true;
                    if (Environment.TickCount - lastKeyEvent < 1000)
                        RenameMemberAtCaret();
                    else
                        lastKeyEvent = Environment.TickCount;
                }
            }
        }

        static int lastKeyEvent = 0;

        public static void OnSavedOrUndo()
        {
            if (Config.Instance.PostFormattingUndoCaretReset && SourceCodeFormatter.CaretBeforeLastFormatting != -1)
            {
            }
        }

        static void AddInternalShortcuts(string shortcutSpec, string displayName, Action handler, Dictionary<Keys, int> uniqueKeys)
        {
            ShortcutKey shortcut = Plugin.ParseAsShortcutKey(shortcutSpec);

            internalShortcuts.Add(shortcut, new Tuple<string, Action>(displayName, handler));

            var key = (Keys)shortcut._key;
            if (!uniqueKeys.ContainsKey(key))
                uniqueKeys.Add(key, 0);
        }

        static IEnumerable<Keys> BindInternalShortcuts()
        {
            var uniqueKeys = new Dictionary<Keys, int>();

            AddInternalShortcuts("_ShowAutoComplete:Ctrl+Space",
                                 "Show auto-complete list",
                                 ShowSuggestionList, uniqueKeys);

            AddInternalShortcuts("_InsertCodeSnippet:Ctrl+Shift+Space",
                                 "Insert Code Snippet",
                                 ShowSnippetsList, uniqueKeys);

            AddInternalShortcuts("_FindAllReferences:Shift+F12",
                                 "Find All References",
                                 FindAllReferences, uniqueKeys);

            AddInternalShortcuts("_GoToDefinition:F12",
                                 "Go To Definition",
                                 GoToDefinition, uniqueKeys);

            return uniqueKeys.Keys;
        }

        public static Dictionary<ShortcutKey, Tuple<string, Action>> internalShortcuts = new Dictionary<ShortcutKey, Tuple<string, Action>>();

        static bool invokeInProgress = false;

        static void InvokeShortcutHandler(Action action)
        {
            //notepad++ plugins invoked with Keyboard interceptor are reentrant !!!!!
            Cursor.Current = Cursors.WaitCursor;
            if (!invokeInProgress)
            {
                invokeInProgress = true;
                action();
                invokeInProgress = false;
            }
            Cursor.Current = Cursors.Default;
        }

        static void HandleErrors(Action action, Action finalAction = null)
        {
            try
            {
                action();
            }
#if DEBUG
            catch (Exception e)
            {
                //for usability reasons is better not to popup message boxes
                MessageBox.Show("Error: \n" + e.ToString(), "Notepad++");
            }
#else
            catch { }
#endif
            finally
            {
                finalAction?.Invoke();
            }
        }

        static SnippetContext currentSnippetContext = null;

        static void InsertCodeSnippet(string token, Point tokenPoints)
        {
            string replacement = Snippets.GetTemplate(token);
            if (replacement != null)
            {
                var document = Npp.GetCurrentDocument();
                int currentLineNum = document.GetCurrentLineNumber();
                int lineStartPos = document.PositionFromLine(currentLineNum);

                int horizontalOffset = tokenPoints.X - lineStartPos;

                //relative selection in the replacement text
                currentSnippetContext = Snippets.PrepareForIncertion(replacement, horizontalOffset, tokenPoints.X);

                document.ReplaceWordAtCaret(currentSnippetContext.ReplacementString);

                document.SetIndicatorStyle(SnippetContext.indicatorId, SciMsg.INDIC_BOX, Color.Blue);

                foreach (var point in currentSnippetContext.Parameters)
                {
                    document.PlaceIndicator(SnippetContext.indicatorId, point.X, point.Y);
                }

                if (currentSnippetContext.CurrentParameter.HasValue)
                {
                    document.SetSelection(currentSnippetContext.CurrentParameter.Value.X, currentSnippetContext.CurrentParameter.Value.Y);
                    currentSnippetContext.CurrentParameterValue = document.GetTextBetween(currentSnippetContext.CurrentParameter.Value);
                }

                if (autocompleteForm != null)
                {
                    if (autocompleteForm.Visible)
                        autocompleteForm.Close();
                    autocompleteForm = null;
                }
            }
        }

        static void FindAllReferences()
        {
            HandleErrors(() =>
            {
                if (Npp.Editor.IsCurrentDocScriptFile())
                {
                    DisplayInOutputPanel("Searching for references...");

                    string[] references = FindAllReferencesAtCaret();
                    if (references.Count() == 0)
                    {
                        //It's hard to believe but Roslyn may return some references if just executed second time.
                        //Somehow timing matters. Most likely it's be fixed in the Roslyn production release.
                        Thread.Sleep(100);
                        Npp.Editor.SaveCurrentFile();
                        EnsureCurrentFileParsed();
                        references = FindAllReferencesAtCaret();
                    }

                    if (references.Count() > 0)
                    {
                        string text = string.Format("{0} reference{1} found:{2}{3}",
                            references.Count(),
                            (references.Count() == 1 ? "" : "s"),
                            Environment.NewLine,
                            string.Join(Environment.NewLine, references));

                        DisplayInOutputPanel(text);
                    }
                    else
                        DisplayInOutputPanel("0 references found");
                }
            });
        }

        static int GetDocPosition(string file, int line, int column) //offsets are 1-based
        {
            var pos = 0;
            if (file == Npp.Editor.GetCurrentFilePath())
                pos = Npp.GetCurrentDocument().GetPositionFromLineColumn(line - 1, column - 1); //more accurate as the file can be modified
            else
                pos = StringExtesnions.GetPosition(file, line - 1, column - 1);
            return pos;
        }

        static void RenameMemberAtCaret()
        {
            //+ adjust caret pos after replacement
            //handle external files refs
            //+ add defenition ref (e.g. F12) instead of caretRef
            //+ Hook to Ctrl+R+R
            HandleErrors(() =>
            {
                Cursor.Current = Cursors.WaitCursor;

                if (Npp.Editor.IsCurrentDocScriptFile())
                {
                    var document = Npp.GetCurrentDocument();
                    //note initial state
                    string currentDocFile = Npp.Editor.GetCurrentFilePath();
                    int initialCaretPos = document.GetCurrentPos();
                    Point wordAtCaretLocation;
                    string wordToReplace = document.GetWordAtCursor(out wordAtCaretLocation, SimpleCodeCompletion.Delimiters);

                    document.SetSelection(wordAtCaretLocation.X, wordAtCaretLocation.Y);

                    //prompt user
                    using (var input = new RenameForm(wordToReplace))
                    {
                        input.ShowDialog();
                        string replacementWord = input.RenameTo;

                        Cursor.Current = Cursors.WaitCursor;

                        if (replacementWord.Any() && replacementWord != wordToReplace)
                        {
                            document.ClearSelection();
                            Npp.Editor.SaveCurrentFile();

                            //resolve
                            List<string> references = FindAllReferencesAtCaret().ToList();

                            DomRegion definition = ResolveMemberAtCaret();

                            if (!definition.IsEmpty)
                            {
                                references.Add($"{definition.FileName}({definition.BeginLine},{definition.BeginColumn}): " + wordToReplace);
                            }

                            //consolidate references
                            var replacements = references.Select(refString =>
                                                                 {
                                                                     string file;
                                                                     int line, column;

                                                                     //Example" "C:\Users\<user>\Documents\C# Scripts\dev.cs(20,5): new Test..."

                                                                     if (StringExtesnions.ParseAsErrorFileReference(refString, out file, out line, out column))
                                                                     {
                                                                         StringExtesnions.NormaliseFileReference(ref file, ref line);

                                                                         var pos = GetDocPosition(file, line, column);

                                                                         return new
                                                                         {
                                                                             File = file,
                                                                             Start = pos,
                                                                             End = pos + wordToReplace.Length
                                                                         };
                                                                     }
                                                                     else
                                                                         return null;
                                                                 });

                            replacements = replacements.ToArray();

                            //do the replacement
                            var fileReplecements = replacements.Where(x => x != null)
                                                               .OrderByDescending(x => x.Start)
                                                               .GroupBy(x => x.File)
                                                               .ToDictionary(x => x.Key, x => x);

                            foreach (var file in fileReplecements.Keys)
                            {
                                var items = fileReplecements[file];

                                int diff = 0;
                                if (file == currentDocFile)
                                {
                                    int itemsBeforeCaret = items.Count(x => x.Start < wordAtCaretLocation.X);
                                    diff = (wordToReplace.Length - replacementWord.Length) * itemsBeforeCaret;
                                }

                                string code = File.ReadAllText(file);
                                var newCode = new StringBuilder(500);

                                var entries = items.OrderBy(x => x.Start).ToArray();

                                int latsItemEnd = 0;
                                foreach (var item in entries)
                                {
                                    int start = item.Start;
                                    int end = item.End;

                                    newCode.Append(code.Substring(latsItemEnd, start - latsItemEnd));
                                    newCode.Append(replacementWord);
                                    latsItemEnd = end;
                                }

                                newCode.Append(code.Substring(latsItemEnd, code.Length - latsItemEnd));

                                File.WriteAllText(file, newCode.ToString());
                                Debug.WriteLine(file);
                                npp.ReloadFile(false, file);

                                if (file == currentDocFile)
                                {
                                    document.SetCurrentPos(initialCaretPos - diff);
                                    document.ClearSelection();
                                }
                            }
                        }

                        document.ClearSelection();
                    }
                }

                Cursor.Current = Cursors.Default;
            });
        }

        static public void OnBeforeDocumentSaved()
        {
            if (Config.Instance.FormatOnSave)
                FormatDocument();
        }

        private const int MARK_BREAKPOINT = 7;
        private const int MARK_BREAKPOINT_MASK = 1 << 7;

        static bool formattingInProgress = false;

        static void FormatDocument()
        {
            if (formattingInProgress)
                return; //aborting non critical operation

            formattingInProgress = true;

            HandleErrors(() =>
            {
                if (Npp.Editor.IsCurrentDocScriptFile())
                {
                    // note berakpoints and their lines
                    var doc = Npp.GetCurrentDocument();

                    var text_before = doc.AllText();
                    var break_points = doc.LinesWithMarker(MARK_BREAKPOINT);

                    var p = doc.PositionFromLine(3).Value;
                    var l = doc.LineFromPosition(new Position(p));

                    SourceCodeFormatter.FormatDocument();

                    var text_after = doc.AllText();

                    // restore breakpoints after formatting
                    doc.DeleteAllMarkers(MARK_BREAKPOINT);
                    foreach (var line_before_format in break_points)
                    {
                        var line_after_format = text_after.MapLine(line_before_format, text_before);
                        doc.PlaceMarker(MARK_BREAKPOINT, line_after_format);
                    }
                }
            },
            () => formattingInProgress = false);
        }

        static void GoToDefinition()
        {
            HandleErrors(() =>
            {
                if (Npp.Editor.IsCurrentDocScriptFile())
                {
                    DomRegion region = ResolveMemberAtCaret();

                    if (!region.IsEmpty)
                    {
                        Npp.Editor.Open(region.FileName);

                        var document = Npp.GetCurrentDocument();

                        document.GoToLine(region.BeginLine);
                        document.ScrollToCaret();
                        document.GrabFocus();
                    }
                }
            });
        }

        static void ShowMethodInfo()
        {
            ShowInfo(false);
        }

        static void ShowQuickInfo()
        {
            if (!Config.Instance.ShowQuickInfoAsNativeNppTooltip)
                ShowInfo(true);
        }

        public static bool IsShowingMemberInfo
        {
            get { return memberInfoPopup != null && memberInfoPopup.IsShowing; }
        }

        public static bool IsShowingAutocompletion
        {
            get { return autocompleteForm != null && autocompleteForm.Visible; }
        }

        public static bool IsShowingNamespaceMenu
        {
            get { return namespaceMenu != null && namespaceMenu.Visible; }
        }

        public static bool IsShowingInteractivePopup
        {
            get { return IsShowingMemberInfo || IsShowingAutocompletion || IsShowingNamespaceMenu; }
        }

        static void ShowInfo(bool simple)
        {
            if (!Config.Instance.DisableMethodInfo)
                HandleErrors(() =>
                {
                    if (memberInfoPopup.IsShowing)
                    {
                        if (simple)
                        {
                            return;
                        }
                        else
                        {
                            memberInfoPopup.Close();
                        }
                    }

                    int methodStartPosition = 0;

                    string[] data = GetMemberUnderCursorInfo(simple, ref methodStartPosition);

                    if (data.Length > 0)
                    {
                        if (simple && Config.Instance.ShowQuickInfoInStatusBar)
                            npp.SetStatusbarLabel(data.FirstOrDefault() ?? defaultStatusbarLabel);
                        else
                            memberInfoPopup.TriggerPopup(simple, methodStartPosition, data);
                    }
                });
        }

        public static string[] GetMemberUnderCursorInfo()
        {
            int methodStartPosition = 0;
            return GetMemberUnderCursorInfo(true, ref methodStartPosition);
        }

        static string[] GetMemberUnderCursorInfo(bool simple, ref int methodStartPosition)
        {
            string file = Npp.Editor.GetCurrentFilePath();
            var document = Npp.GetCurrentDocument();

            if (file.IsScriptFile())
            {
                int pos;

                if (simple)
                    pos = document.GetPositionFromMouseLocation();
                else
                    pos = document.GetCurrentPos();

                if (pos != -1)
                {
                    int rawPos = pos; //note non-decorated position

                    string text = document.GetTextBetween(0, npp.DocEnd);

                    CSScriptHelper.DecorateIfRequired(ref text, ref pos);

                    EnsureCurrentFileParsed();

                    int methodStartPosTemp;

                    string[] data = SimpleCodeCompletion.GetMemberInfo(text, pos, file, simple, out methodStartPosTemp);

                    methodStartPosition = rawPos + (methodStartPosTemp - pos);

                    return data;
                }
            }

            return new string[0];
        }

        static CustomContextMenu namespaceMenu;

        static void AddAllMissingUsings()
        {
            if (UltraSharp.Cecil.Reflector.GetCodeCompileOutput != null)
                HandleErrors(() =>
                {
                    if (Npp.Editor.IsCurrentDocScriptFile())
                    {
                        var cursor = Cursor.Current;
                        try
                        {
                            Cursor.Current = Cursors.WaitCursor;

                            bool usingInserted = false;
                            int count = 0;

                            do
                            {
                                Npp.Editor.SaveCurrentFile();
                                var document = Npp.GetCurrentDocument();
                                usingInserted = false;
                                count++;

                                List<string> presentUsings = Reflector.GetCodeUsings(document.GetTextBetween(0, -1)).ToList();

                                //c:\Users\user\Documents\C# Scripts\TooltipTest1.cs(27,9): error CS0103: The name 'Debug' does not exist in the current context

                                string currentDocument = Npp.Editor.GetCurrentFilePath();

                                string[] output = UltraSharp.Cecil.Reflector.GetCodeCompileOutput(currentDocument);

                                var missingNamespaceErrors = output.Select(x => x.ToFileErrorReference())
                                                                   .Where(x => string.Compare(x.File, currentDocument, true) == 0)
                                                                   .ToArray();

                                var namespacesToInsert = new List<string>();

                                foreach (FileReference item in missingNamespaceErrors)
                                {
                                    int errorPosition = document.PositionFromLine(item.Line - 1) + item.Column - 1;
                                    IEnumerable<Intellisense.Common.TypeInfo> items = ResolveNamespacesAtPosition(errorPosition)
                                                                   .Where(x => !presentUsings.Contains(x.Namespace));

                                    if (items.Count() == 1) //do only if there is no ambiguity about what namespace it is
                                    {
                                        string resolvedNamespace = items.First().Namespace;
                                        if (Npp.Editor.GetCurrentFilePath().IsVbFile())
                                            namespacesToInsert.Add("Imports " + resolvedNamespace);
                                        else
                                            namespacesToInsert.Add("using " + resolvedNamespace + ";");

                                        presentUsings.Add(resolvedNamespace);
                                        usingInserted = true;
                                    }
                                }

                                namespacesToInsert.ForEach(x => NppEditor.InsertNamespace(x));
                            } while (usingInserted && count < 10); //10 just a safe guard
                        }
                        catch { }
                        finally
                        {
                            Cursor.Current = cursor;
                        }
                    }
                });
        }

        static void AddMissingUsing()
        {
            HandleErrors(() =>
            {
                if (Npp.Editor.IsCurrentDocScriptFile())
                {
                    var document = Npp.GetCurrentDocument();
                    string[] presentUsings = UltraSharp.Cecil.Reflector.GetCodeUsings(document.GetTextBetween(0, -1));

                    IEnumerable<Intellisense.Common.TypeInfo> items = ResolveNamespacesAtCaret().Where(x => !presentUsings.Contains(x.Namespace)).ToArray();

                    if (items.Count() > 0)
                    {
                        Point point = document.GetCaretScreenLocation();

                        if (namespaceMenu != null && namespaceMenu.Visible)
                        {
                            namespaceMenu.Close();
                        }

                        namespaceMenu = new CustomContextMenu();
                        namespaceMenu.Left = point.X;
                        namespaceMenu.Top = point.Y + 18;

                        var usings = items.Where(x => !x.IsNested)
                                          .Select(x =>
                                          {
                                              if (Npp.Editor.GetCurrentFilePath().IsVbFile())
                                                  return "Imports " + x.Namespace;
                                              else
                                                  return "using " + x.Namespace + ";";
                                          });

                        var inline = items.Select(x => x.FullName);

                        foreach (string item in usings)
                            namespaceMenu.Add(item, Images.Images.namespace_add, NppEditor.InsertNamespace);

                        namespaceMenu.AddSeparator();

                        foreach (string item in inline)
                            namespaceMenu.Add(item, null, (replecementText) => Npp.GetCurrentDocument().ReplaceWordAtCaret(replecementText));

                        namespaceMenu.Popup();
                        namespaceMenu.Show();
                    }
                }
            });
        }

        static void ShowAboutBox()
        {
            using (var form = new AboutBox())
                form.ShowDialog();
        }

        static public void ShowConfig()
        {
            using (var form = new ConfigForm(Config.Instance))
            {
                form.ShowDialog();
                Config.Instance.Save();
                ReflectorExtensions.IgnoreDocumentationExceptions = Config.Instance.IgnoreDocExceptions;
            }
        }

        static AutocompleteForm autocompleteForm;

        static void ShowSnippetsList()
        {
            HandleErrors(() =>
            {
                if (Npp.Editor.IsCurrentDocScriptFile())
                {
                    ShowSuggestionList(true);
                }
                else if (Config.Instance.InterceptCtrlSpace)
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)WinMsg.WM_COMMAND, (int)NppMenuCmd.IDM_EDIT_FUNCCALLTIP, 0);
                }
            });
        }

        static void ShowSuggestionList()
        {
            HandleErrors(() =>
            {
                if (Npp.Editor.IsCurrentDocScriptFile())
                {
                    ShowSuggestionList(false);
                }
                else if (Config.Instance.InterceptCtrlSpace)
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)WinMsg.WM_COMMAND, (int)NppMenuCmd.IDM_EDIT_AUTOCOMPLETE, 0);
                }
            });
        }

        static void ShowSuggestionList(bool snippetsOnly, bool memberInfoWasShowing = false)
        {
            IEnumerable<ICompletionData> items;

            bool cssSugesstion = false;

            if (snippetsOnly)
            {
                items = GetSnippetsItems();
            }
            else
            {
                var document = Npp.GetCurrentDocument();
                items = GetSuggestionItemsAtCaret();
                bool namespaceSuggestion = items.All(x => x.CompletionType == CompletionType._namespace);

                Point point;
                var word = document.GetWordAtCursor(out point);
                int wordStartPos = point.X;

                string textOnLeft = document.TextBeforePosition(wordStartPos, 300);

                bool memberSugesstion = textOnLeft.EndsWith(".");
                bool assignmentSugesstion = textOnLeft.TrimEnd().EndsWith("=");

                if (!memberSugesstion && !namespaceSuggestion && !assignmentSugesstion)
                {
                    cssSugesstion = textOnLeft.Split('\n').Last().TrimStart().StartsWith("//css_");

                    if (!cssSugesstion)
                    {
                        if (textOnLeft == "//" && word.StartsWith("css_"))
                            cssSugesstion = true;
                    }

                    if (!cssSugesstion)
                    {
                        bool usingSuggestion = document.GetCurrentLine().Trim() == "using";
                        if (!usingSuggestion)
                            items = items.Concat(GetSnippetsItems());
                    }
                }
            }

            if (items.Count() > 0)
            {
                if (memberInfoPopup.IsShowing)
                {
                    memberInfoPopup.Close();
                    NppUI.Marshal(() => ShowSuggestionList(snippetsOnly, true));
                    return;
                }

                Point point = Npp.GetCurrentDocument().GetCaretScreenLocation();

                if (autocompleteForm != null && autocompleteForm.Visible)
                {
                    autocompleteForm.Close();
                }

                var document = Npp.GetCurrentDocument();
                Action<ICompletionData> OnAccepted = data => OnAutocompletionAccepted(data, snippetsOnly);

                // no need to pass initial SuggestionHint as it may swallow (auto accept) the whole autocompletion window
                // in case of the hint to be the full match of one of the items. Just do it for a better UX

                autocompleteForm = new AutocompleteForm(OnAccepted, items, null);
                autocompleteForm.Left = point.X;
                autocompleteForm.Top = point.Y + document.TextHeight(document.GetCurrentLineNumber());
                autocompleteForm.FormClosed += (sender, e) =>
                {
                    if (memberInfoWasShowing)
                        NppUI.Marshal(() => Dispatcher.Schedule(100, ShowMethodInfo));
                };
                //form.KeyPress += (sender, e) =>
                //{
                //    if (e.KeyChar >= ' ' || e.KeyChar == 8) //8 is backspace
                //        OnAutocompleteKeyPress(e.KeyChar);
                //};
                autocompleteForm.Show();

                OnAutocompleteKeyPress(allowNoText: true); //to grab current word at the caret an process it as a hint
            }
        }

        static void OnAutocompletionAccepted(ICompletionData data, bool snippetsOnlyMode)
        {
            if (data != null)
            {
                var document = Npp.GetCurrentDocument();
                // int newSelEnd = -1;
                int currentPos = document.GetCurrentPos();
                Point p;
                string word = document.GetWordAtCursor(out p);

                if (word == "=") // .Load +=|
                {
                    document.SetSelection(p.X, p.Y);
                    document.ReplaceSel("= ");

                    currentPos = document.GetCurrentPos();
                    word = "";
                }

                if (word != "")  // e.g. Console.Wr| but not Console.|
                {
                    string textOnLeft = document.TextBeforePosition(p.X, 300);
                    if (textOnLeft.EndsWith("//") && word.StartsWith("css_"))
                        document.SetSelection(p.X - 2, p.Y);
                    else
                        document.SetSelection(p.X, p.Y);
                }
                else
                {
                    string leftChar = document.GetTextBetween(p.X - 1, p.X);
                    if (leftChar == "=")
                    {
                        document.ReplaceSelection(" "); //add space
                        //currentPos = Npp.SetCaretPosition(currentPos + 1);
                        document.ClearSelection();
                    }

                    // myForm.Result =   |
                    var lStart = document.PositionFromLine(document.LineFromPosition(currentPos));
                    string lineLeftPart = document.GetTextBetween(lStart, currentPos);
                    string textOnLeft = lineLeftPart.TrimEnd();
                    if (textOnLeft.EndsWith("="))
                    {
                        int dif = lineLeftPart.Length - textOnLeft.Length;
                        //set it to  myForm.Result = |
                        if (dif > 1)
                        {
                            document.SetCurrentPos(currentPos - dif + 1);
                            currentPos = document.GetCurrentPos();
                        }
                    }
                }

                //Note CompletionText caret position if any
                var completionText = data.CompletionText;
                if (data.CompletionText.EndsWith("()"))
                {
                    int selEnd = p.Y;
                    string rightText = document.TextAfterPosition(selEnd, 512);
                    if (rightText.TrimStart().StartsWith("()"))
                    {
                        // the completion text ends with "()" and the word at the caret already has "()"
                        // so to avoid duplication trim completion text brackets
                        completionText = completionText.Substring(0, completionText.Length - 2);
                    }
                }

                InsertCompletion(completionText);

                //check for extra content to insert
                if (data.Tag is Dictionary<string, object>)
                {
                    try
                    {
                        var dict = data.Tag as Dictionary<string, object>;
                        if (dict.ContainsKey("insertionPos") && dict.ContainsKey("insertionContent"))
                        {
                            var insertionPos = (int)dict["insertionPos"];
                            var insertionContent = (string)dict["insertionContent"];

                            //the insertion point could be already shifted because of the previous insertion
                            if (currentPos < insertionPos)
                                insertionPos += data.CompletionText.Length;

                            document.SetCurrentPos(insertionPos);
                            document.ClearSelection();
                            InsertCompletion(insertionContent);
                        };
                    }
                    catch { }
                }

                if (snippetsOnlyMode)
                    TriggerCodeSnippetInsertion();
            }

            if (autocompleteForm != null)
            {
                if (autocompleteForm.Visible)
                    autocompleteForm.Close();
                autocompleteForm = null;
            }
        }

        static void InsertCompletion(string completionText)
        {
            var document = Npp.GetCurrentDocument();

            int newCarrentPos = -1;
            int completionCaretPos = completionText.IndexOf("$|$");
            if (completionCaretPos != -1)
            {
                completionText = completionText.Replace("$|$", "");
                newCarrentPos = document.GetCurrentPos() + completionCaretPos;// + "$|$".Length;
            }

            //the actual completion injection
            document.ReplaceSelection(completionText);
            document.ClearSelection();

            //process new caret position if was requested
            if (newCarrentPos != -1)
            {
                document.MoveCaretTo(newCarrentPos);
            }
        }

        static public void OnAutocompleteKeyPress(char keyChar = char.MinValue, bool allowNoText = false)
        {
            try
            {
                var document = Npp.GetCurrentDocument();
                int currentPos = document.GetCurrentPos();

                string text = document.GetTextBetween(Math.Max(0, currentPos - 30), currentPos); //check up to 30 chars from left
                string hint = null;

                char[] delimiters = SimpleCodeCompletion.Delimiters;
                string word = document.GetWordAtPosition(currentPos);
                if (word.StartsWith("css_")) //CS-Script directive
                    delimiters = SimpleCodeCompletion.CSS_Delimiters;

                int pos = text.LastIndexOfAny(delimiters);
                if (pos != -1)
                {
                    // hint = text.Substring(pos + 1).Trim();
                    hint = word;
                }
                else if (text.Length == currentPos) //start of the doc
                    hint = text;

                var charOnRigt = document.GetTextBetween(currentPos, currentPos + 1);
                var charOnLeft = document.GetTextBetween(currentPos - 1, currentPos);
                Debug.WriteLine("charOnLeft ({0}); charOnRigth ({1})", charOnLeft, charOnRigt);
                if (char.IsWhiteSpace(charOnLeft[0])) // Console.Wr |
                    hint = null;
                else if (charOnRigt[0].IsOneOf('.', '[', '<', '(', '{')) // Console|.Wr
                    hint = null;

                if (autocompleteForm != null)
                {
                    try
                    {
                        if (hint != null)
                        {
                            //Debug.WriteLine("Autocomplete hint: " + hint);
                            autocompleteForm.FilterFor(hint);
                        }
                        else
                        {
                            if (!allowNoText)
                            {
                                autocompleteForm.Close();
                                autocompleteForm = null;
                            }
                        }
                    }
                    catch { } //possible to be called after disposing the form
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e); //there can be legitimate failures
            }
        }

        static void Reparse()
        {
            parsedFiles.Clear();
            currentFile = null;
            EnsureCurrentFileParsedAsynch();
        }

        static string[] mustBeAssignedWithLitterals = "var,string,String,bool,byte,sbyte,char,decimal,double,float,int,uint,long,ulong,short,ushort,Byte,SByte,Char,Decimal,Double,Single,Int32,UInt32,Int64,UInt64,Int16,UInt16,IntPtr".Split(',');

        static public void OnCharTyped(char c)
        {
            if (Npp.Editor.IsCurrentDocScriptFile())
            {
                SourceCodeFormatter.CaretBeforeLastFormatting = -1;

                var document = Npp.GetCurrentDocument();
                var caret = document.GetCurrentPos();
                bool typingNewWord = false;

                if (Config.Instance.AutoSuggestOnOpenEndLine)
                {
                    string textOnRight = document.GetTextBetween(caret, -1);
                    if (textOnRight.HasText() && !char.IsWhiteSpace(c))
                    {
                        textOnRight = textOnRight.GetLines(2).FirstOrDefault() ?? "";
                        typingNewWord = textOnRight.Trim() == "";
                    }
                }

                if (c == '.' || c == '_' || c == '=')
                {
                    // works well but because of auto-accept single suggestion it inserts
                    if (c == '=')
                    {
                        string lineLeftPart = document.GetTextBetween(Math.Max(0, caret - 200), caret).GetLines().LastOrDefault();
                        string textOnLeft = lineLeftPart.TrimEnd();
                        string[] words = textOnLeft.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Reverse()
                                                   .ToArray();
                        // = name var
                        if (words.Count() > 2 && mustBeAssignedWithLitterals.Contains(words[2]))
                        {
                            SourceCodeFormatter.OnCharTyped(c);
                            return;
                        }
                    }
                    ShowSuggestionList();
                }
                else if (c == '(')
                {
                    if (!Config.Instance.DisableMethodInfoAutoPopup)
                    {
                        if (!memberInfoPopup.IsShowing || memberInfoPopup.Simple)
                            Dispatcher.Schedule(1, ShowMethodInfo); //to allow N++ bracket auto-insertion to proceed
                    }
                }
                else if (typingNewWord)
                {
                    ShowSuggestionList();
                }
                else if (memberInfoPopup.IsShowing)
                {
                    memberInfoPopup.CheckIfNeedsClosing();
                }
                else if (autocompleteForm != null && autocompleteForm.Visible)
                {
                    if (c >= ' ' || c == 8) //8 is backspace
                        OnAutocompleteKeyPress(c);
                }
                else
                    SourceCodeFormatter.OnCharTyped(c);
            }
        }

        static string defaultStatusbarLabel = "C# source file";

        static public void OnCurrentFileChanegd()
        {
            if (Plugin.Enabled)
            {
                Plugin.EnsureCurrentFileParsedAsynch();

                if (!Config.Instance.DisableMethodInfo)
                {
                    memberInfoPopup.Enabled = Npp.Editor.IsCurrentDocScriptFile();
                }
            }
        }

        static MouseMonitor mouseHook = new MouseMonitor();

        static public void OnNppReady()
        {
            mouseHook.MouseLClick += MouseHook_MouseLClick;
            mouseHook.Install();

            Plugin.EnsureCurrentFileParsedAsynch();
            Snippets.Init();
        }

        private static void MouseHook_MouseLClick()
        {
            if (Config.Instance.GoToDefinitionOnCtrlClick)
            {
                var modifiers = KeyInterceptor.GetModifiers();

                if (modifiers.IsCtrl && !modifiers.IsShift && !modifiers.IsAlt)
                {
                    var document = Npp.GetCurrentDocument();
                    if (document.GetSelectedText().IsNullOrEmpty())
                    {
                        var caret = document.GetPositionFromMouseLocation();
                        if (caret != -1)
                            Task.Factory.StartNew(GoToDefinition); //let click to get handled, otherwise accidental selection will occur
                    }
                }
            }
        }

        static public void EnsureCurrentFileParsedAsynch()
        {
            if (Plugin.Enabled)
                Task.Factory.StartNew(EnsureCurrentFileParsed);
        }

        static public Func<string> ResolveCurrentFile = Npp.Editor.GetCurrentFilePath; //the implementation can be injected by the host or other plugins. To be used in the future.
        static public Action<string> DisplayInOutputPanel = Npp.Editor.DisplayInNewDocument; //the implementation can be injected by the host or other plugins. To be used in the future.

        static Dictionary<string, DateTime> currentSourcesStates = new Dictionary<string, DateTime>();

        static bool CurrentSourcesChanged()
        {
            //checking the hash for all source files can be very expensive so checking timestamps only
            foreach (string file in currentSourcesStates.Keys)
                if (!File.Exists(file) || currentSourcesStates[file] != File.GetLastWriteTimeUtc(file))
                    return true;
            return false;
        }

        static void NoteCurrentSourcesStates(string[] sourceFiles)
        {
            currentSourcesStates.Clear();
            foreach (string file in sourceFiles)
                currentSourcesStates.Add(file, File.GetLastWriteTimeUtc(file));
        }

        static public void EnsureCurrentFileParsed()
        {
            if (Plugin.Enabled)
                lock (typeof(Plugin))
                {
                    try
                    {
                        string file = ResolveCurrentFile();

                        if (string.IsNullOrWhiteSpace(file) || !file.IsScriptFile())
                            return;

                        int newCssHash = -1;
                        //when to regenerate the project:
                        // - not initialized yet
                        // - new file which is not a part of the 'current' project
                        // - the current file changed and saved with the new set of CS-Script instructions (for multi-file mode only)
                        if (currentFile == null ||
                            (currentFile != file && !parsedFiles.Contains(file)) ||
                            (!SingleFileMode && CurrentSourcesChanged()) ||
                            (!SingleFileMode && currentFileTimestamp != File.GetLastWriteTime(file) && currentFileCssHash != (newCssHash = NppEditor.GetCssHash(file))))
                        {
                            currentFile = file;
                            currentFileCssHash = (newCssHash != -1) ? newCssHash : NppEditor.GetCssHash(file);
                            currentFileTimestamp = File.GetLastWriteTime(file);

                            Tuple<string[], string[]> project = CSScriptHelper.GetProjectFiles(file);

                            string[] sourceFiles = project.Item1;
                            string[] assemblyFiles = project.Item2;

                            NoteCurrentSourcesStates(sourceFiles);

                            var sourcesInfos = new List<Tuple<string, string>>();

                            if (SingleFileMode)
                            {
                                string code = Npp.GetCurrentDocument().GetTextBetween(0, npp.DocEnd);
                                CSScriptHelper.DecorateIfRequired(ref code);
                                sourcesInfos.Add(new Tuple<string, string>(code, file));
                            }
                            else
                            {
                                foreach (string srcFile in sourceFiles)
                                {
                                    string code = File.ReadAllText(srcFile);
                                    if (srcFile == currentFile)
                                        CSScriptHelper.DecorateIfRequired(ref code);
                                    sourcesInfos.Add(new Tuple<string, string>(code, srcFile));
                                }
                            }

                            SimpleCodeCompletion.ResetProject(sourcesInfos.ToArray(), assemblyFiles);
                        }
                    }
                    catch { }
                }
        }

        static void WithTextAtCaret(Action<string, int, string> action)
        {
            var document = Npp.GetCurrentDocument();
            string file = Npp.Editor.GetCurrentFilePath();
            string text = document.GetTextBetween(0, npp.DocEnd);
            int currentPos = document.GetCurrentPos();

            CSScriptHelper.DecorateIfRequired(ref text, ref currentPos);
            EnsureCurrentFileParsed();

            action(text, currentPos, file);
        }

        static IEnumerable<ICompletionData> GetSnippetsItems()
        {
            return Snippets.Keys.Select(x => new SnippetCompletionData { CompletionText = x, DisplayText = x });
        }

        static IEnumerable<ICompletionData> GetSuggestionItemsAtCaret()
        {
            IEnumerable<ICompletionData> retval = null;

            WithTextAtCaret((text, currentPos, file) =>
                retval = SimpleCodeCompletion.GetCompletionData(text, currentPos, file)
                                             .Where(item => item != null));
            return retval;
        }

        static IEnumerable<Intellisense.Common.TypeInfo> ResolveNamespacesAtCaret()
        {
            int currentPos = Npp.GetCurrentDocument().GetCurrentPos();
            return ResolveNamespacesAtPosition(currentPos);
        }

        static int[] WordEndsOf(string text)
        {
            var delimiters = ".,\t ;<>{}()|\\/?=-*+&^%!\r\n";
            var result = new List<int>();
            bool wordStarted = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (delimiters.Contains(text[i]))
                {
                    if (wordStarted)
                        result.Add(i);
                    wordStarted = false;
                }
                else
                {
                    wordStarted = true;
                }
            }
            return result.ToArray();
        }

        static IEnumerable<Intellisense.Common.TypeInfo> ResolveNamespacesAtPosition(int currentPos)
        {
            var document = Npp.GetCurrentDocument();

            string file = Npp.Editor.GetCurrentFilePath();
            string text = document.GetTextBetween(0, npp.DocEnd);

            int lineNum = document.LineFromPosition(currentPos);
            int start = document.PositionFromLine(lineNum);
            string line = document.GetLine(lineNum);

            int currentPosOffset = currentPos - start; //note the diff between cuurrPos and start of the line
            int[] probingOffsets = WordEndsOf(line); //note the all end positions of the words

            probingOffsets = probingOffsets.OrderBy(x => x - currentPosOffset).ToArray();
            var words = probingOffsets.Select(x => line.Substring(x)).ToArray();

            //start from currentPosOffset and go left, then go to the right
            //probingOffsets = probingOffsets.Where(x => x <= currentPosOffset)
            //                               .Reverse()
            //                               .Concat(probingOffsets.Where(x => x > currentPosOffset))
            //                               .ToArray();

            words = probingOffsets.Select(x => line.Substring(x)).ToArray();

            CSScriptHelper.DecorateIfRequired(ref text, ref currentPos);

            EnsureCurrentFileParsed();

            var actualStart = currentPos - currentPosOffset; //after the decoration currentPos is changed so the line start

            var allUsings = new List<Intellisense.Common.TypeInfo>();
            foreach (int offset in probingOffsets) //try to resolve any 'word' in the line
            {
                var result = SimpleCodeCompletion.GetMissingUsings(text, actualStart + offset, file);
                if (result.Any())
                {
                    allUsings.AddRange(result);
                    if (Config.Instance.UsingRoslyn) //Roslyn is slow with resolving namespaces so do it less aggressive way than NRefactory
                        break;
                }
            }

            return allUsings.DistinctBy(x => x.Namespace).ToArray();
        }

        static IEnumerable<Intellisense.Common.TypeInfo> ResolveNamespacesAtPositionOld(int currentPos)
        {
            string file = Npp.Editor.GetCurrentFilePath();
            string text = Npp.GetCurrentDocument().GetTextBetween(0, npp.DocEnd);

            CSScriptHelper.DecorateIfRequired(ref text, ref currentPos);

            EnsureCurrentFileParsed();

            return SimpleCodeCompletion.GetMissingUsings(text, currentPos, file);
        }

        static DomRegion ResolveMemberAtCaret()
        {
            var document = Npp.GetCurrentDocument();

            string file = Npp.Editor.GetCurrentFilePath();
            string text = document.GetTextBetween(0, npp.DocEnd);
            int currentPos = document.GetCurrentPos();

            try
            {
                // just to handle NPP strange concept of caret position being not a point of the text
                // but an index of the byte array
                currentPos = document.CaretToTextPosition(currentPos);
            }
            catch { }

            var decorated = CSScriptHelper.DecorateIfRequired(ref text, ref currentPos);

            EnsureCurrentFileParsed();
            DomRegion result = SimpleCodeCompletion.ResolveMember(text, currentPos, file);

            if (decorated && result.FileName == file)
                CSScriptHelper.Undecorate(text, ref result);

            return result;
        }

        static string[] FindAllReferencesAtCaret()
        {
            string[] retval = null;

            WithTextAtCaret((text, currentPos, file) =>
            {
                retval = SimpleCodeCompletion.FindReferences(text, currentPos, file);
            });

            return retval;
        }

        public static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, string shortcutSpec)
        {
            PluginBase.SetCommand(index, commandName, functionPointer, ParseAsShortcutKey(shortcutSpec), false);
        }

        public static ShortcutKey ParseAsShortcutKey(string shortcutSpec)
        {
            var parts = shortcutSpec.Split(':');

            string shortcutName = parts[0];
            string shortcutData = parts[1];

            try
            {
                var actualData = Config.Shortcuts.GetValue(shortcutName, shortcutData);
                return new ShortcutKey(actualData);
            }
            catch
            {
                Config.Shortcuts.SetValue(shortcutName, shortcutData);
                return new ShortcutKey(shortcutData);
            }
        }
    }
}