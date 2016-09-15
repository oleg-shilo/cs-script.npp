using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSScriptIntellisense
{
    public class FileReference
    {
        public string File;
        public int Line;
        public int Column;
    }

    public static class StringExtesnions
    {
        public static bool IsScriptFile(this string file)
        {
            //return file.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(".csx", StringComparison.InvariantCultureIgnoreCase);
            return file.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase) || 
                   (Config.Instance.VbSupportEnabled && file.IsVbFile()) || 
                   file.EndsWith(".csx", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsVbFile(this string file)
        {
            return file.EndsWith(".vb", StringComparison.InvariantCultureIgnoreCase);
        }

        public static string PathChangeDir(this string file, string newDir)
        {
            return Path.Combine(newDir, Path.GetFileName(file));
        }

        public static bool IsPythonFile(this string file)
        {
            return file.EndsWith(".py", StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(".pyw", StringComparison.InvariantCultureIgnoreCase);
        }

        //need to read text as we cannot ask NPP to calculate the position as the file may not be opened (e.g. auto-generated)
        public static int GetPosition(string file, int line, int column) //offsets are 0-based
        {
            using (var reader = new StreamReader(file))
            {
                int lineCount = 0;
                int columnCount = 0;
                int pos = 0;

                while (reader.Peek() >= 0)
                {
                    var c = (char) reader.Read();

                    if (lineCount == line && columnCount == column)
                        break;

                    pos++;

                    if (lineCount == line)
                        columnCount++;

                    if (c == '\n')
                        lineCount++;
                }

                return pos;
            }
        }

        public static string NormalizeNewLines(this string code)
        {
            return code.Replace("\r\n", "${NL}")
                       .Replace("\r", "${NL}")
                       .Replace("\n", "${NL}")
                       .Replace("${NL}", Environment.NewLine);
        }

        public static bool IsToken(this string text, string pattern, int position)
        {
            if (position < text.Length)
            {
                int endPos = position;
                for (; endPos < text.Length; endPos++)
                    if (char.IsWhiteSpace(text[endPos]))
                    {
                        break;
                    }

                int startPos = position - 1;
                for (; startPos >= 0; startPos--)
                    if (char.IsWhiteSpace(text[startPos]))
                    {
                        startPos = startPos + 1;
                        break;
                    }

                if (startPos == -1)
                    startPos = 0;

                if ((endPos - startPos) == pattern.Length)
                    return (text.IndexOf(pattern, startPos) == startPos);
            }
            return false;
        }

        public static bool IsOneOf(this char ch, params char[] patterns)
        {
            foreach (char c in patterns)
                if (c == ch)
                    return true;
            return false;
        }

        public static bool IsNonWhitespaceNext(this string text, string pattern, int startPos)
        {
            if (startPos < text.Length)
                for (int i = startPos; i < text.Length; i++)
                {
                    if (!char.IsWhiteSpace(text[i]))
                        return (text.IndexOf(pattern, i) == i);
                }
            return false;
        }

        public static int GetByteCount(this string text)
        {
            return Encoding.Default.GetByteCount(text);
        }

        public static int GetUtf8ByteCount(this string text)
        {
            return Encoding.UTF8.GetByteCount(text);
        }

        public static bool IsControlStatement(this string text)
        {
            text = text.TrimEnd();

            if (text.EndsWith(")"))
            {
                if (Regex.Match(text, @"\s*foreach\s*\(").Success)
                    return true;
                else if (Regex.Match(text, @"\s*for\s*\(").Success)
                    return true;
                else if (Regex.Match(text, @"\s*while\s*\(").Success)
                    return true;
                else if (Regex.Match(text, @"\s*if\s*\(").Success)
                    return true;
                //else if (Regex.Match(text, @"\s*else\s*\(").Success)
                //    return true;
            }

            return false;
        }

        public static bool IsInlineElseIf(this string text)
        {
            text = text.TrimEnd();

            if (text.EndsWith(")"))
            {
                if (Regex.Match(text, @"\s*else\s*if \s*\(").Success)
                    return text.EndsWith("}") || text.EndsWith(";");
            }

            return false;
        }

        public static StringBuilder Append(this StringBuilder builder, string text, int count)
        {
            for (int i = 0; i < count; i++)
                builder.Append(text);
            return builder;
        }

        public static string MultiplyBy(this string text, int count)
        {
            string retval = "";
            for (int i = 0; i < count; i++)
                retval += text;
            return retval;
        }

        public static bool IsSameLine(this StringBuilder builder, int startPos, int endPos)
        {
            if (builder.Length > startPos && builder.Length > endPos)
            {
                for (int i = startPos; i <= endPos; i++)
                    if (builder[i] == '\n')
                        return false;
                return true;
            }
            else
                return false;
        }

        public static bool EndsWith(this StringBuilder builder, string pattern)
        {
            if (builder.Length >= pattern.Length)
            {
                for (int i = 0; i < pattern.Length; i++)
                    if (pattern[i] != builder[builder.Length - pattern.Length + i])
                        return false;
                return true;
            }
            else
                return false;
        }

        public static bool EndsWithEscapeChar(this StringBuilder builder, char escapeChar)
        {
            if (builder.Length > 0)
            {
                int matchCount = 0;
                for (int i = builder.Length - 1; i >= 0; i--)
                {
                    if (builder[i] == escapeChar)
                        matchCount++;
                    else
                        break;
                }

                return matchCount % 2 != 0;
            }
            else
                return false;
        }

        //public static bool EndsWith(this StringBuilder builder, params char[] patterns)
        //{
        //    if (builder.Length > 0)
        //    {
        //        char endChar = builder[builder.Length - 1];

        //        foreach(char c in patterns)
        //            if (c == endChar)
        //                return false;
        //        return true;
        //    }
        //    else
        //        return false;
        //}

        public static bool ContainsAt(this StringBuilder builder, string pattern, int pos)
        {
            if ((builder.Length - pos) >= pattern.Length)
            {
                for (int i = 0; i < pattern.Length; i++)
                    if (pattern[i] != builder[pos + i])
                        return false;
                return true;
            }
            else
                return false;
        }

        public static bool EndsWithWhiteSpacesLine(this StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                for (int i = builder.Length - 1; i >= 0 && builder[i] != '\n'; i--)
                    if (!char.IsWhiteSpace(builder[i]))
                        return false;
                return true;
            }
            else
                return false;
        }

        public static string GetLastLine(this StringBuilder builder)
        {
            return builder.GetLineFrom(builder.Length - 1);
        }

        public static char LastChar(this StringBuilder builder)
        {
            return builder[builder.Length - 1];
        }

        public static string GetLineFrom(this StringBuilder builder, int position)
        {
            if (position == (builder.Length - 1) && builder[position] == '\n')
                return "";

            if (builder.Length > 0 && position < builder.Length)
            {
                int lineEnd = position;
                for (; lineEnd < builder.Length; lineEnd++)
                {
                    if (builder[lineEnd] == '\n')
                    {
                        lineEnd -= Environment.NewLine.Length - 1;
                        break;
                    }
                }

                int lineStart = position - 1;
                for (; lineStart >= 0; lineStart--)
                    if (builder[lineStart] == '\n')
                    {
                        lineStart = lineStart + 1;
                        break;
                    }

                if (lineStart == -1)
                    lineStart = 0;

                var chars = new char[lineEnd - lineStart];

                builder.CopyTo(lineStart, chars, 0, chars.Length);
                return new string(chars);
            }
            else
                return null;
        }

        public static StringBuilder TrimEmptyEndLines(this StringBuilder builder, int maxLineToLeave = 1)
        {
            int lastNonWS = builder.LastNonWhiteSpace();

            if (lastNonWS == -1)
                builder.Length = 0; //the whole content was empty lines only
            else
            {
                int count = 0;
                int maxLineBreak = maxLineToLeave + 1;

                for (int i = lastNonWS + 1; i < builder.Length; i++)
                {
                    if (builder.ContainsAt(Environment.NewLine, i))
                        count++;
                    if (count > maxLineBreak)
                    {
                        builder.Length = i;
                        break;
                    }
                }
            }
            return builder;
        }

        public static int LastNonWhiteSpace(this StringBuilder builder)
        {
            for (int i = builder.Length - 1; i >= 0; i--)
                if (!char.IsWhiteSpace(builder[i]))
                    return i;
            return -1;
        }

        public static bool IsLastWhiteSpace(this StringBuilder builder)
        {
            if (builder.Length != 0)
                return char.IsWhiteSpace(builder[builder.Length - 1]);
            return false;
        }

        public static bool LastNonWhiteSpaceToken(this StringBuilder builder, string expected)
        {
            int pos = builder.LastNonWhiteSpace();

            if (pos != -1 && pos >= expected.Length)
            {
                int startPos = pos - (expected.Length - 1);
                for (int i = 0; i < expected.Length; i++)
                {
                    if (expected[i] != builder[startPos + i])
                        return false;
                }

                if (startPos == 0 || char.IsWhiteSpace(builder[startPos - 1]))
                    return true;
            }

            return false;
        }

        public static StringBuilder TrimEnd(this StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                int i;
                for (i = builder.Length - 1; i >= 0; i--)
                    if (!char.IsWhiteSpace(builder[i]))
                        break;

                builder.Length = i + 1;
            }
            return builder;
        }

        public static StringBuilder TrimLineEnd(this StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                int i;
                for (i = builder.Length - 1; i >= 0 && builder[i] != '\n'; i--)
                    if (!char.IsWhiteSpace(builder[i]))
                        break;

                builder.Length = i + 1;
            }
            return builder;
        }

        public static FileReference ToFileErrorReference(this string text)
        {
            var result = new FileReference();
            text.ParseAsErrorFileReference(out result.File, out result.Line, out result.Column);

            NormaliseFileReference(ref result.File, ref result.Line);

            return result;
        }

        static public void NormaliseFileReference(ref string file, ref int line)
        {
            try
            {
                if (file.EndsWith(".g.csx") || file.EndsWith(".g.cs") && file.Contains(@"CSSCRIPT\Cache"))
                {
                    //it is an auto-generated file so try to find the original source file (logical file)
                    string dir = Path.GetDirectoryName(file);
                    string infoFile = Path.Combine(dir, "css_info.txt");
                    if (File.Exists(infoFile))
                    {
                        string[] lines = File.ReadAllLines(infoFile);
                        if (lines.Length > 1 && Directory.Exists(lines[1]))
                        {
                            string logicalFile = Path.Combine(lines[1], Path.GetFileName(file).Replace(".g.csx", ".csx").Replace(".g.cs", ".cs"));
                            if (File.Exists(logicalFile))
                            {
                                string code = File.ReadAllText(file);
                                int pos = code.IndexOf("///CS-Script auto-class generation");
                                if (pos != -1)
                                {
                                    int injectedLineNumber = code.Substring(0, pos).Split('\n').Count() - 1;
                                    if (injectedLineNumber <= line)
                                        line -= 1; //a single line is always injected
                                }
                                file = logicalFile;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public static bool ParseAsErrorFileReference(this string text, out string file, out int line, out int column)
        {
            line = -1;
            column = -1;
            file = "";
            //@"c:\Users\user\AppData\Local\Temp\CSSCRIPT\Cache\-1529274573\New Script2.g.cs(11,1): error";
            var match = Regex.Match(text, @"\(\d+,\d+\):\s+");
            if (match.Success)
            {
                //"(11,1):"
                string[] parts = match.Value.Substring(1, match.Value.Length - 4).Split(',');
                if (!int.TryParse(parts[0], out line))
                    return false;
                else if (!int.TryParse(parts[1], out column))
                    return false;
                else
                    file = text.Substring(0, match.Index).Trim();
                return true;
            }
            return false;
        }

        public static bool ParseAsExceptionFileReference(this string text, out string file, out int line, out int column)
        {
            line = -1;
            column = 1;
            file = "";
            //@"   at ScriptClass.main(String[] args) in c:\Users\osh\AppData\Local\Temp\CSSCRIPT\Cache\-1529274573\dev.g.csx:line 12";
            var match = Regex.Match(text, @".*:line\s\d+\s?");
            if (match.Success)
            {
                //"...mp\CSSCRIPT\Cache\-1529274573\dev.g.csx:line 12"
                int pos = match.Value.LastIndexOf(":line");
                if (pos != -1)
                {
                    string lineRef = match.Value.Substring(pos + 5, match.Value.Length - (pos + 5));
                    if (!int.TryParse(lineRef, out line))
                        return false;

                    var fileRef = match.Value.Substring(0, pos);
                    pos = fileRef.LastIndexOf(":");
                    if (pos > 0)
                    {
                        file = fileRef.Substring(pos - 1);
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public static class SyntaxMapper
    {
        public static int MapAbsPosition(string textA, int positionA, string textB)
        {
            //position is a caret position that is a strong pos+1 for the case when the caret is 
            //after the char at pos
            if (positionA == textA.Length)
                return textB.Length;

            int rightOffset = textA.OffsetToNextToken(positionA); //move to the next token if currently at white space

            int syntaxLength = textA.PosToSyntaxLength(positionA + rightOffset);
            int positionB = textB.SyntaxLengthToPos(syntaxLength);

            return positionB;
        }

        internal static int OffsetToNextToken(this string text, int pos)
        {
            int offset = 0;
            for (int i = pos; i < text.Length; i++)
            {
                if (IsMeaningfull(text[i], true))
                    break;
                offset++;
            }
            return offset;
        }

        static bool IsMeaningfull(char c, bool countLineBreaks = false)
        {
            if (countLineBreaks)
                return (c == '\r' || c == '\n' || !char.IsWhiteSpace(c));
            else
                return !char.IsWhiteSpace(c);
        }

        internal static int PosToSyntaxLength(this string text, int pos)
        {
            int syntaxLength = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (IsMeaningfull(text[i]))
                {
                    syntaxLength++;
                }

                if (i == pos)
                    break;

            }

            return syntaxLength;
        }

        internal static int SyntaxLengthToPos(this string text, int syntaxLength)
        {
            var textBuf = new StringBuilder();

            int absolutePos = -1;
            int currentSyntaxLength = 0;

            for (int i = 0; i < text.Length; i++)
            {
                absolutePos++;

                if (IsMeaningfull(text[i]))
                {
                    currentSyntaxLength++;
                    if (currentSyntaxLength == syntaxLength)
                        break;
                }
            }

            return absolutePos;
        }
    }
}