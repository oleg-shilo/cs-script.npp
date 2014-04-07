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
            content.OnDagDropText += content_OnDagDropText;
            content.OnEditCellComplete += content_OnEditCellComplete;
            Debugger.OnWatchUpdate += Debugger_OnWatchUpdate;
        }

        void content_OnEditCellComplete(int column, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(oldValue) && !string.IsNullOrEmpty(newValue))
                Debugger.AddWatch(newValue);
            else if (!string.IsNullOrEmpty(oldValue) && string.IsNullOrEmpty(newValue))
                Debugger.RemoveWatch(oldValue);
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
            content.AddWatchExpression(Utils.GetStatementAtCaret());
        }

        private void deleteExpressionBtn_Click(object sender, EventArgs e)
        {
            content.DeleteSelected();
        }

        private void deleteAllExpressionsBtn_Click(object sender, EventArgs e)
        {
            content.ClearWatchExpressions();
        }
    }
}
