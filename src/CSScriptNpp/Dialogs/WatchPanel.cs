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
            Debugger.OnWatchUpdate += Debugger_OnWatchUpdate;
        }

        void content_OnDagDropText(string data)
        {
            content.AddWatchObject(new DbgObject
                {
                    DbgId = "",
                    Name = data,
                    IsExpression = true
                });
        }

        void Debugger_OnWatchUpdate(string data)
        {
            content.SetData(data);
        }
    }
}
