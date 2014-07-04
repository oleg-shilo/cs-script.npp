using System;
using System.Diagnostics;
using System.IO;

namespace CSScriptNpp
{
    /// <summary>
    /// Of course XML based config is more natural, however ini file reading (just a few values) is faster.
    /// </summary>
    public class Config : IniFile
    {
        public static string Location = Plugin.ConfigDir;

        public static Shortcuts Shortcuts = new Shortcuts();
        public static Config Instance { get { return instance ?? (instance = new Config()); } }
        public static Config instance;

        public string Section = "Generic";

        public Config()
        {
            base.file = Path.Combine(Location, "settings.ini");
            Open();
        }

        public string GetFileName()
        {
            return base.file;
        }

        public bool ClasslessScriptByDefault = false;
        public bool DistributeScriptAsScriptByDefault = true;
        public bool InterceptConsole = false;
        public bool QuickViewAutoRefreshAvailable = false;
        public bool NavigateToRawCodeOnDblClickInOutput = false;
        //public bool BuildOnF7 = true;
        public bool BreakOnException = false;
        public string UpdateMode = "custom";
        public bool CheckUpdatesOnStartup = true;
        public bool WordWrapInVisualizer = true;
        public bool ListManagedProcessesOnly = true;
        public bool RunExternalInDebugMode = true;
        public bool FloatingPanelsWarningAlreadyPropted = false;
        public string TargetVersion = "v4.0.30319";
        public string LastExternalProcess = "";
        public string LastExternalProcessArgs = "";
        public int LastExternalProcessCpu = 0;
        public string ReleaseNotesViewedFor = "";
        public string LastUpdatesCheckDate = DateTime.MinValue.ToString("yyyy-MM-dd");
        public string SciptHistory = "";
        public int SciptHistoryMaxCount = 10;
        public int CollectionItemsInTooltipsMaxCount = 15;
        public int CollectionItemsInVisualizersMaxCount = 1000;
        public int DebugPanelInitialTab = 0;
        public bool ShowLineNuberInCodeMap = false;
        public bool ShowProjectPanel = false;
        public bool ShowOutputPanel = false;
        public bool DebugAsConsole = true;
        public bool ShowDebugPanel = false;
        public int OutputPanelCapacity = 10000; //num of characters
        public bool LocalDebug = true;

        public void Save()
        {
            lock (this)
            {
                File.WriteAllText(this.file, ""); //clear to get rid of all obsolete values

                SetValue(Section, "ShowProjectPanel", ShowProjectPanel);
                SetValue(Section, "ShowOutputPanel", ShowOutputPanel);
                SetValue(Section, "DebugAsConsole", DebugAsConsole);
                SetValue(Section, "ShowLineNuberInCodeMap", ShowLineNuberInCodeMap);
                SetValue(Section, "ShowDebugPanel", ShowDebugPanel);
                SetValue(Section, "WordWrapInVisualizer", WordWrapInVisualizer);
                SetValue(Section, "ListManagedProcessesOnly", ListManagedProcessesOnly);
                SetValue(Section, "RunExternalInDebugMode", RunExternalInDebugMode);
                SetValue(Section, "OutputPanelCapacity", OutputPanelCapacity);
                SetValue(Section, "NavigateToRawCodeOnDblClickInOutput", NavigateToRawCodeOnDblClickInOutput);
                SetValue(Section, "QuickViewAutoRefreshAvailable", QuickViewAutoRefreshAvailable);
                SetValue(Section, "InterceptConsole", InterceptConsole);
                SetValue(Section, "ReleaseNotesViewedFor", ReleaseNotesViewedFor);
                SetValue(Section, "SciptHistory", SciptHistory);
                SetValue(Section, "SciptHistoryMaxCount", SciptHistoryMaxCount);
                SetValue(Section, "CollectionItemsInTooltipsMaxCount", CollectionItemsInTooltipsMaxCount);
                SetValue(Section, "CollectionItemsInVisualizersMaxCount", CollectionItemsInVisualizersMaxCount);
                SetValue(Section, "DebugPanelInitialTab", DebugPanelInitialTab);
                SetValue(Section, "LocalDebug", LocalDebug);
                SetValue(Section, "BreakOnException", BreakOnException);
                SetValue(Section, "LastUpdatesCheckDate", LastUpdatesCheckDate);
                SetValue(Section, "CheckUpdatesOnStartup", CheckUpdatesOnStartup);
                SetValue(Section, "UpdateMode", UpdateMode);
                SetValue(Section, "FloatingPanelsWarningAlreadyPropted", FloatingPanelsWarningAlreadyPropted);
                SetValue(Section, "LastExternalProcess", LastExternalProcess);
                SetValue(Section, "LastExternalProcessArgs", LastExternalProcessArgs);
                SetValue(Section, "LastExternalProcessCpu", LastExternalProcessCpu);
                SetValue(Section, "TargetVersion", TargetVersion);
                SetValue(Section, "ClasslessScriptByDefault", ClasslessScriptByDefault);
                SetValue(Section, "DistributeScriptAsScriptByDefault", DistributeScriptAsScriptByDefault);

                CSScriptIntellisense.Config.Instance.Save();

                Shortcuts.Save();
            }
        }

