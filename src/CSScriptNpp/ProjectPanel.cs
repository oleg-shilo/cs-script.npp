using CSScriptIntellisense;
using CSScriptLibrary;
using CSScriptNpp.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public partial class ProjectPanel : Form
    {
        static internal string currentScript;
        private CodeMapPanel mapPanel;

        public ProjectPanel()
        {
            InitializeComponent();

            UpdateButtonsTooltips();

            Debugger.OnDebuggerStateChanged += RefreshControls;

            mapPanel = new CodeMapPanel();
            tabControl1.AddTab("Code Map", mapPanel);

            RefreshControls();
            ReloadScriptHistory();
            LoadReleaseNotes();

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

        ToolStripPersistance toolStripPersistance;

        //void watcher_Changed(object sender, FileSystemEventArgs e)
        //{
        //    Invoke((Action)delegate { OrrangeToolstripButtons(); });
        //}

        //FileSystemWatcher watcher = new FileSystemWatcher();

        private string settingsFile = Path.Combine(Plugin.ConfigDir, "toolbar_buttons.txt");

        //private ToolStripItem[] buttons;

        //string[] SerializeButtons(IEnumerable<ToolStripItem> items)
        //{
        //    return items.Select(x =>
        //        {
        //            string visibility = x.Visible ? "" : "-";
        //            return x.Name.StartsWith("toolStripSeparator") ? "---" : visibility + x.Name;
        //        }).ToArray();
        //}

        //string[] ButtonsDefaultLayout;


        private void UpdateButtonsTooltips()
        {
            validateBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("_BuildFromMenu", "Ctrl+Shift+B") + " or " + Config.Shortcuts.GetValue("Build", "F7"));
            runBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("_Run", "F5"));
            loadBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("LoadCurrentDocument", "Ctrl+F7"));
        }

        private void LoadReleaseNotes()
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

        private void ReloadScriptHistory()
        {
            this.historyBtn.DropDownItems.Clear();
            string[] files = Config.Instance.SciptHistory.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
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

                                var scripts = Config.Instance.SciptHistory.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                                                                          .Distinct()
                                                                          .Where(x => x != script)
                                                                          .ToArray();

                                Config.Instance.SciptHistory = string.Join("|", scripts);
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
                            Config.Instance.SciptHistory = "";
                            Config.Instance.Save();
                            ReloadScriptHistory();
                        };
                    this.historyBtn.DropDownItems.Add(item);
                }
            }
        }

        private void runBtn_Click(object sender, EventArgs e)
        {
            Plugin.RunScript();  //important not to call Run directly but run the injected Plugin.RunScript
        }

        private void EditItem(string scriptFile)
        {
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_DOOPEN, 0, scriptFile);
            Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
        }

        private string scriptsDirectory;

        private string ScriptsDirectory
        {
            get
            {
                if (scriptsDirectory == null)
                    scriptsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "C# Scripts");
                return scriptsDirectory;
            }
        }

        private void newBtn_Click(object sender, EventArgs e)
        {
            using (var input = new ScripNameInput())
            {
                if (input.ShowDialog() != DialogResult.OK)
                    return;

                if (!Directory.Exists(ScriptsDirectory))
                    Directory.CreateDirectory(ScriptsDirectory);

                string scriptName = NormalizeScriptName(input.ScriptName ?? "New Script");

                int index = Directory.GetFiles(ScriptsDirectory, scriptName + "*.cs").Length;

                string newScript = Path.Combine(ScriptsDirectory, scriptName + ".cs");
                if (index != -1)
                {
                    int count = 0;
                    do
                    {
                        index++;
                        count++;
                        newScript = Path.Combine(ScriptsDirectory, string.Format("{0}{1}.cs", scriptName, index));
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
                    Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_ADDTEXT, scriptCode.Length, scriptCode);

                    //for some reason setting the lexer does not work
                    int SCLEX_CPP = 3;
                    Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_SETLEXER, SCLEX_CPP, 0);
                    Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_SETLEXERLANGUAGE, 0, "cpp");
                }
            }
        }

        private string NormalizeScriptName(string text)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + "_";

            foreach (char c in invalid)
            {
                text = text.Replace(c.ToString(), "");
            }

            return text;
        }

        private void SelectScript(string scriptFile)
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

        private bool CurrentDocumentBelongsToProject()
        {
            string file;
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, out file);

            if (treeView1.Nodes.Count > 0)
                foreach (TreeNode item in treeView1.Nodes[0].Nodes)
                    if (item.Tag is ProjectItem && string.Compare((item.Tag as ProjectItem).File, file, true) == 0)
                        return true;

            return false;
        }

        private const string defaultScriptCode =
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

        private const string defaultClasslessScriptCode =
@"//css_args /ac
using System;
using System.Diagnostics;

