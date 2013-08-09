using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

/*
 * NppScripts integration
 * Right-click: Open in VS
 * debug params
 * Help
 * ShortcutHook
 * LoadHistory
 * - Intellisense integration
 * - Debug with System Debugger
 * - About
 * - Right-click: unload all scriprs (on solution node)
 * - Right-click: open containing folder
 * - Output panel double-click
 * - Indicate primary script
 * - Script item tooltip
 * - Intercept Console output
 * - Intercept Debug output
 * - Run
 * - Synch
 * - tab Icon
 * - update output combo on external show() calls
 */

namespace CSScriptNpp
{
    public partial class Plugin
    {
        public const string PluginName = "CS-Script";
        public static int projectPanelId = -1;
        public static int outputPanelId = -1;

        static internal void CommandMenuInit()
        {
            //System.Diagnostics.Debug.Assert(false);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            int index = 0;

            SetCommand(projectPanelId = index++, "Build", Build, new ShortcutKey(true, false, true, Keys.B));
            SetCommand(projectPanelId = index++, "Run", Run, new ShortcutKey(true, false, false, Keys.F5));
            SetCommand(index++, "---", null);
            SetCommand(projectPanelId = index++, "Project Panel", DoProjectPanel, Config.Instance.ShowProjectPanel);
            SetCommand(outputPanelId = index++, "Output Panel", DoOutputPanel, Config.Instance.ShowOutputPanel);
            SetCommand(index++, "---", null);
            SetCommand(index++, "About", ShowAbout);
        }

        static public void ShowAbout()
        {
            using (var dialog = new AboutBox())
                dialog.ShowDialog();
        }

        static public OutputPanel OutputPanel;
        static public ProjectPanel ProjectPanel;

        static public void DoProjectPanel()
        {
            ProjectPanel = ShowDockablePanel<ProjectPanel>("CS-Script", projectPanelId, NppTbMsg.DWS_DF_CONT_LEFT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR);
            ProjectPanel.Focus();
        }

        static public void DoOutputPanel()
        {
            Plugin.OutputPanel = ShowDockablePanel<OutputPanel>("Output", outputPanelId, NppTbMsg.CONT_BOTTOM | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR);
        }

        static public void Build()
        {
            if (Plugin.ProjectPanel == null)
                DoProjectPanel();
            Plugin.ProjectPanel.Build();
        }

        static public void Run()
        {
            if (Plugin.ProjectPanel == null)
                DoProjectPanel();
            Plugin.ProjectPanel.Run();
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

        static internal void InitView()
        {
            if (Config.Instance.ShowProjectPanel)
                DoProjectPanel();

            if (Config.Instance.ShowOutputPanel)
                DoOutputPanel();
        }

        static internal void CleanUp()
        {
            Config.Instance.ShowProjectPanel = (dockedManagedPanels.ContainsKey(projectPanelId) && dockedManagedPanels[projectPanelId].Visible);
            Config.Instance.ShowOutputPanel = (dockedManagedPanels.ContainsKey(outputPanelId) && dockedManagedPanels[outputPanelId].Visible);
            Config.Instance.Save();
            OutputPanel.Clean();
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                if (args.Name.StartsWith("CSScriptLibrary,"))
                    return Assembly.LoadFrom(Path.Combine(rootDir, @"CSScriptNpp\CSScriptLibrary.dll"));
                else if (args.Name == Assembly.GetExecutingAssembly().FullName)
                    return Assembly.GetExecutingAssembly();
            }
            catch { }
            return null;
        }

        public static void OnNotification(SCNotification data)
        {
        }

        public static void RefreshToolbarImages()
        {
            SetToolbarImage(Resources.Resources.css_logo_16x16_tb, projectPanelId);
        }
    }
}