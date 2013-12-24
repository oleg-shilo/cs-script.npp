using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;

namespace CSScriptIntellisense
{
    public class SourceCodeFormatter : NppCodeFormatter
    {
        public static void FormatDocument()
        {
            int currentPos = Npp.GetCaretPosition();
            string code = Npp.GetTextBetween(0, Npp.DocEnd);
            int topScrollOffset = Npp.GetLineNumber(currentPos) - Npp.GetFirstVisibleLine();

            string newCode = FormatCode(code, ref currentPos);
            Npp.SetTextBetween(newCode, 0, Npp.DocEnd);

            Npp.SetCaretPosition(currentPos);
            Npp.ClearSelection();

            Npp.SetFirstVisibleLine(Npp.GetLineNumber(currentPos) - topScrollOffset);
        }

        public static void FormatDocumentPrevLines()
        {
            int currentLineNum = Npp.GetCaretLineNumber();
            int prevLineEnd = Npp.GetLineStart(currentLineNum) - Environment.NewLine.Length;
            int topScrollOffset = currentLineNum - Npp.GetFirstVisibleLine();

            string code = Npp.GetTextBetween(0, prevLineEnd);
            int currentPos = Npp.GetCaretPosition();
            string newCode = FormatCode(code, ref currentPos);
            Npp.SetTextBetween(newCode, 0, prevLineEnd);

            //no need to set the caret as it is after the formatted text anyway

            Npp.SetFirstVisibleLine(Npp.GetLineNumber(currentPos) - topScrollOffset);
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

        public static string FormatCode(string code, ref int pos)
        {
            return FormatCodeManual(code, ref pos);
            //return FormatCodeWithNRefactory(code, ref pos);
        }

        public static string FormatCodeManual(string code, ref int pos)
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

            int i = 0;
            bool posSet = false;

            // note getNext() is the first call in the loop so after the call i always points to the next char
            Func<char> nextChar = () => (i < code.Length ? code[i] : char.MinValue);
            Func<char> getNext = () => code[i++];
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

                if (current == 'N')
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
                                        if (!formatted.EndsWith("\\")) //only if it is a true end of the string declaration
                                            currentStringing = none;
                                    }
                                    else if (currentStringing == literalString)
                                    {
                                        if (!formatted.EndsWith("\"")) //only if it is a true end of the string declaration
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
                                                 .Append(current)
                                                 .AppendLine("");
                                    }
                                    else
                                    {

                                        //formatted.TrimEnd() //for JS style bracketing
                                        formatted.TrimEmptyEndLines(0)
                                                    .TrimLineEnd()
                                                    .Append(blockCustomOffset)
                                                    .Append(IndentText, blockLevel)
                                                    .Append(current)
                                                    .AppendLine("");
                                    }
                                }
                                else
                                {
                                    if (!char.IsWhiteSpace(formatted.LastChar()))
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
                                if (formatted.EndsWithWhiteSpacesLine())
                                {
                                    formatted.TrimEmptyEndLines(0)
                                             .TrimLineEnd()
                                             .Append(blockCustomOffset)
                                             .Append(IndentText, blockLevel)
                                             .Append(current);

                                    if (!nextChar().IsOneOf(')', ';')) //not an inline lambda exp in the method call and not an if...else 
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
                                                    singleLineIndent++;

                                                indentLevel += singleLineIndent;
                                            }
                                            else if (singleLineIndent > 0)
                                            {
                                                //CalculateBlockOffset();
                                                singleLineIndent = 0;
                                            }


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