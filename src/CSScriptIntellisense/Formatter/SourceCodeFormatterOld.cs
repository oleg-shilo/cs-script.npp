using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;

namespace CSScriptIntellisense
{
    public class FallbackSourceCodeFormatter : NppCodeFormatter
    {
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

            int i = 0;
            bool posSet = false;

            // note getNext() is the first call in the loop so after the call i always points to the next char
            Func<char> nextChar = () => (i < code.Length ? code[i] : char.MinValue);
            Func<char> getNext = () => code[i++];

            Action ResetSingleLineIndent = () =>
                {
                    //disabled temporary
                    //if (singleLineIndent > 0) 
                    //{
                    //    blockLevel -= singleLineIndent;
                    //    singleLineIndent = 0;
                    //}
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

                                    ResetSingleLineIndent();

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

                                if (current == '1')
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
                                                    singleLineIndent++;
                                                    //blockLevel++; //should uncomment when ResetSingleLineIndent is enabled
                                                }

                                                indentLevel += singleLineIndent;
                                            }
                                            else if (singleLineIndent > 0)
                                            {
                                                //CalculateBlockOffset();
                                                ResetSingleLineIndent();
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
    }
}