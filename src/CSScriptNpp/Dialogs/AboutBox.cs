using CSScriptNpp.Dialogs;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSScriptNpp
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            this.Text = "About CS-Script";
            this.label3.Text = "Version: " + AssemblyVersion;
            this.label5.Text = AssemblyCopyright;
            this.textBoxDescription.Text = AssemblyDescription;

            if (downloadingMsi)
                SetUpdateStatus("Downloading...");
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return
                    Assembly.GetExecutingAssembly().GetName().Version.ToString() +
                    "  (script engine cscs.exe: " + CSScriptHelper.ScriptEngineVersion() + ")";
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        #endregion Assembly Attribute Accessors

        void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://csscript.net/npp/license.txt");
            }
            catch { }
        }

        void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://csscript.net/npp/");
            }
            catch { }
        }

        void updateCheckBtn_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            SetUpdateStatus("Checking...");
            Task.Factory.StartNew(CheckForUpdates);
        }

        static bool downloadingMsi = false;

        void SetUpdateStatus(string status = null)
        {
            if (status == null)
            {
                updateCheckBtn.Enabled = true;
                updateCheckBtn.Text = "Check for Updates...";
            }
            else
            {
                updateCheckBtn.Enabled = true;
                updateCheckBtn.Text = status;
            }
        }


        void CheckForUpdates()
        {
            string version = CSScriptHelper.GetLatestAvailableVersion();

            Invoke((Action)delegate
            {
                SetUpdateStatus();
                Cursor = Cursors.Default;
            });

            if (version == null)
            {
                MessageBox.Show("Cannot check for updates. The latest release Web page will be opened instead.", "CS-Script");
                try
                {
                    Process.Start(Plugin.HomeUrl);
                }
                catch { }
            }
            else
            {
                var latestVersion = new Version(version);
                var nppVersion = Assembly.GetExecutingAssembly().GetName().Version;

                if (nppVersion == latestVersion)
                {
                    MessageBox.Show("You are already running the latest version - v" + version, "CS-Script");
                }
                else if (nppVersion > latestVersion)
                {
                    MessageBox.Show("Wow... your version is even newer than the latest one - v" + version + ".", "CS-Script");
                }
                else if (nppVersion < latestVersion)
                {
                    using (var dialog = new UpdateOptionsPanel(version))
                        dialog.ShowDialog();
                }
            }
        }

        void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("mailto:csscript.support@gmail.com?subject=CS-Script_Npp");
            }
            catch { }
        }

        int blinkingCount = 0;
        void timer1_Tick(object sender, EventArgs e)
        {
            if (updateCheckBtn.Text.StartsWith("Downloading"))
            {
                blinkingCount++;
                if (blinkingCount > 3)
                    blinkingCount = 0;

                updateCheckBtn.Text = "Downloading" + new string('.', blinkingCount);
            }
        }

        void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(Plugin.LogDir);
            }
            catch { }
        }
    }
}