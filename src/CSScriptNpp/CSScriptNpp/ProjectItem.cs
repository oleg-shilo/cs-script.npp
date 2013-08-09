using System;
using System.IO;

namespace CSScriptNpp
{
    public class ProjectItem
    {
        public string File;
        public string Name;
        public bool IsPrimary;
        public object Tag;

        public ProjectItem(string file)
        {
            File = file;
            Name = Path.GetFileName(file);
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsAssembly
        {
            get
            {
                return File.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}