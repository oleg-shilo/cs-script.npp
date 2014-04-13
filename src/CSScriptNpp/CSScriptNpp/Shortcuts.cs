using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSScriptNpp
{
    public class Shortcuts : IniFile
    {
        public class ConfigInfo
        {
            public string Name;
            public string DisplayName;
            public string Shortcut;
            public bool IsManagedByNpp { get { return Name.StartsWith("_"); } }
        }

        public Shortcuts()
        {
            base.file = Path.Combine(Plugin.ConfigDir, "shortcuts.ini");
            CSScriptIntellisense.Config.Shortcuts.SetFileName(base.file);
        }

        public string GetFileName()
        {
            return base.file;
        }

        public string Section = "Shortcuts";

        public void Save()
        {
            //for now allow editing the settings file manually only
            //Though create the file if it does not exist
            if (!File.Exists(this.file))
            {
                File.WriteAllText(this.file, ""); //clear to get rid of all obsolete values

                foreach (string name in namedValues.Keys)
                    SetValue(Section, name, namedValues[name]);

                CSScriptIntellisense.Config.Shortcuts.Save();
            }
        }

        Dictionary<string, string> namedValues = new Dictionary<string, string>(); //_Run:F5
        Dictionary<string, string> displayNames = new Dictionary<string, string>(); //_Run:Run

        public ConfigInfo[] GetConfigInfo()
        {
            return namedValues.Select(x =>
                new ConfigInfo
                {
                    Name = x.Key,
                    Shortcut = x.Value,
                    DisplayName = GetShortcutDisplayName(x.Key) ?? x.Key
                }).ToArray();
        }

        public void MapDisplayName(string name, string displayName)
        {
            displayNames[name] = displayName;
        }

        public string GetShortcutDisplayName(string name)
        {
            if (displayNames.ContainsKey(name))
                return displayNames[name];
            return null;
        }

        //public string GetShortcutName(string displayName)
        //{
        //    if (displayNames.ContainsKey(displayName))
        //        return displayNames[displayName];
        //    return null;
        //}

        public string GetValue(string name, string defaultValue)
        {
            var value = GetValue(Section, name, defaultValue);
            if (namedValues.ContainsKey(name))
                namedValues[name] = value;
            else
                namedValues.Add(name, value);
            return value;
        }

        public string SetValue(string name, string value)
        {
            SetValue(Section, name, value);
            if (namedValues.ContainsKey(name))
                namedValues[name] = value;
            else
                namedValues.Add(name, value);
            return value;
        }
    }
}