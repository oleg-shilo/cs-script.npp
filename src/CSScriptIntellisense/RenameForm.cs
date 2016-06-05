using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSScriptIntellisense
{
    public partial class RenameForm : Form
    {
        public RenameForm(string from)
        {
            InitializeComponent();
            toTextBox.Text =
            fromTextBox.Text = from;
            toTextBox.SelectionStart = 0;
            toTextBox.SelectionLength = from.Length;
        }

        private void RenameForm_Deactivate(object sender, EventArgs e)
        {
            Close();
        }

        private void RenameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.KeyCode == Keys.Return)
            {
                RenameTo = toTextBox.Text;
                Close();
            }
        }

        public string RenameTo { get; set; } = "";

        private void toTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //if (e.KeyCode == Keys.Return)
            //{
            //    RenameTo = toTextBox.Text;
            //    Close();
            //}
        }
    }
}
