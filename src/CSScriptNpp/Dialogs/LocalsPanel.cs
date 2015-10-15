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
    public partial class LocalsPanel : Form
    {
        DebugObjectsPanel content;
        public LocalsPanel()
        {
            InitializeComponent();
            content = new DebugObjectsPanel();
            content.TopLevel = false;
            content.FormBorderStyle = FormBorderStyle.None;
            content.Parent = this;
            this.Controls.Add(content);
            content.Dock = DockStyle.Fill;
            content.Visible = true;
            content.OnEditCellStart += Content_OnEditCellStart;
            content.OnEditCellComplete += Content_OnEditCellComplete;        

            Debugger.OnWatchUpdate += Debugger_OnWatchUpdate;
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

        public void SetData(string data)
        {
            content.SetData(data);
        }
    }
}
