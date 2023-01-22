using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace CSScriptIntellisense
{
    public static class NppExtensions
    {
        static public void DisplayInNewDocument(this NotepadPPGateway editor, string text)
        {
            editor.FileNew();

            var document = Npp.GetCurrentDocument();
            document.GrabFocus();
            document.AddText(text);
        }

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

            return document.get_text_range(start, end, end - start + 1); //+1 for null termination
        }

        static public string TextBeforePosition(this IScintillaGateway document, int position, int maxLength)
        {
            int bufCapacity = maxLength + 1;
            IntPtr hCurrentEditView = PluginBase.GetCurrentScintilla();
            int currentPos = position;
            int beginPos = currentPos - maxLength;
            int startPos = (beginPos > 0) ? beginPos : 0;
            int size = currentPos - startPos;

            if (size > 0)
                return document.get_text_range(startPos, currentPos, bufCapacity);
            else
                return null;
        }

        static public string TextBeforeCursor(this IScintillaGateway document, int maxLength)
        {
            int currentPos = document.GetCurrentPos();
            return document.TextBeforePosition(currentPos, maxLength);
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

        static public void ClearIndicator(this ScintillaGateway document, int indicator, int startPos, int endPos = -1)
        {
            if (endPos == -1)
                endPos = document.GetLength();

            document.SetIndicatorCurrent(indicator);
            document.IndicatorClearRange(startPos, endPos - startPos);
        }

        static public int ClosestNonEmptyLineTo(this ScintillaGateway document, int line)
        {
            if (document.GetLine(line).HasText())
                return line;

            int lineCount = document.GetLineCount();

            for (int i = 1; i < lineCount; i++)
            {
                if (line - i >= 0)
                    if (document.GetLine(line - i).HasText())
                        return line - i;

                if (line + i < lineCount)
                    if (document.GetLine(line + i).HasText())
                        return line + i;
            }

            return -1;
        }

        static public IntPtr PlaceMarker(this ScintillaGateway document, int markerId, int line)
        {
            return (IntPtr)document.MarkerAdd(line, markerId);       //'line, marker#
        }

        static public string AllText(this ScintillaGateway document)
        {
            return document.GetText(document.GetLength() + 10);
        }

        static public void DeleteMarker(this ScintillaGateway document, IntPtr handle)
        {
            document.MarkerDeleteHandle(handle.ToInt32());
        }

        static public int HasMarker(this ScintillaGateway document, int line)
        {
            return document.MarkerGet(line);
        }

        static public int[] LinesWithMarker(this ScintillaGateway document, int marker)
        {
            Application.DoEvents();

            int mask = 1 << marker;
            var result = new List<int>();

            // ideal solution but for unknown reason MarkerNext does not work reliably.
            // So resorting to line by line inefficient iterations.
            // int line = -1;
            // while (-1 != (line = document.MarkerNext(++line, mask)))
            //     result.Add(line);

            for (int line = 0; line < document.GetLineCount(); line++)
            {
                var markers = document.HasMarker(line);
                if ((markers & mask) != 0)
                    result.Add(line);
            }

            return result.ToArray();
        }

        static public int GetLineOfMarker(this ScintillaGateway document, IntPtr markerHandle)
        {
            return document.MarkerLineFromHandle(markerHandle.ToInt32());
        }

        static public void DeleteAllMarkers(this ScintillaGateway document, int markerId)
        {
            document.MarkerDeleteAll(markerId);
        }

        static public void PlaceIndicator(this ScintillaGateway document, int indicator, int startPos, int endPos)
        {
            // !!!
            // The implementation is identical to "ClearIndicator". Looks like intentional.
            document.SetIndicatorCurrent(indicator);
            document.IndicatorFillRange(startPos, endPos - startPos);
        }

        static public int GetPositionFromLineColumn(this ScintillaGateway document, int line, int column)
        {
            return document.PositionFromLine(line) + column;
        }

        static public void ReplaceWordAtCaret(this ScintillaGateway document, string text)
        {
            string word = document.GetWordAtCursor(out Point p, SimpleCodeCompletion.Delimiters);

            document.SetSel(p.X, p.Y);
            document.ReplaceSelection(text);
        }

        private static char[] statementDelimiters = " ,:;'\"=[]{}()".ToCharArray();

        static public string GetStatementAtPosition(this ScintillaGateway document, int position = -1)
        {
            if (position == -1)
                position = document.GetCurrentPos();

            return document.GetWordAtPosition(position, out Point point, statementDelimiters);
        }

        static public string GetWordAtCursor(this ScintillaGateway document, char[] wordDelimiters = null)
        {
            return document.GetWordAtCursor(out Point point, wordDelimiters);
        }

        static public string GetWordAtCursor(this ScintillaGateway document, out Point point, char[] wordDelimiters = null)
        {
            int currentPos = document.GetCurrentPos();
            return Npp.GetCurrentDocument().GetWordAtPosition(currentPos, out point, wordDelimiters);
        }

        static public string GetWordAtPosition(this ScintillaGateway document, int position, char[] wordDelimiters = null)
        {
            return document.GetWordAtPosition(position, out Point point, wordDelimiters);
        }

        private static Regex rx = new Regex(@".*\(\d+\,\d+\)\:");

        static public string ChangeLineNumberInLocation(this string fileLocation, int lineNumberChange)
        {
            try
            {
                // C:\Users\user\Documents\C# Scripts\New Script7.cs(8,11): Main(string[] args)"
                var match = rx.Match(fileLocation, 0);

                var location_items = match.Value.Substring(0, match.Length - 2) // C:\...Script7.cs(8,11
                                                .Split(separator: '(');

                var filePath = location_items.Take(location_items.Count() - 1)
                                             .JoinLines("("); // file path may contain '('

                var line_and_char = location_items.Last() // 8,11
                                                  .Split(',');

                var description = fileLocation.Substring(match.Value.Length);

                if (int.TryParse(line_and_char.First(), out var line))
                    line += lineNumberChange;

                var newfileLocation = $"{filePath}({line},{line_and_char.Last()}):{description}";
                return newfileLocation;
            }
            catch { }
            return fileLocation;
        }

        static public string GetWordAtPosition(this ScintillaGateway document, int position, out Point point, char[] wordDelimiters = null)
        {
            int currentPos = position;
            int fullLength = document.GetLength();

            string leftText = document.TextBeforePosition(currentPos, 512);
            string rightText = document.TextAfterPosition(currentPos, 512);

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

        public static void SetSelection(this ScintillaGateway document, int start, int end)
        {
            document.SetSelectionStart(start);
            document.SetSelectionEnd(end);
        }

        /// <summary>
        /// Goes to line and scrolls.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="line">The line.</param>
        static public void GoToLine(this ScintillaGateway document, int line)
        {
            document.GotoLine(line + 20);
            document.GotoLine(line - 1);
        }

        static public void ScrollToCaret(this ScintillaGateway document)
        {
            document.ScrollCaret();
            document.LineScroll(0, 1); //bottom scrollbar can hide the line
            document.ScrollCaret();
        }

        public static void ReplaceSelection(this ScintillaGateway document, string text)
        {
            document.ReplaceSel(text); // the name looks so unappealing
        }

        public static void MoveCaretTo(this ScintillaGateway document, int position)
        {
            document.SetCurrentPos(position);
            document.ClearSelection();
        }

        public static void ClearSelection(this ScintillaGateway document)
        {
            int currentPos = document.GetCurrentPos();
            document.SetSelectionStart(currentPos);
            document.SetSelectionEnd(currentPos);
        }

        static public Point[] FindIndicatorRanges(this ScintillaGateway document, int indicator)
        {
            var ranges = new List<Point>();

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

                int rangeStart = document.IndicatorStart(indicator, testPosition);
                int rangeEnd = document.IndicatorEnd(indicator, testPosition);
                int value = document.IndicatorValueAt(indicator, testPosition);
                if (value == 1) //indicator is present
                    ranges.Add(new Point(rangeStart, rangeEnd));

                if (testPosition == rangeEnd)
                    break;

                testPosition = rangeEnd;
            }

            return ranges.ToArray();
        }

        [DllImport("user32")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        static public Point GetCaretScreenLocation(this ScintillaGateway document)
        {
            int pos = document.GetCurrentPos();
            int x = document.PointXFromPosition(pos);
            int y = document.PointYFromPosition(pos);

            var point = new Point(x, y);
            ClientToScreen(document.Handle, ref point);
            return point;
        }

        static public int GetPositionFromMouseLocation(this ScintillaGateway document)
        {
            Point point = Cursor.Position;
            ScreenToClient(document.Handle, ref point);

            int pos = document.CharPositionFromPointClose(point.X, point.Y);
            return pos;
        }

        static public string TextAfterCursor(this ScintillaGateway document, int maxLength)
        {
            int currentPos = document.GetCurrentPos();
            return TextAfterPosition(document, currentPos, maxLength);
        }

        static public string TextAfterPosition(this ScintillaGateway document, int position, int maxLength)
        {
            int bufCapacity = maxLength + 1;
            int currentPos = position;
            int fullLength = document.GetLength();
            int startPos = currentPos;
            int endPos = Math.Min(currentPos + bufCapacity, fullLength);
            int size = endPos - startPos;

            if (size > 0)
                return document.get_text_range(startPos, endPos, bufCapacity);
            else
                return null;
        }

        static public int[] GetNppVersion(out bool newVersioning)
        {
            // Starting from 8.4.1  new API is available for getting the version with LOWORD always returning 3 digits
            // if new messages is not supported so use the old one

            // !!! This mechanism is not reliable. NPPM_GETNPPVERSION_NEW may return different
            // result (0x0000000000000000 vs 0x000000000000000a) depending if it was started normally or restarted due
            // to the "need to be admin to save"

            const uint NPPM_GETNPPVERSION_NEW = (uint)(NppMsg.NPPMSG + 109);

            IntPtr version = Win32.SendMessage(Npp.Editor.Handle, NPPM_GETNPPVERSION_NEW, 0, 0);

            if (version != IntPtr.Zero)
            {
                newVersioning = true;
                int major = unchecked((short)((long)version >> 16));
                int minor = unchecked((short)(long)version);
                return new[] { major, minor };
            }
            else
            {
                newVersioning = false;
                return PluginBase.GetNppVersion(); // API that is based on old message NPPM_GETNPPVERSION
            }
        }

        static bool? isNewNppApiVersion;

        static bool IsNewNppApiVersion
        {
            get
            {
                if (!isNewNppApiVersion.HasValue)
                    isNewNppApiVersion = GetIsNewNppApiVersion();
                return isNewNppApiVersion.Value;
            }
        }

        static bool GetIsNewNppApiVersion()
        {
            return true; // there is no reliable way to get the version of NPP. See GetNppVersion inline-comments

            bool newVersioning;
            int[] version = GetNppVersion(out newVersioning);

            // starting from v8.30 N++ uses Scintilla interface with TextRange min and max members as `IntPtr`
            // while previously they were `int`. If a wrong type passed N++ just crashes.

            if (newVersioning)
            {
                // version[1] will always have three digits.
                // Technically speaking if we are here then version[1] >= 410 is guaranteed. It is the first version where
                // newVersioning is implemented
                return version[0] > 8 || (version[0] == 8 && version[1] > 300);
            }

            bool newNppVersion;

            if (version[0] > 8) // major version
            {
                newNppVersion = true;
            }
            else if (version[0] == 8)
            {
                // newNppVersion = version[1] >= 30; // minor version
                // Code above is disabled as it is too simplistic. If the approach is used it would lead to
                // comparing v8.4.0 > v8.3.1 would lead to the invalid comparison 4 < 31 since
                // GetNppVersion would return [8, 4] vs [8, 31]
                //
                // so implementing a more convoluted algorithm :(

                // Thank you Robert https://github.com/oleg-shilo/cs-script.npp/issues/64#issuecomment-1108448994
                // the list of versions is in the message (url above)
                // collect the "minor" versions of 8.1.9.1, etc.
                var patchReleases = new int[] { 191, 192, 193 };

                newNppVersion =
                    // 1. allow "short" versions from 8.3 to 8.10 (only 8.1 and 8.2 are excluded);
                    //    since version 8.11 would match 8.1.1, we just hope that 9.0 comes before then
                    ((version[1] >= 3 && version[1] <= 10) ||
                        // 2. now we can be more inclusive, with the exception of the "long" versions
                        (version[1] >= 31 && !patchReleases.Any(v => v == version[1])));
            }
            else
                newNppVersion = false;

            return newNppVersion;
        }

        static string get_text_range(this IScintillaGateway document, int startPos, int endPos, int bufCapacity)
        {
            // bool newNppVersion = true;
            bool newNppVersion = IsNewNppApiVersion;

            if (Environment.GetEnvironmentVariable("CSSCRIPT_NPP_NEW_NPP_API") != null)
                newNppVersion = Environment.GetEnvironmentVariable("CSSCRIPT_NPP_NEW_NPP_API").ToLower() == "true";

            if (newNppVersion)
                using (var tr = new TextRange((IntPtr)startPos, (IntPtr)endPos, bufCapacity))
                {
                    document.GetTextRange(tr);
                    return tr.lpstrText;
                }
            else
                using (var tr = new TextRangeLegacy(startPos, endPos, bufCapacity))
                {
                    document.GetTextRangeLegacy(tr);
                    return tr.lpstrText;
                }
        }

        /// <summary>
        /// Open the file and navigate to the 0-based line and column position.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        static public void NavigateToFileContent(this ScintillaGateway document, string file, int line, int column)
        {
            try
            {
                Npp.Editor.Open(file);
                document.GrabFocus();
                document.GotoLine(line); //SCI lines are 0-based

                //at this point the caret is at the most left position (col=0)
                var currentPos = document.GetCurrentPos();
                document.GotoPos(currentPos + column - 1);
            }
            catch { }
        }

        static public void OpenFile(this NotepadPPGateway editor, string file, bool grabFocus)
        {
            editor.Open(file);
            if (grabFocus)
                Npp.GetCurrentDocument().GrabFocus();
        }

        static public void SetIndicatorTransparency(this ScintillaGateway document, int indicator, int innerAlpha, int borderAlpha)
        {
            document.IndicSetAlpha(indicator, innerAlpha);
            document.IndicSetOutlineAlpha(indicator, borderAlpha);
        }

        static public Kbg.NppPluginNET.PluginInfrastructure.Colour ToColour(this Color color)
        {
            return new Colour(color.R, color.G, color.B);
        }

        static public void SetMarkerStyle(this ScintillaGateway document, int marker, SciMsg style, Color foreColor, Color backColor)
        {
            int mask = document.GetMarginMaskN(1);
            document.MarkerDefine(marker, (int)style);
            document.MarkerSetFore(marker, foreColor.ToColour());
            document.MarkerSetBack(marker, backColor.ToColour());
            document.SetMarginMaskN(1, (1 << marker) | mask);
        }

        static public void SetMarkerStyle(this ScintillaGateway document, int marker, Bitmap bitmap)
        {
            int mask = document.GetMarginMaskN(1);

            string bookmark_xpm = ConvertToXPM(bitmap, "#FF00FF");
            document.MarkerDefinePixmap(marker, bookmark_xpm);
            document.SetMarginMaskN(1, (1 << marker) | mask);
        }

        public static string ConvertToXPM(Bitmap bmp, string transparentColor)
        {
            var sb = new StringBuilder();
            var colors = new List<string>();
            var chars = new List<char>();
            int width = bmp.Width;
            int height = bmp.Height;
            int index;
            sb.Append("/* XPM */static char * xmp_data[] = {\"").Append(width).Append(" ").Append(height).Append(" ? 1\"");
            int colorsIndex = sb.Length;
            string col;
            char c;
            for (int y = 0; y < height; y++)
            {
                sb.Append(",\"");
                for (int x = 0; x < width; x++)
                {
                    col = ColorTranslator.ToHtml(bmp.GetPixel(x, y));
                    index = colors.IndexOf(col);
                    if (index < 0)
                    {
                        index = colors.Count + 65;
                        colors.Add(col);
                        if (index > 90) index += 6;
                        c = Encoding.ASCII.GetChars(new byte[] { (byte)(index & 0xff) })[0];
                        chars.Add(c);
                        sb.Insert(colorsIndex, ",\"" + c + " c " + col + "\"");
                        colorsIndex += 14;
                    }
                    else c = chars[index];
                    sb.Append(c);
                }
                sb.Append("\"");
            }
            sb.Append("};");
            string result = sb.ToString();
            int p = result.IndexOf("?");
            string finalColor = result.Substring(0, p) + colors.Count + result.Substring(p + 1).Replace(transparentColor.ToUpper(), "None");

            return finalColor;
        }
    }
}