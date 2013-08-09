using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public partial class OutputPanel : Form
    {
        public const string BuildOutputName = "Build";
        public const string ConsoleOutputName = "Console";
        public const string DebugOutputName = "Debug";

        public OutputPanel()
        {
            InitializeComponent();

            var cb = new CheckBox();
            cb.Text = "Intercept StdOut";
            cb.Checked = Config.Instance.InterceptConsole;
            cb.CheckStateChanged += (s, ex) => Config.Instance.InterceptConsole = cb.Checked;
            toolStrip1.Items.Insert(3, new ToolStripControlHost(cb) { ToolTipText= "Check to redirect the console output to the output panel" });

            AddOutputType(BuildOutputName);
            AddOutputType(DebugOutputName);
            AddOutputType(ConsoleOutputName);
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

        public void AttachDebuger()
        {
            if (dbgMonitor == null)
            {
                DebugViewBtn.PerformClick();
            }
        }

        public Output Show(Output output)
        {
            Output retval = null;

            this.InUiThread(() =>
                {
                    foreach (OutputInfo item in outputType.Items)
                    {
                        if (item.Output == output)
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

        public Output GetCurrentShow()
        {
            Output retval = null;

            foreach (OutputInfo item in outputType.Items)
            {
                if (item.Output.Visible)
                    retval = item.Output;
            }

            return retval;
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

        void AddOutputType(string name)
        {
            var textBox = new TextBox();
            textBox.Multiline = true;
            textBox.Location = new Point(0, toolStrip1.Height);
            textBox.Size = new Size(this.ClientRectangle.Width, this.ClientRectangle.Height - toolStrip1.Height);
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Font = new Font("Courier New", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox.MouseDoubleClick += textBox_MouseDoubleClick;

            textBox.Visible = true;
            textBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            this.Controls.Add(textBox);

            outputType.Items.Add(new OutputInfo { Output = new Output(textBox), Name = name });
        }

        public Output BuildOutput { get { return GetOutputType(BuildOutputName); } }

        public Output DebugOutput { get { return GetOutputType(DebugOutputName); } }

        public Output ConsoleOutput { get { return GetOutputType(ConsoleOutputName); } }

        private void outputType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (outputType.SelectedItem is OutputInfo)
            {
                var info = (outputType.SelectedItem as OutputInfo);
                ShowOutput(info.Name);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
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
        public string localDebugPreffix = null;
        public string[] ignoreLocalDebug = new[] { "SHIMVIEW: ShimInfo(Complete)" }; //some system dll injection

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

        private void DebugViewBtn_Click(object sender, EventArgs e)
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
                var output = this.DebugOutput;

                ShowOutput(DebugOutputName);

                Task.Factory.StartNew(() =>
                    {
                        foreach (var proc in Process.GetProcessesByName("DbMon"))
                        {
                            try
                            {
                                proc.Kill();
                            }
                            catch { }
                        }

                        string dbmon = @"E:\Galos\Projects\CS-Script.Npp\DbMon\bin\Debug\DbMon.exe";

                        var p = new Process();
                        dbgMonitor = p;
                        this.InUiThread(RefreshControls);

                        p.StartInfo.FileName = dbmon;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.CreateNoWindow = true;

                        p.Start();


                        string line = null;
                        while (null != (line = p.StandardOutput.ReadLine()))
                        {

                            if (Config.Instance.LocalDebug)
                            {
                                if (localDebugPreffix != null && line.StartsWith(localDebugPreffix))
                                {
                                    bool ignore = false;
                                    foreach (var item in ignoreLocalDebug)
                                        if (line.Contains(item))
                                        {
                                            ignore = true;
                                            break;
                                        }

                                    if (!ignore)
                                        output.WriteLine(line.Substring(localDebugPreffix.Length));
                                }
                            }
                            else
                            {
                                output.WriteLine(line);
                            }
                        }
                        p.WaitForExit();

                        if (p.ExitCode == 3)
                        {
                            output.Clear();
                            output.WriteLine("===== Error: There is already another attached instance of the Debug Listener =====");
                        }
                        else if (p.ExitCode != 0)
                        {
                            output.WriteLine("===== Error: Debug Listener has been detached =====");
                        }

                        dbgMonitor = null;
                        this.InUiThread(RefreshControls);
                    });
            }
        }

        bool initialised = false;
        private void OutputPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (!initialised && this.Visible)
            {
                initialised = true;

                ShowOutput(BuildOutputName);

                RefreshControls();
            }
        }

        private void debugFilterBtn_Click(object sender, EventArgs e)
        {
            Config.Instance.LocalDebug = !Config.Instance.LocalDebug;
            RefreshControls();
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
                int pos = textBox.SelectedText.IndexOf(":", 3);

                string fileSpec = textBox.SelectedText.Substring(0, pos);
                pos = fileSpec.LastIndexOf("(");
                string file = fileSpec.Substring(0, pos);
                string caretSpec = fileSpec.Substring(pos + 1, fileSpec.Length - (pos + 1) - 1);

                string[] parts = caretSpec.Split(',');
                string line = parts[0];
                string column = parts[1];

                Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_DOOPEN, 0, file);
                Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
                Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GOTOLINE, int.Parse(line) - 1, 0); //SCI lines are 0-based

                //at this point the caret is at the most left position (col=0)
                int currentPos = (int)Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);
                Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GOTOPOS, currentPos + int.Parse(column) - 1, 0); //SCI columns are 0-based
            }
            catch { } //it is expected to fail if the line does not contain the file content position spec. This is also the reason for not validating any "IndexOf" results.
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
        public Output(TextBox control)
        {
            this.control = control;
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
               }, true);
            return this;
        }

        public Output WriteLine(string text, params object[] args)
        {
            control.InUiThread(() =>
               {
                   if (string.IsNullOrEmpty(text))
                       control.Text += Environment.NewLine;
                   else
                       control.Text += string.Format(text, args) + Environment.NewLine;
                   EnsureCapacity(); 
                   ScrollToView();
               }, true);
            return this;
        }

        public Output Write(string text, params object[] args)
        {
            control.InUiThread(() =>
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        control.Text += string.Format(text, args);
                        EnsureCapacity();
                        ScrollToView();
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