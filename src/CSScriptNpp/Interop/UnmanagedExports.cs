using NppPlugin.DllExport;
using System;
using System.Runtime.InteropServices;

namespace CSScriptNpp
{
    class UnmanagedExports
    {
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static bool isUnicode()
        {
            return true;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            //System.Diagnostics.Debug.Assert(false);
            Bootstrapper.Init();

            Plugin.NppData = notepadPlusData;

            InitPlugin();
        }

        static void InitPlugin()
        {
            CSScriptIntellisense.Plugin.NppData._nppHandle = Plugin.NppData._nppHandle;
            CSScriptIntellisense.Plugin.NppData._scintillaMainHandle = Plugin.NppData._scintillaMainHandle;
            CSScriptIntellisense.Plugin.NppData._scintillaSecondHandle = Plugin.NppData._scintillaSecondHandle;

            CSScriptNpp.Plugin.CommandMenuInit(); //this will also call CSScriptIntellisense.Plugin.CommandMenuInit

            foreach (var item in CSScriptIntellisense.Plugin.FuncItems.Items)
                Plugin.FuncItems.Add(item.ToLocal());

            CSScriptIntellisense.Plugin.FuncItems.Items.Clear();
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
            SCNotification nc = (SCNotification)Marshal.PtrToStructure(notifyCode, typeof(SCNotification));
            if (nc.nmhdr.code == (uint)NppMsg.NPPN_READY)
            {
                CSScriptIntellisense.Plugin.OnNppReady();
                CSScriptNpp.Plugin.OnNppReady();
            }
            else if (nc.nmhdr.code == (uint)NppMsg.NPPN_TBMODIFICATION)
            {
                CSScriptNpp.Plugin.OnToolbarUpdate();
            }
            else if (nc.nmhdr.code == (uint)SciMsg.SCN_CHARADDED)
            {
                CSScriptIntellisense.Plugin.OnCharTyped((char)nc.ch);
            }
            else if (nc.nmhdr.code == (uint)NppMsg.NPPN_BUFFERACTIVATED)
            {
                CSScriptIntellisense.Plugin.OnCurrentFileChanegd();
                CSScriptNpp.Plugin.OnCurrentFileChanged();
            }
            else if (nc.nmhdr.code == (uint)NppMsg.NPPN_SHUTDOWN)
            {
                Marshal.FreeHGlobal(_ptrPluginName);

                CSScriptNpp.Plugin.CleanUp();
            }

            Plugin.OnNotification(nc);
        }
    }
}