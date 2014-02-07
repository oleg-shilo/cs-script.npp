using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CSScriptIntellisense
{
    /// <summary>
    /// Of course XML based config is more natural, however ini file reading (just a few values) 
    /// is faster. 
    /// </summary>
    public class Config : IniFile
    {
        static Config instance;

        public static Config Instance
        {
            get
            {
                if (instance == null)
                    instance = new Config();
                return instance;
            }
        }

        public Config(string file)
            : base(file)
        {
            Open();
        }

        public Config()
        {
            var configDir = Path.Combine(Npp.GetConfigDir(), "CSharpIntellisense");

            //if (!Directory.Exists(configDir))
            //    Directory.CreateDirectory(configDir);
            string path = Path.Combine(configDir, "settings.ini");

            base.file = path;
            Open();
        }

        public void SetFileName(string path)
        {
            UsingExternalFile = true;
            base.file = path;
            Open();
        }

        public string GetFileName()
        {
            return base.file;
        }

        public bool UseArrowToAccept = true;
        public bool UseTabToAccept = true;

        public bool SnapshotsEnabled = false;
        public bool InterceptCtrlSpace = true;
        public bool ShowQuickInfoInStatusBar = false;
        public bool UseMethodBrackets = false;

        bool disableMethodInfo;

        public bool DisableMethodInfo
        {
            get { return disableMethodInfo; }
            set { disableMethodInfo = value; }
        }


        public bool FormatAsYouType = true;

        public bool UsingExternalFile = false;
        public bool SmartIndenting = true;
        public bool IgnoreDocExceptions = false;

        public string Section = "settings";

        public void Save()
        {
            lock (this)
            {
                var configDir = Path.GetDirectoryName(this.file);

                if (!Directory.Exists(configDir))
                    Directory.CreateDirectory(configDir);

                if (!UsingExternalFile)
                    File.WriteAllText(this.file, ""); //clear to get rid of all obsolete values

                SetValue(Section, "UseArrowToAccept", UseArrowToAccept);
                SetValue(Section, "UseTabToAccept", UseTabToAccept);
                SetValue(Section, "InterceptCtrlSpace", InterceptCtrlSpace);
                SetValue(Section, "UseMethodBrackets", UseMethodBrackets);
                SetValue(Section, "SnapshotsEnabled", SnapshotsEnabled);
                SetValue(Section, "ShowQuickInfoInStatusBar", ShowQuickInfoInStatusBar);
                SetValue(Section, "IgnoreDocExceptions", IgnoreDocExceptions);
                SetValue(Section, "SmartIndenting", SmartIndenting);
                SetValue(Section, "DisableMethodInfo", DisableMethodInfo);
                SetValue(Section, "FormatAsYouType", FormatAsYouType);
            }
        }

        public void Open()
        {
            lock (this)
            {
                UseTabToAccept = GetValue(Section, "UseTabToAccept", UseTabToAccept);
                UseArrowToAccept = GetValue(Section, "UseArrowToAccept", UseArrowToAccept);
                InterceptCtrlSpace = GetValue(Section, "InterceptCtrlSpace", InterceptCtrlSpace);
                UseMethodBrackets = GetValue(Section, "UseMethodBrackets", UseMethodBrackets);
                SmartIndenting = GetValue(Section, "SmartIndenting", SmartIndenting);
                SnapshotsEnabled = GetValue(Section, "SnapshotsEnabled", SnapshotsEnabled);
                FormatAsYouType = GetValue(Section, "FormatAsYouType", FormatAsYouType);
                ShowQuickInfoInStatusBar = GetValue(Section, "ShowQuickInfoInStatusBar", ShowQuickInfoInStatusBar);
                IgnoreDocExceptions = GetValue(Section, "IgnoreDocExceptions", IgnoreDocExceptions);
                DisableMethodInfo = GetValue(Section, "DisableMethodInfo", DisableMethodInfo);
            }
        }
    }
}
