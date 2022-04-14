using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace CSScriptNpp
{
    public partial class OutputPanel : Form
    {
        public const string BuildOutputName = "Build";
        public const string ConsoleOutputName = "Console";
        public const string DebugOutputName = "Debug";
        public const string GeneralOutputName = "General";
        public const string PluginLogOutputName = "Plugin Log";

        static public void DisplayInGenericOutputPanel(string text)
        {
            if (instance != null)
            {
                var output = instance.GetOutputType(GeneralOutputName);
                instance.Show(output);
                output.Clear();
                output.Write(text);
            }
        }

        public OutputPanel ClearAllDefaultOutputs()
        {
            ConsoleOutput.Clear();
            BuildOutput.Clear();
            return this;
        }

        public void TryNavigateToFileReference(bool toNext)
        {
            int line;
            int column;
            string file;

            Output output = GetVisibleOutput();

            if (output == null)
                return;

            int currentPos = -1;
            int prevPos = -1;

            Func<string> selectNextLineInOutput = null;

            if (toNext)
                selectNextLineInOutput = () => output.MoveToNextLine(out currentPos);
            else
                selectNextLineInOutput = () => output.MoveToPrevLine(out currentPos);

            string lineText = selectNextLineInOutput();
            do
            {
                prevPos = currentPos;
                if (lineText != null && lineText.ParseAsFileReference(out file, out line, out column))
                {
                    NormaliseFileReference(ref file, ref line);
                    this.NavigateToFileContent(file, line, column);
                    return;
                }
                lineText = selectNextLineInOutput();
            }
            while (lineText != null && prevPos != currentPos);
        }

        static OutputPanel instance;

        public OutputPanel()
        {
            instance = this;

            InitializeComponent();

            var cb = new CheckBox();
            cb.Text = "Intercept StdOut";
            cb.Checked = Config.Instance.InterceptConsole;
            cb.CheckStateChanged += (s, ex) => Config.Instance.InterceptConsole = cb.Checked;
            toolStrip1.Items.Insert(3, new ToolStripControlHost(cb) { ToolTipText = "Check to redirect the console output to the output panel" });

            AddOutputType(BuildOutputName);
            // AddOutputType(DebugOutputName);
            AddOutputType(ConsoleOutputName);
            AddOutputType(GeneralOutputName);
#if DEBUG
            AddOutputType(PluginLogOutputName);
#endif
        }

        protected override void WndProc(ref Message m)
        {
            //Listen for the closing of the dockable panel as the result of Npp native close ("cross") button on the window
            switch (m.Msg)
            {
                case win32.WM_NOTIFY:
                    var notify = (ScNotificationHeader)Marshal.PtrToStructure(m.LParam, typeof(ScNotificationHeader));
                    if (notify.Code == (int)DockMgrMsg.DMN_CLOSE)
                        Plugin.SetDockedPanelVisible(this, Plugin.outputPanelId, false);
                    break;
            }
            base.WndProc(ref m);
        }

        public Output GetOutputType(string name)
        {
            Output retval = null;

            this.InUiThread(() =>
                {
                    foreach (OutputInfo item in outputType.Items)
                        if (item.Name == name)
                        {
                            retval = item.Output;
                        }
                });

            return retval;
        }

        public void AttachDebgMonitor()
        {
            if (dbgMonitor == null)
            {
                DebugViewBtn.PerformClick();
            }
        }

        Output Show(Output output)
        {
            Output retval = null;

            this.InUiThread(() =>
                {
                    foreach (OutputInfo item in outputType.Items)
                    {
                        if (item.Output == output)
                        {
                            if (outputType.SelectedItem != item)
                                outputType.SelectedItem = item;
                            outputType.Text = item.Name;
                            retval = item.Output;

                            break;
                        }
                    }
                });

            return retval;
        }

        public Output ShowBuildOutput()
        {
            return Show(BuildOutput);
        }

        // public Output ShowDebugOutput()
        // {
        //     return Show(DebugOutput);
        // }

        public Output ShowConsoleOutput()
        {
            return Show(ConsoleOutput);
        }

        public Output ShowOutput(string name)
        {
            Output retval = null;

            this.InUiThread(() =>
            {
                foreach (OutputInfo item in outputType.Items)
                {
                    item.Output.Visible = (item.Name == name);

                    if (item.Output.Visible)
                    {
                        outputType.SelectedItem = item;
                        outputType.Text = item.Name;
                        retval = item.Output;
                        break;
                    }
                }
            });

            return retval;
        }

        public Output GetVisibleOutput()
        {
            foreach (OutputInfo item in outputType.Items)
                if (item.Output.Visible)
                    return item.Output;

            return null;
        }

        public Output OpenOrCreateOutput(string name)
        {
            var output = GetOutputType(name);
            if (output == null)
                AddOutputType(name);
            return ShowOutput(name);
        }

        void AddOutputType(string name)
        {
            var textBox = new TextBox();
            textBox.Multiline = true;
            textBox.HideSelection = false;
            textBox.Location = new Point(0, toolStrip1.Height);
            textBox.Size = new Size(this.ClientRectangle.Width, this.ClientRectangle.Height - toolStrip1.Height);
            textBox.ScrollBars = ScrollBars.Both;
            textBox.ReadOnly = true;
            toolTip1.SetToolTip(textBox, "F4 - Navigate to the next file location\nCtrl+F4 - Navigate to the previous file location\nCtrl+DblClick - Navigate to the raw file location (e.g. auto-generated files)");
            textBox.BackColor = Color.White;

            textBox.Font = new Font("Courier New", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox.MouseDoubleClick += textBox_MouseDoubleClick;
            textBox.KeyDown += TextBox_KeyDown;
            textBox.AttachMouseControlledZooming();

            textBox.Visible = true;

            textBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            this.Controls.Add(textBox);

            outputType.Items.Add(new OutputInfo { Output = new Output(textBox), Name = name });
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                (sender as TextBox).SelectAll();
        }

        public Output BuildOutput { get { return GetOutputType(BuildOutputName); } }

        // public Output DebugOutput { get { return GetOutputType(DebugOutputName); } }

        public Output PluginLogOutput { get { return GetOutputType(PluginLogOutputName); } }

        public Output ConsoleOutput { get { return GetOutputType(ConsoleOutputName); } }

        void outputType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (outputType.SelectedItem is OutputInfo)
            {
                var info = (outputType.SelectedItem as OutputInfo);
                ShowOutput(info.Name);
            }
        }

        void toolStripButton1_Click(object sender, EventArgs e)
        {
            var output = GetVisibleOutput();
            if (output != null)
                output.Clear();
        }

        void RefreshControls()
        {
            if (dbgMonitor == null)
            {
                DebugViewBtn.Image = global::CSScriptNpp.Resources.Resources.debug_attach;
                DebugViewBtn.ToolTipText = "Attach to the Debug output stream";
            }
            else
            {
                DebugViewBtn.Image = global::CSScriptNpp.Resources.Resources.debug_detach;
                DebugViewBtn.ToolTipText = "Detach from the Debug output stream";
            }
            DebugViewBtn.Enabled = true;

            if (Config.Instance.LocalDebug)
            {
                debugFilterBtn.Image = global::CSScriptNpp.Resources.Resources.remove_dbg_filter;
                debugFilterBtn.ToolTipText = "Remove 'Local Only' Debug Listener filter";
            }
            else
            {
                debugFilterBtn.Image = global::CSScriptNpp.Resources.Resources.set_dbg_filter;
                debugFilterBtn.ToolTipText = "Apply 'Local Only' Debug Listener filter";
            }
        }

        static Process dbgMonitor;
        public string localDebugPrefix = null;

        public static void Clean()
        {
            if (dbgMonitor != null)
            {
                try
                {
                    dbgMonitor.Kill();
                    dbgMonitor = null;
                }
                catch { }
            }
        }

        static string dbMonPath;

        static string DbMonPath
        {
            get
            {
                //if (dbMonPath == null || !File.Exists(dbMonPath) || !Utils.IsSameTimestamp(Assembly.GetExecutingAssembly().Location, dbMonPath))
                if (dbMonPath == null || !File.Exists(dbMonPath))
                {
                    dbMonPath = Path.Combine(CSScriptHelper.GetScriptTempDir(), "CSScriptNpp\\DbMon.exe");
                    try
                    {
                        var dir = Path.GetDirectoryName(dbMonPath);

                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        File.WriteAllBytes(dbMonPath, Resources.Resources.DbMon); //always try to override existing to ensure the latest version
                        //Utils.SetSameTimestamp(Assembly.GetExecutingAssembly().Location, dbMonPath);
                    }
                    catch { } //it can be already locked (running)
                }
                return dbMonPath;
            }
        }

        void DebugViewBtn_Click(object sender, EventArgs e)
        {
            DebugViewBtn.Enabled = false;

            if (dbgMonitor != null)
            {
                try
                {
                    dbgMonitor.Kill();
                    dbgMonitor = null;
                }
                catch { }
            }
            else
            {
                // var output = this.DebugOutput;

                // if (!Config.Instance.AllowDebugMonitor)
                //     return;

                // ShowOutput(DebugOutputName);

                // Task.Factory.StartNew(() =>
                //     {
                //         foreach (var proc in Process.GetProcessesByName("DbMon"))
                //         {
                //             try
                //             {
                //                 proc.Kill();
                //             }
                //             catch { }
                //         }

                //         var p = new Process();
                //         dbgMonitor = p;
                //         this.InUiThread(RefreshControls);

                //         p.StartInfo.FileName = DbMonPath;
                //         p.StartInfo.CreateNoWindow = true;
                //         p.StartInfo.UseShellExecute = false;
                //         p.StartInfo.RedirectStandardOutput = true;
                //         p.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;

                //         //p.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentUICulture.TextInfo.OEMCodePage);

                //         p.Start();

                //         string[] ignoreLocalDebug = Config.Instance.BlockLocalDebugOutputContaining.Split(new string[] { "{NL}" }, StringSplitOptions.None);

                //         string line = null;
                //         while (null != (line = p.StandardOutput.ReadLine()))
                //         {
                //             if (Config.Instance.LocalDebug)
                //             {
                //                 if (localDebugPrefix != null && line.StartsWith(localDebugPrefix))
                //                 {
                //                     bool ignore = false;
                //                     foreach (var item in ignoreLocalDebug)
                //                         if (line.Contains(item))
                //                         {
                //                             ignore = true;
                //                             break;
                //                         }

                //                     if (!ignore)
                //                         output.WriteLine(line.Substring(localDebugPrefix.Length));
                //                 }
                //             }
                //             else
                //             {
                //                 output.WriteLine(line);
                //             }
                //         }

                //         p.WaitForExit();

                //         if (p.ExitCode == 3)
                //         {
                //             output.Clear();
                //             output.WriteLine("===== Error: There is already another attached instance of the Debug Listener =====");
                //         }
                //         else if (p.ExitCode != 0)
                //         {
                //             output.WriteLine("===== Error: Debug Listener has been detached =====");
                //         }

                //         dbgMonitor = null;
                //         this.InUiThread(RefreshControls);
                //     });
            }
        }

        bool initialised = false;

        void OutputPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (!initialised && this.Visible)
            {
                initialised = true;

                ShowOutput(BuildOutputName);

                RefreshControls();

                System.Diagnostics.Debug.WriteLine("OutputPanel visible changed to -> " + this.Visible);
            }
        }

        void debugFilterBtn_Click(object sender, EventArgs e)
        {
            Config.Instance.LocalDebug = !Config.Instance.LocalDebug;
            RefreshControls();
        }

        void NavigateToFileContent(string file, int line, int column)
        {
            try
            {
                PluginBase.Editor.Open(file);

                var document = PluginBase.GetCurrentDocument();

                document.GrabFocus();
                document.GotoLine(line - 1); //SCI lines are 0-based

                //at this point the caret is at the most left position (col=0)
                var currentPos = document.GetCurrentPos();
                document.GotoPos(currentPos + column - 1);
            }
            catch { }
        }

        static public void NormaliseFileReference(ref string file, ref int line)
        {
            if (!Config.Instance.NavigateToRawCodeOnDblClickInOutput)
            {
                CSScriptIntellisense.StringExtesnions.NormaliseFileReference(ref file, ref line);
            }
        }

        void textBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            NavigateToFileContent((TextBox)sender);
        }

        void NavigateToFileContent(TextBox textBox)
        {
            try
            {
                int caretPosition = textBox.SelectionStart;
                int startLinePosition = textBox.Text.LastIndexOf("\n", caretPosition);
                int endLinePosition = textBox.Text.IndexOf("\n", startLinePosition + 1);
                textBox.SelectionStart = startLinePosition + 1;
                textBox.SelectionLength = endLinePosition - startLinePosition;

                string lineText = textBox.SelectedText;
                int line;
                int column;
                string file;

                if (lineText != null && lineText.ParseAsFileReference(out file, out line, out column))
                {
                    if (!KeyInterceptor.IsPressed(Keys.ControlKey))
                        NormaliseFileReference(ref file, ref line);
                    this.NavigateToFileContent(file, line, column);
                }
            }
            catch { } //it is expected to fail if the line does not contain the file content position spec. This is also the reason for not validating any "IndexOf" results.
        }

        bool reset = false;

        /// <summary>
        /// This is a work around that nasty-nasty defect associated with comboboxes hosted by OutputPanel.
        /// The problem manifests itself in the freshly inserted empty combobox being populated after startup with
        /// exactly these two items:
        /// "English (Great Britain) [!For All Users]"
        /// "English (United States) [!For All Users]"
        /// And this is despite the fact that the user code has NO code that inserts ANY item at all!!!!! It only instantiates
        /// the combobox and places it on the form. That is it.
        ///
        /// This happens on NPP startup in ~50% of cases. Hard to tell who does the item insertion but the user experience
        /// is severely affected. Thus the work around is to clear and repopulate the combobox items when we detect that
        /// the Items content is not what we expect (e.g. Text is none of the Items).
        /// </summary>
        void CheckAndFixCombobox()
        {
            var items = outputType.Items.Cast<OutputInfo>().ToArray();

            if (!reset && items.Where(x => x.Name == outputType.Text).Count() == 0)//  == "" || outputType.Text.Contains("English (Great Britain) [!For All Users]")))
            {
                reset = true;

                var item = outputType.SelectedItem;

                outputType.Items.Clear();
                outputType.Items.AddRange(items);
                outputType.SelectedItem = item;
            }
            else
                reset = false;
        }

        void outputType_DropDown(object sender, EventArgs e)
        {
            CheckAndFixCombobox();
        }

        private void OutputPanel_Shown(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("OutputPanel shown");
        }

        private void OutputPanel_Activated(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("OutputPanel activated");
        }

        private void OutputPanel_Deactivate(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("OutputPanel deactivated");
        }
    }

    public class OutputInfo
    {
        public string Name;
        public Output Output;

        public override string ToString()
        {
            return Name;
        }
    }

    public class Output
    {
        TextBox control;

        public bool IsEmpty
        {
            get
            {
                bool retval = true;
                control.InUiThread(() =>
                {
                    retval = string.IsNullOrEmpty(control.Text);
                }, true);
                return retval;
            }
        }

        public Output SetCaretAtStart()
        {
            control.SelectionStart = 0;
            control.SelectionLength = 0;
            return this;
        }

        public void SelectLineAtPosition(int pos)
        {
            int lineStart = control.Text.LastIndexOf(Environment.NewLine, pos);
            int lineEnd = control.Text.IndexOf(Environment.NewLine, pos);

            if (lineStart == -1)
                lineStart = 0;
            else
                lineStart += Environment.NewLine.Length;

            if (lineEnd == -1)
                lineEnd = control.Text.Length;

            control.SelectionStart = lineStart;
            control.SelectionLength = lineEnd - lineStart;
        }

        public string MoveToNextLine(out int pos)
        {
            pos = -1;

            if (string.IsNullOrEmpty(control.Text))
                return null;

            control.SelectionStart += control.SelectionLength; //clear current selection
            control.SelectionLength = 0;

            pos = control.Text.IndexOf(Environment.NewLine, control.SelectionStart);
            if (pos == -1)
            {
                //the caret might be at the last line
                //so select the first line
                SelectLineAtPosition(0);
                pos = control.SelectionStart;
                return control.SelectedText;
            }

            //move caret to the next line
            pos = pos + Environment.NewLine.Length;
            SelectLineAtPosition(pos);
            pos = control.SelectionStart;
            return control.SelectedText;
        }

        public string MoveToPrevLine(out int pos)
        {
            pos = -1;

            if (string.IsNullOrEmpty(control.Text))
                return null;

            control.SelectionLength = 0; //clear current selection

            pos = control.Text.LastIndexOf(Environment.NewLine, control.SelectionStart);
            if (pos == -1)
            {
                //the caret might be at the first line
                //so select the last line
                SelectLineAtPosition(control.Text.Length);
                pos = control.SelectionStart;
                return control.SelectedText;
            }

            //move caret to the prev line
            pos = pos - Environment.NewLine.Length;
            SelectLineAtPosition(pos);
            pos = control.SelectionStart;
            return control.SelectedText;
        }

        public Output(TextBox control)
        {
            this.control = control;
            this.control.HideSelection = false;
            this.control.WordWrap = false;
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            Flash();
        }

        internal bool Visible
        {
            get { return control.Visible; }
            set
            {
                control.Visible = value;
            }
        }

        public void ScrollToView()
        {
            control.InUiThread(() =>
                {
                    if (control.Text != null && control.Text.Length > 0)
                    {
                        control.SelectionStart = control.Text.Length - 1;
                        control.SelectionLength = 0;
                        control.ScrollToCaret();
                    }
                }, true);
        }

        public Output Clear()
        {
            control.InUiThread(() =>
                {
                    control.Text = null;
                    caretPos = 0;
                }, true);
            return this;
        }

        public Output WriteLine(string text, params object[] args)
        {
            string newText;
            if (string.IsNullOrEmpty(text))
                newText = Environment.NewLine;
            else if (args.Length == 0)
                newText = text + Environment.NewLine;
            else
                newText = string.Format(text, args) + Environment.NewLine;

            return WriteText(newText);
        }

        public Output WriteText(string text)
        {
            control.InUiThread(() =>
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        caretPos += text.Length;
                        control.AppendText(text);
                        EnsureCapacity();
                        ScrollToView();
                    }
                }, true);
            return this;
        }

        public Output Write(string text, params object[] args)
        {
            string newText;
            if (args.Length == 0)
                newText = text + Environment.NewLine;
            else
                newText = string.Format(text, args);

            return WriteText(newText);
        }

        Timer timer = new Timer();
        StringBuilder buffer = new StringBuilder();

        void Flash()
        {
            if (!isBuffered || (buffer.Length > 0 && buffer[buffer.Length - 1] == '\n'))
            {
                control.AppendText(buffer.ToString());
                buffer.Clear();
            }
            timer.Enabled = false;
        }

        void AppendChar(char c)
        {
            buffer.Append(c);
            timer.Enabled = false;
            timer.Enabled = true;
        }

        bool isBuffered = true;
        int caretPos = 0;

        public Output WriteConsoleChar(char c)
        {
            control.InUiThread(() =>
            {
                if (c == '\n')
                {
                    if (!isBuffered)
                    {
                        control.AppendText(Environment.NewLine);
                    }
                    else
                    {
                        AppendChar(c);
                        Flash();
                    }

                    EnsureCapacity();
                    ScrollToView();
                    caretPos = control.Text.Length;
                }
                else if (c == '\r')
                {
                    Flash();
                    if (!isBuffered)
                        for (; caretPos > 0; caretPos--)
                            if (control.Text[caretPos - 1] == '\n')
                                break;
                }
                else
                {
                    if (caretPos == control.Text.Length || isBuffered)
                    {
                        if (!isBuffered)
                            control.AppendText(c.ToString());
                        else
                            AppendChar(c);
                    }
                    else
                    {
                        control.SelectionStart = caretPos;
                        control.SelectionLength = 1;
                        var newChar = c.ToString();
                        if (newChar != control.SelectedText)
                            control.SelectedText = newChar;
                        else
                            control.SelectionLength = 0;
                    }
                    caretPos++;
                }
            }, true);
            return this;
        }

        void EnsureCapacity()
        {
            if (control.Text.Length > Config.Instance.OutputPanelCapacity)
                control.Text.Substring(control.Text.Length - Config.Instance.OutputPanelCapacity);
        }
    }

    public static class ControlExtensions
    {
        public static void InUiThread(this Control control, Action action, bool handleExceptions = false)
        {
            try
            {
                if (control.InvokeRequired)
                    control.Invoke(action);
                else
                    action();
            }
            catch
            {
                if (!handleExceptions)
                    throw;
            }
        }
    }
}