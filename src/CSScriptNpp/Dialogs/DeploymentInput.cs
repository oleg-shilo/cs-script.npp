using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public partial class DeploymentInput : Form
    {
        public class RuntimeVersionItem
        {
            public string Version;
            public string DispalyTitle;

            public override string ToString()
            {
                return DispalyTitle;
            }
        }

        public RuntimeVersionItem SelectedVersion
        {
            get
            {
                if (versionsList.SelectedItem != null)
                    return (RuntimeVersionItem)versionsList.SelectedItem;
                else
                    return null;
            }
        }

        IEnumerable<RuntimeVersionItem> Versions
        {
            get { return versionsList.Items.Cast<RuntimeVersionItem>(); }
        }

        public DeploymentInput()
        {
            InitializeComponent();

            try
            {
                versionsList.Items.Add(new RuntimeVersionItem { Version = $"v5.0", DispalyTitle = ".NET 5/Core" });
                versionsList.SelectedIndex = 0;
                windowApp.Enabled = false;
                windowApp.Checked = false;
            }
            catch { }

            versionsList.SelectedItem = Versions.Where(x => x.Version == Config.Instance.TargetVersion)
                                                .FirstOrDefault();

            if (versionsList.SelectedItem == null)
                versionsList.SelectedItem = Versions.First();

            asScript.Checked = Config.Instance.DistributeScriptAsScriptByDefault;
            asExe.Checked = !asScript.Checked;
            windowApp.Checked = Config.Instance.DistributeScriptAsWindowApp;
            asDll.Checked = Config.Instance.DistributeScriptAsDll;
        }

        public bool AsScript
        {
            get { return asScript.Checked; }
        }

        public bool AsDll
        {
            get { return asDll.Checked; }
        }

        public bool AsWindowApp
        {
            get { return windowApp.Checked; }
        }

        void okBtn_Click(object sender, EventArgs e)
        {
            Config.Instance.TargetVersion = SelectedVersion.Version;
            Config.Instance.DistributeScriptAsScriptByDefault = asScript.Checked;
            Config.Instance.DistributeScriptAsWindowApp = windowApp.Checked;
            Config.Instance.DistributeScriptAsDll = asDll.Checked;
            Config.Instance.Save();
        }

        private void asExe_CheckedChanged(object sender, EventArgs e)
        {
            windowApp.Enabled = false;
        }
    }
}