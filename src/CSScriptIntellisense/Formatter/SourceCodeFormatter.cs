using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.NRefactory.CSharp;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace CSScriptIntellisense
{
    public class SourceCodeFormatter : NppCodeFormatter
    {
        public static int CaretBeforeLastFormatting = -1;
        public static int TopScrollOffsetBeforeLastFormatting = -1;

        public static void FormatDocument()
        {
            try
            {
                var document = Npp.GetCurrentDocument();

                int currentPos = document.GetCurrentPos();
                CaretBeforeLastFormatting = currentPos;
                string code = document.GetTextBetween(0, npp.DocEnd);

                if (code.Any() && currentPos != -1 && currentPos < code.Length)
                {
                    code = NormalizeNewLines(code, ref currentPos);

                    int topScrollOffset = document.LineFromPosition(currentPos) - document.GetFirstVisibleLine();
                    TopScrollOffsetBeforeLastFormatting = topScrollOffset;

                    string newCode = FormatCode(code, ref currentPos, Npp.Editor.GetCurrentFilePath());

                    if (newCode != null)
                    {
                        document.SetText(newCode);

                        document.SetCurrentPos(currentPos);
                        document.ClearSelection();

                        document.SetFirstVisibleLine(document.LineFromPosition(currentPos) - topScrollOffset);
                    }
                }
            }
            catch
            {
#if DEBUG
                throw;
#endif
                //formatting errors are not critical so can be ignored in release mode
            }
        }

        static public string NormalizeNewLines(string code, ref int currentPos)
        {
            var codeLeft = code.Substring(0, currentPos).NormalizeNewLines();
            var codeRight = code.Substring(currentPos, code.Length - currentPos).NormalizeNewLines();

            currentPos = codeLeft.Length;

            return codeLeft + codeRight;
        }

        public static void FormatDocumentPrevLines()
        {
            var document = Npp.GetCurrentDocument();

            int currentLineNum = NppExtensions.GetCurrentLineNumber(document);
            var prevLineEnd = document.PositionFromLine(currentLineNum) - Environment.NewLine.Length;
            int topScrollOffset = currentLineNum - document.GetFirstVisibleLine();

            string code = document.GetTextBetween(0, prevLineEnd.Value);
            int currentPos = document.GetCurrentPos();
            string newCode = FormatCode(code, ref currentPos, Npp.Editor.GetCurrentFilePath());
            document.SetTextBetween(newCode, 0, prevLineEnd.Value);

            //no need to set the caret as it is after the formatted text anyway

            document.SetFirstVisibleLine(document.LineFromPosition(currentPos) - topScrollOffset);
        }

        public static string FormatCode(string code, ref int pos, string file)
        {
            return FormatCodeWithRoslyn(code, ref pos, file);
        }

        delegate string FormatMethod(string code, string file);

        static string FormatCodeWithRoslyn(string code, ref int pos, string file)
        {
            try
            {
                return Syntaxer.Format(code, "csharp", ref pos);
            }
            catch (Exception e)
            {
                MessageBox.Show("Cannot use Roslyn Formatter.\nError: " + e.Message + "\n\nThis can be caused by the absence of .NET 4.6.\n\nRoslyn Formatter will be disabled and the default formatter enabled instead. You can always reenable RoslynFormatter from the settings dialog.", "CS-Script");
            }
            return code;
        }
    }
}