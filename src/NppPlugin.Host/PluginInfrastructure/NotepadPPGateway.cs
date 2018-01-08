// NPP plugin platform for .Net v0.93.96 by Kasper B. Graversen etc.
using System;
using System.Text;
using NppPluginNET.PluginInfrastructure;

namespace Kbg.NppPluginNET.PluginInfrastructure
{
    public class Npp
    {
        public static NotepadPPGateway Editor { get { return PluginBase.Editor; } }

        public static ScintillaGateway GetCurrentDocument()
        {
            return (ScintillaGateway)PluginBase.GetCurrentDocument();
        }
    }

    public interface INotepadPPGateway
    {
        NotepadPPGateway FileNew();

        string GetCurrentFilePath();

        unsafe string GetFilePath(int bufferId);

        NotepadPPGateway SetCurrentLanguage(LangType language);
    }

    /// <summary>
    /// This class holds helpers for sending messages defined in the Msgs_h.cs file. It is at the moment
    /// incomplete. Please help fill in the blanks.
    /// </summary>
    public class NotepadPPGateway : INotepadPPGateway
    {
        public IntPtr Handle { get { return PluginBase.nppData._nppHandle; } }

        private const int Unused = 0;

        IntPtr send(NppMsg command, int wParam, NppMenuCmd lParam)
        {
            return Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)command, wParam, lParam);
        }

        IntPtr send(NppMsg command, IntPtr wParam, IntPtr lParam)
        {
            return Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)command, wParam, lParam);
        }

        public NotepadPPGateway FileNew()
        {
            send(NppMsg.NPPM_MENUCOMMAND, Unused, NppMenuCmd.IDM_FILE_NEW);
            return this;
        }

        public NotepadPPGateway ReloadFile(string file, bool showAlert)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_RELOADFILE, showAlert ? 1 : 0, file);
            return this;
        }

        public NotepadPPGateway SaveCurrentFile()
        {
            send(NppMsg.NPPM_SAVECURRENTFILE, Unused, Unused);
            return this;
        }

        public NotepadPPGateway FileExit()
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_MENUCOMMAND, Unused, NppMenuCmd.IDM_FILE_EXIT);
            return this;
        }

        public NotepadPPGateway Open(string file)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DOOPEN, Unused, file);
            return this;
        }

        public string[] GetOpenFiles()
        {
            var count = send(NppMsg.NPPM_GETNBOPENFILES, Unused, Unused);

            using (var cStrArray = new ClikeStringArray(count.ToInt32(), Win32.MAX_PATH))
            {
                if (send(NppMsg.NPPM_GETOPENFILENAMES, cStrArray.NativePointer, count) != IntPtr.Zero)
                    return cStrArray.ManagedStringsUnicode.ToArray();
                else
                    return new string[0];
            }
        }

        public string GetTabFile(IntPtr index)
        {
            var path = new StringBuilder(2000);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFULLPATHFROMBUFFERID, index, path);
            return path.ToString();
        }

        /// <summary>
        /// Gets the path of the current document.
        /// </summary>
        public string GetPluginsConfigDir()
        {
            var path = new StringBuilder(2000);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, path.Capacity, path);
            return path.ToString();
        }

        /// <summary>
        /// Gets the path of the current document.
        /// </summary>
        public string GetCurrentFilePath()
        {
            var path = new StringBuilder(2000);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFULLCURRENTPATH, path.Capacity, path);
            return path.ToString();
        }

        /// <summary>
        /// Gets the path of the current document.
        /// </summary>
        public unsafe string GetFilePath(int bufferId)
        {
            var path = new StringBuilder(2000);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFULLPATHFROMBUFFERID, bufferId, path);
            return path.ToString();
        }

        public NotepadPPGateway SetCurrentLanguage(LangType language)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETCURRENTLANGTYPE, Unused, (int)language);
            return this;
        }
    }

    /// <summary>
    /// This class holds helpers for sending messages defined in the Resource_h.cs file. It is at the moment
    /// incomplete. Please help fill in the blanks.
    /// </summary>
    class NppResource
    {
        private const int Unused = 0;

        public void ClearIndicator()
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)Resource.NPPM_INTERNAL_CLEARINDICATOR, Unused, Unused);
        }
    }
}