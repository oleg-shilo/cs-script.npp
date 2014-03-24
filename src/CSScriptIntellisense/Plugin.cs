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
    /*
     TODO:
     */

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
                (index, name, handler, isCtrl, isAlt, isShift, key) =>
                {
                    Plugin.SetCommand(index, name, handler, new ShortcutKey(isCtrl, isAlt, isShift, key));
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

                setCommand(cmdIndex++, "Show auto-complete list", ShowSuggestionList, true, false, false, Keys.Space);
                setCommand(cmdIndex++, "Insert Code Snippet", ShowSnippetsList, true, false, true, Keys.Space);
                setCommand(cmdIndex++, "Add missing 'using'", AddMissingUsings, true, false, false, Keys.OemPeriod);
                setCommand(cmdIndex++, "Re-analyze current document", Reparse, false, false, false, Keys.None);
                if (!Config.Instance.DisableMethodInfo)
                    setCommand(cmdIndex++, "Show Method Info", ShowMethodInfo, false, false, false, Keys.F6);
                setCommand(cmdIndex++, "Format Document", FormatDocument, true, false, false, Keys.F8);
                setCommand(cmdIndex++, "Go To Definition", GoToDefinition, false, false, false, Keys.F12);
                setCommand(cmdIndex++, "Find All References", FindAllReferences, false, false, true, Keys.F12);
                setCommand(cmdIndex++, "---", null, false, false, false, Keys.None);
                setCommand(cmdIndex++, "Settings", ShowConfig, false, false, false, Keys.None);
                setCommand(cmdIndex++, "Manage Code Snippets", Snippets.EditSnippetsConfig, false, false, false, Keys.None);
                setCommand(cmdIndex++, "---", null, false, false, false, Keys.None);
#if DEBUG
                //setCommand(cmdIndex++, "Test", Test, true, false, true, Keys.L);
#endif
                if (standaloneSetup)
                    setCommand(cmdIndex++, "About", ShowAboutBox, false, false, false, Keys.None);

                memberInfoPopup = new MemberInfoPopupManager(ShowQuickInfo);

                //NPP already intercepts these shortcuts so we need to hook keyboard messages
                KeyInterceptor.Instance.Add(Keys.Tab);
                KeyInterceptor.Instance.Add(Keys.Return);
                KeyInterceptor.Instance.Add(Keys.Escape);
                KeyInterceptor.Instance.Add(Keys.Tab);
                KeyInterceptor.Instance.Add(Keys.Space);
                KeyInterceptor.Instance.Add(Keys.F12);
                KeyInterceptor.Instance.KeyDown += Instance_KeyDown;
            }
            else
                SetCommand(cmdIndex++, "About", ShowAboutBox);
        }

        static bool TriggerCodeSnippetInsertion()
        {
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
            if (Config.Instance.SnapshotsEnabled)
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
                    Modifiers modifiers = KeyInterceptor.GetModifiers();

                    if (key == Keys.Space && modifiers.IsCtrl && !modifiers.IsAlt && modifiers.IsShift)
                    {
                        handled = true;
                        Dispatcher.Shedule(10, () => Invoke(ShowSnippetsList));
                    }
                    else if (key == Keys.Space && modifiers.IsCtrl && !modifiers.IsAlt && !modifiers.IsShift)
                    {
                        handled = true;
                        Dispatcher.Shedule(10, () => Invoke(ShowSuggestionList));
                    }
                    else if (key == Keys.F12 && !modifiers.IsCtrl && modifiers.IsShift && !modifiers.IsAlt)
                    {
                        Dispatcher.Shedule(10, () => Invoke(FindAllReferences));
                        handled = true;
                    }
                    else if (key == Keys.F12 && !modifiers.IsCtrl && !modifiers.IsShift && !modifiers.IsAlt)
                    {
                        Dispatcher.Shedule(10, () => Invoke(GoToDefinition));
                        handled = true;
                    }
                }
            }
        }

        static bool invokeInProgress = false;

        static void Invoke(Action action)
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

        //static bool toStyle = false;
        static void Test()
        {
            //toStyle = !toStyle;
            //if (toStyle)
            //{
            //    Style();
            //}
            //else
            //{
            //    Unstyle();
            //}
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
            ShowInfo(true);
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

                            int methodStartPos;

                            string[] data = SimpleCodeCompletion.GetMemberInfo(text, pos, file, simple, out methodStartPos);

                            if (data.Length > 0)
                            {
                                if (simple && Config.Instance.ShowQuickInfoInStatusBar)
                                    Npp.SetStatusbarLabel(data.FirstOrDefault() ?? defaultStatusbarLabel);
                                else
                                    memberInfoPopup.TriggerPopup(simple, rawPos + (methodStartPos - pos), data);
                            }
                        }
                    }
                });
        }

        static CustomContextMenu namespaceMenu;

        static void AddMissingUsings()
        {
            HandleErrors(() =>
            {
                if (Npp.IsCurrentScriptFile())
                {
                    IEnumerable<TypeInfo> items = ResolveNamespacesAtCaret().ToArray();

                    if (items.Count() > 0)
                    {
                        Point point = Npp.GetCaretScreenLocation();

                        if (namespaceMenu != null && namespaceMenu.Visible)
                            namespaceMenu.Close();

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

        static void ShowSuggestionList(bool snippetsOnly)
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
                bool memberInfoWasShowing = memberInfoPopup.IsShowing;
                if (memberInfoWasShowing)
                    memberInfoPopup.Close();

                Point point = Npp.GetCaretScreenLocation();

                if (form != null && form.Visible)
                    form.Close();

                Action<ICompletionData> OnAccepted = data => OnAutocompletionAccepted(data, snippetsOnly);

                form = new AutocompleteForm(OnAccepted, items, NppEditor.GetSuggestionHint());
                form.Left = point.X;
                form.Top = point.Y + 18;
                form.FormClosed += (sender, e) =>
                {
                    if (memberInfoWasShowing)
                        ShowMethodInfo();
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

                Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, 0, data.CompletionText);

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
            string file = Npp.GetCurrentFile();
            string text = Npp.GetTextBetween(0, Npp.DocEnd);
            int currentPos = Npp.GetCaretPosition();

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