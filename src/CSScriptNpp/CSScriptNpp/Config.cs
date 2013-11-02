using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CSScriptNpp
{
    /// <summary>
    /// Of course XML based config is more natural, however ini file reading (just a few values) is faster. 
    /// </summary>
    public class Config : IniFile
    {
        static Config instance = new Config();

        public static Config Instance { get { return instance; } }

        public Config()
        {
            base.file = Path.Combine(Plugin.ConfigDir, "settings.ini");
            Open();
        }

        public bool ClasslessScriptByDefault = false;
        public bool DistributeScriptAsScriptByDefault = true;
        public bool InterceptConsole = false;
        public bool NavigateToRawCodeOnDblClickInOutput = false;
        public bool BuildOnF7 = true;
        public string TargetVersion = "v4.0.30319";
        public string ReleaseNotesViewedFor = "";
        public string SciptHistory = "";
        public int SciptHistoryMaxCount = 10;
        public bool ShowProjectPanel = false;
        public bool ShowOutputPanel = false;
        public bool ShowCodeMapPanel = false;
        public int OutputPanelCapacity = 10000; //num of characters
        public bool IntegrateWithIntellisense = true;
        public bool LocalDebug = true;

        public void Save()
        {
            File.WriteAllText(this.file, ""); //clear to get rid of all obsolete values

            SetValue("settings", "ShowProjectPanel", ShowProjectPanel);
            SetValue("settings", "ShowOutputPanel", ShowOutputPanel);
            SetValue("settings", "ShowCodeMapPanel", ShowCodeMapPanel);
            SetValue("settings", "OutputPanelCapacity", OutputPanelCapacity);
            SetValue("settings", "NavigateToRawCodeOnDblClickInOutput", NavigateToRawCodeOnDblClickInOutput);
            SetValue("settings", "InterceptConsole", InterceptConsole);
            SetValue("settings", "ReleaseNotesViewedFor", ReleaseNotesViewedFor);
            SetValue("settings", "SciptHistory", SciptHistory);
            SetValue("settings", "SciptHistoryMaxCount", SciptHistoryMaxCount);
            SetValue("settings", "LocalDebug", LocalDebug);
            SetValue("settings", "BuildOnF7", BuildOnF7);
            SetValue("settings", "TargetVersion", TargetVersion);
            SetValue("settings", "ClasslessScriptByDefault", ClasslessScriptByDefault);
            SetValue("settings", "DistributeScriptAsScriptByDefault", DistributeScriptAsScriptByDefault);
            SetValue("settings", "IntegrateWithIntellisense", IntegrateWithIntellisense);
        }

        public void Open()
        {
            ShowProjectPanel = GetValue("settings", "ShowProjectPanel", ShowProjectPanel);
            ShowOutputPanel = GetValue("settings", "ShowOutputPanel", ShowOutputPanel);
            ShowCodeMapPanel = GetValue("settings", "ShowCodeMapPanel", ShowCodeMapPanel);
            SciptHistory = GetValue("settings", "SciptHistory", SciptHistory);
            SciptHistoryMaxCount = GetValue("settings", "SciptHistoryMaxCount", SciptHistoryMaxCount);
            OutputPanelCapacity = GetValue("settings", "OutputPanelCapacity", OutputPanelCapacity);
            NavigateToRawCodeOnDblClickInOutput = GetValue("settings", "NavigateToRawCodeOnDblClickInOutput", NavigateToRawCodeOnDblClickInOutput);
            InterceptConsole = GetValue("settings", "InterceptConsole", InterceptConsole);
            LocalDebug = GetValue("settings", "LocalDebug", LocalDebug);
            TargetVersion = GetValue("settings", "TargetVersion", TargetVersion);
            ReleaseNotesViewedFor = GetValue("settings", "ReleaseNotesViewedFor", ReleaseNotesViewedFor);
            BuildOnF7 = GetValue("settings", "BuildOnF7", BuildOnF7);
            ClasslessScriptByDefault = GetValue("settings", "ClasslessScriptByDefault", ClasslessScriptByDefault);
            DistributeScriptAsScriptByDefault = GetValue("settings", "DistributeScriptAsScriptByDefault", DistributeScriptAsScriptByDefault);
            IntegrateWithIntellisense = GetValue("settings", "IntegrateWithIntellisense", IntegrateWithIntellisense);
        }
    }
}
