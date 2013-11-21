using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CSScriptIntellisense
{
    class Npp
    {
        public const int DocEnd = -1;

        static public string GetCurrentFile()
        {
            var path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(Plugin.NppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            return path.ToString();
        }

        static public void DisplayInNewDocument(string text)
        {
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.IDM_FILE_NEW);
            Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
            Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_ADDTEXT, text.Length, text);
        }

        /// <summary>
        /// Determines whether the current file has the specified extension (e.g. ".cs").
        /// <para>Note it is case insensitive.</para>
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        static public bool IsCurrentFileHasExtension(string extension)
        {
            var path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(Plugin.NppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            string file = path.ToString();
            return !string.IsNullOrWhiteSpace(file) && file.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
        }

        static public bool IsCurrentScriptFile()
        {
            var path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(Plugin.NppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            string file = path.ToString();
            return !string.IsNullOrWhiteSpace(file) && file.IsScriptFile();
        }

        static public string GetConfigDir()
        {
            var buffer = new StringBuilder(260);
            Win32.SendMessage(Plugin.NppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, 260, buffer);
            return buffer.ToString();
        }

        static public string GetLine(int line)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();

            int length = (int)Win32.SendMessage(sci, SciMsg.SCI_LINELENGTH, line, 0);
            var buffer = new StringBuilder(length + 1);
            Win32.SendMessage(sci, SciMsg.SCI_GETLINE, line, buffer);
            buffer.Length = length; //NPP may inject some rubbish at the end of the line
            return buffer.ToString();
        }

        static public int GetLineNumber(int position)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, SciMsg.SCI_LINEFROMPOSITION, position, 0);
        }

        static public int GetLineStart(int line)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, SciMsg.SCI_POSITIONFROMLINE, line, 0);
        }

        static public int GetFirstVisibleLine()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, SciMsg.SCI_GETFIRSTVISIBLELINE, 0, 0);
        }

        static public void SetFirstVisibleLine(int line)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SETFIRSTVISIBLELINE, line, 0);
        }

        static public int GetLineCount()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int)Win32.SendMessage(sci, SciMsg.SCI_GETLINECOUNT, 0, 0);
        }

        static public string GetShortcutsFile()
        {
            return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Npp.GetConfigDir())), "shortcuts.xml");
        }

        [DllImport("user32")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        public static extern long GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        static public Point GetCaretScreenLocation()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            int pos = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            int x = (int)Win32.SendMessage(sci, SciMsg.SCI_POINTXFROMPOSITION, 0, pos);
            int y = (int)Win32.SendMessage(sci, SciMsg.SCI_POINTYFROMPOSITION, 0, pos);

            Point point = new Point(x, y);
            ClientToScreen(sci, ref point);
            return point;
        }

        static public int GetPositionFromMouseLocation()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();

            Point point = Cursor.Position;
            ScreenToClient(sci, ref point);

            int pos = (int)Win32.SendMessage(sci, SciMsg.SCI_CHARPOSITIONFROMPOINTCLOSE, point.X, point.Y);

            return pos;
        }

        static public void Exit()
        {
            const int WM_COMMAND = 0x111;
            Win32.SendMessage(Plugin.NppData._nppHandle, (NppMsg)WM_COMMAND, (int)NppMenuCmd.IDM_FILE_EXIT, 0);
        }

        static public string GetTextBetween(int start, int end = -1)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();

            if (end == -1)
                end = (int)Win32.SendMessage(sci, SciMsg.SCI_GETLENGTH, 0, 0);

            using (var tr = new Sci_TextRange(start, end, end - start + 1)) //+1 for null termination
            {
                Win32.SendMessage(sci, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);
                return tr.lpstrText;
            }
        }

        static public void SetTextBetween(string text, int start, int end = -1)
        {
            //supposed not to scroll
            IntPtr sci = Plugin.GetCurrentScintilla();

            if (end == -1)
                end = (int)Win32.SendMessage(sci, SciMsg.SCI_GETLENGTH, 0, 0);

            Win32.SendMessage(sci, SciMsg.SCI_SETTARGETSTART, start, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETTARGETEND, end, 0);
            Win32.SendMessage(sci, SciMsg.SCI_REPLACETARGET, text.Length, text);
        }

        static public string TextAfterCursor(int maxLength)
        {
            int bufCapacity = maxLength + 1;
            IntPtr hCurrentEditView = Plugin.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            int fullLength = (int)Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETLENGTH, 0, 0);
            int startPos = currentPos;
            int endPos = Math.Min(currentPos + bufCapacity, fullLength);
            int size = endPos - startPos;

            if (size > 0)
            {
                using (var tr = new Sci_TextRange(startPos, endPos, bufCapacity))
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
            IntPtr sci = Plugin.GetCurrentScintilla();

            Point p;
            string word = Npp.GetWordAtCursor(out p, SimpleCodeCompletion.Delimiters);

            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTION, p.X, p.Y);
            Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, 0, text);
        }

        static public string GetWordAtCursor(out Point point, char[] wordDelimiters = null)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();

            int currentPos = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            int fullLength = (int)Win32.SendMessage(sci, SciMsg.SCI_GETLENGTH, 0, 0);

            string leftText = Npp.TextBeforeCursor(512);
            string rightText = Npp.TextAfterCursor(512);

            char[] delimiters = wordDelimiters ?? SourceCodeFormatter.AllWordDelimiters;

            int startPos = leftText.LastIndexOfAny(delimiters);
            int endPos = rightText.IndexOfAny(delimiters);

            string wordLeftPart = (startPos != -1) ? leftText.Substring(startPos + 1) : "";
            string wordRightPart = (endPos != -1) ? rightText.Substring(0, endPos) : "";

            string retval = wordLeftPart + wordRightPart;

            int relativeStartPos = leftText.Length - startPos;

            startPos = (startPos != -1) ? (currentPos - relativeStartPos) + 1 : 0;
            endPos = startPos + retval.Length;

            point = new Point(startPos, endPos);
            return retval;
        }

        static public IntPtr CurrentScintilla
        {
            get { return Plugin.GetCurrentScintilla(); }
        }

        static public IntPtr NppHandle
        {
            get { return Plugin.NppData._nppHandle; }
        }

        public static int GetCaretPosition()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            return currentPos;
        }

        public static int GetCaretLineNumber()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);

            return (int)Win32.SendMessage(sci, SciMsg.SCI_LINEFROMPOSITION, currentPos, 0);
        }

        public static void SetCaretPosition(int pos)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SETCURRENTPOS, pos, 0);
        }

        public static void ClearSelection()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, currentPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, currentPos, 0); ;
        }

        static public int GrabFocus()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(sci, SciMsg.SCI_GRABFOCUS, 0, 0);
            return currentPos;
        }

        static public void ScrollToCaret()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SCROLLCARET, 0, 0);
            Win32.SendMessage(sci, SciMsg.SCI_LINESCROLL, 0, 1); //bottom scrollbar can hide the line
            Win32.SendMessage(sci, SciMsg.SCI_SCROLLCARET, 0, 0);
        }

        static public void OpenFile(string file)
        {
            IntPtr sci = Plugin.NppData._nppHandle;
            Win32.SendMessage(sci, NppMsg.NPPM_DOOPEN, 0, file);
        }

        static public void GoToLine(int line)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
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
            IntPtr sci = Plugin.GetCurrentScintilla();

            Rectangle r = new Rectangle();
            GetWindowRect(sci, ref r);
            return r;
        }

        static public string TextBeforeCursor(int maxLength)
        {
            int bufCapacity = maxLength + 1;
            IntPtr hCurrentEditView = Plugin.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            int beginPos = currentPos - maxLength;
            int startPos = (beginPos > 0) ? beginPos : 0;
            int size = currentPos - startPos;

            if (size > 0)
            {
                using (var tr = new Sci_TextRange(startPos, currentPos, bufCapacity))
                {
                    Win32.SendMessage(hCurrentEditView, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);
                    return tr.lpstrText;
                }
            }
            else
                return null;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, IntPtr lParam);

        public const int SB_SETTEXT = 1035;
        public const int SB_SETPARTS = 1028;
        public const int SB_GETPARTS = 1030;

        static public string GetStatusbarLabel()
        {
            throw new NotImplementedException();
        }

        //static public unsafe void SetStatusbarLabel(string labelText)
        static public void SetStatusbarLabel(string labelText)
        {
            IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
            // find status bar control on the main window of the application
            IntPtr statusBarHandle = FindWindowEx(mainWindowHandle, IntPtr.Zero, "msctls_statusbar32", IntPtr.Zero);
            if (statusBarHandle != IntPtr.Zero)
            {
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
        }
    }
}