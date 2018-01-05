using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// using NppPlugin.DllExport;

namespace CSScriptIntellisense
{
    static class UnmanagedExports
    {
        static IntPtr _ptrPluginName = IntPtr.Zero;

        // [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr old_getName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(Plugin.PluginName);
            return _ptrPluginName;
        }

        // [DllExport(CallingConvention = CallingConvention.Cdecl)]
        // static void old_beNotified(IntPtr notifyCode)
        // {
        //     Debug.WriteLine("-------------------------------");

        //     SCNotification nc = (SCNotification)Marshal.PtrToStructure(notifyCode, typeof(SCNotification));

        //     //Debug.WriteLine("<<<<< ncnc.nmhdr.code={0}, {1}", nc.nmhdr.code, (int)nc.nmhdr.code);

        //     if (Plugin.Enabled && nc.nmhdr.code == (uint)NppMsg.NPPN_TBMODIFICATION)
        //     {
        //         Plugin.FuncItems.RefreshItems();
        //     }
        //     else if (Plugin.Enabled && nc.nmhdr.code == (uint)SciMsg.SCN_SAVEPOINTREACHED)
        //     {
        //         Plugin.OnSavedOrUndo();
        //     }
        //     else if (Plugin.Enabled && nc.nmhdr.code == (uint)SciMsg.SCN_CHARADDED)
        //     {
        //         Plugin.OnCharTyped((char)nc.ch);
        //     }
        //     else if (nc.nmhdr.code == (uint)SciMsg.SCN_DWELLSTART)
        //     {
        //         Debug.WriteLine("Tooltip started...");
        //     }
        //     else if (nc.nmhdr.code == (uint)SciMsg.SCN_DWELLEND)
        //     {
        //         Debug.WriteLine("Tooltip started...");
        //     }
        //     else if (Plugin.Enabled && nc.nmhdr.code == (uint)NppMsg.NPPN_BUFFERACTIVATED)
        //     {
        //         Plugin.OnCurrentFileChanegd();
        //     }
        //     else if (Plugin.Enabled && nc.nmhdr.code == (uint)NppMsg.NPPN_READY)
        //     {
        //         Plugin.OnNppReady();
        //     }
        //     else if (Plugin.Enabled && nc.nmhdr.code == (uint)NppMsg.NPPN_FILESAVED)
        //     {
        //         Plugin.OnBeforeDocumentSaved();
        //     }
        //     else if (Plugin.Enabled && nc.nmhdr.code == (uint)NppMsg.NPPN_FILEOPENED)
        //     {
        //         Plugin.OnCurrentFileChanegd();
        //     }
        //     else if (nc.nmhdr.code == (uint)NppMsg.NPPN_SHUTDOWN)
        //     {
        //         if (_ptrPluginName != IntPtr.Zero)
        //             Marshal.FreeHGlobal(_ptrPluginName);
        //     }
        // }
    }
}