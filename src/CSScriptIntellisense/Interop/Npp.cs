using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CSScriptIntellisense;
using Kbg.NppPluginNET.PluginInfrastructure;

public partial class PluginEnv
{
    public static string ConfigDir
    {
        get { return Npp.Editor.GetPluginsConfigDir().PathJoin("CSScriptNpp").EnsureDir(); }
    }

    public static string LogDir
    {
        get { return Path.Combine(Environment.SpecialFolder.ApplicationData.Path(), @"Notepad++\plugins\logs\CSScriptNpp").EnsureDir(); }
    }

    public static void CustomLog(string message)
    {
        var logFile = Path.Combine(LogDir, "CSScriptNpp.log");
        File.AppendAllText(logFile, $"{DateTime.Now}: {message}{Environment.NewLine}");
    }


    public static string PluginDir
    {
        get { return Assembly.GetExecutingAssembly().Location.GetDirName(); }
    }

    public static string Locate(string fileName, params string[] subDirs)
    {
        var dir = PluginDir;
        var file = Path.Combine(dir, fileName);

        foreach (var item in subDirs)
            if (File.Exists(file))
                return file;
            else
                file = Path.Combine(dir, item, fileName);

        return file;
    }
}

namespace CSScriptIntellisense
{
    public static class npp_extensions
    {
        static public bool IsCurrentDocScriptFile(this NotepadPPGateway editor)
        {
            return editor.GetCurrentFilePath().IsScriptFile();
        }
    }

    public class npp
    {
        public const int DocEnd = -1;

        static public void ReloadFile(bool showAlert, string file)
        {
            Npp.Editor.FileNew();
            Npp.Editor.ReloadFile(file, showAlert);
        }

        const int SW_SHOWNOACTIVATE = 4;
        const int HWND_TOPMOST = -1;
        const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(
            int hWnd,             // Window handle
            int hWndInsertAfter,  // Placement-order handle
            int X,                // Horizontal position
            int Y,                // Vertical position
            int cx,               // Width
            int cy,               // Height
            uint uFlags);         // Window positioning flags

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void ShowInactiveTopmost(Form frm)
        {
            ShowWindow(frm.Handle, SW_SHOWNOACTIVATE);
            SetWindowPos(frm.Handle.ToInt32(), PluginBase.nppData._nppHandle.ToInt32(), frm.Left, frm.Top, frm.Width, frm.Height, SWP_NOACTIVATE);
        }

        static public string GetShortcutsFile()
        {
            return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Npp.Editor.GetPluginsConfigDir())), "shortcuts.xml");
        }

        static public string GetNppConfigFile()
        {
            return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Npp.Editor.GetPluginsConfigDir())), "config.xml");
        }

        static public string ContextMenuFile
        {
            get { return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Npp.Editor.GetPluginsConfigDir())), "contextMenu.xml"); }
        }

        [DllImport("user32.dll")]
        public static extern long GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        static public Rectangle GetClientRect()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();

            var r = new Rectangle();
            GetWindowRect(sci, ref r);
            return r;
        }

        static public Rectangle GetWindowRect()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();

            var r = new Rectangle();
            GetWindowRect(sci, ref r);
            return r;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, IntPtr lParam);

        public const int SB_SETTEXT = 1035;
        public const int SB_SETPARTS = 1028;
        public const int SB_GETPARTS = 1030;

        private const uint WM_USER = 0x0400;

        private const uint SB_GETTEXTLENGTH = WM_USER + 12;

        private const uint SB_GETTEXT = WM_USER + 13;

        static public string GetStatusbarLabel()
        {
            throw new NotImplementedException();
        }

        static public string SetStatusbarLabel(string labelText)
        {
            string retval = null;
            IntPtr mainWindowHandle = PluginBase.nppData._nppHandle;
            // find status bar control on the main window of the application
            IntPtr statusBarHandle = FindWindowEx(mainWindowHandle, IntPtr.Zero, "msctls_statusbar32", IntPtr.Zero);
            if (statusBarHandle != IntPtr.Zero)
            {
                //cut current text
                var size = (int)SendMessage(statusBarHandle, SB_GETTEXTLENGTH, 0, IntPtr.Zero);
                var buffer = new StringBuilder(size);
                Win32.SendMessage(statusBarHandle, (uint)SB_GETTEXT, 0, buffer);
                retval = buffer.ToString();

                // set text for the existing part with index 0
                IntPtr text = Marshal.StringToHGlobalAuto(labelText);
                SendMessage(statusBarHandle, SB_SETTEXT, 0, text);

                Marshal.FreeHGlobal(text);
            }
            return retval;
        }
    }
}