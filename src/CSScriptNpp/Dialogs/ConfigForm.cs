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
        private const string defaultLauncher = "<script engine>";
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

            embeddedEngine.Checked = data.UseEmbeddedEngine;
            customLocationBtn.Checked = !embeddedEngine.Checked;

            restorePanels.Checked = data.RestorePanelsAtStartup;

            RefreshUseCustomLauncherCmd(data.UseCustomLauncher);

            customEngineLocation.Text = data.CustomEngineAsm;
            customSyntaxerExe.Text = data.CustomSyntaxerAsm;
            syntaxerPort.Text = data.CustomSyntaxerPort.ToString();

            if (customEngineLocation.Text.IsEmpty() && CSScriptHelper.IsCSScriptInstalled)
                customEngineLocation.Text = CSScriptHelper.SystemCSScriptDir.PathJoin("cscs.dll");

            if (customSyntaxerExe.Text.IsEmpty() && CSScriptHelper.IsCSSyntaxerInstalled)
                customSyntaxerExe.Text = CSScriptHelper.SystemCSSyntaxerDir.PathJoin("syntaxer.dll");

            cssInstallCmd.Text = CSScriptHelper.InstallCssCmd;
            cssyntaxerInstallCmd.Text = CSScriptHelper.InstallCsSyntaxerCmd;

            customLocationBtn_CheckedChanged(null, null);
        }

        bool skipSavingConfig = false;

        private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            panel.OnClosing();

            data.CheckUpdatesOnStartup = checkUpdates.Checked;
            data.UseEmbeddedEngine = embeddedEngine.Checked;
            data.RestorePanelsAtStartup = restorePanels.Checked;
            data.ScriptsDir = scriptsDir.Text;
            //data.UseRoslynProvider = useCS6.Checked;
            //all Roslyn individual config values are merged into RoslynIntellisense;
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

        private void ConfigForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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

        void useCustomLauncher_CheckedChanged(object sender, EventArgs e)
        {
            RefreshUseCustomLauncherCmd();
        }

        static string useCustomLauncherCmdCache = null;

        void RefreshUseCustomLauncherCmd(string launcherPath = null)
        {
            if (!launcherPath.IsEmpty())
                useCustomLauncherCmd.Text =
                useCustomLauncherCmdCache = launcherPath;

            this.useCustomLauncherCmd.Enabled = this.useCustomLauncher.Checked;

            if (this.useCustomLauncher.Checked)
            {
                useCustomLauncherCmd.Text = launcherPath ?? useCustomLauncherCmdCache;
            }
            else
            {
                if (!useCustomLauncherCmd.Text.IsEmpty())
                    useCustomLauncherCmdCache = useCustomLauncherCmd.Text;
                useCustomLauncherCmd.Text = defaultLauncher;
            }
        }

        private void update_Click(object sender, EventArgs e)
        {
            Dispatcher.Schedule(300, () =>
            {
                using (var dialog = new UpdateOptionsPanel(Distro.FromFixedLocation(customUpdateUrl.Text)))
                    dialog.ShowModal();
            });

            Close();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://github.com/oleg-shilo/cs-script.npp/wiki/Deploy-CS-Script");
            }
            catch { }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://github.com/oleg-shilo/cs-script.npp/wiki/Deploy-Syntaxer");
            }
            catch { }
        }

        static void InstallDependencies(bool engineOnly = false)
        {
            var batchFileContent = new List<string>();
            if (!CSScriptHelper.IsChocoInstalled)
            {
                batchFileContent.Add("powershell Set-ExecutionPolicy Bypass -Scope Process -Force;");
                batchFileContent.Add("powershell iex((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))");
            }

            if (engineOnly)
                batchFileContent.Add($"powershell {CSScriptHelper.InstallCssCmd}");
            else
                // installing syntaxer will auto-install cs-script as a dependency
                batchFileContent.Add($"powershell {CSScriptHelper.InstallCsSyntaxerCmd}");

            if (batchFileContent.Count == 1)
                InstallDependenciesDialog.Execute(batchFileContent.First());
            else
                InstallDependenciesDialog.ShowDialog(string.Join(Environment.NewLine, batchFileContent));
        }

        public void deployCSScript_Click(object sender, EventArgs e) => InstallDependencies(engineOnly: true);

        private void deploySyntaxer_Click(object sender, EventArgs e) => InstallDependencies(engineOnly: false);

        private void autodetect_Click(object sender, EventArgs e)
        {
            if (CSScriptHelper.IsCSScriptInstalled && CSScriptHelper.IsCSSyntaxerInstalled)
            {
                customEngineLocation.Text = CSScriptHelper.SystemCSScriptDir.PathJoin("cscs.dll");
                customSyntaxerExe.Text = CSScriptHelper.SystemCSSyntaxerDir.PathJoin("syntaxer.dll");
            }
            else
            {
                string error = "The following dependencies could not be found:\n\n";
                if (!CSScriptHelper.IsCSScriptInstalled)
                    error += "CS-Script\n";
                if (!CSScriptHelper.IsCSSyntaxerInstalled)
                    error += "Syntaxer\n";

                error += "\nYou can try to install them from the `Update` tab of this dialog";
                MessageBox.Show(error, "CS-Script");
            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {
        }

        private void customLocationBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (embeddedEngine.Checked == customLocationBtn.Checked)
                embeddedEngine.Checked = !customLocationBtn.Checked;

            syntaxerPort.Enabled =
            autoDetectBtn.Enabled =
            customSyntaxerExe.Enabled =
            customEngineLocation.Enabled = customLocationBtn.Checked;
        }

        private void Explore_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", Runtime.dependenciesDirRoot);
            }
            catch { }
        }

        private void embeddedEngine_CheckedChanged(object sender, EventArgs e)
        {
            if (customLocationBtn.Checked == embeddedEngine.Checked)
                customLocationBtn.Checked = !embeddedEngine.Checked;
        }
    }
}