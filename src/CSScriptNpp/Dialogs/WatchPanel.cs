using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSScriptNpp.Dialogs
{
    public partial class WatchPanel : Form
    {
        DebugObjectsPanel content;
        public WatchPanel()
        {
            InitializeComponent();
            content = new DebugObjectsPanel();
            content.TopLevel = false;
            content.FormBorderStyle = FormBorderStyle.None;
            content.Parent = this;
            contentPanel.Controls.Add(content);
            content.Dock = DockStyle.Fill;
            content.Visible = true;
            content.IsReadOnly = false;
            content.IsPinnable = true;
            content.OnPinClicked += content_OnPinClicked;
            content.ClearWatchExpressions();
            content.OnDagDropText += content_OnDagDropText;
            content.OnEditCellComplete += content_OnEditCellComplete;
            Debugger.OnWatchUpdate += Debugger_OnWatchUpdate;
            DebuggerServer.OnDebuggerStateChanged += DebuggerServer_OnDebuggerStateChanged;
        }

        void DebuggerServer_OnDebuggerStateChanged()
        {
            if (!Debugger.IsInBreak)
            {
                content.InvalidateExpressions();
            }
        }

        void content_OnPinClicked(DbgObject dbgObject)
        {
            content.AddWatchExpression(dbgObject.Path);
        }

        void content_OnEditCellComplete(int column, string oldValue, string newValue, DbgObject context, ref bool cancel)
        {
            if (column == 0) //change watch variable name
            {
                bool evalRefreshRequest = (newValue != null && newValue.Contains("("));

                if (oldValue != newValue || evalRefreshRequest)
                {
                    if (!string.IsNullOrEmpty(oldValue))
                        Debugger.RemoveWatch(oldValue);

                    if (!string.IsNullOrEmpty(newValue))
                        Debugger.AddWatch(newValue);
                }
            }
            else if (column == 1) //set value
            {
                Debugger.InvokeResolve("resolve", context.Name + "=" + newValue.Trim());
                cancel = true; //debugger will send the update with the fresh actual value
            }
        }

        void content_OnDagDropText(string data)
        {
            content.AddWatchExpression(data);
        }

        void Debugger_OnWatchUpdate(string data)
        {
            content.UpdateData(data);
        }

        private void addExpressionBtn_Click(object sender, EventArgs e)
        {
            content.StartAddWatch();
        }

        private void deleteExpressionBtn_Click(object sender, EventArgs e)
        {
            content.DeleteSelected();
        }

        private void deleteAllExpressionsBtn_Click(object sender, EventArgs e)
        {
            content.ClearWatchExpressions();
        }

        private void addAtCaretBtn_Click(object sender, EventArgs e)
        {
            content.AddWatchExpression(Utils.GetStatementAtCaret());
        }
    }
}
