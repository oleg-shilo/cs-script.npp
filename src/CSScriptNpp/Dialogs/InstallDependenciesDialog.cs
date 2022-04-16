using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSScriptNpp.Dialogs
{
    public partial class InstallDependenciesDialog : Form
    {
        public InstallDependenciesDialog()
        {
            InitializeComponent();
        }

        public static DialogResult ShowDialog(string cmd)
        {
            var dialog = new InstallDependenciesDialog();
            dialog.cmdTextBox.Text = cmd;
            return dialog.ShowDialog();
        }

        void installButton_Click(object sender, EventArgs e) => Execute(cmdTextBox.Text);

        private void helpButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/oleg-shilo/cs-script.npp/wiki/Deploy-Dependencies");
            }
            catch { }
        }

        internal static void Execute(string command)
        {
            var batchFile = Path.GetTempPath().PathJoin("cs-script.npp.dep-install.cmd");
            try
            {
                File.WriteAllText(batchFile, command + Environment.NewLine + "pause");

                MessageBox.Show("You are about to install CS-Script component(s).\n" +
                                "The change will take full effect after Notepad++ is restarted", "CS-Script");

                var p = new Process();
                p.StartInfo.FileName = batchFile;
                p.StartInfo.Verb = "runas";
                p.Start();
            }
            catch
            {
            }
        }
    }
}