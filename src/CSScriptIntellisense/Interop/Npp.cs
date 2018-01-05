using CSScriptIntellisense;
using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

public partial class PluginEnv
{
    public static string ConfigDir
    {
        get
        {
            return Npp.Editor.GetPluginsConfigDir().PathJoin("CSScriptNpp").EnsureDir();
        }
    }

    public static string LogDir
    {
        get
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Notepad++\plugins\logs\CSScriptNpp");
            return dir.EnsureDir();
        }
    }

    public static string PluginDir
    {
        get
        {
            return Assembly.GetExecutingAssembly().Location.GetDirName();
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
}

namespace CSScriptIntellisense
{
    public static class NppExtensions
    {
        public static int GetCurrentLineNumber(this ScintillaGateway document)
        {
            return document.LineFromPosition(document.GetCurrentPos());
        }

        public static string GetCurrentLine(this ScintillaGateway document)
        {
            return document.GetLine(document.LineFromPosition(document.GetCurrentPos()));
        }

        static public string GetTextBetween(this ScintillaGateway document, Point point)
        {
            return GetTextBetween(document, point.X, point.Y);
        }

        static public string GetTextBetween(this ScintillaGateway document, int start, int end = -1)
        {
            if (end == -1)
                end = document.GetLength();

            using (var tr = new TextRange(start, end, end - start + 1)) //+1 for null termination
            {
                document.GetTextRange(tr);
                return tr.lpstrText;
            }
        }

        static public void SetTextBetween(this ScintillaGateway document, string text, Point point)
        {
            document.SetTextBetween(text, point.X, point.Y);
        }

        static public void SetTextBetween(this ScintillaGateway document, string text, int start, int end = -1)
        {
            //supposed not to scroll

            if (end == -1)
                end = document.GetLength();

            document.SetTargetStart(new Position(start));
            document.SetTargetEnd(new Position(end));
            document.ReplaceTarget(text.Length, text);
        }

        public static int CaretToTextPosition(this ScintillaGateway document, int position)
        {
            string text = document.GetTextBetween(0, position);
            return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text)).Length;
        }

        public static string GetSelectedText(this ScintillaGateway document)
        {
            int start = document.GetSelectionStart();
            int end = document.GetSelectionEnd();
            return document.GetTextBetween(start, end);
        }

        static public void SetIndicatorStyle(this ScintillaGateway document, int indicator, SciMsg style, Color color)
        {
            document.IndicSetStyle(indicator, (int)style);
            document.IndicSetFore(indicator, new Colour(ColorTranslator.ToWin32(color)));
        }

        static public void ClearIndicator(this ScintillaGateway document, int indicator, int startPos, int endPos)
        {
            document.SetIndicatorCurrent(indicator);
            document.IndicatorClearRange(startPos, endPos - startPos);
        }

        static public void PlaceIndicator(this ScintillaGateway document, int indicator, int startPos, int endPos)
        {
            // !!!
            // The implementation is identical to "ClearIndicator". Looks like intentional.
            document.SetIndicatorCurrent(indicator);
            document.IndicatorClearRange(startPos, endPos - startPos);
        }
    }

    public class Npp1
    {
        public static NotepadPPGateway Editor => PluginBase.Editor;

        public static ScintillaGateway GetCurrentDocument() => (ScintillaGateway)PluginBase.GetCurrentDocument();

        public const int DocEnd = -1;

        static public void DisplayInNewDocument(string text)
        {
            Editor.FileNew();

            var document = PluginBase.GetCurrentDocument();
            document.GrabFocus();
            document.AddText(text);
        }

        static public bool IsCurrentScriptFile()
        {
            return Npp.Editor.GetCurrentFilePath().IsScriptFile();
        }

        static public void ReloadFile(bool showAlert, string file)
        {
            Editor.FileNew();
            Editor.ReloadFile(file, showAlert);
        }

        const int SW_SHOWNOACTIVATE = 4;
        const int HWND_TOPMOST = -1;
        const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(
             int hWnd,             // Window handle
             int hWndInsertAfter,  // Placement-order handle
             int X,                // Horizontal position
             int Y,                // Vertical position
             int cx,               // Width
             int cy,               // Height
             uint uFlags);         // Window positioning flags

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void ShowInactiveTopmost(Form frm)
        {
            ShowWindow(frm.Handle, SW_SHOWNOACTIVATE);
            //SetWindowPos(frm.Handle.ToInt32(), HWND_TOPMOST, frm.Left, frm.Top, frm.Width, frm.Height, SWP_NOACTIVATE);
            SetWindowPos(frm.Handle.ToInt32(), PluginBase.nppData._nppHandle.ToInt32(), frm.Left, frm.Top, frm.Width, frm.Height, SWP_NOACTIVATE);
        }

        static public Point[] FindIndicatorRanges(int indicator)
        {
            var ranges = new List<Point>();

            IntPtr sci = PluginBase.GetCurrentScintilla();

            int testPosition = 0;

            while (true)
            {
                //finding the indicator ranges
                //For example indicator 4..6 in the doc 0..10 will have three logical regions:
                //0..4, 4..6, 6..10
                //Probing will produce following when outcome:
                //probe for 0 : 0..4
                //probe for 4 : 4..6
                //probe for 6 : 4..10

                int rangeStart = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATORSTART, indicator, testPosition);
                int rangeEnd = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATOREND, indicator, testPosition);
                int value = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATORVALUEAT, indicator, testPosition);
                if (value == 1) //indicator is present
                    ranges.Add(new Point(rangeStart, rangeEnd));

                if (testPosition == rangeEnd)
                    break;

                testPosition = rangeEnd;
            }

            return ranges.ToArray();
        }

        static public int GetLineNumber(int position)
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, SciMsg.SCI_LINEFROMPOSITION, position, 0);
        }

        static public int GetLineStart(int line)
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, SciMsg.SCI_POSITIONFROMLINE, line, 0);
        }

        static public int GetFirstVisibleLine()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, SciMsg.SCI_GETFIRSTVISIBLELINE, 0, 0);
        }

        static public void SetFirstVisibleLine(int line)
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SETFIRSTVISIBLELINE, line, 0);
        }

        static public int GetLineCount()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, SciMsg.SCI_GETLINECOUNT, 0, 0);
        }

        static public string GetShortcutsFile()
        {
            return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Npp.Editor.GetPluginsConfigDir())), "shortcuts.xml");
        }

        static public string GetNppConfigFile()
        {
            return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Npp.Editor.GetPluginsConfigDir())), "config.xml");
        }

        static public string ContextMenuFile
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Npp.Editor.GetPluginsConfigDir())), "contextMenu.xml");
            }
        }

        //public delegate void NppFuncItemDelegate();

        [DllImport("user32")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        public static extern long GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        static public Point GetCaretScreenLocation()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            int pos = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            int x = (int)Win32.SendMessage(sci, SciMsg.SCI_POINTXFROMPOSITION, 0, pos);
            int y = (int)Win32.SendMessage(sci, SciMsg.SCI_POINTYFROMPOSITION, 0, pos);

            Point point = new Point(x, y);
            ClientToScreen(sci, ref point);
            return point;
        }

        static public int GetPositionFromLineColumn(int line, int column)
        {
            return Npp1.GetLineStart(line) + column;
        }

        static public int GetPositionFromMouseLocation()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();

            Point point = Cursor.Position;
            ScreenToClient(sci, ref point);

            int pos = (int)Win32.SendMessage(sci, SciMsg.SCI_CHARPOSITIONFROMPOINTCLOSE, point.X, point.Y);

            return pos;
        }

        static public void Exit()
        {
            const int WM_COMMAND = 0x111;
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)WM_COMMAND, (int)NppMenuCmd.IDM_FILE_EXIT, 0);
        }

        static public string TextAfterCursor(int maxLength)
        {
            IntPtr hCurrentEditView = PluginBase.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            return TextAfterPosition(currentPos, maxLength);
        }

        static public string TextAfterPosition(int position, int maxLength)
        {
            int bufCapacity = maxLength + 1;
            IntPtr hCurrentEditView = PluginBase.GetCurrentScintilla();
            int currentPos = position;
            int fullLength = (int)Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETLENGTH, 0, 0);
            int startPos = currentPos;
            int endPos = Math.Min(currentPos + bufCapacity, fullLength);
            int size = endPos - startPos;

            if (size > 0)
            {
                using (var tr = new TextRange(startPos, endPos, bufCapacity))
                {
                    Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);
                    return tr.lpstrText;
                }
            }
            else
                return null;
        }

        static public void ReplaceWordAtCaret(string text)
        {
            // IntPtr sci = PluginBase.GetCurrentScintilla();

            Point p;
            string word = Npp1.GetWordAtCursor(out p, SimpleCodeCompletion.Delimiters);

            // Win32.SendMessage(sci, SciMsg.SCI_SETSELECTION, p.X, p.Y);
            // Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, text);

            var document = PluginBase.GetCurrentDocument();
            document.SetSel(new Position(p.X), new Position(p.Y));
            document.ReplaceSel(text);
        }

        static public string GetWordAtCursor(char[] wordDelimiters = null)
        {
            Point point;
            return GetWordAtCursor(out point, wordDelimiters);
        }

        static public string GetWordAtCursor(out Point point, char[] wordDelimiters = null)
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            return GetWordAtPosition(currentPos, out point, wordDelimiters);
        }

        static public string GetWordAtPosition(int position, char[] wordDelimiters = null)
        {
            Point point;
            return GetWordAtPosition(position, out point, wordDelimiters);
        }

        static char[] statementDelimiters = " ,:;'\"=[]{}()".ToCharArray();

        static public string GetStatementAtPosition(int position = -1)
        {
            Point point;
            if (position == -1)
                position = Npp1.GetCaretPosition();

            var retval = GetWordAtPosition(position, out point, statementDelimiters);

            return retval;
        }

        static public string GetWordAtPosition(int position, out Point point, char[] wordDelimiters = null)
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();

            int currentPos = position;
            int fullLength = (int)Win32.SendMessage(sci, SciMsg.SCI_GETLENGTH, 0, 0);

            string leftText = Npp1.TextBeforePosition(currentPos, 512);
            string rightText = Npp1.TextAfterPosition(currentPos, 512);

            //if updating do not forger to update SimpleCodeCompletion.Delimiters
            var delimiters = "\\·\t\n\r .,:;'\"=[]{}()+-/!?@$%^&*«»><#|~`".ToCharArray();

            if (wordDelimiters != null)
                delimiters = wordDelimiters;

            string wordLeftPart = "";
            int startPos = currentPos;

            if (leftText != null)
            {
                bool startOfDoc = leftText.Length == currentPos;
                startPos = leftText.LastIndexOfAny(delimiters);
                wordLeftPart = (startPos != -1) ? leftText.Substring(startPos + 1) : (startOfDoc ? leftText : "");
                int relativeStartPos = leftText.Length - startPos;
                startPos = (startPos != -1) ? (currentPos - relativeStartPos) + 1 : 0;
            }

            string wordRightPart = "";
            int endPos = currentPos;
            if (rightText != null)
            {
                endPos = rightText.IndexOfAny(delimiters);
                wordRightPart = (endPos != -1) ? rightText.Substring(0, endPos) : "";
                endPos = (endPos != -1) ? currentPos + endPos : fullLength;
            }

            point = new Point(startPos, endPos);
            return wordLeftPart + wordRightPart;
        }

        static public IntPtr CurrentScintilla
        {
            get { return PluginBase.GetCurrentScintilla(); }
        }

        static public IntPtr NppHandle
        {
            get { return PluginBase.nppData._nppHandle; }
        }

        public static int GetCaretPosition()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            return currentPos;
        }

        public static int SetCaretPosition(int pos)
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SETCURRENTPOS, pos, 0);
            return (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
        }

        public static void Undo()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_UNDO, 0, 0);
        }

        public static bool CanUndo()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            return 0 != (int)Win32.SendMessage(sci, SciMsg.SCI_CANUNDO, 0, 0);
        }

        static int execute(SciMsg msg, int wParam, int lParam = 0)
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, msg, wParam, lParam);
        }

        static int execute(NppMsg msg, int wParam, int lParam = 0)
        {
            return (int)Win32.SendMessage(Npp1.NppHandle, (uint)msg, wParam, lParam);
        }

        public static void ClearSelection()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, currentPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, currentPos, 0);
        }

        public static void SetSelection(int start, int end)
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, start, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, end, 0);
        }

        public static void SetSelectionText(string text)
        {
            // IntPtr sci = PluginBase.GetCurrentScintilla();
            // Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, text);

            PluginBase.GetCurrentDocument().ReplaceSel(text);
        }

        static public int GrabFocus()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(sci, SciMsg.SCI_GRABFOCUS, 0, 0);
            return currentPos;
        }

        static public void ScrollToCaret()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SCROLLCARET, 0, 0);
            Win32.SendMessage(sci, SciMsg.SCI_LINESCROLL, 0, 1); //bottom scrollbar can hide the line
            Win32.SendMessage(sci, SciMsg.SCI_SCROLLCARET, 0, 0);
        }

        static public void OpenFile(string file)
        {
            IntPtr sci = PluginBase.nppData._nppHandle;
            Win32.SendMessage(sci, (uint)NppMsg.NPPM_DOOPEN, 0, file);
        }

        static public void GoToLine(int line)
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            //Win32.SendMessage(sci, SciMsg.SCI_ENSUREVISIBLE, line - 1, 0);
            Win32.SendMessage(sci, SciMsg.SCI_GOTOLINE, line + 20, 0);
            Win32.SendMessage(sci, SciMsg.SCI_GOTOLINE, line - 1, 0);
            /*
            SCI_GETFIRSTVISIBLELINE = 2152,
            SCI_GETLINE = 2153,
            SCI_GETLINECOUNT = 2154,*/
        }

        static public Rectangle GetClientRect()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();

            var r = new Rectangle();
            GetWindowRect(sci, ref r);
            return r;
        }

        static public Rectangle GetWindowRect()
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();

            var r = new Rectangle();
            GetWindowRect(sci, ref r);
            return r;
        }

        static public string TextBeforePosition(int position, int maxLength)
        {
            int bufCapacity = maxLength + 1;
            IntPtr hCurrentEditView = PluginBase.GetCurrentScintilla();
            int currentPos = position;
            int beginPos = currentPos - maxLength;
            int startPos = (beginPos > 0) ? beginPos : 0;
            int size = currentPos - startPos;

            if (size > 0)
            {
                using (var tr = new TextRange(startPos, currentPos, bufCapacity))
                {
                    Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);
                    return tr.lpstrText;
                }
            }
            else
                return null;
        }

        static public string TextBeforeCursor(int maxLength)
        {
            IntPtr hCurrentEditView = PluginBase.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETCURRENTPOS, 0, 0);

            return TextBeforePosition(currentPos, maxLength);
        }

        /// <summary>
        /// Retrieve the height of a particular line of text in pixels.
        /// </summary>
        static public int GetTextHeight(int line)
        {
            return (int)Win32.SendMessage(CurrentScintilla, SciMsg.SCI_TEXTHEIGHT, line, 0);
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, IntPtr lParam);

        public const int SB_SETTEXT = 1035;
        public const int SB_SETPARTS = 1028;
        public const int SB_GETPARTS = 1030;

        private const uint WM_USER = 0x0400;

        //private const uint SB_SETPARTS = WM_USER + 4;
        //private const uint SB_GETPARTS = WM_USER + 6;
        private const uint SB_GETTEXTLENGTH = WM_USER + 12;

        private const uint SB_GETTEXT = WM_USER + 13;

        static public string GetStatusbarLabel()
        {
            throw new NotImplementedException();
        }

        //static public unsafe void SetStatusbarLabel(string labelText)
        static public string SetStatusbarLabel(string labelText)
        {
            string retval = null;
            IntPtr mainWindowHandle = PluginBase.nppData._nppHandle;
            // find status bar control on the main window of the application
            IntPtr statusBarHandle = FindWindowEx(mainWindowHandle, IntPtr.Zero, "msctls_statusbar32", IntPtr.Zero);
            if (statusBarHandle != IntPtr.Zero)
            {
                //cut current text
                var size = (int)SendMessage(statusBarHandle, SB_GETTEXTLENGTH, 0, IntPtr.Zero);
                var buffer = new StringBuilder(size);
                Win32.SendMessage(statusBarHandle, (uint)SB_GETTEXT, 0, buffer);
                retval = buffer.ToString();

                // set text for the existing part with index 0
                IntPtr text = Marshal.StringToHGlobalAuto(labelText);
                SendMessage(statusBarHandle, SB_SETTEXT, 0, text);

                Marshal.FreeHGlobal(text);

                //the foolowing may be needed for puture features
                // create new parts width array
                //int nParts = SendMessage(statusBarHandle, SB_GETPARTS, 0, IntPtr.Zero).ToInt32();
                //nParts++;
                //IntPtr memPtr = Marshal.AllocHGlobal(sizeof(int) * nParts);
                //int partWidth = 100; // set parts width according to the form size
                //for (int i = 0; i < nParts; i++)
                //{
                //    Marshal.WriteInt32(memPtr, i * sizeof(int), partWidth);
                //    partWidth += partWidth;
                //}
                //SendMessage(statusBarHandle, SB_SETPARTS, nParts, memPtr);
                //Marshal.FreeHGlobal(memPtr);

                //// set text for the new part
                //IntPtr text0 = Marshal.StringToHGlobalAuto("new section text 1");
                //SendMessage(statusBarHandle, SB_SETTEXT, nParts - 1, text0);
                //Marshal.FreeHGlobal(text0);
            }
            return retval;
        }
    }

    static class GenericExtensions
    {
        public static void SetSelection(this IScintillaGateway document, int start, int end)
        {
            document.SetSelectionStart(start.ToPosition());
            document.SetSelectionEnd(end.ToPosition());
        }

        public static Position ToPosition(this int pos)
        {
            return new Position(pos);
        }

        public static int IndexOfFirst<T>(this IEnumerable<T> collection, Predicate<T> condition)
        {
            var indexOf = collection.Select((item, index) => new { item, index })
                                    .FirstOrDefault(entry => condition(entry.item))?.index ?? -1;
            return indexOf;
        }

        public static int IndexOfLast<T>(this IEnumerable<T> collection, Predicate<T> condition)
        {
            var indexOf = collection.Select((item, index) => new { item, index })
                                    .LastOrDefault(entry => condition(entry.item))?.index ?? -1;
            return indexOf;
        }
    }
}