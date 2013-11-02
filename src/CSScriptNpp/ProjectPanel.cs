using CSScriptLibrary;
using System;
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

        public ProjectPanel()
        {
            InitializeComponent();

            if (Config.Instance.BuildOnF7)
                validateBtn.ToolTipText += " or F7";

            RefreshControls();
            ReloadScriptHistory();
            LoadReleaseNotes();
        }

        void LoadReleaseNotes()
        {
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
            this.histotyBtn.DropDownItems.Clear();
            string[] files = Config.Instance.SciptHistory.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (files.Count() == 0)
            {
                this.histotyBtn.DropDownItems.Add(new ToolStripMenuItem("empty") { Enabled = false });
            }
            else
            {
                foreach (string file in files)
                {
                    var item = new ToolStripMenuItem(file);
                    item.Click += (s, e) => LoadScript(file);
                    this.histotyBtn.DropDownItems.Add(item);
                }

                {
                    this.histotyBtn.DropDownItems.Add(new ToolStripSeparator());
                    var item = new ToolStripMenuItem("Clear Recent Scripts List");
                    item.Click += (s, e) =>
                        {
                            this.histotyBtn.DropDownItems.Clear();
                            Config.Instance.SciptHistory = "";
                            Config.Instance.Save();
                            ReloadScriptHistory();
                        };
                    this.histotyBtn.DropDownItems.Add(item);
                }
            }
        }

        void runBtn_Click(object sender, EventArgs e)
        {
            Plugin.RunScript();  //important not to call Run directly but run the injected Plugin.RunScript
        }

        void EditItem(string scriptFile)
        {
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_DOOPEN, 0, scriptFile);
            Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
        }

        string scriptsDirectory;

        string ScriptsDirectory
        {
            get
            {
                if (scriptsDirectory == null)
                    scriptsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "C# Scripts");
                return scriptsDirectory;
            }
        }

        void newBtn_Click(object sender, EventArgs e)
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
                    EditItem(currentScript);
                    Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);

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
                        outputPanel.ConsoleOutput.Clear();
                        outputPanel.BuildOutput.Clear();
                        outputPanel.DebugOutput.Clear();

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

        public void Debug()
        {
            if (currentScript == null)
                loadBtn.PerformClick();

            if (currentScript == null)
            {
                MessageBox.Show("Please load some script file first.", "CS-Script");
            }
            else
            {
                Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        CSScriptHelper.ExecuteDebug(currentScript);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                });
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
            this.Invoke((Action)RefreshControls);
        }

        void OnConsoleOut(string line)
        {
            if (Plugin.OutputPanel.ConsoleOutput.IsEmpty)
                Plugin.OutputPanel.ShowConsoleOutput();

            Plugin.OutputPanel.ConsoleOutput.WriteLine(line);
        }

        void Job()
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
                        EditItem(currentScript);
                        Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);

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

        void RefreshControls()
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
            runBtn.Visible = !running;
            stopBtn.Visible = running;

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
                MessageBox.Show("Not Implemented Yet");
                //Process.Start("https://dl.dropboxusercontent.com/u/2192462/NPP/NppScriptsHelp.html");
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
                if (!scriptFile.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The file type '" + Path.GetExtension(scriptFile) + "' is not supported.", "CS-Script");
                }
                else
                {
                    try
                    {
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
                        Intellisense.Refresh();

                        var history = Config.Instance.SciptHistory.Split('|').ToList();
                        history.Insert(0, scriptFile);
                        Config.Instance.SciptHistory = string.Join("|", history.Take(Config.Instance.SciptHistoryMaxCount).ToArray());
                        Config.Instance.Save();
                        ReloadScriptHistory();
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                MessageBox.Show("Script '" + scriptFile + "' does not exist.", "CS-Script");
            }
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
            if (Plugin.RunningScript != null)
            {
                try
                {
                    Plugin.RunningScript.Kill();
                }
                catch (Exception ex)
                {
                    Plugin.OutputPanel.BuildOutput.WriteLine(null)
                                                  .WriteLine(ex.Message);
                }
            }
        }

        void ProjectPanel_KeyDown(object sender, KeyEventArgs e)
        {
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
            if (Intellisense.ShowConfig != null)
                Intellisense.ShowConfig();
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
                            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);

                            string selectedTargetVersion = dialog.SelectedVersion.Version;
                            string path = CSScriptHelper.Isolate(currentScript, dialog.AsScript, selectedTargetVersion);
                            
                            if (path != null)
                            {
                                string pluginClrVersion = "v"+Environment.Version.ToString();

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
    }
}