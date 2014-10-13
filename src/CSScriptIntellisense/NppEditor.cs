using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace CSScriptIntellisense
{
    class NppEditor : Npp
    {
        public static void InsertNamespace(string text)
        {
            //it is unlikely all 'usings' take more than 2000 lines
            for (int i = 0; i < Math.Min(Npp.GetLineCount(), 2000); i++)
            {
                string line = Npp.GetLine(i);

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("/*") || line.StartsWith("*/") || line.StartsWith("*"))
                    continue;

                if (line.Trim() == text) //the required 'using' statement is already there
                    return;

                if (line.TrimStart().StartsWith("using ")) //first 'using' statement
                {
                    var pos = Npp.GetLineStart(i);
                    Npp.SetTextBetween(text + Environment.NewLine, pos, pos);
                    return;
                }
            }

            //did not find any 'usings' so insert on top
            Npp.SetTextBetween(text + Environment.NewLine, 0, 0);
        }

        public static void ProcessDeleteKeyDown()
        {
            int currentPos = Npp.GetCaretPosition();
            IntPtr sci = Npp.CurrentScintilla;

            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, currentPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, currentPos + 1, 0);
            Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, 0, "");
        }

        public static string ProcessKeyPress(char keyChar)
        {
            int currentPos = Npp.GetCaretPosition();
            IntPtr sci = Npp.CurrentScintilla;
            string justTypedText = "";
            if (keyChar == 8)
            {
                Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, currentPos - 1, 0);
                Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, currentPos, 0);
                Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, 0, "");
            }
            else
            {
                if (keyChar != 0)
                {
                    justTypedText = keyChar.ToString();
                    Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, justTypedText);
                }
            }

            return justTypedText;
        }

        public static IEnumerable<string> GetMethodOverloadHint(int methodStartPos)
        {
            string text;
            return GetMethodOverloadHint(methodStartPos, out text);
        }
        public static IEnumerable<string> GetMethodOverloadHint(int methodStartPos, out string text)
        {
            text = null;
            int currentPos = Npp.GetCaretPosition();
            if (currentPos > methodStartPos)
            {
                text = Npp.GetTextBetween(methodStartPos, currentPos);
                return text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
                return null;
        }

        public static string GetSuggestionHint()
        {
            int currentPos = Npp.GetCaretPosition();
            IntPtr sci = Npp.CurrentScintilla;

            string text = Npp.GetTextBetween(Math.Max(0, currentPos - 30), currentPos); //check up to 30 chars from left
            int pos = text.LastIndexOf(".");
            if (pos != -1)
            {
                string token = text.Substring(pos + 1);// +justTypedText;
                return token.Trim();
            }

            return null;
        }

        public static int GetCssHash(string file)
        {
            //very primitive hash of all //css_ directives.
            //Assuming it cannot be longer than 20 lines of code.

            if (!File.Exists(file))
                return -1;

            int retval = 0;
            int count = 0;
            using (var reader = new StreamReader(file))
            {
                string line;
                while (null != (line = reader.ReadLine()) && count < 20)
                {
                    count++;
                    if (line.StartsWith("//css_", StringComparison.Ordinal))
                        retval += line.GetHashCode();
                }
            }

            return retval;
        }
    }
}