using System;
using System.Diagnostics;
using System.IO;
using CSScriptIntellisense;

namespace CSScriptNpp
{
    /// <summary>
    /// Of course XML based config is more natural, however ini file reading (just a few values) is faster.
    /// </summary>
    public class Config : IniFile
    {
        public static void InitData()
        {
            //the rest will be initialized from their corresponding static constructors
            Config.Instance.Open();
            Config.Instance.VbSupportEnabled = CSScriptIntellisense.Config.Instance.VbSupportEnabled;
        }

        public static string Location = PluginEnv.ConfigDir;

        public static Shortcuts Shortcuts = new Shortcuts();
        public static Config Instance { get { return instance ?? (instance = new Config()); } }
        public static Config instance;

        public string Section = "Generic";

        public Config()
        {
            base.file = Path.Combine(Location, "settings.ini");
        }

        public string GetFileName()
        {
            return base.file;
        }

        public bool ClasslessScriptByDefault = false;
        public bool DistributeScriptAsScriptByDefault = true;
        public bool DistributeScriptAsWindowApp = false;
        public bool DistributeScriptAsDll = false;
        public bool InterceptConsole = true;
        public bool InterceptConsoleByCharacter = false;
        public bool UseEmbeddedEngine = true;
        public string CustomEngineAsm = "";
        public bool QuickViewAutoRefreshAvailable = false;
        public bool NavigateToRawCodeOnDblClickInOutput = false;

        public bool ShowOpenInVsAlways = false;

        //public bool BuildOnF7 = true;
        public bool BreakOnException = false;

        public bool ReloadActiveScriptOnRun = true;

        public bool UpdateAfterExit = false;
        public bool UseTogglingPanelVisibility = true;
        public string UpdateMode = "custom";
        public string VSProjectTemplatePath = "";
        public bool CheckUpdatesOnStartup = true;
        public bool CheckPrereleaseUpdates = false;
        public bool VbSupportEnabled = true;
        public bool RestorePanelsAtStartup = true;
        public string UseCustomLauncher = "";
        public string CustomSyntaxerAsm = "";
        public int CustomSyntaxerPort = 18001;
        public bool StartRoslynServerAtNppStartup = true;
        public bool ImproveWin10ListVeiwRendering = true;
        public bool HideDefaultAssemblies = true;
        public bool WordWrapInVisualizer = true;
        public bool ListManagedProcessesOnly = true;
        public bool StartDebugMonitorOnScriptExecution = false;
        public string BlockLocalDebugOutputContaining = @"onecoreuap\inetcore\{NL}mincore\com\{NL}SHIMVIEW: ShimInfo(Complete)";
        public bool SyncSecondaryPanelsWithProjectPanel = true;
        public bool FloatingPanelsWarningAlreadyPropted = false;
        public string TargetVersion = "v4.0.30319";
        public string ScriptsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "C# Scripts");
        public string SkipUpdateVersion = "";
        public string VbCodeProvider = "CSSCodeProvider.v4.0.dll";
        public string CsSConsoleEncoding = "utf-8";
        public string HotkeyDocumentsExclusions = ".cmd;.bat;.test";
        public string LastExternalProcess = "";
        public string LastExternalProcessArgs = "";
        public int LastExternalProcessCpu = 0;
        public bool NativeAutoCompletionChecked = false;
        public string ReleaseNotesViewedFor = "";
        public string LastUpdatesCheckDate = DateTime.MinValue.ToString("yyyy-MM-dd");
        public string ScriptHistory = "";
        public string DebugStepPointColor = "Yellow";
        public string DebugStepPointForeColor = "Black";
        public int SciptHistoryMaxCount = 10;
        public int CollectionItemsInTooltipsMaxCount = 15;
        public int CollectionItemsInVisualizersMaxCount = 1000;
        public int DebugPanelInitialTab = 0;
        public bool ShowLineNuberInCodeMap = false;
        public bool ShowProjectPanel = false;
        public bool ShowDebugPanel = false;
        public bool ShowOutputPanel = false;
        public bool DebugAsConsole = true;
        public bool HandleSaveAs = true;
        public int OutputPanelCapacity = 10000; //num of characters
        public bool LocalDebug = true;

