using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSScriptIntellisense
{
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
    }
}
