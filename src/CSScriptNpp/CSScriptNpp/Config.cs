using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CSScriptNpp
{
    /// <summary>
    /// Of course XML based config is more natural, however ini file reading (just a few values) is faster. 
    /// </summary>
    public class Config : IniFile
    {
        static Config instance = new Config();

        public static Config Instance { get { return instance; } }

        public Config()
        {
            base.file = Path.Combine(Plugin.ConfigDir, "settings.ini");
            Open();
        }

        public bool ClasslessScriptByDefault = false;
        public bool InterceptConsole = false;
        public bool BuildOnF7 = true;
        public bool ShowProjectPanel = false;
        public bool ShowOutputPanel = false;
        public int OutputPanelCapacity = 10000; //num of characters
        public bool IntegrateWithIntellisense = true;
        public bool LocalDebug = true;

        public void Save()
        {
            SetValue("settings", "ShowProjectPanel", ShowProjectPanel);
            SetValue("settings", "ShowOutputPanel", ShowOutputPanel);
            SetValue("settings", "OutputPanelCapacity", OutputPanelCapacity);
            SetValue("settings", "InterceptConsole", InterceptConsole);
            SetValue("settings", "LocalDebug", LocalDebug);
            SetValue("settings", "BuildOnF7", BuildOnF7);
            SetValue("settings", "ClasslessScriptByDefault", ClasslessScriptByDefault);
            SetValue("settings", "IntegrateWithIntellisense", IntegrateWithIntellisense);
        }

        public void Open()
        {
            ShowProjectPanel = GetValue("settings", "ShowProjectPanel", ShowProjectPanel);
            ShowOutputPanel = GetValue("settings", "ShowOutputPanel", ShowOutputPanel);
            OutputPanelCapacity = GetValue("settings", "OutputPanelCapacity", OutputPanelCapacity);
            InterceptConsole = GetValue("settings", "InterceptConsole", InterceptConsole);
            LocalDebug = GetValue("settings", "LocalDebug", LocalDebug);
            BuildOnF7 = GetValue("settings", "BuildOnF7", BuildOnF7);
            ClasslessScriptByDefault = GetValue("settings", "ClasslessScriptByDefault", ClasslessScriptByDefault);
            IntegrateWithIntellisense = GetValue("settings", "IntegrateWithIntellisense", IntegrateWithIntellisense);
        }
    }
}
