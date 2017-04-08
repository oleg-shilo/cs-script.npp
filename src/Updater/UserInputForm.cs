using CSScriptNpp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Updater
{
    public partial class UserInputForm : Form
    {
        public static string GetDistro()
        {
            using (var form = new UserInputForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                    return form.distroFile;
                else
                    return null;
            }
        }

        public UserInputForm()
        {
            InitializeComponent();
        }

        string distroFile = null;

        private void okBtn_Click(object sender, EventArgs e)
        {
            distroFile = url.Text;
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void url_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                okBtn.PerformClick();
            else if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void UserInputForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }
    }
}