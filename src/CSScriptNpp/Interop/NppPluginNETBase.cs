using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CSScriptNpp
{
    partial class Plugin
    {
        // public static NppData NppData;
        // public static FuncItems FuncItems = new FuncItems();

        public static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer)
        {
            PluginBase.SetCommand(index, commandName, functionPointer, new ShortcutKey(), false);
        }

        // public static void SetCommand(int index, string commandName, Action functionPointer, ShortcutKey shortcut)
        // {
        //     SetCommand(index, commandName, functionPointer, shortcut, false);
        // }

        public static void SetCommand(int index, string commandName, Action functionPointer, string shortcutSpec)
        {
            ShortcutKey shortcutKey;

            if (string.IsNullOrEmpty(shortcutSpec))
                shortcutKey = new ShortcutKey(false, false, false, Keys.None);
            else
                shortcutKey = shortcutSpec.ParseAsShortcutKey(commandName);

            PluginBase.SetCommand(index, commandName, new NppFuncItemDelegate(functionPointer), shortcutKey, false);
        }

        // public static void SetCommand(int index, string commandName, Action functionPointer, bool checkOnInit)
        // {
        //     SetCommand(index, commandName, functionPointer, new ShortcutKey(), checkOnInit);
        // }

        // public static void SetCommand(int index, string commandName, Action functionPointer, ShortcutKey shortcut, bool checkOnInit)
        // {
        //     FuncItem funcItem = new FuncItem();
        //     funcItem._cmdID = index;
        //     funcItem._itemName = commandName;
        //     if (functionPointer != null)
        //         funcItem._pFunc = new Action(functionPointer);
        //     if (shortcut._key != 0)
        //         funcItem._pShKey = shortcut;
        //     funcItem._init2Check = checkOnInit;
        //     FuncItems.Add(funcItem);
        // }

        // public static IntPtr GetCurrentScintilla()
        // {
        //     int curScintilla;
        //     Win32.SendMessage(NppData._nppHandle, NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
        //     return (curScintilla == 0) ? NppData._scintillaMainHandle : NppData._scintillaSecondHandle;
        // }

        public static string ConfigDir
        {
            get
            {
                var configDir = Path.Combine(Npp.GetConfigDir(), Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location));

                if (!Directory.Exists(configDir))
                    Directory.CreateDirectory(configDir);

                return configDir;
            }
        }

        public static string LogDir
        {
            get
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Notepad++\plugins\logs\CSScriptNpp");

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return dir;
            }
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

        public static string PluginDir
        {
            get
            {
                string assemblyFile = Assembly.GetExecutingAssembly().Location;
                return Path.Combine(Path.GetDirectoryName(assemblyFile), Path.GetFileNameWithoutExtension(assemblyFile));
            }
        }

        static public void SetToolbarImage(Bitmap image, int pluginId)
        {
            var tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = image.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[pluginId]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        public static void DockPanel(Form panel, int scriptId, string name, Icon tollbarIcon, NppTbMsg tbMsg, bool initiallyVisible = true)
        {
            var tbIcon = tollbarIcon ?? Utils.NppBitmapToIcon(Resources.Resources.css_logo_16x16);

            NppTbData _nppTbData = new NppTbData();
            _nppTbData.hClient = panel.Handle;
            _nppTbData.pszName = name;
            // the dlgDlg should be the index of funcItem where the current function pointer is,
            //in this case is 15. so the initial value of funcItem[15]._cmdID - not the updated internal one !
            _nppTbData.dlgID = scriptId;
            // define the default docking behavior
            _nppTbData.uMask = tbMsg;
            //_nppTbData.uMask = NppTbMsg.DWS_DF_CONT_BOTTOM | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
            _nppTbData.hIconTab = (uint)tbIcon.Handle;
            _nppTbData.pszModuleName = PluginName;
            IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
            Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

            Win32.SendMessage(Npp.NppHandle, (uint)NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);

            //Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SETMENUITEMCHECK, Plugin.FuncItems.Items[scriptId]._cmdID, 1); //from this moment the panel is visible

            if (!initiallyVisible)
                SetDockedPanelVisible(panel, scriptId, initiallyVisible);

            if (dockedManagedPanels.ContainsKey(scriptId))
            {
                //there is already another panel
                Win32.SendMessage(Npp.NppHandle, (uint)NppMsg.NPPM_DMMHIDE, 0, dockedManagedPanels[scriptId].Handle);
                dockedManagedPanels[scriptId] = panel;
            }
            else
                dockedManagedPanels.Add(scriptId, panel);
        }

        public static void ToggleDockedPanelVisible(Form panel, int scriptId)
        {
            SetDockedPanelVisible(panel, scriptId, !panel.Visible);
        }

        public static void SetDockedPanelVisible(Form panel, int scriptId, bool visible)
        {
            if (visible)
            {
                Win32.SendMessage(Npp.NppHandle, (uint)NppMsg.NPPM_DMMSHOW, 0, panel.Handle);
                //Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SETMENUITEMCHECK, Plugin.FuncItems.Items[scriptId]._cmdID, 1);
            }
            else
            {
                Win32.SendMessage(Npp.NppHandle, (uint)NppMsg.NPPM_DMMHIDE, 0, panel.Handle);
                //Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SETMENUITEMCHECK, Plugin.FuncItems.Items[scriptId]._cmdID, 0);
            }
        }

        static T ShowDockablePanel<T>(string name, int panelId, NppTbMsg tbMsg) where T : Form, new()
        {
            if (!dockedManagedPanels.ContainsKey(panelId))
            {
                var panel = new T();
                DockPanel(panel, panelId, name, null, tbMsg); //this will also add the panel to the dockedManagedPanels

                //disabled chck box in menu item since tracking of the visibility of the output panes is impossible (N++ limitation)
                //Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SETMENUITEMCHECK, FuncItems.Items[panelId]._cmdID, 1);
            }
            else
            {
                ToggleDockedPanelVisible(dockedManagedPanels[panelId], panelId);
            }
            return (T)dockedManagedPanels[panelId];
        }

        static Dictionary<int, Form> dockedManagedPanels = new Dictionary<int, Form>();

        static public void RestartNpp(int? afterProcessExit = null)
        {
            Win32.SendMenuCmd(Npp.NppHandle, NppMenuCmd.IDM_FILE_EXIT, 0);

            string file = Npp.GetCurrentFile();

            if (string.IsNullOrEmpty(file)) //the Exit request has been processed and user did not cancel it
            {
                Application.DoEvents();

                int processIdToWaitForExit = afterProcessExit ?? Process.GetCurrentProcess().Id;
                string appToStart = Process.GetCurrentProcess().MainModule.FileName;

                string restarter = Path.Combine(Plugin.PluginDir, "launcher.exe");

                //the re-starter will also wait for this process to exit
                Process.Start(restarter, string.Format("/start {0} \"{1}\"", processIdToWaitForExit, appToStart));
            }
        }
    }
}