using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Xml.Linq;
using CSScriptIntellisense;
using Kbg.NppPluginNET;
using Kbg.NppPluginNET.PluginInfrastructure;

public class NppPluginBinder
{
    static public void bind(string hostAssemblyFile)
    {
        Assembly.LoadFrom(hostAssemblyFile); // to avoid complicated probing scenarios
    }
}

namespace CSScriptNpp
{
    public class UnmanagedExports : IUnmanagedExports
    {
        public bool isUnicode()
        {
            return true;
        }

        public void setInfo(NppData notepadPlusData)
        {
            try
            {
                PluginBase.nppData = notepadPlusData;
                Bootstrapper.Init();
                InitPlugin();
            }
            catch
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Notepad++\plugins\logs\CSScriptNpp");

                MessageBox.Show("Cannot load the plugin.\nThe error information has been logged into '" + dir + "' directory", "CS-Script");
                throw;
            }
        }

        void InitPlugin()
        {
            try
            {
                // Debug.Assert(false);

                Intellisense.EnsureIntellisenseIntegration();

                CSScriptNpp.Plugin.CommandMenuInit(); //this will also call CSScriptIntellisense.Plugin.CommandMenuInit

                // Debugger.OnFrameChanged += () => npp.OnCalltipRequest(-2); //clear_all_cache
            }
            catch (Exception e)
            {
                MessageBox.Show("Initialization failure: " + e, "CS-Script");
            }
        }

        public IntPtr getFuncsArray(ref int nbF)
        {
            nbF = PluginBase._funcItems.Items.Count;
            return PluginBase._funcItems.NativePointer;
        }

        public uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            //WM_ACTIVATE                     0x0006
            //WM_ACTIVATEAPP                  0x001C
            if (Message == 0x001C || Message == 0x0006)
                CSScriptNpp.Plugin.Repaint(); //some nasty painting artifacts need to be fixed when switching between apps

