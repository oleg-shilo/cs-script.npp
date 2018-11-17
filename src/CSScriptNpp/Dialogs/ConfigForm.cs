using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSScriptIntellisense.Interop;
using CSScriptNpp.Dialogs;

namespace CSScriptNpp
{
    public partial class ConfigForm : Form
    {
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
            useCustomLauncher.Checked = !data.UseCustomLauncher.IsEmpty();

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
        }

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

            if (this.useCustomLauncher.Checked)
                data.UseCustomLauncher = useCustomLauncherCmd.Text;
            else
                data.UseCustomLauncher = "";

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
                useCustomLauncherCmd.Text = "<script engine>";
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
    }
}