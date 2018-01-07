using Kbg.NppPluginNET.PluginInfrastructure;
using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace CSScriptIntellisense
{
    class NppEditor : npp
    {
        public static void InsertNamespace(string text)
        {
            bool isVB = Npp.Editor.GetCurrentFilePath().IsVbFile();

            var document = Npp.GetCurrentDocument();

            //it is unlikely all 'usings' take more than 2000 lines
            for (int i = 0; i < Math.Min(document.GetLineCount(), 2000); i++)
            {
                string line = document.GetLine(i);

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("/*") || line.StartsWith("*/") || line.StartsWith("*"))
                    continue;

                if (line.Trim() == text) //the required 'using' statement is already there
                    return;

                var usingKeyword = isVB ? "Imports " : "using ";
                if (line.TrimStart().StartsWith(usingKeyword)) //first 'using' statement
                {
                    var pos = document.PositionFromLine(i);
                    document.SetTextBetween(text + Environment.NewLine, pos, pos);
                    return;
                }
            }

            //did not find any 'usings' so insert on top
            document.SetTextBetween(text + Environment.NewLine, 0, 0);
        }

        public static void ProcessDeleteKeyDown()
        {
            var document = Npp.GetCurrentDocument();
            int currentPos = document.GetCurrentPos();
            IntPtr sci = Npp.GetCurrentDocument().Handle;

            document.SetSelectionStart(currentPos);
            document.SetSelectionEnd(currentPos + 1);
            document.ReplaceSelection("");
        }

        public static string ProcessKeyPress(char keyChar)
        {
            var document = Npp.GetCurrentDocument();
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
                    document.ReplaceSelection(justTypedText);
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
            var document = Npp.GetCurrentDocument();

            text = null;
            int currentPos = document.GetCurrentPos();
            if (currentPos > methodStartPos)
            {
                text = document.GetTextBetween(methodStartPos, currentPos);
                return text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
                return null;
        }

        public static string GetSuggestionHint()
        {
            var document = Npp.GetCurrentDocument();
            int currentPos = document.GetCurrentPos();

            string text = document.GetTextBetween(Math.Max(0, currentPos - 30), currentPos); //check up to 30 chars from left
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