using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSScriptNpp.Dialogs;
using UltraSharp.Cecil;

namespace CSScriptNpp
{
    /*TODO:
     * - Outstanding features
     *  - Debugger does not treat DateTime members as primitives
     *  - Some objects cannot be inspected:
     *      - new FileInfo(this.GetType().Assembly.Location);
     *      - Process.GetCurrentProcess();
     *  - F12 generated definition for FileInfo cannot be "code mapped"
     *  - Integrate surrogate hosting //css_host /version:v4.0 /platform:x86; 
     *      - Chinese characters
     *      - Debugging
     *  - In CS-Script implement object inspector (Dump)
     *      - allow custom routine to be specified for the dump algorithm
     *  - in CS-S.Npp allow calling object inspector and redirecting the outot to the debug window.
     * -------------------------------------------------------------------
     * 
     *  - Desirable but not essential features:
     *  
     *     - Debugger attach to process
     *          - check presence of dbg info and open source file if possible
     *          - integrate with OS (http://www.codeproject.com/Articles/132742/Writing-Windows-Debugger-Part)
     *     
     *     - Rendering current step indicator sometimes (very rare occasions) is not reliable (e.g. at first breakpoint hit)
     *       Very hard to reproduce. Pressing "Break" fixes it anyway
     *     
     *     - Debug panel
     *          - Locals panel cached update (not recommended as it requires asynch funcevals)
     *              - clear the tree on frame change (embedded in 'locals update' message)
     *              - reconstruct the tree branch by branch
     *          - Debug Objects panel
     *              - Refresh value on demand
     *          - QuickWatch panel
     *              - auto update
     *              - Setting the variable/expression value
     *                   ( MdbgCommands.SetCmd should ResolveVariable even if it is an expression e.g. 'name.length'
     *
     *                             lsMVar = Debugger.Processes.Active.ResolveVariable(varName,
     *                                         Debugger.Processes.Active.Threads.Active.CurrentFrame); )
     *             - Handle method expressions like Console.WriteLine("test")
     *     - Debugger: make handling Debug.Assert user friendlier
     *
     */

    public partial class Plugin
    {
        public const string PluginName = "CS-Script";
        public static int projectPanelId = -1;
        public static int outputPanelId = -1;
        public static int debugPanelId = -1;

        public static Dictionary<ShortcutKey, Tuple<string, Action>> internalShortcuts = new Dictionary<ShortcutKey, Tuple<string, Action>>();

        static internal void CommandMenuInit()
        {
            int index = 0;

            //'_' prefix in the shortcutName means "pluging action shortcut" as opposite to "plugin key interceptor action"
            SetCommand(projectPanelId = index++, "Build (validate)", Build, "_BuildFromMenu:Ctrl+Shift+B");
            SetCommand(projectPanelId = index++, "Run", Run, "_Run:F5");
            SetCommand(projectPanelId = index++, "Debug", Debug, "_Debug:Alt+F5");
            SetCommand(projectPanelId = index++, "Debug External Process", DebugEx, "_DebugExternal:Ctrl+Shift+F5");
            SetCommand(index++, "---", null);
            SetCommand(projectPanelId = index++, "Project Panel", DoProjectPanel, Config.Instance.ShowProjectPanel);
            SetCommand(outputPanelId = index++, "Output Panel", DoOutputPanel, Config.Instance.ShowOutputPanel);
            SetCommand(debugPanelId = index++, "Debug Panel", DoDebugPanel, Config.Instance.ShowDebugPanel);
            SetCommand(index++, "---", null);
            LoadIntellisenseCommands(ref index);
            SetCommand(index++, "About", ShowAbout);

            IEnumerable<Keys> keysToIntercept = BindInteranalShortcuts();

            KeyInterceptor.Instance.Install();

            foreach (var key in keysToIntercept)
                KeyInterceptor.Instance.Add(key);
            KeyInterceptor.Instance.Add(Keys.Tab);
            KeyInterceptor.Instance.KeyDown += Instance_KeyDown;

            //setup dependency injection, which may be overwritten by other plugins (e.g. NppScripts)
            Plugin.RunScript = () => Plugin.ProjectPanel.Run();
            Plugin.RunScriptAsExternal = () => Plugin.ProjectPanel.RunAsExternal();
            Plugin.DebugScript = () => Plugin.ProjectPanel.Debug(false);
        }

        static public Action RunScript;
        static public Action RunScriptAsExternal;
        static public Action DebugScript;

