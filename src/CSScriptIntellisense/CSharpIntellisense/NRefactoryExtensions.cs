using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Editor;

namespace CSScriptIntellisense
{
    public static class NRefactoryExtensions
    {
        public static int GetArgumentCount(string memberInfo)
        {
            //Method: void Class.Method(string text, Dictionary<int, string> map)
            //Method: void Console.WriteLine(bool value)

            int start = memberInfo.IndexOf('(');
            int end = memberInfo.LastIndexOf(')');
            if (start != -1 && end != -1 && (end - start) != 1)
            {
                char[] array = memberInfo.Substring(start, end - start).ToCharArray();

                int count = 1;
                int level = 0;
                foreach (char c in array)
                {
                    //exclude all comas in the angle brackets
                    if (c == '<')
                        level++;
                    else if (c == '>')
                        level--;
                    if (level == 0 && c == ',')
                        count++;
                }
                return count;
            }
            else
            {
                return 0;
            }
        }

        public static string ToTooltip(this ICSharpCode.NRefactory.TypeSystem.IEntity entity, bool full)
        {
            try
            {
                var builder = new StringBuilder();
                builder.Append(entity.EntityType);
                builder.Append(": ");

                var property = (entity as IProperty);
                var field = (entity as IField);

                var method = (entity as IMethod);
                if (method != null)
                {
                    builder.Append(method.ReturnType.ReflectionName);
                    builder.Append(' ');
                }
                else if (property != null)
                {
                    if (property.CanGet)
                        builder.Append(property.Getter.ReturnType.ReflectionName);
                    else if (property.CanSet)
                        builder.Append(property.Setter.ReturnType.ReflectionName);

                    builder.Append(' ');
                }
                else if (field != null)
                {
                    builder.Append(field.ReturnType.ReflectionName);
                    builder.Append(' ');
                }

                builder.Append(entity.DeclaringType.ReflectionName);
                builder.Append('.');
                builder.Append(entity.Name);

                if (method != null && method.TypeParameters.Count > 0)
                {
                    builder.Append('`');
                    builder.Append(method.TypeParameters.Count);
                    builder.Append('[');
                    for (int i = 0; i < method.TypeParameters.Count; i++)
                    {
                        if (i > 0)
                            builder.Append(",");
                        builder.Append("[``");
                        builder.Append(i);
                        builder.Append("]");
                    }

                    builder.Append(']');
                }

                if (entity.EntityType == EntityType.Method || entity.EntityType == EntityType.Constructor || entity.EntityType == EntityType.Destructor)
                {
                    builder.Append('(');
                    if (method != null)
                    {
                        for (int i = 0; i < method.Parameters.Count; i++)
                        {
                            if (i > 0)
                                builder.Append(", ");

                            object param = method.Parameters[i];
                            string s = param.ToString();
                            builder.Append(method.Parameters[i].ToDisplayString());
                        }
                    }
                    else
                    {
                        builder.Append("...");
                    }

                    builder.Append(')');
                }

                string retval = builder.ToString().ProcessGenricNotations().ReplaceClrAliaces();

                if (entity.Documentation != null)
                {
                    /* 
                     * <summary>Writes the current line terminator to the standard output stream.</summary>
                        <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
                        <filterpriority>1</filterpriority>
                     */

                    if (full)
                    {
                        retval += "\r\n" + entity.Documentation.Xml.Text.XmlToPlainText();
                    }
                    else
                    {
                        var doc = XElement.Parse("<root>" + entity.Documentation.Xml.Text + "</root>");

                        //retval += "\r\n";

                        var summary = doc.Element("summary");
                        if (summary != null)
                            retval += "\r\n" + summary.Value.NormalizeLines();

                        var returns = doc.Element("returns");
                        if (returns != null)
                            retval += "\r\nReturns: " + returns.Value.NormalizeLines();
                    }
                }
                return retval;
            }
            catch
            {
                return null;
            }
        }

        public static string GetCrefAttribute(this XmlTextReader reader)
        {
            string typeName = reader.GetAttribute("cref");
            if (typeName.StartsWith("T:") || typeName.StartsWith("F:"))
                typeName = typeName.Substring(2);
            return typeName;
        }

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

            //return retval;
        }