        public void Save()
        {
            lock (typeof(Config))
            {
                try
                {
                    // Debug.WriteLine("---> Config.Save");
                    File.WriteAllText(this.file, ""); //clear to get rid of all obsolete values

                    SetValue(Section, nameof(ShowProjectPanel), ShowProjectPanel);
                    SetValue(Section, nameof(ShowOutputPanel), ShowOutputPanel);
                    SetValue(Section, nameof(DebugAsConsole), DebugAsConsole);
                    SetValue(Section, nameof(HandleSaveAs), HandleSaveAs);
                    SetValue(Section, nameof(ShowLineNuberInCodeMap), ShowLineNuberInCodeMap);
                    SetValue(Section, nameof(ShowDebugPanel), ShowDebugPanel);
                    SetValue(Section, nameof(WordWrapInVisualizer), WordWrapInVisualizer);
                    SetValue(Section, nameof(ListManagedProcessesOnly), ListManagedProcessesOnly);
                    SetValue(Section, nameof(StartDebugMonitorOnScriptExecution), StartDebugMonitorOnScriptExecution);
                    SetValue(Section, nameof(BlockLocalDebugOutputContaining), BlockLocalDebugOutputContaining);
                    SetValue(Section, nameof(SyncSecondaryPanelsWithProjectPanel), SyncSecondaryPanelsWithProjectPanel);
                    SetValue(Section, nameof(OutputPanelCapacity), OutputPanelCapacity);
                    SetValue(Section, nameof(HotkeyDocumentsExclusions), HotkeyDocumentsExclusions);
                    SetValue(Section, nameof(NavigateToRawCodeOnDblClickInOutput), NavigateToRawCodeOnDblClickInOutput);
                    SetValue(Section, nameof(QuickViewAutoRefreshAvailable), QuickViewAutoRefreshAvailable);
                    SetValue(Section, nameof(InterceptConsole), InterceptConsole);
                    SetValue(Section, nameof(InterceptConsoleByCharacter), InterceptConsoleByCharacter);
                    SetValue(Section, nameof(UseEmbeddedEngine), UseEmbeddedEngine);
                    SetValue(Section, nameof(CustomEngineAsm), CustomEngineAsm);
                    SetValue(Section, nameof(ReleaseNotesViewedFor), ReleaseNotesViewedFor);
                    SetValue(Section, nameof(ScriptHistory), ScriptHistory);
                    SetValue(Section, nameof(DebugStepPointColor), DebugStepPointColor);
                    SetValue(Section, nameof(DebugStepPointForeColor), DebugStepPointForeColor);
                    SetValue(Section, nameof(SciptHistoryMaxCount), SciptHistoryMaxCount);
                    SetValue(Section, nameof(CollectionItemsInTooltipsMaxCount), CollectionItemsInTooltipsMaxCount);
                    SetValue(Section, nameof(CollectionItemsInVisualizersMaxCount), CollectionItemsInVisualizersMaxCount);
                    SetValue(Section, nameof(DebugPanelInitialTab), DebugPanelInitialTab);
                    SetValue(Section, nameof(LocalDebug), LocalDebug);
                    SetValue(Section, nameof(BreakOnException), BreakOnException);
                    SetValue(Section, nameof(ShowOpenInVsAlways), ShowOpenInVsAlways);
                    SetValue(Section, nameof(ReloadActiveScriptOnRun), ReloadActiveScriptOnRun);
                    SetValue(Section, nameof(UpdateAfterExit), UpdateAfterExit);
                    SetValue(Section, nameof(LastUpdatesCheckDate), LastUpdatesCheckDate);
                    SetValue(Section, nameof(CheckUpdatesOnStartup), CheckUpdatesOnStartup);
                    SetValue(Section, nameof(UseTogglingPanelVisibility), UseTogglingPanelVisibility);
                    SetValue(Section, nameof(CheckPrereleaseUpdates), CheckPrereleaseUpdates);
                    SetValue(Section, nameof(SkipUpdateVersion), SkipUpdateVersion);
                    SetValue(Section, nameof(VbCodeProvider), VbCodeProvider, true);
                    SetValue(Section, nameof(ScriptsDir), ScriptsDir);
                    SetValue(Section, nameof(StartRoslynServerAtNppStartup), StartRoslynServerAtNppStartup);
                    SetValue(Section, nameof(ImproveWin10ListVeiwRendering), ImproveWin10ListVeiwRendering);
                    SetValue(Section, nameof(HideDefaultAssemblies), HideDefaultAssemblies);
                    SetValue(Section, nameof(RestorePanelsAtStartup), RestorePanelsAtStartup);
                    SetValue(Section, nameof(UseCustomLauncher), UseCustomLauncher, true);
                    SetValue(Section, nameof(CustomSyntaxerAsm), CustomSyntaxerAsm);
                    SetValue(Section, nameof(CustomSyntaxerPort), CustomSyntaxerPort);
                    SetValue(Section, nameof(UpdateMode), UpdateMode);
                    SetValue(Section, nameof(VSProjectTemplatePath), VSProjectTemplatePath);
                    SetValue(Section, nameof(FloatingPanelsWarningAlreadyPropted), FloatingPanelsWarningAlreadyPropted);
                    SetValue(Section, nameof(LastExternalProcess), LastExternalProcess);
                    SetValue(Section, nameof(LastExternalProcessArgs), LastExternalProcessArgs);
                    SetValue(Section, nameof(LastExternalProcessCpu), LastExternalProcessCpu);
                    SetValue(Section, nameof(NativeAutoCompletionChecked), NativeAutoCompletionChecked);
                    SetValue(Section, nameof(TargetVersion), TargetVersion);
                    SetValue(Section, nameof(CsSConsoleEncoding), CsSConsoleEncoding);
                    SetValue(Section, nameof(ClasslessScriptByDefault), ClasslessScriptByDefault);
                    SetValue(Section, nameof(DistributeScriptAsScriptByDefault), DistributeScriptAsScriptByDefault);
                    SetValue(Section, nameof(DistributeScriptAsWindowApp), DistributeScriptAsWindowApp);
                    SetValue(Section, nameof(DistributeScriptAsDll), DistributeScriptAsDll);

                    CSScriptIntellisense.Config.Instance.Save();

                    Shortcuts.Save();
                }
                catch
                {
                    Debug.Assert(false);
                    throw;
                }
                Debug.WriteLine("<--- Config.Save");
            }
        }

