using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public partial class DeploymentInput : Form
    {
        public DeploymentInput()
        {
            InitializeComponent();

            try
            {
                windowApp.Enabled = false;
                windowApp.Checked = false;
            }
            catch { }

            asScript.Checked = Config.Instance.DistributeScriptAsScriptByDefault;
            asExe.Checked = !asScript.Checked;
            windowApp.Enabled = asExe.Checked;
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
            Config.Instance.DistributeScriptAsScriptByDefault = asScript.Checked;
            Config.Instance.DistributeScriptAsWindowApp = windowApp.Checked;
            Config.Instance.DistributeScriptAsDll = asDll.Checked;
            Config.Instance.Save();
        }

        private void asExe_CheckedChanged(object sender, EventArgs e)
        {
            windowApp.Enabled = false;
            // windowApp.Enabled = asExe.Checked;
        }
    }
}