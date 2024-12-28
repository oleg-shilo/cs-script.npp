using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSScriptIntellisense.Interop;
using CSScriptNpp.Dialogs;

namespace CSScriptNpp
{
    public partial class ConfigForm : Form
    {
        const string defaultLauncher = "<script engine>";
        Config data;

        CSScriptIntellisense.ConfigForm panel;

        public ConfigForm()
        {
            InitializeComponent();
        }

        public ConfigForm(Config data)
        {
            this.data = data;

            InitializeComponent();

            panel = new CSScriptIntellisense.ConfigForm(CSScriptIntellisense.Config.Instance);
            generalPage.Controls.Add(panel.ContentPanel);

            checkUpdates.Checked = data.CheckUpdatesOnStartup;

            scriptsDir.Text = data.ScriptsDir;

            // embeddedEngine.Checked = data.UseEmbeddedEngine;
            // customLocationBtn.Checked = !embeddedEngine.Checked;

            restorePanels.Checked = data.RestorePanelsAtStartup;

            customEngineLocation.Text = data.CustomEngineAsm;
            customSyntaxerExe.Text = data.CustomSyntaxerAsm;
            syntaxerPort.Text = data.CustomSyntaxerPort.ToString();

            // if (customEngineLocation.Text.IsEmpty() && CSScriptHelper.IsCSScriptInstalled)
            //     customEngineLocation.Text = CSScriptHelper.SystemCSScriptDir.PathJoin("cscs.dll");

            // if (customSyntaxerExe.Text.IsEmpty() && CSScriptHelper.IsCSSyntaxerInstalled)
            //     customSyntaxerExe.Text = CSScriptHelper.SystemCSSyntaxerDir.PathJoin("syntaxer.dll");

            cssInstallCmd.Text = CSScriptHelper.InstallCssDotnetCmd;
            deployCSScript.Text = CSScriptHelper.IsCSScriptInstalled ? "Update" : "Install";
            cssyntaxerInstallCmd.Text = CSScriptHelper.InstallCsSyntaxerDotnetCmd;
            deploySyntaxer.Text = CSScriptHelper.IsCSSyntaxerInstalled ? "Update" : "Install";

            UpdateStatus();

            // customLocationBtn_CheckedChanged(null, null);
        }

        void UpdateStatus()
        {
            statusLbl.Text = CSScriptHelper.Integration.IsCssIntegrated() ? "Status: integrated" : "Status: not integrated";
        }

        bool skipSavingConfig = false;

        void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            panel.OnClosing();

            data.CheckUpdatesOnStartup = checkUpdates.Checked;
            data.UseEmbeddedEngine = embeddedEngine.Checked;
            data.RestorePanelsAtStartup = restorePanels.Checked;
            data.ScriptsDir = scriptsDir.Text;
            data.VbSupportEnabled = CSScriptIntellisense.Config.Instance.VbSupportEnabled;

            data.CustomEngineAsm = customEngineLocation.Text;
            data.CustomSyntaxerAsm = customSyntaxerExe.Text;

            if (int.TryParse(syntaxerPort.Text, out int port))
            {
                data.CustomSyntaxerPort = port;
            }

            Runtime.Init();

            if (this.useCustomLauncher.Checked)
                data.UseCustomLauncher = useCustomLauncherCmd.Text;
            else
                data.UseCustomLauncher = "";

            if (!skipSavingConfig)
                Config.Instance.Save();
        }

        void ConfigForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string file = Config.Instance.GetFileName();
            Config.Instance.Save();
            skipSavingConfig = true;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Thread.Sleep(500);
                    DateTime timestamp = File.GetLastWriteTimeUtc(file);
                    Process.Start("notepad.exe", file).WaitForExit();
                    if (File.GetLastWriteTimeUtc(file) != timestamp)
                        Config.Instance.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: \n" + ex.ToString(), "Notepad++");
                }
            });

            Close();
        }

        void update_Click(object sender, EventArgs e)
        {
            Close();
            Task.Run(() =>
            {
                Thread.Sleep(300);

                var distro = Distro.FromFixedLocation(customUpdateUrl.Text);
                using (var dialog = new UpdateOptionsPanel(distro))
                {
                    dialog.ShowModal();
                }
            });
        }

        void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://github.com/oleg-shilo/cs-script.npp/wiki/Deploy-CS-Script");
            }
            catch { }
        }

        void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://github.com/oleg-shilo/cs-script.npp/wiki/Deploy-Syntaxer");
            }
            catch { }
        }

        static void InstallDependencies(string command)
        {
            var batchFileContent = new List<string>();

            // if (!CSScriptHelper.IsChocoInstalled)
            // {
            //     batchFileContent.Add("powershell Set-ExecutionPolicy Bypass -Scope Process -Force;");
            //     batchFileContent.Add("powershell iex((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))");
            // }

            batchFileContent.Add($"powershell {command}");

            if (batchFileContent.Count == 1)
                InstallDependenciesDialog.Execute(batchFileContent.First());
            else
                InstallDependenciesDialog.ShowDialog(string.Join(Environment.NewLine, batchFileContent));
        }

        public void deployCSScript_Click(object sender, EventArgs e) => InstallDependencies(CSScriptHelper.InstallCssDotnetCmd);

        void deploySyntaxer_Click(object sender, EventArgs e) => InstallDependencies(CSScriptHelper.InstallCsSyntaxerDotnetCmd);

        void autodetectCSS_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            CSScriptHelper.Integration.IntegrateCSScript();
            Cursor.Current = Cursors.Default;
            // if (CSScriptHelper.IsCSScriptInstalled)
            //     customEngineLocation.Text = CSScriptHelper.SystemCSScriptDir.PathJoin("cscs.dll");

            // if (CSScriptHelper.IsCSSyntaxerInstalled)
            //     customSyntaxerExe.Text = CSScriptHelper.SystemCSSyntaxerDir.PathJoin("syntaxer.dll");

            if (CSScriptHelper.Integration.IsCssIntegrated())
            {
                customSyntaxerExe.Text = Runtime.syntaxer_asm;
                customEngineLocation.Text = Runtime.cscs_asm;
                UpdateStatus();
                CSScriptHelper.Integration.ShowIntegrationInfo();
                Close();
            }
            else
            {
                string error = "Could not find some CS-Script tools (script engine or syntaxer).\n";
                error += "\nYou can try to install them from the `Update` tab of this dialog";
                MessageBox.Show(error, "CS-Script");
                CSScriptHelper.Integration.ShowIntegrationWarning();
                Close();
            }
        }

        void customLocationBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (embeddedEngine.Checked == customLocationBtn.Checked)
                embeddedEngine.Checked = !customLocationBtn.Checked;

            syntaxerPort.Enabled =
            autoDetectBtn.Enabled =
            customSyntaxerExe.Enabled =
            customEngineLocation.Enabled = customLocationBtn.Checked;
        }

        void Explore_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", Runtime.dependenciesDirRoot);
            }
            catch { }
        }

        void embeddedEngine_CheckedChanged(object sender, EventArgs e)
        {
            if (customLocationBtn.Checked == embeddedEngine.Checked)
                customLocationBtn.Checked = !embeddedEngine.Checked;
        }

        void autodetectSyntaxer_Click(object sender, EventArgs e)
        {
        }

        private void customEngineLocation_TextChanged(object sender, EventArgs e)
        {
        }

        private void label8_Click(object sender, EventArgs e)
        {
        }
    }
}