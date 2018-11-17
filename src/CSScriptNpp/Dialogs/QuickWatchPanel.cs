using CSScriptIntellisense;
using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Linq;
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

            var document = Npp.GetCurrentDocument();

            if (ownedByNpp)
            {
                var dialog = new QuickWatchPanel();
                Instance = dialog;
                //string expression = Utils.GetStatementAtCaret();
                string expression = document.GetSelectedText();

                dialog.SetExpression(expression)
                      .SetAutoRefreshAvailable(Config.Instance.QuickViewAutoRefreshAvailable);

                var nativeWindow = new NativeWindow();
                nativeWindow.AssignHandle(Npp.Editor.Handle);

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
                                        string expression = document.GetSelectedText();

                                        if (string.IsNullOrWhiteSpace(expression))
                                            expression = Npp.GetCurrentDocument().GetStatementAtPosition();

                                        dialog.SetExpression(expression);

                                        dialog.ShowModal();
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
            expressionBox.Text = data;
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
            content.OnEditCellComplete += Content_OnEditCellComplete;
            content.OnEditCellStart += Content_OnEditCellStart;

            Debugger.OnWatchUpdate += Debugger_OnWatchUpdate;
            Debugger.OnFrameChanged += Debugger_OnFrameChanged;
            DebuggerServer.OnDebuggerStateChanged += Debugger_OnDebuggerStateChanged;
        }

        void Content_OnEditCellStart(int column, string value, DbgObject context, ref bool cancel)
        {
            if (column != 1)
                cancel = true;
        }

        void Content_OnEditCellComplete(int column, string oldValue, string newValue, DbgObject context, ref bool cancel)
        {
            if (column == 1) //set value
            {
                Debugger.InvokeResolve("resolve", context.Name + "=" + newValue.Trim());
                cancel = true; //debugger will send the update with the fresh actual value
            }
        }

        void Debugger_OnWatchUpdate(string data)
        {
            content.UpdateData(data);
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
                string expression = expressionBox.Text.NormalizeExpression();
                string data = Debugger.InvokeResolve("resolve", expression);

                bool repopulate = true;

                if (expression.IsSetExpression())
                {
                    string name = expression.Split('=').First().Trim();

                    if (content.FindDbgObject(name) != null)
                        repopulate = false; //let watch notification to be received if it is a set expression
                }

                if (repopulate)
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