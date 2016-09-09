using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using CSScriptIntellisense;
using CSScriptLibrary;
using CSScriptNpp.Dialogs;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CSScriptNpp
{
    public partial class ProjectPanel : Form
    {
        static internal string currentScript;
        internal CodeMapPanel mapPanel;
        FavoritesPanel favPanel;

        public ProjectPanel()
        {
            InitializeComponent();

            this.VisibleChanged += ProjectPanel_VisibleChanged;

            UpdateButtonsTooltips();

            Debugger.OnDebuggerStateChanged += RefreshControls;

            //tabControl1.Bac
            tabControl1.AddTab("Code Map", mapPanel = new CodeMapPanel());
            tabControl1.AddTab("Favorites", favPanel = new FavoritesPanel());

            favPanel.OnOpenScriptRequest = file =>
                {
                    if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                        LoadScript(file);
                    else
                        Npp.OpenFile(file);
                };

            RefreshControls();
            ReloadScriptHistory();
            LoadReleaseNotes();

            treeView1.AttachMouseControlledZooming();

            toolStripPersistance = new ToolStripPersistance(toolStrip1, settingsFile);
            toolStripPersistance.Load();

            //buttons = toolStrip1.Items.Cast<ToolStripItem>().ToArray();
            //ButtonsDefaultLayout = buttons.Select(x => x.Name.StartsWith("toolStripSeparator") ? "---" : x.Name).ToArray();

            //OrrangeToolstripButtons();

            //watcher = new FileSystemWatcher(Plugin.ConfigDir, "toolbar_buttons.txt");
            //watcher.NotifyFilter = NotifyFilters.LastWrite;
            //watcher.Changed += watcher_Changed;
            //watcher.EnableRaisingEvents = true;
        }

        private void ProjectPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (Config.Instance.SyncSecondaryPanelsWithProjectPanel && !this.Visible)
                Plugin.HideSecondaryPanels();
        }

        ToolStripPersistance toolStripPersistance;

        //void watcher_Changed(object sender, FileSystemEventArgs e)
        //{
        //    Invoke((Action)delegate { OrrangeToolstripButtons(); });
        //}

        //FileSystemWatcher watcher = new FileSystemWatcher();

        string settingsFile = Path.Combine(Plugin.ConfigDir, "toolbar_buttons.txt");

        //ToolStripItem[] buttons;

        //string[] SerializeButtons(IEnumerable<ToolStripItem> items)
        //{
        //    return items.Select(x =>
        //        {
        //            string visibility = x.Visible ? "" : "-";
        //            return x.Name.StartsWith("toolStripSeparator") ? "---" : visibility + x.Name;
        //        }).ToArray();
        //}

        //string[] ButtonsDefaultLayout;


        void UpdateButtonsTooltips()
        {
            validateBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("_BuildFromMenu", "Ctrl+Shift+B") + " or " + Config.Shortcuts.GetValue("Build", "F7"));
            runBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("_Run", "F5"));
            loadBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("LoadCurrentDocument", "Ctrl+F7"));
        }

        void LoadReleaseNotes()
        {
            //System.Diagnostics.Debug.Assert(false);
            whatsNewPanel.Visible = false;
            string pluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (Config.Instance.ReleaseNotesViewedFor != pluginVersion)
            {
                whatsNewTxt.Text = CSScriptNpp.Resources.Resources.WhatsNew;
                whatsNewPanel.Visible = true;
                Config.Instance.ReleaseNotesViewedFor = pluginVersion;
                Config.Instance.Save();
            }
        }

        void ReloadScriptHistory()
        {
            this.historyBtn.DropDownItems.Clear();
            string[] files = Config.Instance.ScriptHistory.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            if (files.Count() == 0)
            {
                this.historyBtn.DropDownItems.Add(new ToolStripMenuItem("empty") { Enabled = false });
            }
            else
            {
                foreach (string file in files)
                {
                    var item = new ToolStripMenuItem(file);
                    item.Click += (s, e) =>
                        {
                            string script = file;
                            if (File.Exists(script))
                            {
                                LoadScript(script);
                            }
                            else if (DialogResult.Yes == MessageBox.Show("File '" + script + "' cannot be found.\nDo you want to remove it from the Recent Scripts List?", "CS-Script", MessageBoxButtons.YesNo))
                            {
                                this.historyBtn.DropDownItems.Remove(item);

                                var scripts = Config.Instance.ScriptHistory.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                                                                          .Distinct()
                                                                          .Where(x => x != script)
                                                                          .ToArray();

                                Config.Instance.ScriptHistory = string.Join("|", scripts);
                                Config.Instance.Save();
                            }
                        };
                    this.historyBtn.DropDownItems.Add(item);
                }

                {
                    this.historyBtn.DropDownItems.Add(new ToolStripSeparator());
                    var item = new ToolStripMenuItem("Clear Recent Scripts List");
                    item.Click += (s, e) =>
                        {
                            this.historyBtn.DropDownItems.Clear();
                            Config.Instance.ScriptHistory = "";
                            Config.Instance.Save();
                            ReloadScriptHistory();
                        };
                    this.historyBtn.DropDownItems.Add(item);
                }
            }
        }

        void runBtn_Click(object sender, EventArgs e)
        {
            Plugin.RunScript();  //important not to call Run directly but run the injected Plugin.RunScript
        }

        void EditItem(string scriptFile)
        {
            Npp.OpenFile(scriptFile);
        }

        void newBtn_Click(object sender, EventArgs e)
        {
            using (var input = new ScripNameInput())
            {
                if (input.ShowDialog() != DialogResult.OK)
                    return;

                string scriptName = NormalizeScriptName(input.ScriptName ?? "New Script");

                int index = Directory.GetFiles(CSScriptHelper.ScriptsDir, scriptName + "*.cs").Length;

                string newScript = Path.Combine(CSScriptHelper.ScriptsDir, scriptName + ".cs");
                if (index != 0)
                {
                    int count = 0;
                    do
                    {
                        count++;
                        index++;
                        newScript = Path.Combine(CSScriptHelper.ScriptsDir, string.Format("{0}{1}.cs", scriptName, index));
                        if (count > 10)
                        {
                            MessageBox.Show("Too many script files with the similar name already exists.\nPlease specify a different file name or clean up some existing scripts.", "CS-Script");
                        }
                    }
                    while (File.Exists(newScript));
                }

                string scriptCode = (Config.Instance.ClasslessScriptByDefault ? defaultClasslessScriptCode : defaultScriptCode);
                if (!File.Exists(newScript))
                {
                    File.WriteAllText(newScript, scriptCode);
                    Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_DOOPEN, 0, newScript);
                    Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);

                    loadBtn.PerformClick();
                }
                else
                {
                    Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.IDM_FILE_NEW);
                    Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
                    Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_ADDTEXT, scriptCode.GetByteCount(), scriptCode);

                    //for some reason setting the lexer does not work
                    int SCLEX_CPP = 3;
                    Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_SETLEXER, SCLEX_CPP, 0);
                    Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_SETLEXERLANGUAGE, 0, "cpp");
                }
            }
        }

        string NormalizeScriptName(string text)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + "_";

            foreach (char c in invalid)
            {
                text = text.Replace(c.ToString(), "");
            }

            return text;
        }

        void SelectScript(string scriptFile)
        {
            if (treeView1.Nodes.Count > 0)
                foreach (TreeNode item in treeView1.Nodes[0].Nodes)
                    if (item.Tag is ProjectItem && string.Compare((item.Tag as ProjectItem).File, scriptFile, true) == 0)
                    {
                        treeView1.SelectedNode = item;
                        treeView1.Focus();
                        return;
                    }
        }

        bool CurrentDocumentBelongsToProject()
        {
            string file;
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, out file);
            return DocumentBelongsToProject(file);
        }

        bool DocumentBelongsToProject(string file)
        {
            if (treeView1.Nodes.Count > 0)
                foreach (TreeNode item in treeView1.Nodes[0].Nodes)
                    if (item.Tag is ProjectItem && string.Compare((item.Tag as ProjectItem).File, file, true) == 0)
                        return true;

            return false;
        }

        string[] GetProjectDocuments()
        {
            var files = new List<string>();
            if (treeView1.Nodes.Count > 0)
                foreach (TreeNode item in treeView1.Nodes[0].Nodes)
                    if (item.Tag is ProjectItem)
                        files.Add((item.Tag as ProjectItem).File);

            return files.ToArray();
        }

        const string defaultScriptCode =