        public static string XmlToPlainText(this string xmlDoc, bool isReflectionDocument = false, bool ignoreExceptionsInfo = false)
        {
            //var root.XElement.Parse("<root>" + entity.Documentation.Xml.Text + "</root>");

            var sections = new List<string>();

            var b = new StringBuilder();
            try
            {
                using (XmlTextReader reader = new XmlTextReader(new StringReader("<root>" + xmlDoc + "</root>")))
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

        public static string XmlToPlainTextOld(this string xmlDoc)
        {
            var b = new StringBuilder();
            try
            {
                using (XmlTextReader reader = new XmlTextReader(new StringReader("<root>" + xmlDoc + "</root>")))
                {
                    string lastElementName = null;
                    reader.XmlResolver = null;
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Text:
                                if (lastElementName == "summary")
                                {
                                    b.Insert(0, reader.Value);
                                    b.AppendLine();
                                }
                                else
                                {
                                    b.Append(reader.Value);
                                }
                                break;
                            case XmlNodeType.Element:
                                {
                                    switch (reader.Name)
                                    {
                                        case "filterpriority":
                                            reader.Skip();
                                            break;
                                        case "returns":
                                            b.AppendLine();
                                            b.Append("Returns: ");
                                            break;
                                        case "param":
                                            b.AppendLine();
                                            b.Append(reader.GetAttribute("name") + ": ");
                                            break;
                                        case "remarks":
                                            b.AppendLine();
                                            b.Append("Remarks: ");
                                            break;
                                        case "exception":
                                            b.AppendLine();
                                            b.Append("Exceptions: ");
                                            break;
                                        case "see":
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
                                    lastElementName = reader.Name;
                                    break;
                                }
                        }
                    }
                }
                return b.ToString();
            }
            catch (XmlException)
            {
                return xmlDoc;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string ToDisplayString(this IParameter parameter)
        {
            var builder = new StringBuilder();
            if (parameter.IsRef)
            {
                builder.Append("ref ");
            }
            if (parameter.IsOut)
            {
                builder.Append("out ");
            }
            if (parameter.IsParams)
            {
                builder.Append("params ");
            }
            builder.Append(parameter.Type.ReflectionName);
            if (parameter.IsOptional)
            {
                builder.Append(" = ");
                if (parameter.ConstantValue != null)
                {
                    builder.Append(parameter.ConstantValue.ToString());
                }
                else
                {
                    builder.Append("null");
                }
            }
            builder.Append(' ');
            builder.Append(parameter.Name);

            return builder.ToString();
        }

        static public string HideKnownNamespaces(this string text, params string[] namespaces)
        {
            if (!string.IsNullOrEmpty(text))
                foreach (string item in namespaces.OrderBy(x => x).Reverse())
                    text = text.Replace(item + ".", "");

            return text;
        }

        static string[] lineDelimiters = new string[] { Environment.NewLine };
        static public string[] GetLines(this string text, int count = -1)
        {
            if (count != -1)
                return (text ?? "").Split(lineDelimiters, count, StringSplitOptions.None);
            else
                return (text ?? "").Split(lineDelimiters, StringSplitOptions.None);
        }

        static public string JoinLines(this IEnumerable<string> lines, string separator)
        {
            string[] items;
            if (lines is string[])
                items = (string[])lines;
            else
                items = lines.ToArray();

            return string.Join(separator, items);
        }

        static public string[] LeftAlign(this string[] lines)
        {
            var items = lines.Select(i => i.TrimStart())
                             .ToArray();
            return items;
        }

        static public string NormalizeLines(this string linesText)
        {
            //some of the API XML documentation has "\n" and other "\r\n" as line breaks 
            return Regex.Split(linesText, @"\r?\n|\r").LeftAlign().JoinLines(Environment.NewLine).Trim();
        }

        static public string ReplaceWholeWord(this string text, string pattern, string replacement)
        {
            return Regex.Replace(text, @"\b(" + pattern + @")\b", replacement);
        }

        static public string ReplaceClrAliaces(this string text, bool hideSystemNamespace = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            else
            {
                var retval = text.ReplaceWholeWord("System.Object", "object")
                                 .ReplaceWholeWord("System.Boolean", "bool")
                                 .ReplaceWholeWord("System.Byte", "byte")
                                 .ReplaceWholeWord("System.SByte", "sbyte")
                                 .ReplaceWholeWord("System.Char", "char")
                                 .ReplaceWholeWord("System.Decimal", "decimal")
                                 .ReplaceWholeWord("System.Double", "double")
                                 .ReplaceWholeWord("System.Single", "float")
                                 .ReplaceWholeWord("System.Int32", "int")
                                 .ReplaceWholeWord("System.UInt32", "uint")
                                 .ReplaceWholeWord("System.Int64", "long")
                                 .ReplaceWholeWord("System.UInt64", "ulong")
                                 .ReplaceWholeWord("System.Object", "object")
                                 .ReplaceWholeWord("System.Int16", "short")
                                 .ReplaceWholeWord("System.UInt16", "ushort")
                                 .ReplaceWholeWord("System.String", "string")
                                 .ReplaceWholeWord("System.Void", "void")
                                 .ReplaceWholeWord("Void", "void");
                if (hideSystemNamespace && retval.StartsWith("System."))
                {
                    string typeName = retval.Substring("System.".Length);
                    if (!typeName.Contains('.')) // it is not a complex namespace
                        retval = typeName;
                }

                return retval;
            }
        }

        public static string ProcessGenricNotations(this string text)
        {
            //It is tempting to compose the "tooltip" string from the CLR property values but
            //it has to be done through string conversion as sometimes the name comes as
            //a string already pre-formatted some where deep in the NRefactory (e.g. parameter.Type.ReflectionName)

            const char voidChar = (char)0;

            char[] data = text.ToCharArray();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '`')
                {
                    if (data.HasCharAt('`', i + 1)) // "``0"
                    {
                        // convert "``1" -> " T1"
                        data[i++] = voidChar;
                        data[i] = 'T';
                        if (data.HasCharAt('0', i + 1))
                        {
                            data[++i] = voidChar; //convert "``0" -> " T "
                        }
                    }
                    else // "`1[...]" but not "``0"
                    {
                        int openBracket = data.FindChar('[', i);
                        if (openBracket != -1 && data.AreNumbers(i + 1, openBracket - 1))
                        {
                            int closingBracket = data.FindMatchingBracket(openBracket);
                            if (closingBracket != -1)
                            {
                                //convert "`1[...]" -> "  <...>"
                                data[i++] = voidChar;
                                data[i++] = voidChar;
                                data[i] = '<';
                                data[closingBracket] = '>';
                            }
                        }
                    }
                }
                else if (data[i] == '[')
                {
                    int closingBracket = data.FindMatchingBracket(i);
                    {
                        if (closingBracket != -1)
                        {
                            if (closingBracket == i + 1)  // "[]"
                            {
                                continue;
                            }
                            else
                            {
                                //convert "[``0]" -> " ``0 "
                                //convert "[bool]" -> " bool "
                                data[i] = voidChar;
                                data[closingBracket] = voidChar;
                            }
                        }
                    }
                }
            }

            char[] processedData = data.Where(c => c != voidChar).ToArray();

            return new string(processedData);
        }

        public static int FindChar(this char[] array, char c, int index)
        {
            if (index < array.Length)
                for (int i = index; i < array.Length; i++)
                    if (array[i] == c)
                        return i;
            return -1;
        }

        public static bool HasCharAt(this char[] array, char c, int index)
        {
            if (index < array.Length)
                return array[index] == c;
            return false;
        }

        public static bool AreNumbers(this char[] array, int start, int end)
        {
            for (int i = start; i <= end; i++)
                if (!Char.IsNumber(array[i]))
                    return false;
            return true;
        }

        public static int FindMatchingBracket(this char[] array, int start)
        {
            int level = 1; //it is opened
            for (int i = start + 1; i < array.Length; i++)
            {
                if (array[i] == '[')
                    level++;
                else if (array[i] == ']')
                    level--;
                if (level == 0)
                    return i;
            }
            return -1;
        }

        public static bool AreBracketsClosed(this string text, char openBracket = '(', char closeBracket = ')')
        {
            int level = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == openBracket)
                    level++;
                else if (text[i] == closeBracket)
                    level--;
            }
            return level == 0;
        }

        public static string[] GetUsingNamepseces(this SyntaxTree syntaxTree)
        {
            int iterator = 0;
            var nodes = new List<AstNode>(syntaxTree.Children);
            var usings = new List<string>();

            while (iterator < nodes.Count)
            {
                var node = nodes[iterator];
                nodes.AddRange(node.Children);

                var usingDecl = node as UsingDeclaration;
                if (usingDecl != null)
                    usings.Add(usingDecl.Namespace);

                iterator++;
            }

            return usings.ToArray();
        }

        public static string GetTextOf(this string text, IDocumentLine line)
        {
            return text.Substring(line.Offset, line.Length);
        }

        public static string GetDisplayInfo(this ICompletionData data, bool full)
        {
            //TODO - needs to be revisited to implement non-EntityCompltiondata (e.g. enums) eventually
            var d = (data as EntityCompletionData);
            if (d != null)
            {
                return d.Entity.ToTooltip(full);
            }
            else
                return null;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> collection, Func<TSource, TKey> keySelector)
        {
            return collection.GroupBy(keySelector).Select(y => y.First());
        }

        public static IEnumerable<AstNode> DeepAll(this IEnumerable<AstNode> collection, Func<AstNode, bool> selector)
        {
            //pseudo recursion
            var result = new List<AstNode>();
            var queue = new Queue<AstNode>(collection);

            while (queue.Count > 0)
            {
                AstNode node = queue.Dequeue();
                if (selector(node))
                    result.Add(node);

                foreach (var subNode in node.Children)
                {
                    queue.Enqueue(subNode);
                }
            }

            return result;
        }
    }
}