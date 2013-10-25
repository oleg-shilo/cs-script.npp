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

        public static int GetCaretLineNumber()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);

            return (int)Win32.SendMessage(sci, SciMsg.SCI_LINEFROMPOSITION, currentPos, 0);
        }

        static public int GetLineStart(int line)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, SciMsg.SCI_POSITIONFROMLINE, line, 0);
        }

        static public int GetFirstVisibleLine()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, SciMsg.SCI_GETFIRSTVISIBLELINE, 0, 0);
        }

        static public void SetFirstVisibleLine(int line)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SETFIRSTVISIBLELINE, line, 0);
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