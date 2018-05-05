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

        public static string FormatCodeWithNRefactory(string code, ref int pos)
        {
            //https://github.com/icsharpcode/NRefactory/blob/master/ICSharpCode.NRefactory.CSharp/Formatter/FormattingOptionsFactory.cs

            //it is all great though:
            // all empty lines are lost
            // Fluent calls are destroyed
            // cannot handle classless: if any parsing error happens the generation result is completely unpredictable (the cone can be even lost)
            // Does not allow mixed brace styles despite BraceStyle.DoNotCHange
            // Hard to trace 'pos'
            //
            var option = FormattingOptionsFactory.CreateAllman();
            option.BlankLinesAfterUsings = 2;
            //BraceStyle.NextLine
            //option.SpaceWithinMethodCallParentheses = true;
            //option.BlankLinesBeforeFirstDeclaration = 0;
            //option.BlankLinesBetweenTypes = 1;
            //option.BlankLinesBetweenFields = 0;
            //option.BlankLinesBetweenEventFields = 0;
            //option.BlankLinesBetweenMembers = 1;
            //option.BlankLinesInsideRegion = 1;
            //option.InterfaceBraceStyle = BraceStyle.NextLineShifted;

            var syntaxTree = new CSharpParser().Parse(code, "test.cs");
            return syntaxTree.GetText(option);
        }

        public static string FormatCode(string code, ref int pos, string file)
        {
            if (Config.Instance.FallbackFormatting)
                return FallbackSourceCodeFormatter.FormatCodeManually(code, ref pos);
            else
            {
                if (Config.Instance.RoslynFormatting)
                    return FormatCodeWithRoslyn(code, ref pos, file);
                else
                    return FormatCodeManually(code, ref pos);
            }

            //return FormatCodeWithNRefactory(code, ref pos);
        }

        delegate string FormatMethod(string code, string file);

        //static FormatMethod RoslynFormat;

        static string FormatCodeWithRoslyn(string code, ref int pos, string file)
        {
            try
            {
                return Syntaxer.Format(code, "csharp", ref pos);
            }
            catch (Exception e)
            {
#if !DEBUG
                Config.Instance.RoslynFormatting = false;
                Config.Instance.Save();
#endif
                MessageBox.Show("Cannot use Roslyn Formatter.\nError: " + e.Message + "\n\nThis can be caused by the absence of .NET 4.6.\n\nRoslyn Formatter will be disabled and the default formatter enabled instead. You can always reenable RoslynFormatter from the settings dialog.", "CS-Script");
            }
            return code;
        }

        public static string FormatCodeManually(string code, ref int pos)
        {
            var formatted = new StringBuilder();

            const int none = 0;
            const int lineComment = 1;
            const int blockComment = 2;
            const int lineString = 3;
            const int literalString = 4;

            int openDoWhile = 0;
            int currentCharring = none; //character declaration
            int currentStringing = none;
            int currentCommenting = none;
            int blockLevel = -1;
            string blockCustomOffset = "";
            int blockCustomOffsetStartLevel = 0;
            int singleLineIndent = 0;
            bool isInSingleLineControl = false;

            int i = 0;
            bool posSet = false;

            // note getNext() is the first call in the loop so after the call i always points to the next char
            Func<char> nextChar = () => (i < code.Length ? code[i] : char.MinValue);
            Func<char> getNext = () => code[i++];

            var siMap = new List<int>();

            Action DecrementSingleLineIndent = () =>
            {
                if (singleLineIndent > 0)
                {
                    siMap.Remove(blockLevel);
                    blockLevel--;
                    singleLineIndent--;

                    while (siMap.Contains(blockLevel))
                    {
                        siMap.Remove(blockLevel);
                        blockLevel--;
                        singleLineIndent--;
                    }
                }

                isInSingleLineControl = false;
            };

            Action IncrementSingleLineIndent = () =>
            {
                isInSingleLineControl = true;
                singleLineIndent++;
                blockLevel++;
                siMap.Add(blockLevel);
            };

            Action NoteBlockOffset = () =>
                                {
                                    string currLine = formatted.GetLastLine();
                                    int indentLength = IndentText.Length * blockLevel;
                                    if (currLine.Length > indentLength)
                                    {
                                        blockCustomOffset = currLine.Substring(indentLength);
                                        blockCustomOffsetStartLevel = blockLevel;
                                    }
                                    else
                                        blockCustomOffset = "";
                                };

            Func<bool> hasClosingBracketInSameLine = () =>
                               {
                                   int brcketCount = 0;
                                   for (int j = i; j < code.Length; j++)
                                   {
                                       if (code[j] == '}')
                                           brcketCount++;
                                       else if (code[j] == '{')
                                           brcketCount--;
                                       else if (code[j].IsOneOf('\n', '\r'))
                                           break;
                                   }
                                   return brcketCount >= 1;
                               };

            Func<char, bool> isLastStatementIsComplete = (current) =>
                                {
                                    int lastPos = formatted.LastNonWhiteSpace();
                                    if (lastPos != -1)
                                    {
                                        //lastStatementIsComplete will not work correctly if the prev text is a comment, so the impact is minimal
                                        bool lastStatementIsComplete = (!formatted[lastPos].IsOneOf(operatorChars)
                                                                            &&
                                                                        ((formatted[lastPos] == '}' || formatted[lastPos] == ';' || formatted[lastPos] == ']') ||
                                                                         (formatted[lastPos] == ')' && (current != '.' && !current.IsOneOf(operatorChars)))));
                                        return lastStatementIsComplete;
                                    }
                                    else
                                        return true;
                                };

            while (i < code.Length)
            {
                char current = getNext();

                if (current == 'f')
                    Debug.WriteLine("");

                if (!posSet && i > pos)
                {
                    pos = formatted.Length;
                    posSet = true;
                }

                switch (current)
                {
                    case '"': //string
                        {
                            if (currentCommenting == none)
                            {
                                if (currentStringing == none)
                                {
                                    if (formatted.EndsWith("@"))
                                        currentStringing = literalString;
                                    else
                                        currentStringing = lineString;
                                }
                                else
                                {
                                    if (currentStringing == lineString)
                                    {
                                        if (!formatted.EndsWithEscapeChar('\\')) //only if it is a true end of the string declaration
                                            currentStringing = none;
                                    }
                                    else if (currentStringing == literalString)
                                    {
                                        if (!formatted.EndsWithEscapeChar('"')) //only if it is a true end of the string declaration
                                            currentStringing = none;
                                    }
                                    else
                                        currentStringing = none;
                                }
                            }
                            break;
                        }
                    case '\'': //character
                        {
                            if (currentCommenting == none && currentStringing == none)
                            {
                                if (currentCharring == none)
                                {
                                    currentCharring = 1;
                                }
                                else
                                {
                                    currentCharring = none;
                                }
                            }
                            break;
                        }
                    case '/': //comment
                        {
                            if (currentCommenting == none && currentStringing == none)
                            {
                                if (nextChar() == '/')
                                    currentCommenting = lineComment;
                                else if (nextChar() == '*')
                                    currentCommenting = blockComment;

                                if (currentCommenting != none) //next and continue
                                {
                                    if (formatted.EndsWithWhiteSpacesLine())
                                    {
                                        int lastPos = formatted.LastNonWhiteSpace();
                                        if (lastPos != -1 && formatted[lastPos] == '{')
                                            if (!formatted.IsSameLine(lastPos, formatted.Length - 1)) //do not indent if it is not fluent API
                                                formatted.TrimEmptyEndLines(0);

                                        formatted.TrimLineEnd()
                                                 .Append(blockCustomOffset)
                                                 .Append(IndentText, blockLevel + 1)
                                                 .Append(current)
                                                 .Append(getNext());
                                        continue;
                                    }
                                }
                            }
                            break;
                        }
                    case '\n': //line break
                        {
                            if (currentCommenting == lineComment)
                            {
                                currentCommenting = none;
                                formatted.Append(current);
                                continue;
                            }
                            else if (currentCommenting == none && currentStringing == none)
                            {
                                bool needCR = (formatted.LastChar() == '\r');

                                formatted.TrimLineEnd();

                                if (needCR)
                                    formatted.Append('\r');
                                formatted.Append(current);
                                formatted.TrimEmptyEndLines();
                                continue;
                            }
                            break;
                        }
                    case '*': //comment
                        {
                            if (currentCommenting == blockComment && nextChar() == '/')
                            {
                                currentCommenting = none;
                                formatted.Append(current);
                                formatted.Append(getNext());
                                continue;
                            }
                            break;
                        }
                    case '{': //block start
                        {
                            if (currentCommenting == none && currentStringing == none)
                            {
                                isInSingleLineControl = false;
                                blockLevel++;
                                if (formatted.EndsWithWhiteSpacesLine())
                                {
                                    if (formatted.LastNonWhiteSpaceToken("=>"))
                                        NoteBlockOffset();

                                    if (!isLastStatementIsComplete(current))
                                    {
                                        formatted.TrimEmptyEndLines(0)
                                                 .TrimLineEnd()
                                                 .Append(blockCustomOffset)
                                                 .Append(IndentText, blockLevel)
                                                 .Append(current);
                                    }
                                    else
                                    {
                                        //formatted.TrimEnd() //for JS style bracketing
                                        formatted.TrimEmptyEndLines(0)
                                                    .TrimLineEnd()
                                                    .Append(blockCustomOffset)
                                                    .Append(IndentText, blockLevel)
                                                    .Append(current);
                                    }

                                    if (!hasClosingBracketInSameLine())
                                        formatted.AppendLine("");
                                }
                                else
                                {
                                    if (formatted.Length > 0 && !char.IsWhiteSpace(formatted.LastChar()))
                                        formatted.Append(" ");

                                    formatted.Append(current);

                                    if (!char.IsWhiteSpace(nextChar()))
                                        formatted.Append(" ");
                                }
                                continue;
                            }
                            break;
                        }
                    case '}': //block end
                        {
                            if (currentCommenting == none && currentStringing == none)
                            {
                                if (isInSingleLineControl)
                                {
                                    DecrementSingleLineIndent();
                                }

                                if (formatted.EndsWithWhiteSpacesLine())
                                {
                                    formatted.TrimEmptyEndLines(0)
                                             .TrimLineEnd()
                                             .Append(blockCustomOffset)
                                             .Append(IndentText, blockLevel)
                                             .Append(current);

                                    if (!nextChar().IsOneOf(')', ';', ',')) //not an inline lambda exp in the method call and not an if...else
                                    {
                                        if (code.IsNonWhitespaceNext("else", i))
                                        {
                                        }
                                        else
                                        {
                                            formatted.AppendLine("");
                                            formatted.AppendLine("");
                                        }
                                    }
                                    blockLevel--;

                                    //ResetSingleLineIndent();

                                    if (blockCustomOffsetStartLevel > blockLevel)
                                        blockCustomOffset = "";

                                    continue;
                                }
                                else
                                {
                                    if (!char.IsWhiteSpace(formatted.LastChar()))
                                        formatted.Append(" ");
                                    blockLevel--;
                                }
                            }
                            break;
                        }
                    default:
                        if (currentCommenting == none && currentStringing == none && currentCharring == none)
                        {
                            if (current == ' ' && !formatted.EndsWithWhiteSpacesLine())
                            {
                                if (formatted.EndsWith(" "))
                                    continue;
                                else if (formatted.EndsWith("("))
                                    continue;
                                else if (nextChar() == ')')
                                    continue;
                            }

                            if (current == ';' || current == ')')
                            {
                                if (formatted.EndsWith(" "))
                                    formatted.Length = formatted.Length - 1;
                            }
                            else if (current == ',' && i < code.Length)
                            {
                                if (nextChar() != ' ' && nextChar() != '\t')
                                {
                                    formatted.Append(current);
                                    formatted.Append(' ');
                                    continue;
                                }
                            }
                            else if (current == '=' && formatted.Length > 0)
                            {
                                if (!formatted.EndsWith(" ") && i < code.Length)
                                {
                                    if (!formatted.LastChar().IsOneOf(operatorChars))
                                        formatted.Append(' ');
                                }

                                formatted.Append(current);

                                if (!nextChar().IsOneOf(operatorChars) && !char.IsWhiteSpace(nextChar()))
                                    formatted.Append(' ');

                                continue;
                            }
                            else if (!char.IsWhiteSpace(current))
                            {
                                if (current == 'd') //need to note do..while
                                {
                                    if (code.IsToken("do", i))
                                        openDoWhile++;
                                }

                                if (current == 'e' && code.IsNonWhitespaceNext("lse", i) && code.IsToken("else", i))
                                {
                                    if (isInSingleLineControl)
                                    {
                                        DecrementSingleLineIndent();
                                    }

                                    formatted.TrimEmptyEndLines(0)
                                             .TrimLineEnd()
                                             .Append(blockCustomOffset)
                                             .Append(IndentText, blockLevel + 1)
                                             .Append(current);
                                    continue;
                                }

                                if (current == 'p')
                                    Debug.WriteLine("");

                                int lastPos = formatted.LastNonWhiteSpace();
                                if (lastPos != -1)
                                {
                                    if (formatted[lastPos] == '{')
                                    {
                                        if (!formatted.IsSameLine(lastPos, formatted.Length - 1))
                                        {
                                            formatted.TrimEmptyEndLines(0)
                                                     .TrimLineEnd()
                                                     .Append(blockCustomOffset)
                                                     .Append(IndentText, blockLevel + 1)
                                                     .Append(current);
                                            continue;
                                        }
                                        else if (!char.IsWhiteSpace(current) && (i > 0 && !formatted.IsLastWhiteSpace()))
                                        {
                                            formatted.Append(" ");
                                        }
                                    }
                                    else if (formatted.LastNonWhiteSpaceToken("else"))
                                    {
                                        if (!formatted.IsSameLine(lastPos, formatted.Length - 1))
                                        {
                                            formatted.TrimEmptyEndLines(0)
                                                     .TrimLineEnd()
                                                     .Append(blockCustomOffset)
                                                     .Append(IndentText, blockLevel + 1 + 1) //extra +1 because we are in the bracket-less if..else
                                                     .Append(current);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        //if (code.IsToken("Replace", i))
                                        //    Debug.WriteLine("");

                                        //lastStatementIsComplete will not work correctly if the prev text is a comment, so the impact is minimal
                                        bool lastStatementIsComplete = (!formatted[lastPos].IsOneOf(operatorChars)
                                                                            &&
                                                                        ((formatted[lastPos] == '}' || formatted[lastPos] == ';' || formatted[lastPos] == ']') ||
                                                                         (formatted[lastPos] == ')' && (current != '.' && !current.IsOneOf(operatorChars)))));

                                        var line = formatted.GetLastLine();
                                        if (string.IsNullOrWhiteSpace(line)) //needs indentation (the first character in the line)
                                        {
                                            string prevLine = formatted.GetLineFrom(lastPos);

                                            if (formatted[lastPos] == '}')
                                            {
                                                //"i-1" - start of the current character
                                                if (code.IsNonWhitespaceNext("catch", i - 1))
                                                {
                                                    formatted.TrimEnd()
                                                             .AppendLine();
                                                }
                                                else if (code.IsNonWhitespaceNext("while", i - 1))
                                                {
                                                    //only if it is do...while
                                                    if (openDoWhile > 0)
                                                    {
                                                        formatted.TrimEnd()
                                                                 .AppendLine();
                                                        openDoWhile--;
                                                    }
                                                }
                                            }

                                            int indentLevel = blockLevel + 1;

                                            if (prevLine.IsControlStatement())
                                            {
                                                if (!prevLine.IsInlineElseIf()) //should not increase indent
                                                {
                                                    IncrementSingleLineIndent();
                                                }
                                            }
                                            else
                                            {
                                                DecrementSingleLineIndent();
                                            }

                                            indentLevel = blockLevel + 1;

                                            if (lastStatementIsComplete || prevLine.IsControlStatement())
                                            {
                                                formatted.TrimEmptyEndLines(1)
                                                         .TrimLineEnd()
                                                         .Append(blockCustomOffset)
                                                         .Append(IndentText, indentLevel);
                                            }
                                            formatted.Append(current);

                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }

                formatted.Append(current);
            }

            return formatted.TrimEnd().ToString();
        }
    }
}