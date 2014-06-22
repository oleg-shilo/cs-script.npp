using System;
using System.Threading;
using System.Windows.Forms;

namespace CSScriptNpp.Dialogs
{
    public partial class QuickWatchPanel : Form
    {
        static QuickWatchPanel Instance;
        static bool singleton = true;
        static bool ownedByNpp = true;
        static public void PopupDialog()
        {
            if (singleton && Instance != null)
            {
                Win32.SetForegroundWindow(Instance.Handle);
                Instance.SetExpression(Utils.GetStatementAtCaret());
                return;
            }

            if (ownedByNpp)
            {
                var dialog = new QuickWatchPanel();
                Instance = dialog;
                string expression = Utils.GetStatementAtCaret();

                dialog.SetExpression(expression)
                      .SetAutoRefreshAvailable(Config.Instance.QuickViewAutoRefreshAvailable);

                var nativeWindow = new NativeWindow();
                nativeWindow.AssignHandle(Plugin.NppData._nppHandle);

                dialog.Show(nativeWindow);
            }
            else
            {
                if (singleton && Instance != null)
                {
                    Win32.SetForegroundWindow(Instance.Handle);
                    return;
                }
                else
                {
                    var t = new Thread(() =>
                                {
                                    using (QuickWatchPanel dialog = new QuickWatchPanel())
                                    {
                                        Instance = dialog;
                                        string expression = Npp.GetSelectedText();

                                        if (string.IsNullOrWhiteSpace(expression))
                                            expression = CSScriptIntellisense.Npp.GetStatementAtPosition();

                                        dialog.SetExpression(expression);

                                        dialog.ShowDialog();
                                    }
                                    Instance = null;
                                });
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                }
            }
        }

        QuickWatchPanel SetExpression(string data)
        {
            textBox1.Text = data;
            Reevaluate();
            return this;
        }

        QuickWatchPanel SetAutoRefreshAvailable(bool available)
        {
            if (autoupdate.Visible != available)
            {
                autoupdate.Visible = available;
                if (available)
                    contentPanel.Height -= 16;
                else
                    contentPanel.Height += 16;
            }
            return this;
        }

        DebugObjectsPanel content;

        public QuickWatchPanel()
        {
            InitializeComponent();
            content = new DebugObjectsPanel();
            content.TopLevel = false;
            content.FormBorderStyle = FormBorderStyle.None;
            content.Parent = this;
            contentPanel.Controls.Add(content);
            content.Dock = DockStyle.Fill;
            content.Visible = true;

            Debugger.OnFrameChanged += Debugger_OnFrameChanged;
            Debugger.OnDebuggerStateChanged += Debugger_OnDebuggerStateChanged;
        }

        void Debugger_OnDebuggerStateChanged()
        {
            this.InUiThread(content.Refresh, true);
        }

        void Debugger_OnFrameChanged()
        {
            this.InUiThread(() =>
                {
                    timer1.Interval = 800;
                    timer1.Enabled = autoupdate.Checked;
                }, true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Reevaluate();
            timer1.Enabled = false;
        }

        void Reevaluate()
        {
            if (Debugger.IsInBreak)
            {
                string data = Debugger.Invoke("resolve", textBox1.Text.Trim());
                if (data != null)
                    content.SetData(data);
            }
            else
                content.Refresh();
        }

        void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                Reevaluate();
        }

        private void reevalBtn_Click(object sender, System.EventArgs e)
        {
            Reevaluate();
        }

        private void QuickWatchPanel_FormClosed(object sender, FormClosedEventArgs e)
        {
            Debugger.OnFrameChanged -= Debugger_OnFrameChanged;
            Debugger.OnDebuggerStateChanged -= Debugger_OnDebuggerStateChanged;
            Instance = null;
        }
    }
}