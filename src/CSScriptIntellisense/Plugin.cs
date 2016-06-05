using Intellisense.Common;
using CSScriptIntellisense.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UltraSharp.Cecil;
using System.Runtime.InteropServices;

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
                    Plugin.SetCommand(index, name, handler, shortcut);
                });
        }

        //this method will also be called from the parent plugin
        static public void CommandMenuInit(ref int cmdIndex, CSScriptNpp.SetMenuCommand setCommand)
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
                IEnumerable<Keys> keysToIntercept = BindInteranalShortcuts();

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
                SetCommand(cmdIndex++, "About", ShowAboutBox);
        }

        static bool TriggerCodeSnippetInsertion()
        {
            if (!Config.Instance.CodeSnippetsEnabled)
                return false;

            Debug.WriteLine("------------------ TRIGGER called");

            Point point;
            string token = Npp.GetWordAtCursor(out point);

            if (Snippets.Contains(token))
            {
                Dispatcher.Shedule(10, () =>
                     InsertCodeSnippet(token, point));
                return true;
            }
            return false;
        }

        static void Instance_KeyDown(Keys key, int repeatCount, ref bool handled)
        {
            if (IsShowingInteractivePopup)
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
                if ((key == Keys.Tab || key == Keys.Escape || key == Keys.Return) && Npp.IsCurrentScriptFile())
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

            if (key == Keys.Z && Config.Instance.PostFormattingUndoCaretReset && Npp.IsCurrentScriptFile() && SourceCodeFormatter.CaretBeforeLastFormatting != -1)
            {
                Modifiers modifiers = KeyInterceptor.GetModifiers();
                if (modifiers.IsCtrl && !modifiers.IsShift && !modifiers.IsAlt)
                {
                    if (Npp.CanUndo())
                    {
                        //Native NPP undo moves caret to the end of text unconditionally if the text was reset completely (e.g. CodeFormatting).
                        //Thus manual resetting is required after UNDO. Though NPP doesn't have undo notification so we use shortcut for this.
                        handled = true;
                        Npp.Undo();

                        int newCurrentPos = SourceCodeFormatter.CaretBeforeLastFormatting;
                        SourceCodeFormatter.CaretBeforeLastFormatting = -1;

                        Npp.SetCaretPosition(newCurrentPos);
                        Npp.ClearSelection();
                        Npp.SetFirstVisibleLine(Npp.GetLineNumber(newCurrentPos) - SourceCodeFormatter.TopScrollOffsetBeforeLastFormatting);
                    }
                }
            }

            if (Config.Instance.InterceptCtrlSpace && Npp.IsCurrentScriptFile())
            {
                foreach (var shortcut in internalShortcuts.Keys)
                    if ((byte) key == shortcut._key)
                    {
                        Modifiers modifiers = KeyInterceptor.GetModifiers();

                        if (modifiers.IsCtrl == shortcut.IsCtrl && modifiers.IsShift == shortcut.IsShift && modifiers.IsAlt == shortcut.IsAlt)
                        {
                            handled = true;
                            var handler = internalShortcuts[shortcut];
                            Dispatcher.Shedule(10, () => InvokeShortcutHandler(handler.Item2));

                            break;
                        }
                    }
            }

            if (key == Keys.R && Npp.IsCurrentScriptFile())
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

            var key = (Keys) shortcut._key;
            if (!uniqueKeys.ContainsKey(key))
                uniqueKeys.Add(key, 0);
        }

        static IEnumerable<Keys> BindInteranalShortcuts()
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

        static void HandleErrors(Action action)
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
        }

        static SnippetContext currentSnippetContext = null;

        static void InsertCodeSnippet(string token, Point tokenPoints)
        {
            string replacement = Snippets.GetTemplate(token);
            if (replacement != null)
            {
                int line = Npp.GetCaretLineNumber();
                int lineStartPos = Npp.GetLineStart(line);

                int horizontalOffset = tokenPoints.X - lineStartPos;

                //relative selection in the replacement text
                currentSnippetContext = Snippets.PrepareForIncertion(replacement, horizontalOffset, tokenPoints.X);

                Npp.ReplaceWordAtCaret(currentSnippetContext.ReplacementString);

                Npp.SetIndicatorStyle(SnippetContext.indicatorId, SciMsg.INDIC_BOX, Color.Blue);

                foreach (var point in currentSnippetContext.Parameters)
                {
                    Npp.PlaceIndicator(SnippetContext.indicatorId, point.X, point.Y);
                }

                if (currentSnippetContext.CurrentParameter.HasValue)
                {
                    Npp.SetSelection(currentSnippetContext.CurrentParameter.Value.X, currentSnippetContext.CurrentParameter.Value.Y);
                    currentSnippetContext.CurrentParameterValue = Npp.GetTextBetween(currentSnippetContext.CurrentParameter.Value);
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
                if (Npp.IsCurrentScriptFile())
                {
                    DisplayInOutputPanel("Searching for references...");

                    string[] references = FindAllReferencesAtCaret();
                    if (references.Count() == 0)
                    {
                        //It's hard to believe but Roslyn may return some references if just executed second time. 
                        //Somehow timing matters. Most likely it's be fixed in the Roslyn production release.
                        Thread.Sleep(100);
                        Npp.SaveCurrentFile();
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
            if (file == Npp.GetCurrentFile())
                pos = Npp.GetPositionFromLineColumn(line - 1, column - 1); //more accurate as the file can be modified
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

                if (Npp.IsCurrentScriptFile())
                {
                    //note initial state
                    string currentDocFile = Npp.GetCurrentFile();
                    int initialCaretPos = Npp.GetCaretPosition();
                    Point wordAtCaretLocation;
                    string wordToReplace = Npp.GetWordAtCursor(out wordAtCaretLocation, SimpleCodeCompletion.Delimiters);

                    Npp.SetSelection(wordAtCaretLocation.X, wordAtCaretLocation.Y);

                    //prompt user
                    using (var input = new RenameForm(wordToReplace))
                    {
                        input.ShowDialog();
                        string replacementWord = input.RenameTo;

                        Cursor.Current = Cursors.WaitCursor;

                        if (replacementWord.Any() && replacementWord != wordToReplace)
                        {
                            Npp.ClearSelection();
                            Npp.SaveCurrentFile();

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
                                Npp.ReloadFile(false, file);

                                if (file == currentDocFile)
                                {
                                    Npp.SetCaretPosition(initialCaretPos - diff);
                                    Npp.ClearSelection();
                                }
                            }

                        }

                        Npp.ClearSelection();
                    }
                }

                Cursor.Current = Cursors.Default;
            });
        }

        static void FormatDocument()
        {
            HandleErrors(() =>
            {
                if (Npp.IsCurrentScriptFile())
                {
                    SourceCodeFormatter.FormatDocument();
                }
            });
        }

        static void GoToDefinition()
        {
            HandleErrors(() =>
            {
                if (Npp.IsCurrentScriptFile())
                {
                    DomRegion region = ResolveMemberAtCaret();

                    if (!region.IsEmpty)
                    {
                        Npp.OpenFile(region.FileName);
                        Npp.GoToLine(region.BeginLine);
                        Npp.ScrollToCaret();
                        Npp.GrabFocus();
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
                            Npp.SetStatusbarLabel(data.FirstOrDefault() ?? defaultStatusbarLabel);
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
            string file = Npp.GetCurrentFile();

            if (Npp.IsCurrentScriptFile())
            {
                int pos;

                if (simple)
                    pos = Npp.GetPositionFromMouseLocation();
                else
                    pos = Npp.GetCaretPosition();

                if (pos != -1)
                {
                    int rawPos = pos; //note non-decorated position

                    string text = Npp.GetTextBetween(0, Npp.DocEnd);

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
                    if (Npp.IsCurrentScriptFile())
                    {
                        var cursor = Cursor.Current;
                        try
                        {
                            Cursor.Current = Cursors.WaitCursor;

                            bool usingInserted = false;
                            int count = 0;

                            do
                            {
                                Npp.SaveCurrentFile();

                                usingInserted = false;
                                count++;

                                List<string> presentUsings = Reflector.GetCodeUsings(Npp.GetTextBetween(0, -1)).ToList();

                                //c:\Users\user\Documents\C# Scripts\TooltipTest1.cs(27,9): error CS0103: The name 'Debug' does not exist in the current context

                                string currentDocument = Npp.GetCurrentFile();

                                string[] output = UltraSharp.Cecil.Reflector.GetCodeCompileOutput(currentDocument);

                                var missingNamespaceErrors = output.Select(x => x.ToFileErrorReference())
                                                                   .Where(x => string.Compare(x.File, currentDocument, true) == 0)
                                                                   .ToArray();

                                var namespacesToInsert = new List<string>();

                                foreach (FileReference item in missingNamespaceErrors)
                                {
                                    int errorPosition = Npp.GetLineStart(item.Line - 1) + item.Column - 1;
                                    IEnumerable<TypeInfo> items = ResolveNamespacesAtPosition(errorPosition)
                                                                    .Where(x => !presentUsings.Contains(x.Namespace));

                                    if (items.Count() == 1) //do only if there is no ambiguity about what namespace it is
                                    {
                                        string resolvedNamespace = items.First().Namespace;
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
                if (Npp.IsCurrentScriptFile())
                {
                    string[] presentUsings = UltraSharp.Cecil.Reflector.GetCodeUsings(Npp.GetTextBetween(0, -1));

                    IEnumerable<Intellisense.Common.TypeInfo> items = ResolveNamespacesAtCaret().Where(x => !presentUsings.Contains(x.Namespace)).ToArray();

                    if (items.Count() > 0)
                    {
                        Point point = Npp.GetCaretScreenLocation();

                        if (namespaceMenu != null && namespaceMenu.Visible)
                        {
                            namespaceMenu.Close();
                        }

                        namespaceMenu = new CustomContextMenu();
                        namespaceMenu.Left = point.X;
                        namespaceMenu.Top = point.Y + 18;

                        var usings = items.Where(x => !x.IsNested)
                                          .Select(x => "using " + x.Namespace + ";");

                        var inline = items.Select(x => x.FullName);

                        foreach (string item in usings)
                            namespaceMenu.Add(item, Images.Images.namespace_add, NppEditor.InsertNamespace);

                        namespaceMenu.AddSeparator();

                        foreach (string item in inline)
                            namespaceMenu.Add(item, null, Npp.ReplaceWordAtCaret);

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
                if (Npp.IsCurrentScriptFile())
                {
                    ShowSuggestionList(true);
                }
                else if (Config.Instance.InterceptCtrlSpace)
                {
                    Win32.SendMessage(Plugin.NppData._nppHandle, (NppMsg) WinMsg.WM_COMMAND, (int) NppMenuCmd.IDM_EDIT_FUNCCALLTIP, 0);
                }
            });
        }

        static void ShowSuggestionList()
        {
            HandleErrors(() =>
            {
                if (Npp.IsCurrentScriptFile())
                {
                    ShowSuggestionList(false);
                }
                else if (Config.Instance.InterceptCtrlSpace)
                {
                    Win32.SendMessage(Plugin.NppData._nppHandle, (NppMsg) WinMsg.WM_COMMAND, (int) NppMenuCmd.IDM_EDIT_AUTOCOMPLETE, 0);
                }
            });
        }

        static void ShowSuggestionList(bool snippetsOnly, bool memberInfoWasShowing = false)
        {
            IEnumerable<ICompletionData> items;

            if (snippetsOnly)
            {
                items = GetSnippetsItems();
            }
            else
            {
                items = GetSuggestionItemsAtCaret();
                bool namespaceSuggestion = items.All(x => x.CompletionType == CompletionType._namespace);
                bool memberSugesstion = Npp.TextBeforeCursor(2).EndsWith(".");
                bool assignmentSugesstion = Npp.TextBeforeCursor(10).TrimEnd().EndsWith("=");

                if (!memberSugesstion && !namespaceSuggestion && !assignmentSugesstion)
                {
                    bool cssSugesstion = Npp.TextBeforeCursor(300).Split('\n').Last().TrimStart().StartsWith("//css_");
                    if (!cssSugesstion)
                    {
                        bool usingSuggestion = Npp.GetLine(Npp.GetCaretLineNumber()).Trim() == "using";
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

                Point point = Npp.GetCaretScreenLocation();

                if (autocompleteForm != null && autocompleteForm.Visible)
                {
                    autocompleteForm.Close();
                }

                Action<ICompletionData> OnAccepted = data => OnAutocompletionAccepted(data, snippetsOnly);

                autocompleteForm = new AutocompleteForm(OnAccepted, items, NppEditor.GetSuggestionHint());
                autocompleteForm.Left = point.X;
                autocompleteForm.Top = point.Y + Npp.GetTextHeight(Npp.GetCaretLineNumber());
                autocompleteForm.FormClosed += (sender, e) =>
                {
                    if (memberInfoWasShowing)
                        NppUI.Marshal(() => Dispatcher.Shedule(100, ShowMethodInfo));
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
                IntPtr sci = GetCurrentScintilla();

                int currentPos = Npp.GetCaretPosition();
                Point p;
                string word = Npp.GetWordAtCursor(out p);

                if (word == "=") // .Load +=|
                {
                    Npp.SetSelection(p.X, p.Y);
                    Npp.SetSelectionText("= ");

                    currentPos = Npp.GetCaretPosition();
                    word = "";
                }

                if (word != "")  // e.g. Console.Wr| but not Console.| 
                {
                    Npp.SetSelection(p.X, p.Y);
                }
                else
                {
                    string leftChar = Npp.GetTextBetween(p.X - 1, p.X);
                    if (leftChar == "=")
                    {
                        Npp.SetSelectionText(" "); //add space

                        //currentPos = Npp.SetCaretPosition(currentPos + 1);
                        Npp.ClearSelection();
                    }

                    // myForm.Result =   |
                    var lStart = Npp.GetLineStart(Npp.GetLineNumber(currentPos));
                    string lineLeftPart = Npp.GetTextBetween(lStart, currentPos);
                    string textOnLeft = lineLeftPart.TrimEnd();
                    if (textOnLeft.EndsWith("="))
                    {
                        int dif = lineLeftPart.Length - textOnLeft.Length;
                        //set it to  myForm.Result = |
                        if (dif > 1)
                            currentPos = Npp.SetCaretPosition(currentPos - dif + 1);
                    }
                }

                //Note CompletionText caret position if any
                InsertCompletion(data.CompletionText);

                //check for extra content to insert
                if (data.Tag is Dictionary<string, object>)
                {
                    try
                    {
                        var dict = data.Tag as Dictionary<string, object>;
                        if (dict.ContainsKey("insertionPos") && dict.ContainsKey("insertionContent"))
                        {
                            var insertionPos = (int) dict["insertionPos"];
                            var insertionContent = (string) dict["insertionContent"];

                            //the insertion point could be already shifted because of the previous insertion
                            if (currentPos < insertionPos)
                                insertionPos += data.CompletionText.Length;

                            Npp.SetCaretPosition(insertionPos);
                            Npp.ClearSelection();
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
            int newCarrentPos = -1;
            int completionCaretPos = completionText.IndexOf("$|$");
            if (completionCaretPos != -1)
            {
                completionText = completionText.Replace("$|$", "");
                newCarrentPos = Npp.GetCaretPosition() + completionCaretPos;// + "$|$".Length;
            }

            //the actual completion injection
            Npp.SetSelectionText(completionText);
            Npp.ClearSelection();

            //process new caret position if was requested
            if (newCarrentPos != -1)
            {
                Npp.SetCaretPosition(newCarrentPos);
                Npp.ClearSelection();
            }
        }

        static public void OnAutocompleteKeyPress(char keyChar = char.MinValue, bool allowNoText = false)
        {
            try
            {
                int currentPos = Npp.GetCaretPosition();

                string text = Npp.GetTextBetween(Math.Max(0, currentPos - 30), currentPos); //check up to 30 chars from left
                string hint = null;

                char[] delimiters = SimpleCodeCompletion.Delimiters;
                string word = Npp.GetWordAtPosition(currentPos);
                if (word.StartsWith("css_")) //CS-Script directive
                    delimiters = SimpleCodeCompletion.CSS_Delimiters;


                int pos = text.LastIndexOfAny(delimiters);
                if (pos != -1)
                {
                    hint = text.Substring(pos + 1).Trim();
                }
                else if (text.Length == currentPos) //start of the doc
                    hint = text;

                var charOnRigt = Npp.GetTextBetween(currentPos, currentPos + 1);
                var charOnLeft = Npp.GetTextBetween(currentPos - 1, currentPos);
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

        static public void OnCharTyped(char c)
        {
            if (Npp.IsCurrentScriptFile())
            {
                SourceCodeFormatter.CaretBeforeLastFormatting = -1;

                if (c == '.' || c == '_' || (c == '='))
                {
                    ShowSuggestionList();
                }
                else if (c == '(')
                {
                    if (!memberInfoPopup.IsShowing || memberInfoPopup.Simple)
                        ShowMethodInfo();
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
                    if (Npp.IsCurrentScriptFile())
                        memberInfoPopup.Enabled = true;
                    else
                        memberInfoPopup.Enabled = false;
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
                    var caret = Npp.GetPositionFromMouseLocation();
                    if (caret != -1)
                        Task.Factory.StartNew(GoToDefinition); //let click to get handled, otherwise accidental selection will occur
                }
            }
        }

        static public void EnsureCurrentFileParsedAsynch()
        {
            if (Plugin.Enabled)
                Task.Factory.StartNew(EnsureCurrentFileParsed);
        }

        static public Func<string> ResolveCurrentFile = Npp.GetCurrentFile; //the implementation can be injected by the host or other plugins. To be used in the future.
        static public Action<string> DisplayInOutputPanel = Npp.DisplayInNewDocument; //the implementation can be injected by the host or other plugins. To be used in the future.

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
                                string code = Npp.GetTextBetween(0, Npp.DocEnd);
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
            string file = Npp.GetCurrentFile();
            string text = Npp.GetTextBetween(0, Npp.DocEnd);
            int currentPos = Npp.GetCaretPosition();

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
            int currentPos = Npp.GetCaretPosition();
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
            string file = Npp.GetCurrentFile();
            string text = Npp.GetTextBetween(0, Npp.DocEnd);

            int lineNum = Npp.GetLineNumber(currentPos);
            int start = Npp.GetLineStart(lineNum);
            string line = Npp.GetLine(lineNum);

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

            var allUsings = new List<TypeInfo>();
            foreach (int offset in probingOffsets) //try to resolve any 'word' in the line
            {
                var result = SimpleCodeCompletion.GetMissingUsings(text, actualStart + offset, file);
                if (result.Any())
                {
                    allUsings.AddRange(result);
                    if (Config.Instance.RoslynIntellisense) //Roslyn is slow with resolving namespaces so do it less aggressive way than NRefactory
                        break;
                }
            }

            return allUsings.DistinctBy(x => x.Namespace).ToArray();
        }

        static IEnumerable<Intellisense.Common.TypeInfo> ResolveNamespacesAtPositionOld(int currentPos)
        {
            string file = Npp.GetCurrentFile();
            string text = Npp.GetTextBetween(0, Npp.DocEnd);

            CSScriptHelper.DecorateIfRequired(ref text, ref currentPos);

            EnsureCurrentFileParsed();

            return SimpleCodeCompletion.GetMissingUsings(text, currentPos, file);
        }

        static DomRegion ResolveMemberAtCaret()
        {
            string file = Npp.GetCurrentFile();
            string text = Npp.GetTextBetween(0, Npp.DocEnd);
            int currentPos = Npp.GetCaretPosition();

            try
            {
                //just to handle NPP strange concept of caret position being not a point of the text 
                //but an index of the byte array
                currentPos = Npp.CaretToTextPosition(currentPos);
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
    }
}