@"using System;
using System.Diagnostics;
using System.Windows.Forms;

class Script
{
    [STAThread]
    static public void Main(string[] args)
    {
        Console.WriteLine(""Hello World!"");
        Debug.WriteLine(""Hello World!"");
    }
}";

        const string defaultClasslessScriptCode =
@"//css_args /ac
using System;
using System.Diagnostics;

void main(string[] args)
{
    Console.WriteLine(""Hello World!"");
    Debug.WriteLine(""Hello World!"");
}";

        void synchBtn_Click(object sender, EventArgs e)
        {
            string path;
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, out path);
            SelectScript(path);
        }

        void validateBtn_Click(object sender, EventArgs e)
        {
            Build();
        }

        void debugBtn_Click(object sender, EventArgs e)
        {
            Plugin.DebugScript();  //important not to call Debug directly but run the injected Plugin.DebugScript
        }

        public void RunAsExternal()
        {
            Run(true);
        }

        public void Run()
        {
            if (Debugger.IsRunning)
                Debugger.Go();
            else
                Run(false);
        }

        void Run(bool asExternal)
        {
            if (currentScript == null)
                loadBtn.PerformClick();

            if (currentScript == null)
            {
                MessageBox.Show("Please load some script file first.", "CS-Script");
            }
            else
            {
                try
                {
                    if (!CurrentDocumentBelongsToProject())
                        EditItem(currentScript);

                    Npp.SaveDocuments(GetProjectDocuments());
                    

                    if (asExternal)
                    {
                        try
                        {
                            CSScriptHelper.ExecuteAsynch(currentScript);
                        }
                        catch (Exception e)
                        {
                            Plugin.ShowOutputPanel()
                                  .ShowBuildOutput()
                                  .WriteLine(e.Message)
                                  .SetCaretAtStart();
                        }
                    }
                    else
                    {
                        OutputPanel outputPanel = Plugin.ShowOutputPanel();

                        outputPanel.AttachDebuger();
                        outputPanel.ClearAllDefaultOutputs();

                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                outputPanel.ShowDebugOutput();
                                if (Config.Instance.InterceptConsole)
                                {
                                    CSScriptHelper.Execute(currentScript, OnRunStart, OnConsoleOutChar);
                                }
                                else
                                {
                                    CSScriptHelper.Execute(currentScript, OnRunStart);
                                }
                            }
                            catch (Exception e)
                            {
                                outputPanel.ShowBuildOutput()
                                           .WriteLine(e.Message)
                                           .SetCaretAtStart();
                            }
                            finally
                            {
                                this.InUiThread(() =>
                                {
                                    Plugin.RunningScript = null;
                                    RefreshControls();
                                    Npp.GrabFocus();
                                });
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Plugin.ShowOutputPanel()
                          .ShowBuildOutput()
                          .WriteLine(ex.Message)
                          .SetCaretAtStart();
                }
            }
        }

        public void Debug(bool breakOnFirstStep)
        {
            Plugin.InitDebugPanel();

            if (currentScript == null)
                loadBtn.PerformClick();

            if (currentScript == null)
            {
                MessageBox.Show("Please load some script file first.", "CS-Script");
            }
            else
            {
                Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);

                bool canCompile = CSScriptHelper.Verify(currentScript);

                if (!canCompile)
                {
                    Build();
                }
                else
                {
                    Plugin.ShowOutputPanel().ShowDebugOutput().Clear();

                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            string entryFile = CSScriptHelper.GetEntryFileName(currentScript);
                            Debugger.ScriptFile = currentScript;

                            bool isX86 = false;
                            bool isSurrogateHost = CSScriptHelper.IsSurrogateHosted(currentScript, ref isX86);
                            if (isSurrogateHost)
                            {
                                var scriptAsm = CSScript.GetCachedScriptPath(currentScript);
                                var debuggingHost = scriptAsm + ".host.exe";
                                Debugger.Start(debuggingHost, scriptAsm, isX86 ? Debugger.CpuType.x86 : Debugger.CpuType.x64);
                            }
                            else
                            {
                                string targetType = Debugger.DebugAsConsole ? CSScriptHelper.cscs_exe : CSScriptHelper.csws_exe;
                                string debuggingHost = Path.Combine(Plugin.PluginDir, "css_dbg.exe");
                                Debugger.Start(debuggingHost, string.Format("\"{0}\" /dbg /l {2} \"{1}\"", targetType, currentScript, CSScriptHelper.GenerateDefaultArgs()), Debugger.CpuType.Any);
                            }

                            if (breakOnFirstStep)
                            {
                                Debugger.EntryBreakpointFile = entryFile ?? currentScript;
                            }

                            RefreshControls();
                        }
                        catch (Exception e)
                        {
                            Plugin.OutputPanel.DebugOutput.WriteLine(e.Message);
                        }
                    });
                }
            }
        }

        public void OpenInVS()
        {
            Cursor = Cursors.WaitCursor;
            if (currentScript == null)
            {
                MessageBox.Show("Please load some script file first.", "CS-Script");
            }
            else
            {
                Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);
                try
                {
                    CSScriptHelper.OpenAsVSProjectFor(currentScript);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            Cursor = Cursors.Default;
        }

        void OnRunStart(Process proc)
        {
            Plugin.RunningScript = proc;
            this.Invoke((Action) RefreshControls);
        }

        void OnConsoleOut(string line)
        {
            if (Plugin.OutputPanel.ConsoleOutput.IsEmpty)
                Plugin.OutputPanel.ShowConsoleOutput();

            Plugin.OutputPanel.ConsoleOutput.WriteLine(line);
        }

        void OnConsoleOutChar(char[] buf)
        {
            if (Plugin.OutputPanel.ConsoleOutput.IsEmpty)
                Plugin.OutputPanel.ShowConsoleOutput();

            foreach (char c in buf)
                Plugin.OutputPanel.ConsoleOutput.WriteConsoleChar(c);
        }

        void Job()
        {
            try
            {
                CSScript.CompileFile(currentScript, null, false);
            }
            catch (Exception ex)
            {
                Environment.SetEnvironmentVariable("CSS_COMPILE_ERROR", ex.Message);
            }
        }

        public void Build()
        {
            lock (this)
            {
                if (currentScript == null)
                    loadBtn.PerformClick();

                if (currentScript == null)
                {
                    MessageBox.Show("Please load some script file first.", "CS-Script");
                }
                else
                {
                    OutputPanel outputPanel = Plugin.ShowOutputPanel();

                    outputPanel.ShowBuildOutput();
                    outputPanel.BuildOutput.Clear();
                    outputPanel.BuildOutput.WriteLine("------ Build started: Script: " + Path.GetFileNameWithoutExtension(currentScript) + " ------");

                    try
                    {
                        if (!CurrentDocumentBelongsToProject())
                            EditItem(currentScript);

                        Npp.SaveDocuments(GetProjectDocuments());

                        CSScriptHelper.Build(currentScript);

                        outputPanel.BuildOutput.WriteLine(null)
                                               .WriteLine("========== Build: succeeded ==========")
                                               .SetCaretAtStart();
                    }
                    catch (Exception ex)
                    {
                        outputPanel.ShowBuildOutput()
                                   .WriteLine(null)
                                   .WriteLine(ex.Message)
                                   .WriteLine("========== Build: Failed ==========")
                                   .SetCaretAtStart();
                    }
                }
            }
        }

        void reloadBtn_Click(object sender, EventArgs e)
        {
            LoadScript(currentScript);
        }

        public void RefreshProjectStructure()
        {
            this.InUiThread(() =>
            {
                if (Npp.GetCurrentFile() == currentScript)
                {
                    try
                    {
                        Project project = CSScriptHelper.GenerateProjectFor(currentScript);

                        treeView1.BeginUpdate();

                        /*
                         root
                         references
                            assembly_1 
                            assembly_2 
                            assembly_n 
                         script_1 
                         script_2 
                         script_N 
                         */

                        TreeNode root = treeView1.Nodes[0];
                        TreeNode references = root.Nodes[0];

                        Action<TreeNode, string[]> updateNode =
                            (node, files) =>
                            {
                                string[] currentFiles = node.Nodes
                                                            .Cast<TreeNode>()
                                                            .Where(x => x.Tag is ProjectItem)
                                                            .Select(x => (x.Tag as ProjectItem).File)
                                                            .ToArray();

                                string[] newItems = files.Except(currentFiles).ToArray();

                                var orphantItems = node.Nodes
                                                       .Cast<TreeNode>()
                                                       .Where(x => x.Tag is ProjectItem)
                                                       .Where(x => !files.Contains((x.Tag as ProjectItem).File))
                                                       .Where(x => x != root && x != references)
                                                       .ToArray();

                                orphantItems.ForEach(x => node.Nodes.Remove(x));
                                newItems.ForEach(file =>
                                {
                                    int imageIndex = includeImage;
                                    var info = new ProjectItem(file) { IsPrimary = (file == project.PrimaryScript) };
                                    if (info.IsAssembly)
                                        imageIndex = assemblyImage;
                                    node.Nodes.Add(new TreeNode(info.Name) { ImageIndex = imageIndex, SelectedImageIndex = imageIndex, Tag = info, ToolTipText = file, ContextMenuStrip = itemContextMenu });
                                });
                            };

                        updateNode(references, project.Assemblies);
                        updateNode(root, project.SourceFiles);
                        root.Expand();

                        treeView1.EndUpdate();
                    }
                    catch (Exception e)
                    {
                        e.LogAsError();
                    }
                }
            });
        }

        void RefreshControls()
        {
            this.InUiThread(() =>
            {
                openInVsBtn.Visible = Utils.IsVS2010PlusAvailable;

                newBtn.Enabled = true;

                validateBtn.Enabled =
                reloadBtn.Enabled =
                debugBtn.Enabled =
                openInVsBtn.Enabled =
                synchBtn.Enabled =
                runBtn.Enabled = (treeView1.Nodes.Count > 0);

                bool running = (Plugin.RunningScript != null);
                runBtn.Enabled = !running || Debugger.IsRunning;
                stopBtn.Enabled = running || Debugger.IsRunning;

                if (running)
                {
                    validateBtn.Enabled =
                    debugBtn.Enabled =
                    openInVsBtn.Enabled =
                    loadBtn.Enabled =
                    newBtn.Enabled =
                    reloadBtn.Enabled = false;
                }
                else
                    loadBtn.Enabled = true;
            });
        }

        ProjectItem SelectedItem
        {
            get
            {
                if (treeView1.SelectedNode != null)
                    return treeView1.SelectedNode.Tag as ProjectItem;
                else
                    return null;
            }
        }

        void openInVsBtn_Click(object sender, EventArgs e)
        {
            OpenInVS();
        }

        void aboutBtn_Click(object sender, EventArgs e)
        {
            Plugin.ShowAbout();
        }

        void hlpBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://csscriptnpp.codeplex.com/documentation");
            }
            catch { }
        }

        void loadBtn_Click(object sender, EventArgs e)
        {
            LoadCurrentDoc();
        }

        public void LoadCurrentDoc()
        {
            string path;
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, out path);
            if (!File.Exists(path))
                Win32.SendMenuCmd(Npp.NppHandle, NppMenuCmd.IDM_FILE_SAVE, 0);

            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, out path);
            if (File.Exists(path))
                LoadScript(path);

            RefreshControls();

            Task.Factory.StartNew(CSScriptHelper.ClearVSDir);
        }

        const int scriptImage = 1;
        const int folderImage = 0;
        const int assemblyImage = 2;
        const int includeImage = 3;

        void UnloadScript()
        {
            currentScript = null;
            treeView1.Nodes.Clear();
        }

        public void LoadScript(string scriptFile)
        {
            if (!string.IsNullOrWhiteSpace(scriptFile) && File.Exists(scriptFile))
            {
                if (!scriptFile.IsScriptFile())
                {
                    MessageBox.Show("The file type '" + Path.GetExtension(scriptFile) + "' is not supported.", "CS-Script");
                }
                else
                {
                    try
                    {
                        Npp.OpenFile(scriptFile);

                        Project project = CSScriptHelper.GenerateProjectFor(scriptFile);

                        /*
                        root
                        references
                           assembly_1 
                           assembly_2 
                           assembly_n 
                        script_1 
                        script_2 
                        script_N 
                        */

                        treeView1.BeginUpdate();
                        treeView1.Nodes.Clear();

                        TreeNode root = treeView1.Nodes.Add("Script '" + Path.GetFileNameWithoutExtension(scriptFile) + "'");
                        TreeNode references = root.Nodes.Add("References");

                        root.SelectedImageIndex =
                        root.ImageIndex = scriptImage;
                        references.SelectedImageIndex =
                        references.ImageIndex = assemblyImage;
                        references.ContextMenuStrip = itemContextMenu;

                        root.ContextMenuStrip = solutionContextMenu;
                        root.ToolTipText = "Script: " + scriptFile;

                        Action<TreeNode, string[]> populateNode =
                            (node, files) =>
                            {
                                foreach (var file in files)
                                {
                                    int imageIndex = includeImage;
                                    var info = new ProjectItem(file) { IsPrimary = (file == project.PrimaryScript) };
                                    if (info.IsPrimary)
                                        imageIndex = scriptImage;
                                    if (info.IsAssembly)
                                        imageIndex = assemblyImage;
                                    node.Nodes.Add(new TreeNode(info.Name) { ImageIndex = imageIndex, SelectedImageIndex = imageIndex, Tag = info, ToolTipText = file, ContextMenuStrip = itemContextMenu });
                                };
                            };

                        populateNode(references, project.Assemblies);
                        populateNode(root, project.SourceFiles);
                        root.Expand();

                        treeView1.EndUpdate();

                        currentScript = scriptFile;
                        CSScriptIntellisense.Plugin.EnsureCurrentFileParsed();

                        var history = Config.Instance.ScriptHistory.Split('|').ToList();
                        history.Remove(scriptFile);
                        history.Insert(0, scriptFile);

                        Config.Instance.ScriptHistory = string.Join("|", history.Take(Config.Instance.SciptHistoryMaxCount).ToArray());
                        Config.Instance.Save();
                        ReloadScriptHistory();
                    }
                    catch (Exception e)
                    {
                        //it is not a major use-case so doesn't matter why we failed
                        MessageBox.Show("Cannot load script.\nError: " + e.Message, "CS-Script");
                        e.LogAsError();
                    }
                }
            }
            else
            {
                MessageBox.Show("Script '" + scriptFile + "' does not exist.", "CS-Script");
            }
            RefreshControls();
        }

        void outputBtn_Click(object sender, EventArgs e)
        {
            Plugin.ToggleScondaryPanels();
        }

        void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (SelectedItem != null && !SelectedItem.IsAssembly)
            {
                EditItem(SelectedItem.File);
                Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);
            }
        }

        void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshControls();
        }

        void stopBtn_Click(object sender, EventArgs e)
        {
            Plugin.Stop();
        }

        void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node == treeView1.Nodes[0].Nodes[0])
            {
                e.Node.ImageIndex = e.Node.SelectedImageIndex = folderImage;
            }
        }

        void treeView1_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node == treeView1.Nodes[0].Nodes[0])
            {
                e.Node.ImageIndex = e.Node.SelectedImageIndex = assemblyImage;
            }
        }

        void unloadScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UnloadScript();
        }

        void openCommandPromptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WithSelectedNodeProjectItem(item =>
            {
                string file;
                if (treeView1.SelectedNode == treeView1.Nodes[0]) //root node
                    file = currentScript;
                else if (item != null)
                    file = item.File;
                else
                    return;

                string path = Path.GetDirectoryName(file);

                if (Directory.Exists(path))
                    Process.Start("cmd.exe", "/K \"cd " + path + "\"");
                else
                    MessageBox.Show("Directory '" + path + "' does not exist.", "CS-Script");
            });
        }

        void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WithSelectedNodeProjectItem(item =>
            {
                if (item != null)
                {
                    string path = item.File;
                    if (File.Exists(path))
                        Process.Start("explorer.exe", "/select," + path);
                    else
                        MessageBox.Show("File '" + path + "' does not exist.", "CS-Script");
                }
            });
        }

        void WithSelectedNodeProjectItem(Action<ProjectItem> action)
        {
            if (treeView1.SelectedNode != null)
            {
                try
                {
                    action(treeView1.SelectedNode.Tag as ProjectItem);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "CS-Script");
                }
            }
        }

        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
            }
        }

        void openScriptsFolderBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", CSScriptHelper.ScriptsDir);
            }
            catch { }
        }

        void configBtn_Click(object sender, EventArgs e)
        {
            Plugin.ShowConfig();
        }

        void deployBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentScript == null)
                    LoadCurrentDoc();

                if (currentScript != null) //may not necessarily be loaded successfully

                    using (var dialog = new DeploymentInput())
                        if (DialogResult.OK == dialog.ShowDialog())
                        {
                            EditItem(currentScript);

                            Npp.SaveDocuments(GetProjectDocuments());

                            string selectedTargetVersion = dialog.SelectedVersion.Version;
                            string path = CSScriptHelper.Isolate(currentScript, dialog.AsScript, selectedTargetVersion, dialog.AsWindowApp);

                            if (path != null)
                            {
                                string pluginClrVersion = "v" + Environment.Version.ToString();

                                if (dialog.AsScript && !pluginClrVersion.StartsWith(selectedTargetVersion)) //selectedTargetVersion may not include the build number
                                    MessageBox.Show("Distribution package targets CLR version, which is different from the default version.\r\nPlease verify that the script is compatible with the selected CLR version.", "CS-Script");

                                Process.Start("explorer.exe", path);
                            }
                        }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "CS-Script");
            }
        }

        void shortcutsBtn_Click(object sender, EventArgs e)
        {
            using (var dialog = new PluginShortcuts())
                dialog.ShowDialog();
        }

        void pictureBox1_Click(object sender, EventArgs e)
        {
            whatsNewPanel.Visible = false;
        }

        void treeView1_SizeChanged(object sender, EventArgs e)
        {
            treeView1.Invalidate(); //WinForm TreeView has nasty rendering artifact on window maximize
        }

        void organizeButtonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(settingsFile);
            }
            catch { }
        }

        void restartNppBtn_Click(object sender, EventArgs e)
        {
            Utils.RestartNpp();
        }

        void ProjectPanel_Deactivate(object sender, EventArgs e)
        {
            this.Refresh();
            System.Diagnostics.Debug.WriteLine("ProjectPanel_Deactivate");
        }

        void favoritesBtn_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTabWith(favPanel);
            favPanel.Add(Npp.GetCurrentFile());
        }
    }
}