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
                if(instance == null)
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
            
            if(!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
            string path = Path.Combine(configDir, "settings.ini");
            
            base.file = path;
            Open();
        }

        public string GetFileName()
        {
            return base.file;
        }

        public bool UseArrowToAccept = true;
        public bool InterceptCtrlSpace = true;
        public bool NewPluginConfictReported = false;
        public bool ShowQuickInfoInStatusBar = false;
        public bool UseMethodBrackets = false;
        public bool FormatAsYouType = true;
        public bool SmartIndenting = true;
        public bool IgnoreDocExceptions = false;

        public void Save()
        {
            lock (this)
            {
                File.WriteAllText(this.file, ""); //clear to get rid of all obsolete values

                SetValue("settings", "NewPluginConfictReported", NewPluginConfictReported);
                SetValue("settings", "UseArrowToAccept", UseArrowToAccept);
                SetValue("settings", "InterceptCtrlSpace", InterceptCtrlSpace);
                SetValue("settings", "UseMethodBrackets", UseMethodBrackets);
                SetValue("settings", "ShowQuickInfoInStatusBar", ShowQuickInfoInStatusBar);
                SetValue("settings", "IgnoreDocExceptions", IgnoreDocExceptions);
                SetValue("settings", "SmartIndenting", SmartIndenting);
                SetValue("settings", "FormatAsYouType", FormatAsYouType);
            }
        }

        public void Open()
        {
            lock (this)
            {
                NewPluginConfictReported = GetValue("settings", "NewPluginConfictReported", NewPluginConfictReported);
                UseArrowToAccept = GetValue("settings", "UseArrowToAccept", UseArrowToAccept);
                InterceptCtrlSpace = GetValue("settings", "InterceptCtrlSpace", InterceptCtrlSpace);
                UseMethodBrackets = GetValue("settings", "UseMethodBrackets", UseMethodBrackets);
                SmartIndenting = GetValue("settings", "SmartIndenting", SmartIndenting);
                FormatAsYouType = GetValue("settings", "FormatAsYouType", FormatAsYouType);
                ShowQuickInfoInStatusBar = GetValue("settings", "ShowQuickInfoInStatusBar", ShowQuickInfoInStatusBar);
                IgnoreDocExceptions = GetValue("settings", "IgnoreDocExceptions", IgnoreDocExceptions);
            }
        }
    }
}
