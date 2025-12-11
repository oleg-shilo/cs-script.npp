using System;
using System.Diagnostics;
using System.IO;
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

        void helpButton_Click(object sender, EventArgs e)
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

                var p = new Process();
                p.StartInfo.FileName = batchFile;
                p.StartInfo.Verb = command.Contains("choco ") ? "runas" : "";
                p.Start();
            }
            catch
            {
            }
        }
    }
}