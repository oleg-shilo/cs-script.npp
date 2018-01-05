// NPP plugin platform for .Net v0.93.96 by Kasper B. Graversen etc.
using System;
using System.Windows.Forms;

namespace Kbg.NppPluginNET.PluginInfrastructure
{
    // cs-script.npp
    public delegate void SetMenuCommand(int cmdIndex, string name, NppFuncItemDelegate handler, string shortcut);

    // cs-script.npp
    public partial class PluginBase
    {
        static NotepadPPGateway editor;

        public static IScintillaGateway GetCurrentDocument()
        {
            return GetGatewayFactory()();
        }

        public static NotepadPPGateway Editor
        {
            get
            {
                return editor ?? (editor = new NotepadPPGateway());
            }
        }
    }

    public partial class PluginBase
    {
        public static NppData nppData;
        public static FuncItems _funcItems = new FuncItems();

        public static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer)
        {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), false);
        }

        public static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut)
        {
            SetCommand(index, commandName, functionPointer, shortcut, false);
        }

        public static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, bool checkOnInit)
        {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), checkOnInit);
        }

        public static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit)
        {
            FuncItem funcItem = new FuncItem();
            funcItem._cmdID = index;
            funcItem._itemName = commandName;
            if (functionPointer != null)
                funcItem._pFunc = new NppFuncItemDelegate(functionPointer);
            if (shortcut._key != 0)
                funcItem._pShKey = shortcut;
            funcItem._init2Check = checkOnInit;
            _funcItems.Add(funcItem);
        }

        public static IntPtr GetCurrentScintilla()
        {
            int curScintilla;
            Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
            return (curScintilla == 0) ? nppData._scintillaMainHandle : nppData._scintillaSecondHandle;
        }

        static readonly Func<IScintillaGateway> gatewayFactory = () => new ScintillaGateway(GetCurrentScintilla());

        public static Func<IScintillaGateway> GetGatewayFactory()
        {
            return gatewayFactory;
        }
    }
}