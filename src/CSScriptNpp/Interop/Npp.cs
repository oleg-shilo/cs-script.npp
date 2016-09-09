using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSScriptIntellisense;

namespace CSScriptNpp
{
    public class Npp
    {
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

        public static IntPtr CurrentScintilla { get { return Plugin.GetCurrentScintilla(); } }

        public static IntPtr NppHandle { get { return Plugin.NppData._nppHandle; } }

        static public string GetConfigDir()
        {
            var buffer = new StringBuilder(260);
            Win32.SendMessage(NppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, 260, buffer);
            return buffer.ToString();
        }

        /// <summary>
        /// Open the file and navigate to the 0-based line and column position.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        static public void NavigateToFileContent(string file, int line, int column)
        {
            try
            {
                Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_DOOPEN, 0, file);
                Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
                Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GOTOLINE, line, 0); //SCI lines are 0-based

                //at this point the caret is at the most left position (col=0)
                int currentPos = (int) Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);
                Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GOTOPOS, currentPos + column - 1, 0); //SCI columns are 0-based
            }
            catch { }
        }

        static public void CancelCalltip()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_CALLTIPCANCEL, 0, 0);
            Calltip.IsShowing = false;
        }

        class Calltip
        {
            static public bool IsShowing = false;
            static public string LastExpression = null;
            static public string LastEval = null;
            static public string LastDocument = null;
        }

        static public void OnCalltipRequest(int position)
        {
            if (position == -2)
            {
                Calltip.LastEval =
                Calltip.LastExpression = null; //if DBG frame is changed so clear the data
            }
            else
            {
                if (Calltip.IsShowing) return;

                Calltip.IsShowing = true;

                Task.Factory.StartNew(() =>  //must be asynch to allow processing other Debugger notifications
                    {
                        string underMouseExpression = CSScriptIntellisense.Npp.GetStatementAtPosition(position);
                        string document = Npp.GetCurrentFile();
                        string tooltip = null;

                        //if (Debugger.IsInBreak) //The calltips are used to show the values of the variables only. For everything else (e.g. MemberInfo) modal borderless forms are used
                        //{

                        if (!string.IsNullOrEmpty(underMouseExpression))
                        {
                            //also need to check expression start position (if not debugging) as the same expression can lead to different tooltip
                            //NOTE: if DBG frame is changed the LastExpression is cleared 
                            if (underMouseExpression == Calltip.LastExpression && Calltip.LastDocument == document)
                            {
                                if (Debugger.IsInBreak)
                                    tooltip = Calltip.LastEval;
                            }

                            //if (underMouseExpression != Calltip.LastExpression)
                            //{
                            //    System.Diagnostics.Debug.WriteLine("GetDebugTooltipValue -> expression is changed...");
                            //    System.Diagnostics.Debug.WriteLine("old: " + Calltip.LastExpression);
                            //    System.Diagnostics.Debug.WriteLine("new: " + underMouseExpression);
                            //}

                            //if (Calltip.LastDocument != document)
                            //    System.Diagnostics.Debug.WriteLine("GetDebugTooltipValue -> document is changed...");

                            if (tooltip == null)
                            {
                                if (Debugger.IsInBreak)
                                {
                                    tooltip = Debugger.GetDebugTooltipValue(underMouseExpression);
                                }
                                else if (CSScriptIntellisense.Config.Instance.ShowQuickInfoAsNativeNppTooltip)
                                {
                                    tooltip = CSScriptIntellisense.Plugin.GetMemberUnderCursorInfo().FirstOrDefault();
                                }

                                Calltip.LastDocument = document;
                                Calltip.LastEval = tooltip.TruncateLines(Config.Instance.CollectionItemsInTooltipsMaxCount, "\n<Content was truncated. Use F12 to see the raw API documentation data.>");
                                Calltip.LastExpression = underMouseExpression;
                            }

                            if (tooltip != null)
                            {
                                Npp.ShowCalltip(position, tooltip);
                                return;
                            }
                        }

                        Calltip.IsShowing = false;
                    });
            }
        }

        public const int MemberInfoPanelMaxWidth = 20;

        static public void ShowCalltip(int position, string text)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_CALLTIPCANCEL, 0, 0);
            Win32.SendMessage(sci, SciMsg.SCI_CALLTIPSHOW, position, text);
        }

        public static int ConvertToIntFromRGBA(byte red, byte green, byte blue, byte alpha)
        {
            return ((red << 24) | (green << 16) | (blue << 8) | (alpha));
        }

        static public void SetCalltipTime(int milliseconds)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            IntPtr tt = Win32.SendMessage(sci, SciMsg.SCI_GETMOUSEDWELLTIME, 0, 0);

            //Color: 0xBBGGRR
            Win32.SendMessage(sci, SciMsg.SCI_CALLTIPSETFORE, 0x000000, 0);
            Win32.SendMessage(sci, SciMsg.SCI_CALLTIPSETBACK, 0xE3E3E3, 0);

            Win32.SendMessage(sci, SciMsg.SCI_SETMOUSEDWELLTIME, milliseconds, 0);
        }

        static public string GetTextBetween(int start, int end = -1)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();

            if (end == -1)
                end = (int) Win32.SendMessage(sci, SciMsg.SCI_GETLENGTH, 0, 0);

            using (var tr = new Sci_TextRange(start, end, end - start + 1)) //+1 for null termination
            {
                Win32.SendMessage(sci, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);
                return tr.lpstrText;
            }
        }

        public static int GetCaretLineNumber()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            int currentPos = (int) Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);

            return (int) Win32.SendMessage(sci, SciMsg.SCI_LINEFROMPOSITION, currentPos, 0);
        }

        public static int GetLineFromPosition(int pos)
        {
            return execute(SciMsg.SCI_LINEFROMPOSITION, pos, 0);
        }

        public static string[] GetOpenFiles()
        {
            int count = execute(NppMsg.NPPM_GETNBOPENFILES, 0, 0);
            using (var cStrArray = new ClikeStringArray(count, Win32.MAX_PATH))
            {
                if (execute(NppMsg.NPPM_GETOPENFILENAMES, (int) cStrArray.NativePointer, count) != 0)
                    return cStrArray.ManagedStringsUnicode.ToArray();
                else
                    return new string[0];
            }
        }

        static public int GetLineStart(int line)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int) Win32.SendMessage(sci, SciMsg.SCI_POSITIONFROMLINE, line, 0);
        }

        public static void SetCaretPosition(int pos)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SETCURRENTPOS, pos, 0);
        }


        //Shocking!!! 
        //For selection, ranges, text length, navigation
        //Scintilla operates in units, which are not characters but bytes.
        //thus if for the document content "test" you execute selection(start:0,end:3)
        //it will select the whole word [test]
        //However the same for the Cyrillic content "тест" will
        //select only two characters [те]ст because they compose
        //4 bytes.
        //
        //Basically in Scintilla language "position" is not a character offset
        //but a byte offset.
        //
        //This is a hard to believe Scintilla flaw!!!
        //
        //The problem is discussed here: https://scintillanet.codeplex.com/discussions/218036
        //And here: https://scintillanet.codeplex.com/discussions/455082

        public static int CharOffsetToPosition(int offset, string file)
        {
            using (var reader = new StreamReader(file))
            {
                var buffer = new char[offset];
                reader.Read(buffer, 0, offset);
                return Encoding.UTF8.GetByteCount(buffer);
            }
            //return Encoding.UTF8.GetByteCount(File.ReadAllText(file).Remove(offset));
        }

        public static int PositionToCharOffset(int position, string file)
        {
            using (var reader = File.OpenRead(file))
            {
                var buffer = new byte[position];
                reader.Read(buffer, 0, position);
                return Encoding.UTF8.GetCharCount(buffer);
            }
        }

        public static void ClearSelection()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            int currentPos = (int) Win32.SendMessage(sci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, currentPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, currentPos, 0); ;
        }

        public static string GetSelectedText()
        {
            int start = execute(SciMsg.SCI_GETSELECTIONSTART, 0, 0);
            int end = execute(SciMsg.SCI_GETSELECTIONEND, 0, 0);
            return GetTextBetween(start, end);
        }

        static public int GetFirstVisibleLine()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int) Win32.SendMessage(sci, SciMsg.SCI_GETFIRSTVISIBLELINE, 0, 0);
        }

        static public int GetLinesOnScreen()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int) Win32.SendMessage(sci, SciMsg.SCI_LINESONSCREEN, 0, 0);
        }

        static public void SetFirstVisibleLine(int line)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SETFIRSTVISIBLELINE, line, 0);
        }

        static public int GrabFocus()
        {
            int currentPos = (int) Win32.SendMessage(CurrentScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
            return currentPos;
        }

        static public void SaveAllButNew()
        {
            //Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVEALLFILES, 0, 0);

            var files = Npp.GetOpenFiles();
            var current = Npp.GetCurrentFile();
            foreach (var item in files)
            {
                if (Path.IsPathRooted(item))  //the "new" file is not saved so it has no root
                {
                    Npp.OpenFile(item);
                    Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);

                }
            }
            Npp.OpenFile(current);
        }

        static public void SaveDocuments(string[] files)
        {
            var filesToSave = files.Select(x => Path.GetFullPath(x));
            var openFiles = Npp.GetOpenFiles();
            var current = Npp.GetCurrentFile();
            foreach (var item in files)
            {
                if (Path.IsPathRooted(item))  //the "new" file is not saved so it has no root
                {
                    var path = Path.GetFullPath(item);
                    if (filesToSave.Contains(path))
                    {
                        Npp.OpenFile(item);
                        Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);
                    }
                }
            }
            Npp.OpenFile(current);
        }

        static public void OpenFile(string file)
        {
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_DOOPEN, 0, file);
            Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
        }

        static public string GetCurrentFile()
        {
            var path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(NppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            return path.ToString();
        }

        static public string GetTabFile(int index)
        {
            var path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(NppHandle, NppMsg.NPPM_GETFULLPATHFROMBUFFERID, index, path);
            return path.ToString();
        }

        static public int GetPosition(int line, int column)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int) Win32.SendMessage(sci, SciMsg.SCI_POSITIONFROMLINE, line, 0) + column;
        }

        static public void SetIndicatorStyle(int indicator, SciMsg style, Color color)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_INDICSETSTYLE, indicator, (int) style);
            Win32.SendMessage(sci, SciMsg.SCI_INDICSETFORE, indicator, ColorTranslator.ToWin32(color));
        }

        static public void SetIndicatorTransparency(int indicator, int innerAlpha, int borderAlpha)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_INDICSETALPHA, indicator, innerAlpha);
            Win32.SendMessage(sci, SciMsg.SCI_INDICSETOUTLINEALPHA, indicator, borderAlpha);
        }

        static public void ClearIndicator(int indicator, int startPos, int endPos = -1)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            if (endPos == -1)
                endPos = execute(SciMsg.SCI_GETLENGTH, 0, 0);

            Win32.SendMessage(sci, SciMsg.SCI_SETINDICATORCURRENT, indicator, 0);
            Win32.SendMessage(sci, SciMsg.SCI_INDICATORCLEARRANGE, startPos, endPos - startPos);
        }

        static public void PlaceIndicator(int indicator, int startPos, int endPos)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_SETINDICATORCURRENT, indicator, 0);
            Win32.SendMessage(sci, SciMsg.SCI_INDICATORFILLRANGE, startPos, endPos - startPos);
        }

        static public void SetMarkerStyle(int marker, SciMsg style, Color foreColor, Color backColor)
        {
            int mask = execute(SciMsg.SCI_GETMARGINMASKN, 1, 0);
            execute(SciMsg.SCI_MARKERDEFINE, marker, (int) style);
            execute(SciMsg.SCI_MARKERSETFORE, marker, ColorTranslator.ToWin32(foreColor));
            execute(SciMsg.SCI_MARKERSETBACK, marker, ColorTranslator.ToWin32(backColor));
            execute(SciMsg.SCI_SETMARGINMASKN, 1, (1 << marker) | mask);
        }

        static public void SetMarkerStyle(int marker, Bitmap bitmap)
        {
            int mask = execute(SciMsg.SCI_GETMARGINMASKN, 1, 0);

            string bookmark_xpm = Utils.ConvertToXPM(bitmap, "#FF00FF");
            Win32.SendMessage(Plugin.GetCurrentScintilla(), SciMsg.SCI_MARKERDEFINEPIXMAP, marker, bookmark_xpm);

            execute(SciMsg.SCI_SETMARGINMASKN, 1, (1 << marker) | mask);
        }

        static int execute(SciMsg msg, int wParam, int lParam = 0)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            return (int) Win32.SendMessage(sci, msg, wParam, lParam);
        }

        static int execute(NppMsg msg, int wParam, int lParam = 0)
        {
            return (int) Win32.SendMessage(Npp.NppHandle, msg, wParam, lParam);
        }

        static public IntPtr PlaceMarker(int markerId, int line)
        {
            return (IntPtr) execute(SciMsg.SCI_MARKERADD, line, markerId);       //'line, marker#
        }

        static public int HasMarker(int line)
        {
            return execute(SciMsg.SCI_MARKERGET, line, 0);
        }

        static public int GetLineOfMarker(IntPtr markerHandle)
        {
            return execute(SciMsg.SCI_MARKERLINEFROMHANDLE, (int) markerHandle);
        }

        static public void DeleteAllMarkers(int markerId)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_MARKERDELETEALL, markerId, 0);
        }

        static public void DeleteMarker(IntPtr handle)
        {
            IntPtr sci = Plugin.GetCurrentScintilla();
            Win32.SendMessage(sci, SciMsg.SCI_MARKERDELETEHANDLE, (int) handle, 0);
        }
    }
}