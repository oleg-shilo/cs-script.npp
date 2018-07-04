using CSScriptIntellisense;
using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

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
            //SetWindowPos(frm.Handle.ToInt32(), HWND_TOPMOST, frm.Left, frm.Top, frm.Width, frm.Height, SWP_NOACTIVATE);
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

        //private const uint SB_SETPARTS = WM_USER + 4;
        //private const uint SB_GETPARTS = WM_USER + 6;
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

                //the following may be needed for future features
                // create new parts width array
                //int nParts = SendMessage(statusBarHandle, SB_GETPARTS, 0, IntPtr.Zero).ToInt32();
                //nParts++;
                //IntPtr memPtr = Marshal.AllocHGlobal(sizeof(int) * nParts);
                //int partWidth = 100; // set parts width according to the form size
                //for (int i = 0; i < nParts; i++)
                //{
                //    Marshal.WriteInt32(memPtr, i * sizeof(int), partWidth);
                //    partWidth += partWidth;
                //}
                //SendMessage(statusBarHandle, SB_SETPARTS, nParts, memPtr);
                //Marshal.FreeHGlobal(memPtr);

                //// set text for the new part
                //IntPtr text0 = Marshal.StringToHGlobalAuto("new section text 1");
                //SendMessage(statusBarHandle, SB_SETTEXT, nParts - 1, text0);
                //Marshal.FreeHGlobal(text0);
            }
            return retval;
        }
    }

}