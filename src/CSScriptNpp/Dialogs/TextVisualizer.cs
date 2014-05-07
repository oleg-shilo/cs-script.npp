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
    public partial class TextVisualizer : Form
    {
        public TextVisualizer(string expression, string value)
        {
            InitializeComponent();
            expressionCtrl.Text = "Expression: " + expression;
            valueCtrl.Text = value;
            wordWrap.Checked = Config.Instance.WordWrapInVisualizer;
        }

        private void wordWrap_CheckedChanged(object sender, EventArgs e)
        {
            valueCtrl.WordWrap = wordWrap.Checked;
            Config.Instance.WordWrapInVisualizer = wordWrap.Checked;
            Config.Instance.Save();
        }
    }
}
