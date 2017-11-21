using Microsoft.CodeAnalysis;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace RoslynIntellisense
{
    public static class GenericExtensions
    {
        public static string NormalizeAsPath(this string obj)
        {
            return obj.Replace("\\", "_")
                      .Replace("/", "_")
                      .Replace(":", "_")
                      .Replace("*", "_")
                      .Replace("?", "_")
                      .Replace("\"", "_")
                      .Replace("<", "_")
                      .Replace(">", "_")
                      .Replace("|", "_");
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items != null)
                foreach (var item in items)
                    action(item);
            return items;
        }

        public static string ToLiteral(this string input)
        {
            using (var writer = new StringWriter())
            using (var provider = CodeDomProvider.CreateProvider("CSharp"))
            {
                provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                return writer.ToString();
            }
        }

        public static string ToLiteral(this char input)
        {
            using (var writer = new StringWriter())
            using (var provider = CodeDomProvider.CreateProvider("CSharp"))
            {
                provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                return writer.ToString();
            }
        }

        public static bool IsVbFile(this string file)
        {
            return file.EndsWith(".vb", StringComparison.InvariantCultureIgnoreCase);
        }

        public static int LineNumberOf(this string text, int pos)
        {
            return text.Take(pos).Count(c => c == '\n') + 1;
        }

        public static string GetLineAt(this string text, int pos)
        {
            int start = text.Substring(0, pos).LastIndexOf('\n');
            int end = text.IndexOf('\n', pos);

            return text.Substring(start, end - start).Trim();
        }

        public static int LastLineStart(this string text)
        {
            var start = text.LastIndexOf('\n'); //<doc>\r\n<declaration>
            if (start != -1)
                return start + Environment.NewLine.Length;
            return 0;
        }

        public static bool HasText(this string text)
        {
            return !string.IsNullOrEmpty(text);
        }

        public static bool HasAny<T>(this IEnumerable<T> items)
        {
            return items != null && items.Any();
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> items, T item)
        {
            return items.Concat(new[] { item });
        }

        public static T To<T>(this object obj)
        {
            return (T)obj;
        }

        public static string PathJoin(this string path, params string[] items)
        {
            return Path.Combine(new[] { path }.Concat(items).ToArray());
        }

        public static string GetDirName(this string path)
        {
            return Path.GetDirectoryName(path);
        }

        public static string JoinBy(this IEnumerable<string> items, string separator = "")
        {
            return string.Join(separator, items.ToArray());
        }

        public static bool OneOf(this string text, params string[] items)
        {
            return items.Any(x => x == text);
        }

        public static T Prev<T>(this List<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index > 0)
                return list[index - 1];
            else
                return default(T);
        }

        public static int GetWordStartOf(this string text, int offset)
        {
            if (text[offset] != '.') //we may be at the partially complete word
                for (int i = offset - 1; i >= 0; i--)
                    if (Autocompleter.Delimiters.Contains(text[i]))
                        return i + 1;
            return offset;
        }

        public static object GetProp(this object obj, string name)
        {
            var property = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                throw new Exception("ReflectionExtensions: cannot find property " + name);
            return property.GetValue(obj, null);
        }

        public static bool HasExtension(this string file, string extension)
        {
            return string.Compare(Path.GetExtension(file), extension, true) == 0;
        }

        public static string ShrinkNamespaces(this string statement, params string[] knownNamespaces)
        {
            //statement format: "{<namespace_name>}.type"
            string result = statement;

            foreach (var item in knownNamespaces.Select(x => "{" + x + "}."))
                result = result.Replace(item, "");

            return result.Replace("{", "")
                         .Replace("}", "");
        }
    }

    internal static class XmlDocExtensions
    {
        public static string Shrink(this string text)
        {
            //should be replaced with RegEx (eventually)
            string retval = text.Replace(" \r\n", " ").Replace("\r\n ", " ").Replace("\r\n", " ").Replace("\t", " ");
            //Sandcastle has problem processing <para/> with the content having line breaks. This leads to the
            //multiple joined spaces. The following is a simplistic solution for this.

            string newRetval;
            while (true)
            {
                newRetval = retval.Replace("  ", " ");
                if (newRetval.Length != retval.Length)
                    retval = newRetval;
                else
                    return newRetval;
            }
        }

        public static string GetCrefAttribute(this XmlTextReader reader)
        {
            try
            {
                string typeName = reader.GetAttribute("cref");
                if (typeName != null)
                {
                    if (typeName.StartsWith("T:") || typeName.StartsWith("F:") || typeName.StartsWith("M:"))
                        typeName = typeName.Substring(2);
                }
                else
                {
                    return reader.GetAttribute(0);
                }
                return typeName;
            }
            catch
            {
                return "";
            }
        }

        //XmlTextReader "crawling style" reader fits better the purpose than a "read it all at once" XDocument
        public static string XmlToPlainText(this string xmlDoc, bool isReflectionDocument = false, bool ignoreExceptionsInfo = false)
        {
            //var root.XElement.Parse("<root>" + entity.Documentation.Xml.Text + "</root>");

            var sections = new List<string>();

            var b = new StringBuilder();
            try
            {
                using (var reader = new XmlTextReader(new StringReader("<root>" + xmlDoc + "</root>")))
                {
                    string lastElementName = null;
                    bool exceptionsStarted = false;
                    reader.XmlResolver = null;
                    while (reader.Read())
                    {
                        var nodeType = reader.NodeType;
                        switch (nodeType)
                        {
                            case XmlNodeType.Text:
                                if (lastElementName == "summary")
                                {
                                    b.Insert(0, reader.Value.Shrink());
                                }
                                else
                                {
                                    if (exceptionsStarted)
                                        b.Append("  ");

                                    if (lastElementName == "code")
                                        b.Append(reader.Value); //need to preserve all formatting (line breaks and indents)
                                    else
                                    {
                                        //if (reflectionDocument)
                                        //    b.Append(reader.Value.NormalizeLines()); //need to preserve line breaks but not indents
                                        //else
                                        b.Append(reader.Value.Shrink());
                                    }
                                }
                                break;

                            case XmlNodeType.Element:
                                {
                                    bool silentElement = false;

                                    switch (reader.Name)
                                    {
                                        case "filterpriority":
                                            reader.Skip();
                                            break;

                                        case "root":
                                        case "summary":
                                        case "c":
                                            silentElement = true;
                                            break;

                                        case "paramref":
                                            silentElement = true;
                                            b.Append(reader.GetAttribute("name"));
                                            break;

                                        case "param":
                                            silentElement = true;
                                            b.AppendLine();
                                            b.Append(reader.GetAttribute("name") + ": ");
                                            break;

                                        case "para":
                                            silentElement = true;
                                            b.AppendLine();
                                            break;

                                        case "remarks":
                                            b.AppendLine();
                                            b.Append("Remarks: ");
                                            break;

                                        case "returns":
                                            silentElement = true;
                                            b.AppendLine();
                                            b.Append("Returns: ");
                                            break;

                                        case "exception":
                                            {
                                                if (!exceptionsStarted)
                                                {
                                                    b.AppendLine();
                                                    sections.Add(b.ToString().Trim());
                                                    b.Length = 0;
                                                    if (!ignoreExceptionsInfo)
                                                        b.AppendLine("Exceptions: ");
                                                }
                                                exceptionsStarted = true;

                                                if (!ignoreExceptionsInfo && !reader.IsEmptyElement)
                                                {
                                                    bool printExInfo = false;
                                                    if (printExInfo)
                                                    {
                                                        b.Append("  " + reader.GetCrefAttribute() + ": ");
                                                    }
                                                    else
                                                    {
                                                        b.Append("  " + reader.GetCrefAttribute());
                                                        reader.Skip();
                                                    }
                                                }
                                                break;
                                            }
                                        case "see":
                                            silentElement = true;
                                            if (reader.IsEmptyElement)
                                            {
                                                b.Append(reader.GetCrefAttribute());
                                            }
                                            else
                                            {
                                                reader.MoveToContent();
                                                if (reader.HasValue)
                                                {
                                                    b.Append(reader.Value);
                                                }
                                                else
                                                {
                                                    b.Append(reader.GetCrefAttribute());
                                                }
                                            }
                                            break;
                                    }

                                    if (!silentElement)
                                        b.AppendLine();

                                    lastElementName = reader.Name;
                                    break;
                                }
                            case XmlNodeType.EndElement:
                                {
                                    if (reader.Name == "summary")
                                    {
                                        b.AppendLine();
                                        sections.Add(b.ToString().Trim());
                                        b.Length = 0;
                                    }
                                    else if (reader.Name == "returns")
                                    {
                                        b.AppendLine();
                                        sections.Add(b.ToString().Trim());
                                        b.Length = 0;
                                    }
                                    break;
                                }
                        }
                    }
                }

                sections.Add(b.ToString().Trim());

                string sectionSeparator = (isReflectionDocument ? "\r\n--------------------------\r\n" : "\r\n\r\n");
                return string.Join(sectionSeparator, sections.Where(x => !string.IsNullOrEmpty(x)).ToArray());
            }
            catch (XmlException)
            {
                return xmlDoc;
            }
        }
    }
}