        //must be in a separate method to allow proper assembly probing
        private static void LoadIntellisenseCommands(ref int cmdIndex)
        {
            CSScriptIntellisense.Plugin.CommandMenuInit(ref cmdIndex,
                 (index, name, handler, shortcut) =>
                 {
                     if (name == "Settings")
                         Plugin.SetCommand(index, name, ShowConfig, shortcut);
                     else
                         Plugin.SetCommand(index, name, handler, shortcut);
                 });
        }

        private static void AddInternalShortcuts(string shortcutSpec, string displayName, Action handler, Dictionary<Keys, int> uniqueKeys)
        {
            ShortcutKey shortcut = shortcutSpec.ParseAsShortcutKey(displayName);

            internalShortcuts.Add(shortcut, new Tuple<string, Action>(displayName, handler));

            var key = (Keys)shortcut._key;
            if (!uniqueKeys.ContainsKey(key))
                uniqueKeys.Add(key, 0);
        }

        private static IEnumerable<Keys> BindInteranalShortcuts()
        {
            var uniqueKeys = new Dictionary<Keys, int>();

            AddInternalShortcuts("Build:F7",
                                 "Build (validate)",
                                  Build, uniqueKeys);

            AddInternalShortcuts("LoadCurrentDocument:Ctrl+F7",
                                 "Load Current Document", () =>
                                  {
                                      DoProjectPanel();
                                      ShowProjectPanel();
                                      ProjectPanel.LoadCurrentDoc();
                                  }, uniqueKeys);

            AddInternalShortcuts("Stop:Shift+F5",
                                  "Stop running script",
                                  Stop, uniqueKeys);

            AddInternalShortcuts("_Run:F5",
                                 "Run",
                                  Run, uniqueKeys);

            AddInternalShortcuts("_Debug:Alt+F5",
                                 "Debug", () =>
                                  {
                                      if (!Debugger.IsRunning)
                                          DebugScript();
                                  }, uniqueKeys);

            AddInternalShortcuts("ToggleBreakpoint:F9",
                                 "Toggle Breakpoint",
                                 () => Debugger.ToggleBreakpoint(), uniqueKeys);

            AddInternalShortcuts("QuickWatch:Shift+F9",
                                 "Show QuickWatch...",
                                  QuickWatchPanel.PopupDialog, uniqueKeys);

            AddInternalShortcuts("StepInto:F11",
                                 "Step Into",
                                  Debugger.StepIn, uniqueKeys);

            AddInternalShortcuts("StepOut:Shift+F11",
                                 "Step Out",
                                  Debugger.StepOut, uniqueKeys);

            AddInternalShortcuts("StepOver:F10",
                                 "Step Over",
                                  StepOver, uniqueKeys);

            AddInternalShortcuts("SetNextIP:Ctrl+Shift+F10",
                                 "Set Next Statement",
                                  Debugger.SetInstructionPointer, uniqueKeys);

            AddInternalShortcuts("RunToCursor:Ctrl+F10",
                                 "Run To Cursor",
                                  Debugger.RunToCursor, uniqueKeys);

            AddInternalShortcuts("RunAsExternal:Ctrl+F5",
                                  "Run As External Process", () =>
                                  {
                                      if (Npp.IsCurrentScriptFile())
                                          RunAsExternal();
                                  }, uniqueKeys);

            AddInternalShortcuts("ShowNextFileLocationFromOutput:F4",
                                 "Next File Location in Output", () =>
                                  {
                                      OutputPanel.TryNavigateToFileReference(toNext: true);
                                  }, uniqueKeys);

            AddInternalShortcuts("ShowPrevFileLocationFromOutput:Shift+F4",
                                 "Previous File Location in Output", () =>
                                  {
                                      OutputPanel.TryNavigateToFileReference(toNext: false);
                                  }, uniqueKeys);

            return uniqueKeys.Keys;
        }

        private static void Instance_KeyDown(Keys key, int repeatCount, ref bool handled)
        {
            foreach (var shortcut in internalShortcuts.Keys)
                if ((byte)key == shortcut._key)
                {
                    Modifiers modifiers = KeyInterceptor.GetModifiers();

                    if (modifiers.IsCtrl == shortcut.IsCtrl && modifiers.IsShift == shortcut.IsShift && modifiers.IsAlt == shortcut.IsAlt)
                    {
                        handled = true;
                        var handler = internalShortcuts[shortcut];
                        handler.Item2();
                    }
                }
        }

