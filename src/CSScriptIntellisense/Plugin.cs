using CSScriptIntellisense.Interop;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UltraSharp.Cecil;

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
                KeyInterceptor.Instance.Add(Keys.Tab);
                KeyInterceptor.Instance.Add(Keys.Return);
                KeyInterceptor.Instance.Add(Keys.Escape);
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

            if (Config.Instance.InterceptCtrlSpace)
            {
                if (Npp.IsCurrentScriptFile())
                {
                    foreach (var shortcut in internalShortcuts.Keys)
                        if ((byte)key == shortcut._key)
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
            if (!invokeInProgress)
            {
                invokeInProgress = true;
                action();
                invokeInProgress = false;
            }
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

                if (form != null)
                {
                    if (form.Visible)
                        form.Close();
                    form = null;
                }
            }
        }

        static void FindAllReferences()
        {
            HandleErrors(() =>
            {
                if (Npp.IsCurrentScriptFile())
                {
                    string[] references = FindAllReferencesAtCaret();
                    if (references.Count() > 0)
                    {
                        string text = string.Format("{0} reference{1} found:{2}{3}",
                            references.Count(),
                            (references.Count() == 1 ? "" : "s"),
                            Environment.NewLine,
                            string.Join(Environment.NewLine, references));

                        DisplayInOutputPanel(text);
                    }
                }
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

                    if (region != DomRegion.Empty)
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
            get { return form != null && form.Visible; }
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
                                    IEnumerable<TypeInfo> items = ResolveNamespacesAtPosition(errorPosition);

                                    if (items.Count() == 1) //do only if there is no ambiguity about what namespace it is
                                    {
                                        string resolvedNamespace = items.First().Namespace;
                                        if (!presentUsings.Contains(resolvedNamespace))
                                        {
                                            namespacesToInsert.Add("using " + resolvedNamespace + ";");
                                            presentUsings.Add(resolvedNamespace);
                                            usingInserted = true;
                                        }
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

                    IEnumerable<TypeInfo> items = ResolveNamespacesAtCaret().Where(x => !presentUsings.Contains(x.Namespace)).ToArray();

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

        static AutocompleteForm form;

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
                    Win32.SendMessage(Plugin.NppData._nppHandle, (NppMsg)WinMsg.WM_COMMAND, (int)NppMenuCmd.IDM_EDIT_FUNCCALLTIP, 0);
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
                    Win32.SendMessage(Plugin.NppData._nppHandle, (NppMsg)WinMsg.WM_COMMAND, (int)NppMenuCmd.IDM_EDIT_AUTOCOMPLETE, 0);
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

                if (!Npp.TextBeforeCursor(2).EndsWith(".")) //do not suggest snippets if expecting a member
                    items = items.Concat(GetSnippetsItems());
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

                if (form != null && form.Visible)
                {
                    form.Close();
                }

                Action<ICompletionData> OnAccepted = data => OnAutocompletionAccepted(data, snippetsOnly);

                form = new AutocompleteForm(OnAccepted, items, NppEditor.GetSuggestionHint());
                form.Left = point.X;
                form.Top = point.Y + Npp.GetTextHeight(Npp.GetCaretLineNumber());
                form.FormClosed += (sender, e) =>
                {
                    if (memberInfoWasShowing)
                        NppUI.Marshal(() => Dispatcher.Shedule(100, ShowMethodInfo));
                };
                form.KeyPress += (sender, e) =>
                {
                    if (e.KeyChar >= ' ' || e.KeyChar == 8) //8 is backspace
                        OnAutocompleteKeyPress(e.KeyChar);
                };
                form.Show();

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

                if (word != "")  // e.g. Console.Wr| but not Console.|
                    Win32.SendMessage(sci, SciMsg.SCI_SETSELECTION, p.X, p.Y);

                Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, data.CompletionText);

                if (snippetsOnlyMode)
                    TriggerCodeSnippetInsertion();
            }

            if (form != null)
            {
                if (form.Visible)
                    form.Close();
                form = null;
            }
        }

        static void OnAutocompleteKeyPress(char keyChar = char.MinValue, bool allowNoText = false)
        {
            NppEditor.ProcessKeyPress(keyChar);

            int currentPos = Npp.GetCaretPosition();

            string text = Npp.GetTextBetween(Math.Max(0, currentPos - 30), currentPos); //check up to 30 chars from left
            string hint = null;

            int pos = text.LastIndexOfAny(SimpleCodeCompletion.Delimiters);
            if (pos != -1)
            {
                hint = text.Substring(pos + 1).Trim();
            }
            else if (text.Length == currentPos) //start of the doc
                hint = text;

            if (hint != null)
            {
                form.FilterFor(hint);
            }
            else
            {
                if (!allowNoText)
                    form.Close();
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
                if (c == '.')
                    ShowSuggestionList();
                else if (c == '(')
                {
                    if (!memberInfoPopup.IsShowing || memberInfoPopup.Simple)
                        ShowMethodInfo();
                }
                else if (memberInfoPopup.IsShowing)
                    memberInfoPopup.CheckIfNeedsClosing();

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

        static public void OnNppReady()
        {
            Plugin.EnsureCurrentFileParsedAsynch();
            Snippets.Init();
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
                            string[] assemblyFiles = project.Item2.Where(x=>!x.EndsWith(@"plugins\CSScriptNpp\CSScriptLibrary.dll")).ToArray();

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
                                foreach (string item in sourceFiles)
                                {
                                    string code = File.ReadAllText(item);
                                    if (item == currentFile)
                                        CSScriptHelper.DecorateIfRequired(ref code);
                                    sourcesInfos.Add(new Tuple<string, string>(code, item));
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

        static IEnumerable<TypeInfo> ResolveNamespacesAtCaret()
        {
            int currentPos = Npp.GetCaretPosition();
            return ResolveNamespacesAtPosition(currentPos);
        }

        static IEnumerable<TypeInfo> ResolveNamespacesAtPosition(int currentPos)
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

            CSScriptHelper.DecorateIfRequired(ref text, ref currentPos);

            EnsureCurrentFileParsed();
            return SimpleCodeCompletion.ResolveMember(text, currentPos, file);
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