using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSScriptNpp.Dialogs
{
    public partial class UpdateOptionsPanel : Form
    {
        string version;
        public UpdateOptionsPanel(string version)
        {
            InitializeComponent();
            this.version = version;
            versionLbl.Text = version;

            customDeployment.Checked = (Config.Instance.UpdateMode == (string)customDeployment.Tag);
            msiDeployment.Checked = (Config.Instance.UpdateMode == (string)msiDeployment.Tag);
            manualDeployment.Checked = (Config.Instance.UpdateMode == (string)manualDeployment.Tag);

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

        bool Closed = false;

        private void okBtn_Click(object sender, EventArgs e)
        {
            okBtn.Enabled =
            optionsGroup.Enabled = false;

            progressLbl.Visible =
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Continuous;

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string distroFile = null;
                        if (msiDeployment.Checked)
                        {
                            distroFile = CSScriptHelper.GetLatestAvailableDistro(version, ".msi", UpdateProgress);
                        }
                        else if (customDeployment.Checked || manualDeployment.Checked)
                        {
                            distroFile = CSScriptHelper.GetLatestAvailableDistro(version, ".zip", UpdateProgress);
                        }
                        else
                        {
                            MessageBox.Show("Please select the update mode.", "CS-Script");
                            return;
                        }

                        if (Closed)
                            return;

                        if (distroFile == null || !File.Exists(distroFile))
                        {
                            MessageBox.Show("Cannot download the binaries. The latest release Web page will be opened instead.", "CS-Script");
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

                                string targetDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                                string updater = DeployUpdater(targetDir, Path.GetDirectoryName(distroFile));
                                Process.Start(updater, string.Format("\"{0}\" \"{1}\"", distroFile, targetDir));
                            }

                            Config.Instance.Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Cannot execute install update: " + ex.Message, "CS-Script");
                    }

                    Invoke((Action)Close);
                });
        }

        string DeployUpdater(string pluginDir, string tempDir)
        {
            string srcDir = Path.Combine(pluginDir, "CSScriptNpp");
            string deploymentDir = Path.Combine(tempDir, "CSScriptNpp.Updater");

            if (!Directory.Exists(deploymentDir))
                Directory.CreateDirectory(deploymentDir);

            Action<string> deploy = file => File.Copy(Path.Combine(srcDir, file), Path.Combine(deploymentDir, file), true);

            deploy("updater.exe");
            deploy("7z.exe");
            deploy("7z.dll");

            return Path.Combine(deploymentDir, "updater.exe");
        }

        private void UpdateOptionsPanel_FormClosed(object sender, FormClosedEventArgs e)
        {
            Closed = true;
        }
    }
}
