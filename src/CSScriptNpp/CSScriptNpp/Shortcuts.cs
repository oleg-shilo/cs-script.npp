using System.Collections.Generic;
using System.IO;

namespace CSScriptNpp
{
    public class Shortcuts : IniFile
    {
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
            File.WriteAllText(this.file, ""); //clear to get rid of all obsolete values

            foreach (string name in namedValues.Keys)
                SetValue(Section, name, namedValues[name]);

            CSScriptIntellisense.Config.Shortcuts.Save();
        }

        Dictionary<string, string> namedValues = new Dictionary<string, string>();

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