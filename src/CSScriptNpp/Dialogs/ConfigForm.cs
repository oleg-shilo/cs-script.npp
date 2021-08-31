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
            useCS6.Checked = data.UseRoslynProvider;

            installedEngineLocation.Text = CSScriptHelper.SystemCSScriptDir ?? "<not detected>";
            installedEngineLocation.SelectionStart = installedEngineLocation.Text.Length - 1;
            scriptsDir.Text = data.ScriptsDir;

            embeddedEngine.Checked = data.UseEmbeddedEngine;

            restorePanels.Checked = data.RestorePanelsAtStartup;

            RefreshUseCustomLauncherCmd(data.UseCustomLauncher);
            useCustomLauncher.Checked = !data.UseCustomLauncher.IsEmpty() && data.UseCustomLauncher != defaultLauncher;

            if (!data.UseEmbeddedEngine)
            {
                if (data.UseCustomEngine.IsEmpty())
                {
                    installedEngine.Checked = true;
                }
                else
                {
                    customEngine.Checked = true;
                    customEngineLocation.Text = data.UseCustomEngine;
                }
            }
            customSyntaxer.Checked = data.CustomSyntaxer;
            customSyntaxerExe.Text = data.CustomSyntaxerExe;
            syntaxerPort.Text = data.SyntaxerPort.ToString();
            customSyntaxerExe.ReadOnly = !customSyntaxer.Checked;

            cssInstallCmd.Text = CSScriptHelper.InstallCssCmd;
            cssyntaxerInstallCmd.Text = CSScriptHelper.InstallCsSyntaxerCmd;
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
            data.UseRoslynProvider = CSScriptIntellisense.Config.Instance.RoslynIntellisense;
            data.VbSupportEnabled = CSScriptIntellisense.Config.Instance.VbSupportEnabled;

            if (customEngine.Checked)
            {
                data.UseCustomEngine = customEngineLocation.Text;
            }
            else
            {
                data.UseCustomEngine = "";
                CSScriptHelper.SynchAutoclssDecorationSettings(useCS6.Checked);
            }

            data.CustomSyntaxer = customSyntaxer.Checked;
            data.CustomSyntaxerExe = customSyntaxerExe.Text;
            if (int.TryParse(syntaxerPort.Text, out int port))
            {
                data.SyntaxerPort = port;
            }

            Bootstrapper.DeploySyntaxer();
            CSScriptIntellisense.Syntaxer.RestartServer();

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

        void engine_CheckedChanged(object sender, EventArgs e)
        {
            customEngineLocation.ReadOnly = !customEngine.Checked;
        }

        void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string file = CSScriptHelper.GetProjectTemplate();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Thread.Sleep(500);
                    Process.Start(file);
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

        private void customSyntaxer_CheckedChanged(object sender, EventArgs e)
        {
            customSyntaxerExe.ReadOnly = !customSyntaxer.Checked;
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

        private void enableNetCore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (CSScriptHelper.IsCSScriptInstalled && CSScriptHelper.IsCSSyntaxerInstalled)
            {
                // use installed CS-Script for script execution
                installedEngine.Checked = true;

                // use installed syntaxer for Intellisense
                data.UseRoslynProvider = true;
                customSyntaxer.Checked = true;
                customSyntaxerExe.Text = CSScriptHelper.SystemCSSyntaxerDir.PathJoin("syntaxer.exe");

                MessageBox.Show("The changes will take the full effect after restarting Notepad++", "CS-Script");
            }
            else
            {
                var message = "The required services are not fully available or require update:\n";

                if (CSScriptHelper.SystemCSScriptDir.IsEmpty())
                    message += "  CS-Script: not installed\n";
                else
                    message += "  CS-Script: installed\n";

                if (CSScriptHelper.SystemCSSyntaxerDir.IsEmpty())
                    message += "  Syntaxer: not installed\n";
                else
                    message += "  Syntaxer: installed\n";

                message +=
                    "\nDeploy/Update the services from the Update tab and then " +
                    "try to enable .NET Core integration again.";

                MessageBox.Show(message, "CS-Script");

                contentControl.SelectedIndex = 2;
            }
        }
    }
}