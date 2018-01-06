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

        static public void DisplayInNewDocument(string text)
        {
            Npp.Editor.FileNew();

            var document = PluginBase.GetCurrentDocument();
            document.GrabFocus();
            document.AddText(text);
        }
    }
}