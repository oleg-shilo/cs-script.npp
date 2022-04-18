using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSScriptIntellisense;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace CSScriptNpp.Dialogs
{
    public partial class UpdateOptionsPanel : Form
    {
        Distro distro;

        public UpdateOptionsPanel(Distro distro)
        {
            InitializeComponent();

            this.distro = distro;
            versionLbl.Text = distro.Version ?? distro.ZipUrl.ShrinkUrl();

            updateAfterExit.Checked = Config.Instance.UpdateAfterExit;
            customDeployment.Checked = (Config.Instance.UpdateMode == (string)customDeployment.Tag);
            msiDeployment.Checked = (Config.Instance.UpdateMode == (string)msiDeployment.Tag);
            manualDeployment.Checked = (Config.Instance.UpdateMode == (string)manualDeployment.Tag);

            releaseInfo.Text = this.distro.ReleaseNotesText.NormalizeNewLines();
        }

        void UpdateProgress(long currentStep, long totalSteps)
        {
            if (!Closed)
                try
                {
                    Invoke((Action)delegate
                    {
                        progressBar.Value = (int)((double)currentStep / (double)totalSteps * 100.0);
                        progressBar.Maximum = 100;
                    });
                }
                catch { }
        }

        new bool Closed = false;

        void okBtn_Click(object sender, EventArgs e)
        {
            skipBtn.Enabled =
            okBtn.Enabled =
            optionsGroup.Enabled = false;

            //progressBar.Visible =
            progressLbl.Visible = true;
            progressBar.Style = ProgressBarStyle.Continuous;

            if (customDeployment.Checked && updateAfterExit.Checked)
            {
                Close();
            }

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string distroFile = null;
                        if (msiDeployment.Checked)
                        {
                            distroFile = CSScriptHelper.DownloadDistro(distro.MsiUrl, UpdateProgress);
                        }
                        else if (customDeployment.Checked || manualDeployment.Checked)
                        {
                            if (customDeployment.Checked && updateAfterExit.Checked)
                                distroFile = distro.ZipUrl;
                            else
                                distroFile = CSScriptHelper.DownloadDistro(distro.ZipUrl, UpdateProgress);
                        }
                        else
                        {
                            MessageBox.Show("Please select the update mode.", "CS-Script");
                            return;
                        }

                        if (Closed && !updateAfterExit.Checked)
                        {
                            return;
                        }

                        if (distroFile == null || (!File.Exists(distroFile) && distroFile != distro.ZipUrl))
                        {
                            MessageBox.Show(this, "Cannot download the binaries. The latest release Web page will be opened instead.", "CS-Script");
                            try
                            {
                                Process.Start(Plugin.HomeUrl);
                            }
                            catch { }
                        }
                        else
                        {
                            if (msiDeployment.Checked)
                            {
                                Config.Instance.UpdateMode = (string)msiDeployment.Tag;
                                Process.Start("explorer.exe", "/select,\"" + distroFile + "\"");
                            }
                            else if (manualDeployment.Checked)
                            {
                                Config.Instance.UpdateMode = (string)manualDeployment.Tag;
                                Process.Start(distroFile);
                            }
                            else if (customDeployment.Checked)
                            {
                                Config.Instance.UpdateMode = (string)customDeployment.Tag;

                                string downloadDir = KnownFolders.UserDownloads;

                                string targetDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                                string updater = DeployUpdater(targetDir, downloadDir);

                                try { Invoke((Action)Close); }
                                catch { }

                                if (updateAfterExit.Checked)
                                {
                                    Process.Start(updater, string.Format("\"{0}\" \"{1}\" /asynch_update", distro.ZipUrl, targetDir));
                                    MessageBox.Show("The plugin will be updated after you close Notepad++", "CS-Script");
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show(new Form { TopMost = true }, @"Some installation steps still need to be completed. Notepad++ needs to be restarted in older to complete these steeps. Would you like to restart now?",
                                                                                "CS-Script", MessageBoxButtons.YesNo))
                                    {
                                        Process updater_proc = Process.Start(updater, string.Format("\"{0}\" \"{1}\"", distroFile, targetDir));

                                        string npp_exe = Process.GetCurrentProcess().MainModule.FileName;

                                        // launcher.exe is already deployed (along with 7z.exe)
                                        string restarter = Path.Combine(Path.GetDirectoryName(updater), "launcher.exe");

                                        //the re-starter will also wait for updater process to exit
                                        Process.Start(restarter, $"/start {updater_proc.Id} \"{npp_exe}\"");

                                        PluginBase.Editor.FileExit();
                                    }
                                }
                            }

                            Config.Instance.Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Cannot execute install update: " + ex.Message, "CS-Script");
                    }

                    try { Invoke((Action)Close); }
                    catch { }
                });
        }

        string DeployUpdater(string pluginDir, string tempDir)
        {
            string deploymentDir = Path.Combine(tempDir, "CSScriptNpp.Updater");

            if (!Directory.Exists(deploymentDir))
                Directory.CreateDirectory(deploymentDir);

            Action<string> deploy = file => File.Copy(Path.Combine(pluginDir, file), Path.Combine(deploymentDir, file), true);

            deploy("updater.exe");
            deploy("launcher.exe");

            return Path.Combine(deploymentDir, "updater.exe");
        }

        void UpdateOptionsPanel_FormClosed(object sender, FormClosedEventArgs e)
        {
            Closed = true;
        }

        void releaseNotes_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(distro.ReleasePageUrl);
            }
            catch { }
        }

        void customDeployment_CheckedChanged(object sender, EventArgs e)
        {
            updateAfterExit.Enabled = customDeployment.Checked;
        }

        void updateAfterExit_CheckedChanged(object sender, EventArgs e)
        {
            Config.Instance.UpdateAfterExit = updateAfterExit.Checked;
        }

        void UpdateOptionsPanel_Load(object sender, EventArgs e)
        {
            Win32.SetForegroundWindow(this.Handle);
        }

        void skipBtn_Click(object sender, EventArgs e)
        {
            Config.Instance.SkipUpdateVersion = distro.Version;
            Config.Instance.Save();
            Close();
        }
    }
}