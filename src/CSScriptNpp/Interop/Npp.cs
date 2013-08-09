using System;
using System.Text;

namespace CSScriptNpp
{
    public class Npp
    {
        public static IntPtr CurrentScintilla { get { return Plugin.GetCurrentScintilla(); } }

        public static IntPtr NppHandle { get { return Plugin.NppData._nppHandle; } }

        static public string GetConfigDir()
        {
            var buffer = new StringBuilder(260);
            Win32.SendMessage(NppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, 260, buffer);
            return buffer.ToString();
        }

        static public string GetCurrentFile()
        {
            var path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(NppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            return path.ToString();
        }
    }
}
