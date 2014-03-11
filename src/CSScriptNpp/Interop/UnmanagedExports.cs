using NppPlugin.DllExport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        const int _SC_MARGE_SYBOLE = 1; //bookmark and breakpoint margin
        const int SCI_CTRL = 2; //Ctrl pressed modifier for SCN_MARGINCLICK


        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            try
            {
                SCNotification nc = (SCNotification)Marshal.PtrToStructure(notifyCode, typeof(SCNotification));
                //Debug.WriteLine("Code: " + nc.nmhdr.code);

                if (nc.nmhdr.code == (uint)NppMsg.NPPN_READY)
                {
                    CSScriptIntellisense.Plugin.OnNppReady();
                    CSScriptNpp.Plugin.OnNppReady();
#if DEBUG
                    Npp.SetCalltipTime(400);
#endif
                }
                else if (nc.nmhdr.code == (uint)NppMsg.NPPN_TBMODIFICATION)
                {
                    CSScriptNpp.Plugin.OnToolbarUpdate();
                }
                else if (nc.nmhdr.code == (uint)SciMsg.SCN_CHARADDED)
                {
                    CSScriptIntellisense.Plugin.OnCharTyped((char)nc.ch);
                }
                else if (nc.nmhdr.code == (uint)SciMsg.SCN_MARGINCLICK)
                {
                    if (nc.margin == _SC_MARGE_SYBOLE && nc.modifiers == SCI_CTRL)
                    {
                        int lineClick = Npp.GetLineFromPosition(nc.position);
                        Debugger.ToggleBreakpoint(lineClick);
                    }
                }
                else if (nc.nmhdr.code == (uint)SciMsg.SCN_DWELLSTART) //tooltip
                {
#if DEBUG
                    //Npp.ShowCalltip(nc.position, "\u0001  1 of 3 \u0002  test tooltip " + Environment.TickCount);
                    //Npp.ShowCalltip(nc.position, CSScriptIntellisense.Npp.GetWordAtPosition(nc.position));
                    string tooltip = CSScriptIntellisense.Npp.GetWordAtPosition(nc.position);
                    //                    tooltip = @"Creates all directories and subdirectories as specified by path.
                    //--------------------------
                    //Returns: A System.IO.DirectoryInfo as specified by path.
                    //--------------------------
                    //path: The directory path to create. 
                    //path2: Fake parameter for testing.
                    //--------------------------
                    //Exceptions: 
                    //  System.IO.IOException
                    //  System.UnauthorizedAccessException
                    //  System.ArgumentException
                    //  System.ArgumentNullException
                    //  System.IO.PathTooLongException
                    //  System.IO.DirectoryNotFoundException
                    //  System.NotSupportedException".Replace("\r\n", "\n");
                    Npp.ShowCalltip(nc.position, tooltip);
#endif
                }
                else if (nc.nmhdr.code == (uint)SciMsg.SCN_DWELLEND)
                {
                    Npp.CancelCalltip();
                }
                else if (nc.nmhdr.code == (uint)NppMsg.NPPN_BUFFERACTIVATED)
                {
                    CSScriptIntellisense.Plugin.OnCurrentFileChanegd();
                    CSScriptNpp.Plugin.OnCurrentFileChanged();
                    Debugger.OnCurrentFileChanged();
                }
                else if (nc.nmhdr.code == (uint)NppMsg.NPPN_SHUTDOWN)
                {
                    Marshal.FreeHGlobal(_ptrPluginName);

                    CSScriptNpp.Plugin.CleanUp();
                }

                Plugin.OnNotification(nc);
            }
            catch { }//this is indeed the last line of defense as all CS-S calls have the error handling inside 
        }
    }
}