void main(string[] args)
{
    Console.WriteLine(""Hello World!"");
    Debug.WriteLine(""Hello World!"");
}";

        private void synchBtn_Click(object sender, EventArgs e)
        {
            string path;
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, out path);
            SelectScript(path);
        }

        private void validateBtn_Click(object sender, EventArgs e)
        {
            Build();
        }

        private void debugBtn_Click(object sender, EventArgs e)
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

        private void Run(bool asExternal)
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

                    Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVEALLFILES, 0, 0);

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
                                    CSScriptHelper.Execute(currentScript, OnRunStart, OnConsoleOut);
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
            if (Plugin.DebugPanel == null)
                Plugin.DoDebugPanel();

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

                            string targetType = Debugger.DebugAsConsole ? "cscs.exe" : "csws.exe";
                            string debuggingHost = Path.Combine(Plugin.PluginDir, "css_dbg.exe");

                            Debugger.ScriptFile = currentScript;
                            Debugger.Start(debuggingHost, string.Format("{0} /dbg /l \"{1}\"", targetType, currentScript), Debugger.CpuType.Any);

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

        private void OnRunStart(Process proc)
        {
            Plugin.RunningScript = proc;
            this.Invoke((Action)RefreshControls);
        }

        private void OnConsoleOut(string line)
        {
            if (Plugin.OutputPanel.ConsoleOutput.IsEmpty)
                Plugin.OutputPanel.ShowConsoleOutput();

            Plugin.OutputPanel.ConsoleOutput.WriteLine(line);
        }

        private void Job()
        {
            try
            {
                CSScript.Compile(currentScript, null, false);
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

                        Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVEALLFILES, 0, 0);

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

        private void reloadBtn_Click(object sender, EventArgs e)
        {
            LoadScript(currentScript);
        }

        private void RefreshControls()
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
        }

        private ProjectItem SelectedItem
        {
            get
            {
                if (treeView1.SelectedNode != null)
                    return treeView1.SelectedNode.Tag as ProjectItem;
                else
                    return null;
            }
        }

        private void openInVsBtn_Click(object sender, EventArgs e)
        {
            OpenInVS();
        }

        private void aboutBtn_Click(object sender, EventArgs e)
        {
            Plugin.ShowAbout();
        }

        private void hlpBtn_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("Not Implemented Yet");
                //Process.Start("https://dl.dropboxusercontent.com/u/2192462/NPP/NppScriptsHelp.html");
            }
            catch { }
        }

        private void loadBtn_Click(object sender, EventArgs e)
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

        private const int scriptImage = 1;
        private const int folderImage = 0;
        private const int assemblyImage = 2;
        private const int includeImage = 3;

        private void UnloadScript()
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

                        var history = Config.Instance.SciptHistory.Split('|').ToList();
                        history.Remove(scriptFile);
                        history.Insert(0, scriptFile);

                        Config.Instance.SciptHistory = string.Join("|", history.Take(Config.Instance.SciptHistoryMaxCount).ToArray());
                        Config.Instance.Save();
                        ReloadScriptHistory();
                    }
                    catch
                    {
                        //it is not a major use-case so doesn't matter why we failed
                    }
                }
            }
            else
            {
                MessageBox.Show("Script '" + scriptFile + "' does not exist.", "CS-Script");
            }
            RefreshControls();
        }

        private void outputBtn_Click(object sender, EventArgs e)
        {
            Plugin.ToggleScondaryPanels();
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (SelectedItem != null && !SelectedItem.IsAssembly)
            {
                EditItem(SelectedItem.File);
                Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshControls();
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            Plugin.Stop();
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node == treeView1.Nodes[0].Nodes[0])
            {
                e.Node.ImageIndex = e.Node.SelectedImageIndex = folderImage;
            }
        }

        private void treeView1_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node == treeView1.Nodes[0].Nodes[0])
            {
                e.Node.ImageIndex = e.Node.SelectedImageIndex = assemblyImage;
            }
        }

        private void unloadScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UnloadScript();
        }

        private void openCommandPromptToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void WithSelectedNodeProjectItem(Action<ProjectItem> action)
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

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
            }
        }

        private void openScriptsFolderBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(ScriptsDirectory))
                    Directory.CreateDirectory(ScriptsDirectory);

                Process.Start("explorer.exe", ScriptsDirectory);
            }
            catch { }
        }

        private void configBtn_Click(object sender, EventArgs e)
        {
            Plugin.ShowConfig();
        }

        private void deployBtn_Click(object sender, EventArgs e)
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

                            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVEALLFILES, 0, 0);

                            string selectedTargetVersion = dialog.SelectedVersion.Version;
                            string path = CSScriptHelper.Isolate(currentScript, dialog.AsScript, selectedTargetVersion);

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

        private void shortcutsBtn_Click(object sender, EventArgs e)
        {
            using (var dialog = new PluginShortcuts())
                dialog.ShowDialog();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            whatsNewPanel.Visible = false;
        }

        private void treeView1_SizeChanged(object sender, EventArgs e)
        {
            treeView1.Invalidate(); //WinForm TreeView has nasty rendering artifact on window maximize
        }

        private void organizeButtonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(settingsFile);
            }
            catch { }
        }

        private void restartNppBtn_Click(object sender, EventArgs e)
        {
            Utils.RestartNpp();
        }
    }
}