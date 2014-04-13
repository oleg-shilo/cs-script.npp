using System;
using System.Collections.Generic;
using System.IO;

namespace CSScriptIntellisense
{
    public class Shortcuts : IniFile
    {
        public Shortcuts(string file)
            : base(file)
        {
        }

        public Shortcuts()
        {
            var configDir = Path.Combine(Npp.GetConfigDir(), "CSharpIntellisense");
            base.file = Path.Combine(configDir, "shortcuts.ini");
        }

        public bool UsingExternalFile = false;
        public void SetFileName(string path)
        {
            UsingExternalFile = true;
            base.file = path;
        }

        public string GetFileName()
        {
            return base.file;
        }


        public string Section = "Shortcuts";

        public void Save()
        {
            lock (this)
            {
                var configDir = Path.GetDirectoryName(this.file);

                Directory.CreateDirectory(configDir);

                if (!UsingExternalFile)
                    File.WriteAllText(this.file, ""); //clear to get rid of all obsolete values

                foreach (string name in namedValues.Keys)
                    SetValue(Section, name, namedValues[name]);
            }
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