        public void Open()
        {
            lock (this)
            {
                ShowLineNuberInCodeMap = GetValue(Section, "ShowLineNuberInCodeMap", ShowLineNuberInCodeMap);
                ShowProjectPanel = GetValue(Section, "ShowProjectPanel", ShowProjectPanel);
                ShowOutputPanel = GetValue(Section, "ShowOutputPanel", ShowOutputPanel);
                DebugAsConsole = GetValue(Section, "DebugAsConsole", DebugAsConsole);
                WordWrapInVisualizer = GetValue(Section, "WordWrapInVisualizer", WordWrapInVisualizer);
                ListManagedProcessesOnly = GetValue(Section, "ListManagedProcessesOnly", ListManagedProcessesOnly);
                RunExternalInDebugMode = GetValue(Section, "RunExternalInDebugMode", RunExternalInDebugMode);
                //ShowDebugPanel = GetValue(Section, "ShowDebugPanel", ShowDebugPanel); //ignore; do not show Debug panel as it is heavy. It will be displayed at the first debug step anyway.
                SciptHistory = GetValue(Section, "SciptHistory", SciptHistory, 1024 * 4);
                SciptHistoryMaxCount = GetValue(Section, "SciptHistoryMaxCount", SciptHistoryMaxCount);
                CollectionItemsInTooltipsMaxCount = GetValue(Section, "CollectionItemsInTooltipsMaxCount", CollectionItemsInTooltipsMaxCount);
                CollectionItemsInVisualizersMaxCount = GetValue(Section, "CollectionItemsInVisualizersMaxCount", CollectionItemsInVisualizersMaxCount);
                DebugPanelInitialTab = GetValue(Section, "DebugPanelInitialTab", DebugPanelInitialTab);
                OutputPanelCapacity = GetValue(Section, "OutputPanelCapacity", OutputPanelCapacity);
                NavigateToRawCodeOnDblClickInOutput = GetValue(Section, "NavigateToRawCodeOnDblClickInOutput", NavigateToRawCodeOnDblClickInOutput);
                InterceptConsole = GetValue(Section, "InterceptConsole", InterceptConsole);
                //QuickViewAutoRefreshAvailable = GetValue(Section, "QuickViewAutoRefreshAvailable", QuickViewAutoRefreshAvailable); //disable until auto-refresh approach is finalized
                LocalDebug = GetValue(Section, "LocalDebug", LocalDebug);
                TargetVersion = GetValue(Section, "TargetVersion", TargetVersion);
                LastExternalProcess = GetValue(Section, "LastExternalProcess", LastExternalProcess);
                LastExternalProcessArgs = GetValue(Section, "LastExternalProcessArgs", LastExternalProcessArgs);
                LastExternalProcessCpu = GetValue(Section, "LastExternalProcessCpu", LastExternalProcessCpu);
                ReleaseNotesViewedFor = GetValue(Section, "ReleaseNotesViewedFor", ReleaseNotesViewedFor);
                BreakOnException = GetValue(Section, "BreakOnException", BreakOnException);
                LastUpdatesCheckDate = GetValue(Section, "LastUpdatesCheckDate", LastUpdatesCheckDate);
                CheckUpdatesOnStartup = GetValue(Section, "CheckUpdatesOnStartup", CheckUpdatesOnStartup);
                UpdateMode = GetValue(Section, "UpdateMode", UpdateMode);
                FloatingPanelsWarningAlreadyPropted = GetValue(Section, "FloatingPanelsWarningAlreadyPropted", FloatingPanelsWarningAlreadyPropted);
                ClasslessScriptByDefault = GetValue(Section, "ClasslessScriptByDefault", ClasslessScriptByDefault);
                DistributeScriptAsScriptByDefault = GetValue(Section, "DistributeScriptAsScriptByDefault", DistributeScriptAsScriptByDefault);

                CSScriptIntellisense.Config.Instance.Open();
            }
        }
    }
}