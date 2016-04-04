using NppPlugin.DllExport;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            try
            {
                Bootstrapper.Init();

                Plugin.NppData = notepadPlusData;

                InitPlugin();
            }
            catch (Exception e)
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Notepad++\plugins\logs\CSScriptNpp");

                MessageBox.Show("Cannot load the plugin.\nThe error information has been logged into '"+ dir + "' directory", "CS-Script");
                throw;
            }
        }

        static void InitPlugin()
        {
            try
            {
                CSScriptIntellisense.Plugin.NppData._nppHandle = Plugin.NppData._nppHandle;
                CSScriptIntellisense.Plugin.NppData._scintillaMainHandle = Plugin.NppData._scintillaMainHandle;
                CSScriptIntellisense.Plugin.NppData._scintillaSecondHandle = Plugin.NppData._scintillaSecondHandle;

                Intellisense.EnsureIntellisenseIntegration();

                CSScriptNpp.Plugin.CommandMenuInit(); //this will also call CSScriptIntellisense.Plugin.CommandMenuInit

                foreach (var item in CSScriptIntellisense.Plugin.FuncItems.Items)
                    Plugin.FuncItems.Add(item.ToLocal());

                CSScriptIntellisense.Plugin.FuncItems.Items.Clear();

                Debugger.OnFrameChanged += () => Npp.OnCalltipRequest(-2); //clear_all_cache
            }
            catch (Exception e)
            {
                MessageBox.Show("Initialization failure: " + e, "CS-Script");
            }
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
            //WM_ACTIVATE                     0x0006
            //WM_ACTIVATEAPP                  0x001C
            if (Message == 0x001C || Message == 0x0006)
                CSScriptNpp.Plugin.Repaint(); //some nasty painting artifacts need to be fixed when switching between apps
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

        static string lastActivatedBuffer = null;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            try
            {
                CSScriptIntellisense.Interop.NppUI.OnNppTick();

                SCNotification nc = (SCNotification)Marshal.PtrToStructure(notifyCode, typeof(SCNotification));

                //Debug.WriteLine(">>>>>   ncnc.nmhdr.code={0}, {1}", nc.nmhdr.code, (int)nc.nmhdr.code);

                if (nc.nmhdr.code == (uint)NppMsg.NPPN_READY)
                {
                    CSScriptIntellisense.Plugin.OnNppReady();
                    CSScriptNpp.Plugin.OnNppReady();
                    Npp.SetCalltipTime(200);
                }
                else if (nc.nmhdr.code == (uint)NppMsg.NPPN_TBMODIFICATION)
                {
                    CSScriptNpp.Plugin.OnToolbarUpdate();
                }
                else if (nc.nmhdr.code == (uint)NppMsg.NPPM_SAVECURRENTFILEAS ||
                         (Config.Instance.HandleSaveAs && nc.nmhdr.code == (uint)SciMsg.SCN_SAVEPOINTREACHED)) //for some strange reason NPP doesn't fire NPPM_SAVECURRENTFILEAS but does 2002 instead.
                {
                    string file = Npp.GetCurrentFile();
                    if (file != lastActivatedBuffer)
                        CSScriptNpp.Plugin.OnFileSavedAs(lastActivatedBuffer, file);
                }
                else if (nc.nmhdr.code == (uint)SciMsg.SCN_CHARADDED)
                {
                    CSScriptIntellisense.Plugin.OnCharTyped((char)nc.ch);
                }
                //else if (nc.nmhdr.code == (uint)SciMsg.SCN_KEY)
                //{
                //    System.Diagnostics.Debug.WriteLine("SciMsg.SCN_KEY");
                //}
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
                    //Npp.ShowCalltip(nc.position, "\u0001  1 of 3 \u0002  test tooltip " + Environment.TickCount);
                    //Npp.ShowCalltip(nc.position, CSScriptIntellisense.Npp.GetWordAtPosition(nc.position));
                    //tooltip = @"Creates all directories and subdirectories as specified by path.

                    Npp.OnCalltipRequest(nc.position);
                }
                else if (nc.nmhdr.code == (uint)SciMsg.SCN_DWELLEND)
                {
                    Npp.CancelCalltip();
                }
                else if (nc.nmhdr.code == (uint)NppMsg.NPPN_BUFFERACTIVATED)
                {
                    string file = Npp.GetCurrentFile();
                    lastActivatedBuffer = file;

                    if (file.EndsWith("npp.args"))
                    {
                        Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.IDM_FILE_CLOSE);

                        string args = File.ReadAllText(file);

                        Plugin.ProcessCommandArgs(args);

                        try { File.Delete(file); }
                        catch { }
                    }
                    else
                    {
                        CSScriptIntellisense.Plugin.OnCurrentFileChanegd();
                        CSScriptNpp.Plugin.OnCurrentFileChanged();
                        Debugger.OnCurrentFileChanged();
                    }
                }
                else if (nc.nmhdr.code == (uint)NppMsg.NPPN_FILEOPENED)
                {
                    string file = Npp.GetTabFile((int)nc.nmhdr.idFrom);
                    Debugger.LoadBreakPointsFor(file);
                }
                else if (nc.nmhdr.code == (uint)NppMsg.NPPN_FILESAVED || nc.nmhdr.code == (uint)NppMsg.NPPN_FILEBEFORECLOSE)
                {
                    string file = Npp.GetTabFile((int)nc.nmhdr.idFrom);
                    Debugger.RefreshBreakPointsFromContent();
                    Debugger.SaveBreakPointsFor(file);

                    if (nc.nmhdr.code == (uint)NppMsg.NPPN_FILESAVED)
                        Plugin.OnDocumentSaved();
                }
                else if (nc.nmhdr.code == (uint)NppMsg.NPPN_SHUTDOWN)
                {
                    Marshal.FreeHGlobal(_ptrPluginName);

                    Plugin.CleanUp();
                }

                if (nc.nmhdr.code == (uint)SciMsg.SCI_ENDUNDOACTION)
                {
                    //CSScriptIntellisense.Plugin.OnSavedOrUndo();
                }

                Plugin.OnNotification(nc);
            }
            catch { }//this is indeed the last line of defense as all CS-S calls have the error handling inside
        }
    }
}