            return 1;
        }

        static IntPtr _ptrPluginName = IntPtr.Zero;

        public IntPtr getName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(Plugin.PluginName);
            return _ptrPluginName;
        }

        const int _SC_MARGE_SYBOLE = 1; //bookmark and breakpoint margin
        const int SCI_CTRL = 2; //Ctrl pressed modifier for SCN_MARGINCLICK

        static string lastActivatedBuffer = null;

        public void beNotified(IntPtr notifyCode)
        {
            lock (typeof(UnmanagedExports))
            {
                try
                {
                    CSScriptIntellisense.Interop.NppUI.OnNppTick();

                    ScNotification nc = (ScNotification)Marshal.PtrToStructure(notifyCode, typeof(ScNotification));
                    string contentFile = Npp.Editor.GetTabFile(nc.Header.IdFrom);

                    //Debug.WriteLine(">>>>>   ncnc.nmhdr.code={0}, {1}", nc.nmhdr.code, (int)nc.nmhdr.code);

                    if (nc.Header.Code == (uint)NppMsg.NPPN_READY)
                    {
                        CSScriptIntellisense.Plugin.OnNppReady();
                        CSScriptNpp.Plugin.OnNppReady();
                        npp.SetCalltipTime(500);
                    }
                    else if (nc.Header.Code == (uint)NppMsg.NPPN_SHUTDOWN)
                    {
                        CSScriptNpp.Plugin.StopVBCSCompilers();
                    }
                    else if (nc.Header.Code == (uint)NppMsg.NPPN_TBMODIFICATION)
                    {
                        CSScriptNpp.Plugin.OnToolbarUpdate();
                    }
                    else if (nc.Header.Code == (uint)NppMsg.NPPM_SAVECURRENTFILEAS ||
                             (Config.Instance.HandleSaveAs && nc.Header.Code == (uint)SciMsg.SCN_SAVEPOINTREACHED)) //for some strange reason NPP doesn't fire NPPM_SAVECURRENTFILEAS but does 2002 instead.
                    {
                        if (Plugin.ProjectPanel != null)
                        {
                            var panel_visible = Plugin.ProjectPanel.Visible;
                        }

                        string file = Npp.Editor.GetCurrentFilePath();
                        if (file != lastActivatedBuffer)
                            CSScriptNpp.Plugin.OnFileSavedAs(lastActivatedBuffer, file);
                    }
                    else if (nc.Header.Code == (uint)SciMsg.SCN_CHARADDED)
                    {
                        if (nc.character == 0)
                        {
                            // There is a defect either in Scintilla, Npp ot Npp interop, which prevents correct
                            // SciMsg.SCN_CHARADDED notification. This leads to `nc.character` being set to zero.
                            // Detected in v8.6.2 Npp.
                            //
                            // So extract the character from side of the caret.
                            // This is only a work around as any direct solution is problematic since it is a Scintilla
                            // change in behavior.

                            var document = Npp.GetCurrentDocument();
                            var caret = document.GetCurrentPos();

                            if (caret > 0)
                            {
                                string textOnLeft = document.GetTextBetween(caret - 1, caret);
                                CSScriptIntellisense.Plugin.OnCharTyped(textOnLeft.LastOrDefault());
                            }
                        }
                        else
                            CSScriptIntellisense.Plugin.OnCharTyped(nc.Character);
                    }
                    else if (nc.Header.Code == (uint)SciMsg.SCN_DWELLSTART) //tooltip
                    {
                        //Npp.ShowCalltip(nc.position, "\u0001  1 of 3 \u0002  test tooltip " + Environment.TickCount);
                        //Npp.ShowCalltip(nc.position, CSScriptIntellisense.Npp.GetWordAtPosition(nc.position));
                        //tooltip = @"Creates all directories and subdirectories as specified by path.

                        if (nc.Position.Value != -1)
                            npp.OnCalltipRequest(nc.Position.Value);
                    }
                    else if (nc.Header.Code == (uint)SciMsg.SCN_DWELLEND)
                    {
                        npp.CancelCalltip();
                    }
                    else if (nc.Header.Code == (uint)NppMsg.NPPN_BUFFERACTIVATED)
                    {
                        string file = Npp.Editor.GetCurrentFilePath();
                        lastActivatedBuffer = file;

                        if (file.EndsWith("npp.args"))
                        {
                            Win32.SendMessage(Npp.Editor.Handle, (uint)NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.IDM_FILE_CLOSE);

                            string args = File.ReadAllText(file);

                            Plugin.ProcessCommandArgs(args);

                            try { File.Delete(file); }
                            catch { }
                        }
                        else
                        {
                            CSScriptIntellisense.Plugin.OnCurrentFileChanegd();
                            CSScriptNpp.Plugin.OnCurrentFileChanged();
                        }
                    }
                    else if (nc.Header.Code == (uint)NppMsg.NPPN_FILEOPENED)
                    {
                        string file = Npp.Editor.GetTabFile(nc.Header.IdFrom);
                    }
                    else if (nc.Header.Code == (uint)NppMsg.NPPN_FILESAVED)
                    {
                        Plugin.OnDocumentSaved();
                    }
                    else if (nc.Header.Code == (uint)NppMsg.NPPN_FILEBEFORECLOSE)
                    {
                    }
                    else if (nc.Header.Code == (uint)NppMsg.NPPN_FILEBEFORESAVE)
                    {
                        CSScriptIntellisense.Plugin.OnBeforeDocumentSaved();
                    }
                    else if (nc.Header.Code == (uint)NppMsg.NPPN_SHUTDOWN)
                    {
                        Marshal.FreeHGlobal(_ptrPluginName);
                        Plugin.CleanUp();
                    }

                    if (nc.Header.Code == (uint)SciMsg.SCI_ENDUNDOACTION)
                    {
                    }

                    Plugin.OnNotification(nc);
                }
                catch { }//this is indeed the last line of defense as all CS-S calls have the error handling inside
            }
        }
    }
}