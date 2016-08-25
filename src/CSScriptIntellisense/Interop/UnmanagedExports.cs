using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NppPlugin.DllExport;

namespace CSScriptIntellisense
{
    static class UnmanagedExports
    {
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static bool isUnicode()
        {
            return true;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            Plugin.NppData = notepadPlusData;
            Plugin.CommandMenuInit();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getFuncsArray(ref int nbF)
        {
            nbF = Plugin.FuncItems.Items.Count;
            return Plugin.FuncItems.NativePointer;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        static IntPtr _ptrPluginName = IntPtr.Zero;
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(Plugin.PluginName);
            return _ptrPluginName;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            Debug.WriteLine("-------------------------------");

            SCNotification nc = (SCNotification)Marshal.PtrToStructure(notifyCode, typeof(SCNotification));

            //Debug.WriteLine("<<<<< ncnc.nmhdr.code={0}, {1}", nc.nmhdr.code, (int)nc.nmhdr.code);

            if (Plugin.Enabled && nc.nmhdr.code == (uint)NppMsg.NPPN_TBMODIFICATION)
            {
                Plugin.FuncItems.RefreshItems();
            }
            else if (Plugin.Enabled && nc.nmhdr.code == (uint)SciMsg.SCN_SAVEPOINTREACHED)
            {
                Plugin.OnSavedOrUndo();
            }
            else if (Plugin.Enabled && nc.nmhdr.code == (uint)SciMsg.SCN_CHARADDED)
            {
                Plugin.OnCharTyped((char)nc.ch);
            }
            else if (nc.nmhdr.code == (uint)SciMsg.SCN_DWELLSTART)
            {
                Debug.WriteLine("Tooltip started...");
            }
            else if (nc.nmhdr.code == (uint)SciMsg.SCN_DWELLEND)
            {
                Debug.WriteLine("Tooltip started...");
            }
            else if (Plugin.Enabled && nc.nmhdr.code == (uint)NppMsg.NPPN_BUFFERACTIVATED)
            {
                Plugin.OnCurrentFileChanegd();
            }
            else if (Plugin.Enabled && nc.nmhdr.code == (uint)NppMsg.NPPN_READY)
            {
                Plugin.OnNppReady();
            }
            else if (Plugin.Enabled && nc.nmhdr.code == (uint) NppMsg.NPPN_FILESAVED)
            {
                Plugin.OnDocumentSaved();
            }
            else if (Plugin.Enabled && nc.nmhdr.code == (uint)NppMsg.NPPN_FILEOPENED)
            {
                Plugin.OnCurrentFileChanegd();
            }
            else if (nc.nmhdr.code == (uint)NppMsg.NPPN_SHUTDOWN)
            {
                if (_ptrPluginName != IntPtr.Zero)
                    Marshal.FreeHGlobal(_ptrPluginName);
            }
        }
    }
}


