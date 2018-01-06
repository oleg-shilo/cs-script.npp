using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CSScriptIntellisense
{
    public class NppCodeFormatter
    {
        public static void OnCharTyped(char c)
        {
            //it conflicts with N++ auto-indent (at least from v6.9.2)
            //if (Config.Instance.SmartIndenting)
            //{
            //    switch (c)
            //    {
            //        //case '\n': OnNewLine(); break; //it conflicts with N++ auto-indent
            //        case '{': OnOpenBracket(); break;
            //        case '}': OnCloseBracket(); break;
            //    }
            //}

            //it conflicts with N++ auto-indent
            //else if (Config.Instance.FormatAsYouType)
            //{
            //    switch (c)
            //    {
            //        case '\n': OnNewLine(); break;
            //    }
            //}
        }

        static void OnNewLine()
        {
            if (Config.Instance.FormatAsYouType)
            {
                var document = Npp.GetCurrentDocument();

                int currentLineNum = NppExtensions.GetCurrentLineNumber(document);
                string prevLineText = document.GetLine(currentLineNum - 1).TrimEnd();

                if (prevLineText != "")
                    SourceCodeFormatter.FormatDocumentPrevLines();
            }

            //it conflicts with N++ auto-indent (at least from v6.9.2)
            //if (Config.Instance.SmartIndenting)
            //    FormatCurrentLine();
        }

        static void FormatCurrentLine()
        {
            var document = Npp.GetCurrentDocument();
            int currentLineNum = NppExtensions.GetCurrentLineNumber(document);
            string prevLineText = document.GetLine(currentLineNum - 1).TrimEnd();

            if (prevLineText.EndsWith("{") || prevLineText.IsControlStatement())
                Perform(InsertIndent);
        }

        static void OnOpenBracket()
        {
            var document = Npp.GetCurrentDocument();

            int currentLineNum = NppExtensions.GetCurrentLineNumber(document);
            string currLineText = document.GetLine(currentLineNum);
            string prevLineText = document.GetLine(currentLineNum - 1);

            if (currLineText.Trim() == "{" && prevLineText.IsControlStatement())
                Perform(RemoveIndent);
        }

        static void OnCloseBracket()
        {
            var document = Npp.GetCurrentDocument();
            string currLineText = document.GetCurrentLine();
            string prevText = document.TextBeforeCursor(500); //do not load all all "top" document but its substantial part

            if (currLineText.Trim() == "}" && IsBracketOpened(prevText))
                Perform(RemoveIndent);
        }

        static bool IsBracketOpened(string text)
        {
            //TODO: it is better to use unsafe here.
            int openedBracketsCount = 0;

            if (!string.IsNullOrWhiteSpace(text))
                for (int i = text.Length - 1; i >= 0; i--)
                {
                    if (text[i] == '{')
                        openedBracketsCount++;
                    else if (text[i] == '}')
                        openedBracketsCount--;

                    if (openedBracketsCount > 0)
                        return true;
                }
            return false;
        }

        static void Perform(Action action)
        {
            Task.Factory.StartNew(action); //needs to be asynchronous to not to interfere with the SCI processing the typed chars
        }

        static void InsertIndent()
        {
            // int currentPos = Npp.GetCaretPosition();
            // IntPtr sci = Npp.CurrentScintilla;
            // Win32.SendMessage(sci, SciMsg.SCI_ADDTEXT, IndentText);

            PluginBase.GetCurrentDocument()
                      .AddText(IndentText);
        }

        static string indentText = null;

        static public string IndentText
        {
            get
            {
                if (indentText == null)
                {
                    if (UseTabs)
                    {
                        indentText = "\t";
                    }
                    else
                    {
                        int widthInChars = Npp.GetCurrentDocument().GetTabWidth();
                        indentText = new string(' ', widthInChars);
                    }
                }
                return indentText;
            }

            set
            {
                indentText = value;
            }
        }

        static bool? useTabs;

        static public bool UseTabs
        {
            get
            {
                if (!useTabs.HasValue)
                {
                    useTabs = Npp.GetCurrentDocument().GetUseTabs();
                }
                return useTabs.Value;
            }
            set { useTabs = value; }
        }

        static void RemoveIndent()
        {
            int currentPos = Npp.GetCurrentDocument().GetCurrentPos();
            IntPtr sci = Npp.GetCurrentDocument().Handle;
            int startPos = currentPos - 1 - IndentText.GetByteCount();
            int endPos = currentPos - 1;
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, startPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, endPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, 0, "");
            currentPos = startPos + 1;
            Win32.SendMessage(sci, SciMsg.SCI_SETCURRENTPOS, currentPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, currentPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, currentPos, 0);
        }

        static public char[] operatorChars = new[] { '+', '=', '-', '*', '/', '%', '&', '|', '^', '<', '>', '!' };
        static public char[] wordDelimiters = new[] { '\t', '\n', '\r', '\'', ' ', '.', ';', ',', '[', '{', '(', ')', '}', ']' };

        static public char[] AllWordDelimiters
        {
            get
            {
                return wordDelimiters.Concat(operatorChars).ToArray();
            }
        }
    }
}