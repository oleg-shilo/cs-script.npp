using System;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public delegate void SetMenuCommand(int cmdIndex, string name, Action handler, string shortcut);
}

namespace CSScriptIntellisense
{
    static partial class Plugin
    {
        #region " Fields "

        public static NppData NppData;
        public static FuncItems FuncItems = new FuncItems();

        #endregion " Fields "

        #region " Helper "

        internal static void SetCommand(int index, string commandName, Action functionPointer)
        {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), false);
        }

        public static void SetCommand(int index, string commandName, Action functionPointer, string shortcutSpec)
        {
            SetCommand(index, commandName, functionPointer, ParseAsShortcutKey(shortcutSpec), false);
        }

        internal static void SetCommand(int index, string commandName, Action functionPointer, ShortcutKey shortcut)
        {
            SetCommand(index, commandName, functionPointer, shortcut, false);
        }

        internal static void SetCommand(int index, string commandName, Action functionPointer, bool checkOnInit)
        {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), checkOnInit);
        }

        internal static void SetCommand(int index, string commandName, Action functionPointer, ShortcutKey shortcut, bool checkOnInit)
        {
            FuncItem funcItem = new FuncItem();
            funcItem._cmdID = index;
            funcItem._itemName = commandName;
            if (functionPointer != null)
                funcItem._pFunc = new Action(functionPointer);
            if (shortcut._key != 0)
                funcItem._pShKey = shortcut;
            funcItem._init2Check = checkOnInit;
            FuncItems.Add(funcItem);
        }

        public static IntPtr GetCurrentScintilla()
        {
            int curScintilla;
            Win32.SendMessage(NppData._nppHandle, NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
            return (curScintilla == 0) ? NppData._scintillaMainHandle : NppData._scintillaSecondHandle;
        }

        #endregion " Helper "
        public static ShortcutKey ParseAsShortcutKey(string shortcutSpec)
        {
            var parts = shortcutSpec.Split(':');

            string shortcutName = parts[0];
            string shortcutData = parts[1];

            try
            {
                var actualData = Config.Shortcuts.GetValue(shortcutName, shortcutData);
                return new ShortcutKey(actualData);
            }
            catch
            {
                Config.Shortcuts.SetValue(shortcutName, shortcutData);
                return new ShortcutKey(shortcutData);
            }
        }
    }
}