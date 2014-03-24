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
    public partial class AutoWatchPanel : Form
    {
        DebugObjectsPanel content;
        public AutoWatchPanel()
        {
            InitializeComponent();
            content = new DebugObjectsPanel();
            content.TopLevel = false;
            content.FormBorderStyle = FormBorderStyle.None;
            content.Parent = this;
            this.Controls.Add(content);
            content.Dock = DockStyle.Fill;
            content.Visible = true;
        }

        public void SetData(string data)
        {
            content.SetData(data);
        }
    }
}
