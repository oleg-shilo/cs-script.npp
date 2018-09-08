using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CSScriptIntellisense
{
    public class IniFile
    {
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retval, int size, string file);

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string section, string key, string value, string file);

        protected string file;

        public IniFile()
        {
        }

        public IniFile(string file)
        {
            this.file = file;
        }

        public void SetValue<T>(string section, string key, T value)
        {
            try
            {
                WritePrivateProfileString(section, key, value.ToString(), file);
            }
            catch { }
        }

        public T GetValue<T>(string section, string key, T defaultValue, int size = 255 * 30) // MAX_PATH*30
        {
            try
            {
                var retval = new StringBuilder(size);
                GetPrivateProfileString(section, key, defaultValue.ToString(), retval, size, file);
                return (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(retval.ToString());
            }
            catch
            {
                return defaultValue;
            }
        }

        public T GetValue<T>(string section, string key, ref bool defaulted, T defaultValue, int size = 255)
        {
            try
            {
                var retval = new StringBuilder(size);
                GetPrivateProfileString(section, key, "", retval, size, file);
                if (retval.Length == 0)
                {
                    defaulted = true;
                    return defaultValue;
                }
                else
                    return (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(retval.ToString());
            }
            catch
            {
                defaulted = true;
                return defaultValue;
            }
        }
    }

}