        static public void ShowConfig()
        {
            using (var form = new ConfigForm(Config.Instance))
            {
                form.ShowDialog();
                Config.Instance.Save();
                ReflectorExtensions.IgnoreDocumentationExceptions = CSScriptIntellisense.Config.Instance.IgnoreDocExceptions;
            }
        }

        static public void ShowAbout()
        {
            using (var dialog = new AboutBox())
                dialog.ShowDialog();
        }

        static public OutputPanel OutputPanel;
        static public ProjectPanel ProjectPanel;
        static public CodeMapPanel CodeMapPanel;
        static public DebugPanel DebugPanel;

        static public DebugPanel GetDebugPanel()
        {
            if (Plugin.DebugPanel == null)
                Plugin.DoDebugPanel();
            return Plugin.DebugPanel;
        }

        static public ProjectPanel GetProjectPanel()
        {
            if (Plugin.ProjectPanel == null)
                Plugin.DoProjectPanel();
            return Plugin.ProjectPanel;
        }

        static public void DoOutputPanel()
        {
            Plugin.OutputPanel = ShowDockablePanel<OutputPanel>("Output", outputPanelId, NppTbMsg.DWS_DF_CONT_BOTTOM | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR);
        }

        static public void DoDebugPanel()
        {
            Plugin.DebugPanel = ShowDockablePanel<DebugPanel>("Debug", debugPanelId, NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR);
        }

        static public void ShowSecondaryPanels()
        {
            if (Plugin.OutputPanel == null)
                DoOutputPanel();

            if (Plugin.DebugPanel == null)
                DoDebugPanel();

            Plugin.SetDockedPanelVisible(Plugin.OutputPanel, outputPanelId, true);
            Plugin.SetDockedPanelVisible(Plugin.DebugPanel, debugPanelId, true);
        }

        static public void DoProjectPanel()
        {
            ProjectPanel = ShowDockablePanel<ProjectPanel>("CS-Script", projectPanelId, NppTbMsg.DWS_DF_CONT_LEFT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR);
            ProjectPanel.Focus();
        }

        static public void ShowProjectPanel()
        {
            SetDockedPanelVisible(dockedManagedPanels[projectPanelId], projectPanelId, true);
        }

        //static int toggleScondaryPanelsCount = 0;
        static public void ToggleScondaryPanels()
        {
            Plugin.ShowSecondaryPanels();
            //return;

            //if (toggleScondaryPanelsCount == 0 && (Plugin.OutputPanel == null || !Plugin.OutputPanel.Visible))
            //{
            //    Plugin.DoOutputPanel();
            //}
            //else if (toggleScondaryPanelsCount == 3 && (Plugin.DebugPanel == null || !Plugin.DebugPanel.Visible))
            //{
            //    Plugin.DoDebugPanel();
            //}
            //else if (toggleScondaryPanelsCount == 4)
            //{
            //    Plugin.DoOutputPanel();
            //    Plugin.DoDebugPanel();
            //}

            //toggleScondaryPanelsCount++;
            //if (toggleScondaryPanelsCount > 4)
            //    toggleScondaryPanelsCount = 0;
        }

        static public void Build()
        {
            if (runningScript == null)
            {
                if (Plugin.ProjectPanel == null)
                    DoProjectPanel();
                Plugin.ProjectPanel.Build();
            }
        }

        static public void StepOver()
        {
            if (Debugger.IsRunning)
                Debugger.StepOver();
            else
                GetProjectPanel().Debug(breakOnFirstStep: true);
        }

        static public void Run()
        {
            if (Debugger.IsRunning)
            {
                Debugger.Go();
            }
            else if (Npp.IsCurrentScriptFile() && runningScript == null)
            {
                if (Plugin.ProjectPanel == null)
                    DoProjectPanel();
                Plugin.RunScript();
            }
        }

        static public void DebugEx()
        {
            if (!Debugger.IsRunning)
            {
                if (Plugin.ProjectPanel == null)
                    DoProjectPanel();
                DebugExternal.ShowModal();
            }
        }

        static public void Debug()
        {
            if (!Debugger.IsRunning)
            {
                if (Plugin.ProjectPanel == null)
                    DoProjectPanel();
                Plugin.DebugScript();
            }
        }

        static public void Stop()
        {
            if (Debugger.IsRunning)
            {
                Debugger.Exit();
            }
            else
            {
                try
                {
                    if (Plugin.RunningScript != null)
                        Plugin.RunningScript.Kill();
                }
                catch (Exception ex)
                {
                    Plugin.OutputPanel.DebugOutput.WriteLine(null)
                                                  .WriteLine(ex.Message);
                }
            }
        }

