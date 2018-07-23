using CSScriptIntellisense.Interop;
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
            content.IsEvaluatable = true;
            content.OnPinClicked += content_OnPinClicked;
            content.ClearWatchExpressions();
            content.OnDagDropText += content_OnDagDropText;
            content.OnEditCellComplete += content_OnEditCellComplete;
            content.ReevaluateRequest += Content_ReevaluateRequest;
            Debugger.OnWatchUpdate += Debugger_OnWatchUpdate;
            Debugger.OnNotification += (message) =>
            {
                if (message.StartsWith("source=>")) // advancing in the source code on the debugger "step next"
                    ReevaluateAll();
            };

            Debugger.OnDebuggerStateChanged += DebuggerServer_OnDebuggerStateChanged;
        }

        private void ReevaluateAll()
        {

            if (Debugger.IsInBreak)
                this.InUiThread(
                () =>
                {
                    // too harsh. ideally need some more gentle update
                    // content.ResetAll();

                    var items = content.GetItems();

                    var subItems = items.Where(i => i.Name != i.Path && (i.Parent.IsCollection || i.IsArray));
                    items = items.Except(subItems).ToArray();
                    // need to clear only collections
                    items.ForEach((DbgObject item) =>
                    {
                        item.Children = null;
                        item.IsExpanded = false;
                        Debugger.RemoveWatch(item.Path);
                        Debugger.AddWatch(item.Path);
                    });

                    // subitems are not true watch 
                    // items but their dynamically explored children.
                    // tracking them on debug steps is extremely difficult. 
                    // So close them and reopen as needed. 
                    // If a child item needs to be watched so pin it.
                    content.RemoveSubItems(collectionsOnly:true);
                });
        }

        private void Content_ReevaluateRequest(DbgObject context)
        {
            Debugger.RemoveWatch(context.Path);
            Debugger.AddWatch(context.Path);
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
                bool evalRefreshRequest = (newValue != null && newValue.IsInvokeExpression());

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
            this.InUiThread(() =>
                content.UpdateData(data));
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

        private void reevaluateAllButton_Click(object sender, EventArgs e)
        {
            ReevaluateAll();
        }
    }
}
