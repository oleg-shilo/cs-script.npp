using System;
using System.Linq;
using System.Collections.Generic;
using CSScriptIntellisense;
using Kbg.NppPluginNET.PluginInfrastructure;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

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

            using (var tr = new TextRange(start, end, end - start + 1)) //+1 for null termination
            {
                document.GetTextRange(tr);
                return tr.lpstrText;
            }
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
            {
                using (var tr = new TextRange(startPos, currentPos, bufCapacity))
                {
                    document.GetTextRange(tr);
                    return tr.lpstrText;
                }
            }
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

        static public IntPtr PlaceMarker(this ScintillaGateway document, int markerId, int line)
        {
            return (IntPtr)document.MarkerAdd(line, markerId);       //'line, marker#
        }

        static public void DeleteMarker(this ScintillaGateway document, IntPtr handle)
        {
            document.MarkerDeleteHandle(handle.ToInt32());
        }

        static public int HasMarker(this ScintillaGateway document, int line)
        {
            return document.MarkerGet(line);
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
            document.IndicatorClearRange(startPos, endPos - startPos);
        }

        static public int GetPositionFromLineColumn(this ScintillaGateway document, int line, int column)
        {
            return document.PositionFromLine(line) + column;
        }

        static public void ReplaceWordAtCaret(this ScintillaGateway document, string text)
        {
            Point p;
            string word = document.GetWordAtCursor(out p, SimpleCodeCompletion.Delimiters);

            document.SetSel(p.X, p.Y);
            document.ReplaceSelection(text);
        }

        static char[] statementDelimiters = " ,:;'\"=[]{}()".ToCharArray();

        static public string GetStatementAtPosition(this ScintillaGateway document, int position = -1)
        {
            Point point;
            if (position == -1)
                position = document.GetCurrentPos();

            return document.GetWordAtPosition(position, out point, statementDelimiters);
        }

        static public string GetWordAtCursor(this ScintillaGateway document, char[] wordDelimiters = null)
        {
            Point point;
            return document.GetWordAtCursor(out point, wordDelimiters);
        }

        static public string GetWordAtCursor(this ScintillaGateway document, out Point point, char[] wordDelimiters = null)
        {
            int currentPos = document.GetCurrentPos();
            return Npp.GetCurrentDocument().GetWordAtPosition(currentPos, out point, wordDelimiters);
        }

        static public string GetWordAtPosition(this ScintillaGateway document, int position, char[] wordDelimiters = null)
        {
            Point point;
            return document.GetWordAtPosition(position, out point, wordDelimiters);
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
            /*
            SCI_GETFIRSTVISIBLELINE = 2152,
            SCI_GETLINE = 2153,
            SCI_GETLINECOUNT = 2154,*/
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
            {
                using (var tr = new TextRange(startPos, endPos, bufCapacity))
                {
                    document.GetTextRange(tr);
                    return tr.lpstrText;
                }
            }
            else
                return null;
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
            return new Colour(ColorTranslator.ToWin32(color));
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
            // document.MarkerDefineRGBAImage
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
                    else c = (char)chars[index];
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