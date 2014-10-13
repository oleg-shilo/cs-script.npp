using System;
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
            return file.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(".csx", StringComparison.InvariantCultureIgnoreCase);
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
                    return true;
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
            return result;
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
}