        static public void RunAsExternal()
        {
            if (runningScript == null)
            {
                if (Plugin.ProjectPanel == null)
                    DoProjectPanel();
                Plugin.RunScriptAsExternal();
            }
        }

        static public OutputPanel ShowOutputPanel()
        {
            if (Plugin.OutputPanel == null)
                DoOutputPanel();
            else
                SetDockedPanelVisible(Plugin.OutputPanel, outputPanelId, true);

            UpdateLocalDebugInfo();
            return Plugin.OutputPanel;
        }

        private static Process runningScript;

        public static Process RunningScript
        {
            get
            {
                return runningScript;
            }
            set
            {
                runningScript = value;
                UpdateLocalDebugInfo();
            }
        }

        private static void UpdateLocalDebugInfo()
        {
            if (runningScript == null)
                Plugin.OutputPanel.localDebugPrefix = null;
            else
                Plugin.OutputPanel.localDebugPrefix = runningScript.Id.ToString() + ": ";
        }

        static internal void OnNppReady()
        {
            if (Config.Instance.ShowProjectPanel)
                DoProjectPanel();

            if (Config.Instance.ShowOutputPanel)
                DoOutputPanel();

            if (Config.Instance.ShowDebugPanel)
                DoDebugPanel();

            StartCheckForUpdates();

            OpenAutomationChannel();
        }

        static internal void OnDocumentSaved()
        {
            if (Plugin.ProjectPanel != null)
                Plugin.ProjectPanel.RefreshProjectStructure();
        }

        static internal void CleanUp()
        {
            Config.Instance.ShowProjectPanel = (dockedManagedPanels.ContainsKey(projectPanelId) && dockedManagedPanels[projectPanelId].Visible);
            Config.Instance.ShowOutputPanel = (dockedManagedPanels.ContainsKey(outputPanelId) && dockedManagedPanels[outputPanelId].Visible);
            Config.Instance.ShowDebugPanel = (dockedManagedPanels.ContainsKey(debugPanelId) && dockedManagedPanels[debugPanelId].Visible);
            Config.Instance.Save();
            OutputPanel.Clean();
            CloseAutomationChannel();
        }

        internal static string HomeUrl = "https://csscriptnpp.codeplex.com/";

        private static void StartCheckForUpdates()
        {
            lock (typeof(Plugin))
            {
                if (Config.Instance.CheckUpdatesOnStartup)
                {
                    string date = DateTime.Now.ToString("yyyy-MM-dd");
                    if (Config.Instance.LastUpdatesCheckDate != date)
                    {
                        Config.Instance.LastUpdatesCheckDate = date;
                        Config.Instance.Save();

                        Task.Factory.StartNew(CheckForUpdates);
                    }
                }
            }
        }

        private static void CheckForUpdates()
        {
            Thread.Sleep(2000); //let Notepad++ to complete all initialization

            string version = CSScriptHelper.GetLatestAvailableVersion();

            if (version != null)
            {
                var latestVersion = new Version(version);
                var nppVersion = Assembly.GetExecutingAssembly().GetName().Version;

                if (nppVersion < latestVersion)
                {
                    using (var dialog = new UpdateOptionsPanel(version))
                        dialog.ShowDialog();
                }
            }
        }

        public static void OnNotification(SCNotification data)
        {
        }

        static public void OnCurrentFileChanged()
        {
            if (CodeMapPanel != null)
                CodeMapPanel.RefreshContent();
        }

        public static void OnToolbarUpdate()
        {
            Plugin.FuncItems.RefreshItems();
            SetToolbarImage(Resources.Resources.css_logo_16x16_tb, projectPanelId);
        }

        static void CloseAutomationChannel()
        {
            MessageQueue.AddAutomationCommand("automation.exit");
        }

        static void OpenAutomationChannel()
        {
            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (true)
                        {
                            string message = MessageQueue.WaitForAutomationCommand();
                            if (message == "automation.exit")
                                break;

                            MessageBox.Show(message);
                        }
                    }
                    catch { };
                });
        }

        static public void ProcessCommandArgs(string args)
        {
            //System.Diagnostics.Debug.Assert(false);
            if (args.StartsWith("/css.attach:")) //attach to external process
            {
                try
                {
                    var id = int.Parse(args.Substring("/css.attach:".Length));
                    DebugExternal.AttachTo(id);
                }
                catch { }
            }
        }
    }
}