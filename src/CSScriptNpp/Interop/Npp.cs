using System;
using System.Text;

namespace CSScriptNpp
{
    public class Npp
    {
        /// <summary>
        /// Determines whether the current file has the specified extension (e.g. ".cs").
        /// <para>Note it is case insensitive.</para>
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        static public bool IsCurrentFileHasExtension(string extension)
        {
            var path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(Plugin.NppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            string file = path.ToString();
            return !string.IsNullOrWhiteSpace(file) && file.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
        }

        public static IntPtr CurrentScintilla { get { return Plugin.GetCurrentScintilla(); } }

        public static IntPtr NppHandle { get { return Plugin.NppData._nppHandle; } }

        static public string GetConfigDir()
        {
            var buffer = new StringBuilder(260);
            Win32.SendMessage(NppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, 260, buffer);
            return buffer.ToString();
        }


        static public int GrabFocus()
        {
            int currentPos = (int)Win32.SendMessage(CurrentScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
            return currentPos;
        }

        static public string GetCurrentFile()
        {
            var path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(NppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            return path.ToString();
        }
    }
}