        public void Open()
        {
            lock (typeof(Config))
            {
                Debug.WriteLine("---> Config.Open");
                ShowLineNuberInCodeMap = GetValue(Section, nameof(ShowLineNuberInCodeMap), ShowLineNuberInCodeMap);
                ShowProjectPanel = GetValue(Section, nameof(ShowProjectPanel), ShowProjectPanel);
                ShowOutputPanel = GetValue(Section, nameof(ShowOutputPanel), ShowOutputPanel);
                DebugAsConsole = GetValue(Section, nameof(DebugAsConsole), DebugAsConsole);
                HandleSaveAs = GetValue(Section, nameof(HandleSaveAs), HandleSaveAs);
                WordWrapInVisualizer = GetValue(Section, nameof(WordWrapInVisualizer), WordWrapInVisualizer);
                ListManagedProcessesOnly = GetValue(Section, nameof(ListManagedProcessesOnly), ListManagedProcessesOnly);
                StartDebugMonitorOnScriptExecution = GetValue(Section, nameof(StartDebugMonitorOnScriptExecution), StartDebugMonitorOnScriptExecution);
                BlockLocalDebugOutputContaining = GetValue(Section, nameof(BlockLocalDebugOutputContaining), BlockLocalDebugOutputContaining);
                SyncSecondaryPanelsWithProjectPanel = GetValue(Section, nameof(SyncSecondaryPanelsWithProjectPanel), SyncSecondaryPanelsWithProjectPanel);
                ShowDebugPanel = GetValue(Section, nameof(ShowDebugPanel), ShowDebugPanel); //ignore; do not show Debug panel as it is heavy. It will be displayed at the first debug step anyway.
                //ShowDebugPanel = GetValue(Section, nameof(ShowDebugPanel), ShowDebugPanel); //ignore; do not show Debug panel as it is heavy. It will be displayed at the first debug step anyway.
                DebugStepPointColor = GetValue(Section, nameof(DebugStepPointColor), DebugStepPointColor, 1024 * 4);
                DebugStepPointForeColor = GetValue(Section, nameof(DebugStepPointForeColor), DebugStepPointForeColor, 1024 * 4);
                ScriptHistory = GetValue(Section, nameof(ScriptHistory), ScriptHistory, 1024 * 40);
                SciptHistoryMaxCount = GetValue(Section, nameof(SciptHistoryMaxCount), SciptHistoryMaxCount);
                CollectionItemsInTooltipsMaxCount = GetValue(Section, nameof(CollectionItemsInTooltipsMaxCount), CollectionItemsInTooltipsMaxCount);
                CollectionItemsInVisualizersMaxCount = GetValue(Section, nameof(CollectionItemsInVisualizersMaxCount), CollectionItemsInVisualizersMaxCount);
                DebugPanelInitialTab = GetValue(Section, nameof(DebugPanelInitialTab), DebugPanelInitialTab);
                OutputPanelCapacity = GetValue(Section, nameof(OutputPanelCapacity), OutputPanelCapacity);
                HotkeyDocumentsExclusions = GetValue(Section, nameof(HotkeyDocumentsExclusions), HotkeyDocumentsExclusions);
                NavigateToRawCodeOnDblClickInOutput = GetValue(Section, nameof(NavigateToRawCodeOnDblClickInOutput), NavigateToRawCodeOnDblClickInOutput);
                InterceptConsole = GetValue(Section, nameof(InterceptConsole), InterceptConsole);
                InterceptConsoleByCharacter = GetValue(Section, nameof(InterceptConsoleByCharacter), InterceptConsoleByCharacter);
                UseEmbeddedEngine = GetValue(Section, nameof(UseEmbeddedEngine), UseEmbeddedEngine);
                CustomEngineAsm = GetValue(Section, nameof(CustomEngineAsm), CustomEngineAsm);
                //QuickViewAutoRefreshAvailable = GetValue(Section, nameof(QuickViewAutoRefreshAvailable), QuickViewAutoRefreshAvailable); //disable until auto-refresh approach is finalized
                LocalDebug = GetValue(Section, nameof(LocalDebug), LocalDebug);
                TargetVersion = GetValue(Section, nameof(TargetVersion), TargetVersion);
                CsSConsoleEncoding = GetValue(Section, nameof(CsSConsoleEncoding), CsSConsoleEncoding);
                LastExternalProcess = GetValue(Section, nameof(LastExternalProcess), LastExternalProcess);
                LastExternalProcessArgs = GetValue(Section, nameof(LastExternalProcessArgs), LastExternalProcessArgs);
                LastExternalProcessCpu = GetValue(Section, nameof(LastExternalProcessCpu), LastExternalProcessCpu);
                NativeAutoCompletionChecked = GetValue(Section, nameof(NativeAutoCompletionChecked), NativeAutoCompletionChecked);
                ReleaseNotesViewedFor = GetValue(Section, nameof(ReleaseNotesViewedFor), ReleaseNotesViewedFor);
                BreakOnException = GetValue(Section, nameof(BreakOnException), BreakOnException);
                ShowOpenInVsAlways = GetValue(Section, nameof(ShowOpenInVsAlways), ShowOpenInVsAlways);
                ReloadActiveScriptOnRun = GetValue(Section, nameof(ReloadActiveScriptOnRun), ReloadActiveScriptOnRun);
                UpdateAfterExit = GetValue(Section, nameof(UpdateAfterExit), UpdateAfterExit);
                LastUpdatesCheckDate = GetValue(Section, nameof(LastUpdatesCheckDate), LastUpdatesCheckDate);
                CheckUpdatesOnStartup = GetValue(Section, nameof(CheckUpdatesOnStartup), CheckUpdatesOnStartup);
                UseTogglingPanelVisibility = GetValue(Section, nameof(UseTogglingPanelVisibility), UseTogglingPanelVisibility);
                CheckPrereleaseUpdates = GetValue(Section, nameof(CheckPrereleaseUpdates), CheckPrereleaseUpdates);
                SkipUpdateVersion = GetValue(Section, nameof(SkipUpdateVersion), SkipUpdateVersion);
                ScriptsDir = GetValue(Section, nameof(ScriptsDir), ScriptsDir);
                VbCodeProvider = GetValue(Section, nameof(VbCodeProvider), VbCodeProvider);
                StartRoslynServerAtNppStartup = GetValue(Section, nameof(StartRoslynServerAtNppStartup), StartRoslynServerAtNppStartup);
                RestorePanelsAtStartup = GetValue(Section, nameof(RestorePanelsAtStartup), RestorePanelsAtStartup);
                CustomSyntaxerAsm = GetValue(Section, nameof(CustomSyntaxerAsm), CustomSyntaxerAsm);
                CustomSyntaxerPort = GetValue(Section, nameof(CustomSyntaxerPort), CustomSyntaxerPort);
                ImproveWin10ListVeiwRendering = GetValue(Section, nameof(ImproveWin10ListVeiwRendering), ImproveWin10ListVeiwRendering);
                HideDefaultAssemblies = GetValue(Section, nameof(HideDefaultAssemblies), HideDefaultAssemblies);
                UpdateMode = GetValue(Section, nameof(UpdateMode), UpdateMode);
                VSProjectTemplatePath = GetValue(Section, nameof(VSProjectTemplatePath), VSProjectTemplatePath);
                FloatingPanelsWarningAlreadyPropted = GetValue(Section, nameof(FloatingPanelsWarningAlreadyPropted), FloatingPanelsWarningAlreadyPropted);
                ClasslessScriptByDefault = GetValue(Section, nameof(ClasslessScriptByDefault), ClasslessScriptByDefault);
                DistributeScriptAsScriptByDefault = GetValue(Section, nameof(DistributeScriptAsScriptByDefault), DistributeScriptAsScriptByDefault);
                DistributeScriptAsWindowApp = GetValue(Section, nameof(DistributeScriptAsWindowApp), DistributeScriptAsWindowApp);
                DistributeScriptAsDll = GetValue(Section, nameof(DistributeScriptAsDll), DistributeScriptAsDll);

                CSScriptIntellisense.Config.Instance.Open();

                Debug.WriteLine("<--- Config.Open");
            }
        }
    }
}