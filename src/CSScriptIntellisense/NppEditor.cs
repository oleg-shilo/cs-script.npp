using Kbg.NppPluginNET.PluginInfrastructure;
using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace CSScriptIntellisense
{
    class NppEditor : Npp1
    {
        public static void InsertNamespace(string text)
        {
            bool isVB = Npp1.Editor.GetCurrentFilePath().IsVbFile();

            //it is unlikely all 'usings' take more than 2000 lines
            for (int i = 0; i < Math.Min(Npp1.GetLineCount(), 2000); i++)
            {
                string line = Npp1.GetLine(i);

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("/*") || line.StartsWith("*/") || line.StartsWith("*"))
                    continue;

                if (line.Trim() == text) //the required 'using' statement is already there
                    return;

                var usingKeyword = isVB ? "Imports " : "using ";
                if (line.TrimStart().StartsWith(usingKeyword)) //first 'using' statement
                {
                    var pos = Npp1.GetLineStart(i);
                    Npp1.SetTextBetween(text + Environment.NewLine, pos, pos);
                    return;
                }
            }

            //did not find any 'usings' so insert on top
            Npp1.SetTextBetween(text + Environment.NewLine, 0, 0);
        }

        public static void ProcessDeleteKeyDown()
        {
            int currentPos = Npp1.GetCaretPosition();
            IntPtr sci = Npp1.CurrentScintilla;

            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, currentPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, currentPos + 1, 0);
            Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, 0, "");
        }

        public static string ProcessKeyPress(char keyChar)
        {
            var document = PluginBase.GetCurrentDocument();
            var currentPos = document.GetCurrentPos().Value;

            string justTypedText = "";
            if (keyChar == 8)
            {
                document.SetSelection(currentPos - 1, currentPos);
                document.ReplaceSel("");
            }
            else
            {
                if (keyChar != 0)
                {
                    justTypedText = keyChar.ToString();
                    document.ReplaceSel(justTypedText);
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
            int currentPos = Npp1.GetCaretPosition();
            if (currentPos > methodStartPos)
            {
                text = Npp1.GetTextBetween(methodStartPos, currentPos);
                return text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
                return null;
        }

        public static string GetSuggestionHint()
        {
            int currentPos = Npp1.GetCaretPosition();
            IntPtr sci = Npp1.CurrentScintilla;

            string text = Npp1.GetTextBetween(Math.Max(0, currentPos - 30), currentPos); //check up to 30 chars from left
            int pos = text.LastIndexOfAny(SimpleCodeCompletion.Delimiters);
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