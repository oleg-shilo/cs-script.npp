using CSScriptNpp.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UltraSharp.Cecil;

namespace CSScriptNpp
{
    /*TODO:
     * - Outstanding features
     *      - Debugger
     *          - Debug panel
     *              - QuickWatch panel
     *                  - auto update
     *                  - Setting the variable/expression value
     *              - Watch panel
     *                  - Pining sub-values
     *                  - Setting the variable/expression value
     *                  - Handle global (non variable based) expressions likes Environment.TickCount
     *                  - Handle method expressions like Console.WriteLine("test")
     *              - Debug Objects panel
     *                  - Refresh value on demand
     *              - Breakpoints panel
     *                  - implement 'remove all breakpoints' 
     *          - make handling Debug.Assert user friendlier
     *      
     * - Desirable but not essential features
     *      - F12 should work on constructors e.g. 'new Te|st();'
     *      - CodeMap should reflect all members with the indication of the type name (eventually)
     *      - Debug panel
     *          - Locals panel cached update (not recommended as it requires asynch funcevals)
     *              - clear the tree on frame change (embedded in 'locals update' message)
     *              - reconstruct the tree branch by branch
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

            //'_' preffix in the shortcutName means "pluging action shortcut" as opposite to "plugin key interceptor action"
            SetCommand(projectPanelId = index++, "Build (validate)", Build, "_BuildFromMenu:Ctrl+Shift+B");
            SetCommand(projectPanelId = index++, "Run", Run, "_Run:F5");
            SetCommand(projectPanelId = index++, "Debug", Run, "_Debug:Alt+F5");
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
        static void LoadIntellisenseCommands(ref int cmdIndex)
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


        static void AddInternalShortcuts(string shortcutSpec, string displayName, Action handler, Dictionary<Keys, int> uniqueKeys)
        {
            ShortcutKey shortcut = shortcutSpec.ParseAsShortcutKey(displayName);

            internalShortcuts.Add(shortcut, new Tuple<string, Action>(displayName, handler));

            var key = (Keys)shortcut._key;
            if (!uniqueKeys.ContainsKey(key))
                uniqueKeys.Add(key, 0);
        }

        static IEnumerable<Keys> BindInteranalShortcuts()
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

        static void Instance_KeyDown(Keys key, int repeatCount, ref bool handled)
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

        static int toggleScondaryPanelsCount = 0;
        static public void ToggleScondaryPanels()
        {
            Plugin.ShowSecondaryPanels();
            return;

            if (toggleScondaryPanelsCount == 0 && (Plugin.OutputPanel == null || !Plugin.OutputPanel.Visible))
            {
                Plugin.DoOutputPanel();
            }
            else if (toggleScondaryPanelsCount == 3 && (Plugin.DebugPanel == null || !Plugin.DebugPanel.Visible))
            {
                Plugin.DoDebugPanel();
            }
            else if (toggleScondaryPanelsCount == 4)
            {
                Plugin.DoOutputPanel();
                Plugin.DoDebugPanel();
            }

            toggleScondaryPanelsCount++;
            if (toggleScondaryPanelsCount > 4)
                toggleScondaryPanelsCount = 0;
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

        static Process runningScript;

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

        static void UpdateLocalDebugInfo()
        {
            if (runningScript == null)
                Plugin.OutputPanel.localDebugPreffix = null;
            else
                Plugin.OutputPanel.localDebugPreffix = runningScript.Id.ToString() + ": ";
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
        }

        static internal void CleanUp()
        {
            Config.Instance.ShowProjectPanel = (dockedManagedPanels.ContainsKey(projectPanelId) && dockedManagedPanels[projectPanelId].Visible);
            Config.Instance.ShowOutputPanel = (dockedManagedPanels.ContainsKey(outputPanelId) && dockedManagedPanels[outputPanelId].Visible);
            Config.Instance.ShowDebugPanel = (dockedManagedPanels.ContainsKey(debugPanelId) && dockedManagedPanels[debugPanelId].Visible);
            Config.Instance.Save();
            OutputPanel.Clean();
        }

        internal static string HomeUrl = "http://csscript.net/npp/csscript.html";

        static void StartCheckForUpdates()
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

        static void CheckForUpdates()
        {
            Thread.Sleep(2000); //let Notepad++ to complete all initialization

            string version = CSScriptHelper.GetLatestAvailableVersion();

            if (version != null)
            {
                var latestVersion = new Version(version);
                var nppVersion = Assembly.GetExecutingAssembly().GetName().Version;

                if (nppVersion < latestVersion)
                {
                    string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToLower();
                    string progFiles86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLower();
                    string pluginLocation = Assembly.GetExecutingAssembly().Location.ToLower();

                    //if (!pluginLocation.StartsWith(progFiles) && pluginLocation.StartsWith(progFiles86))
                    //{
                    //    MessageBox.Show("The newer version v" + version + " is available.\nHowever because Notepad++ is not installed in the System 'Program Files' you need to download the plugin binaries and (.7z) install them manually.", "CS-Script");
                    //    try
                    //    {
                    //        Process.Start(HomeUrl);
                    //    }
                    //    catch { }
                    //}
                    //else
                    if (DialogResult.Yes == MessageBox.Show("The newer version v" + version + " is available.\nDo you want to download and install it?\n\nWARNING: If you choose 'Yes' Notepad++ will be closed and all unsaved data may be lost.", "CS-Script", MessageBoxButtons.YesNo))
                    {
                        string msiFile = CSScriptHelper.GetLatestAvailableMsi(version);
                        if (msiFile != null)
                        {
                            try
                            {
                                Process.Start("msiexec.exe", "/i \"" + msiFile + "\" /qb");
                            }
                            catch
                            {
                                MessageBox.Show("Cannot execute setup file: " + msiFile, "CS-Script");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Cannot download the binaries. The latest release Web page will be opened instead.", "CS-Script");
                            try
                            {
                                Process.Start(HomeUrl);
                            }
                            catch { }
                        }
                    }
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
    }
}