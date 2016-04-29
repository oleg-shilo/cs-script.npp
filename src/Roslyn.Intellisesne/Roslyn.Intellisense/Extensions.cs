using Intellisense.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace RoslynIntellisense
{
    public static class Extensions
    {
        public static bool HasText(this string text)
        {
            return !string.IsNullOrEmpty(text);
        }

        public static bool HasAny<T>(this IEnumerable<T> items)
        {
            return items != null && items.Any();
        }

        public static T To<T>(this object obj)
        {
            return (T) obj;
        }

        public static CompletionType ToCompletionType(this SymbolKind kind)
        {
            switch (kind)
            {
                case SymbolKind.Method: return CompletionType.method;
                case SymbolKind.Event: return CompletionType._event;
                case SymbolKind.Field: return CompletionType.field;
                case SymbolKind.Property: return CompletionType.property;
                case SymbolKind.Namespace: return CompletionType._namespace;
                case SymbolKind.Assembly: return CompletionType._namespace;
                default:
                    return CompletionType.unresolved;
            }
        }

        public static string ToDecoratedName(this ITypeSymbol type)
        {
            string result = type.ToDisplayString();

            if (!result.Contains('.'))
                return result;
            //if (clrAliaces.ContainsKey(result))
            //    return clrAliaces[result];

            string nmspace = type.GetNamespace();
            if (!string.IsNullOrEmpty(nmspace))
                nmspace = "{" + nmspace + "}.";
            return (nmspace + type.Name);
        }

        public static string GetNamespace(this ISymbol type)
        {
            List<string> parts = new List<string>();
            var nm = type.ContainingNamespace;

            while (nm != null && nm.Name.HasText())
            {
                parts.Add(nm.Name);
                nm = nm.ContainingNamespace;
            }

            parts.Reverse();
            return string.Join(".", parts.ToArray());
        }

        public static string ToMinimalString(this ISymbol type)
        {
            //var nms = type.GetNamespace();
            //var result = type.ToDisplayString().Replace(nms + ".", "");
            var result = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            return result;
        }

        public static string GetFullName(this ISymbol type)
        {
            if (type == null) return null;

            List<string> parts = new List<string>();
            var nm = type.ContainingNamespace;

            while (nm != null && nm.Name.HasText())
            {
                parts.Add(nm.Name);
                nm = nm.ContainingNamespace;
            }

            parts.Reverse();
            parts.Add(type.Name);
            return string.Join(".", parts.ToArray());
        }

        public static string ToDisplayKind(this ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol)
                return (symbol as INamedTypeSymbol).TypeKind.ToString();

            if (symbol is IMethodSymbol)
            {
                switch ((symbol as IMethodSymbol).MethodKind)
                {
                    case MethodKind.LambdaMethod:
                        return "Lambda";

                    case MethodKind.Constructor:
                        return "Cosntructor";

                    case MethodKind.Destructor:
                        return "Destructor";

                    case MethodKind.ReducedExtension:
                        return "Extension Method";

                    case MethodKind.StaticConstructor:
                        return "Static Constructor";

                    case MethodKind.UserDefinedOperator:
                    case MethodKind.BuiltinOperator:
                        return "Operator";

                    case MethodKind.DeclareMethod:
                    default:
                        return "Method";
                }
            }

            return symbol.Kind.ToString();
        }

        public static string[] UsedNamespaces(this INamedTypeSymbol type)
        {
            var result = new List<string>();

            Action<string> add = (string nms) => { if (nms.HasAny() && !result.Contains(nms)) result.Add(nms); };

            foreach (var item in type.GetMembers())
            {
                if (item.Kind == SymbolKind.Field)
                    add((item as IFieldSymbol).Type.GetNamespace());
                else if (item.Kind == SymbolKind.Property)
                    add((item as IPropertySymbol).Type.GetNamespace());
                else if (item.Kind == SymbolKind.Event)
                    add((item as IEventSymbol).Type.GetNamespace());
                else if (item.Kind == SymbolKind.Method)
                {
                    var method = (item as IMethodSymbol);

                    add(method.ReturnType.GetNamespace());

                    if (method.TypeArguments.HasAny())
                        foreach (var a in method.TypeArguments)
                            add(a.GetNamespace());

                    if (method.Parameters.HasAny())
                        foreach (var a in method.TypeArguments)
                            add(a.GetNamespace());
                }
            }

            return result.Distinct()
                         .Where(x => x != type.GetNamespace())
                         .ToArray();
        }


        public static string Reconstruct(this ISymbol symbol, bool includeDoc = true)
        {
            int pos;
            return symbol.Reconstruct(out pos, includeDoc);
        }

        public static string Reconstruct(this ISymbol symbol, out int startPosition, bool includeDoc = true)
        {
            var code = new StringBuilder();
            startPosition = -1;

            int indent = 0;

            INamedTypeSymbol rootType;

            if (symbol is INamedTypeSymbol)
                rootType = (INamedTypeSymbol) symbol;
            else
                rootType = symbol.ContainingType;

            var usedNamespaces = rootType.UsedNamespaces();
            code.AppendLine(string.Join(Environment.NewLine, usedNamespaces.Select(x => $"using {x};")));
            code.AppendLine();

            string nmsp = rootType.GetNamespace();
            if (nmsp.HasAny())
            {
                code.AppendLine(("namespace " + nmsp).IndentBy(indent));
                code.AppendLine("{".IndentBy(indent++));
            }

            startPosition = code.Length;

            var type = rootType.ToReflectedCode(includeDoc);
            code.AppendLine(type.IndentLinesBy(indent));
            code.AppendLine("{".IndentBy(indent++));

            foreach (var item in rootType.GetMembers())
            {
                if (symbol == item)
                    startPosition = code.Length;

                string memberInfo = item.ToReflectedCode(includeDoc);
                if (memberInfo.HasAny())
                    code.AppendLine(memberInfo.IndentLinesBy(indent));
            }

            code.AppendLine("}".IndentBy(--indent));
            if (nmsp.HasAny()) code.AppendLine("}".IndentBy(--indent));

            return code.ToString().Trim();
        }

        public static string IndentLinesBy(this string text, int indentLevel, string linePreffix = "")
        {
            return string.Join(Environment.NewLine, text.Replace("\r", "")
                                                        .Split('\n')
                                                        .Select(x => x.IndentBy(indentLevel, linePreffix))
                                                        .ToArray());
        }

        public static string IndentBy(this string text, int indentLevel, string linePreffix = "")
        {
            var indent = new string(' ', indentLevel * 4);
            return indent + linePreffix + text;
        }

        public static string GetDocumentationComment(this ISymbol symbol, bool ignoreExceptionsInfo = false)
        {
            string xmlDoc = symbol.GetDocumentationCommentXml();
            var sections = new List<string>();

            var builder = new StringBuilder();
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
                                    builder.Insert(0, reader.Value.Shrink());
                                }
                                else
                                {
                                    if (exceptionsStarted)
                                        builder.Append("  ");

                                    if (lastElementName == "code")
                                        builder.Append(reader.Value); //need to preserve all formatting (line breaks and indents)
                                    else
                                    {
                                        //if (reflectionDocument)
                                        //    b.Append(reader.Value.NormalizeLines()); //need to preserve line breaks but not indents
                                        //else
                                        builder.Append(reader.Value.Shrink());
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
                                            builder.Append(reader.GetAttribute("name"));
                                            break;

                                        case "param":
                                            silentElement = true;
                                            builder.AppendLine();
                                            builder.Append(reader.GetAttribute("name") + ": ");
                                            break;

                                        case "para":
                                            silentElement = true;
                                            builder.AppendLine();
                                            break;

                                        case "remarks":
                                            builder.AppendLine();
                                            builder.Append("Remarks: ");
                                            break;

                                        case "returns":
                                            silentElement = true;
                                            builder.AppendLine();
                                            builder.Append("Returns: ");
                                            break;

                                        case "exception":
                                            {
                                                if (!exceptionsStarted)
                                                {
                                                    builder.AppendLine();
                                                    sections.Add(builder.ToString().Trim());
                                                    builder.Length = 0;
                                                    if (!ignoreExceptionsInfo)
                                                        builder.AppendLine("Exceptions: ");
                                                }
                                                exceptionsStarted = true;

                                                if (!ignoreExceptionsInfo && !reader.IsEmptyElement)
                                                {
                                                    bool printExInfo = false;
                                                    if (printExInfo)
                                                    {
                                                        builder.Append("  " + reader.GetCrefAttribute() + ": ");
                                                    }
                                                    else
                                                    {
                                                        builder.Append("  " + reader.GetCrefAttribute());
                                                        reader.Skip();
                                                    }
                                                }
                                                break;
                                            }
                                        case "see":
                                            silentElement = true;
                                            if (reader.IsEmptyElement)
                                            {
                                                builder.Append(reader.GetCrefAttribute());
                                            }
                                            else
                                            {
                                                reader.MoveToContent();
                                                if (reader.HasValue)
                                                {
                                                    builder.Append(reader.Value);
                                                }
                                                else
                                                {
                                                    builder.Append(reader.GetCrefAttribute());
                                                }
                                            }
                                            break;
                                    }

                                    if (!silentElement)
                                        builder.AppendLine();

                                    lastElementName = reader.Name;
                                    break;
                                }
                            case XmlNodeType.EndElement:
                                {
                                    if (reader.Name == "summary")
                                    {
                                        builder.AppendLine();
                                        sections.Add(builder.ToString().Trim());
                                        builder.Length = 0;
                                    }
                                    else if (reader.Name == "returns")
                                    {
                                        builder.AppendLine();
                                        sections.Add(builder.ToString().Trim());
                                        builder.Length = 0;
                                    }
                                    break;
                                }
                        }
                    }
                }

                sections.Add(builder.ToString().Trim());

                string sectionSeparator = "\r\n--------------------------\r\n";
                return string.Join(sectionSeparator, sections.Where(x => !string.IsNullOrEmpty(x)).ToArray());
            }
            catch (XmlException)
            {
                return xmlDoc;
            }
        }

        public static string ToReflectedCode(this ISymbol symbol, bool includeDoc = true)
        {
            var code = new StringBuilder(150);

            Action<string, int> cosdeAddComment = (text, indent) => { if (text.HasAny()) code.AppendLine(text.IndentLinesBy(indent, "// ")); };
            if (includeDoc)
            {
                var doc = symbol.GetDocumentationComment();
                cosdeAddComment(doc, 0);
            }

            if (symbol.DeclaredAccessibility != Accessibility.Public && symbol.DeclaredAccessibility != Accessibility.Protected)
                return null;

            string modifiers = $"{symbol.DeclaredAccessibility} ".ToLower();

            if (symbol.IsOverride)
                modifiers += "override ";
            if (symbol.IsStatic)
                modifiers += "static ";
            if (symbol.IsAbstract)
                modifiers += "abstract ";
            if (symbol.IsVirtual)
                modifiers += "virtual ";

            switch (symbol.Kind)
            {
                case SymbolKind.Property:
                    {
                        var member = (IPropertySymbol) symbol;
                        var type = member.Type.ToMinimalString();
                        var name = $"{member.ContainingType.ToMinimalString()}.{member.Name}";

                        string body = "{ }";
                        if (member.GetMethod == null)
                            body = "{ set; }";
                        else if (member.SetMethod == null)
                            body = "{ get; }";
                        else
                            body = "{ get; set; }";

                        code.Append($"Property: {type} {name} {body}");

                        break;
                    }
                case SymbolKind.Field:
                    {
                        var field = (IFieldSymbol) symbol;

                        if (field.ContainingType.IsEnum())
                        {
                            if (field.ConstantValue != null)
                                code.Append($"{field.Name} = {field.ConstantValue},");
                            else
                                code.Append($"{field.Name},");
                        }
                        else
                        {
                            var type = field.OriginalDefinition.Type.ToMinimalString();

                            if (field.IsConst)
                                modifiers += "const ";

                            if (field.IsReadOnly)
                                modifiers += "readonly ";

                            code.Append($"{modifiers.Trim()} {type} {field.Name};");
                        }
                        break;
                    }
                case SymbolKind.Event:
                    {
                        var member = (IEventSymbol) symbol;
                        var type = member.OriginalDefinition.Type.ToMinimalString();
                        var name = $"{member.ContainingType.ToMinimalString()}.{member.Name}";
                        code.Append($"Event: {type} {name}");
                        break;
                    }
                case SymbolKind.Method:
                    {
                        if (symbol.ContainingType.IsEnum()) //Enum constructor is hidden from user
                            return null;

                        var method = (symbol as IMethodSymbol);

                        if (method.MethodKind == MethodKind.PropertyGet || method.MethodKind == MethodKind.PropertyGet)
                            return null; //getters and setters are hidden from user

                        code.Append($"{modifiers.Trim()} ");

                        var returnType = method.OriginalDefinition.ReturnType.ToMinimalString();

                        if (method.MethodKind == MethodKind.Constructor)
                            code.Append($"{method.ContainingType.Name}"); // Printer
                        else
                            code.Append($"{returnType} {method.Name}");   //int GetIndex

                        code.Append(method.TypeParameters.ToDeclarationString())  // <T, T2>
                            .Append(method.TypeParameters.GetConstrains());       // where T: class

                        var args = "()";
                        if (method.Parameters.HasAny())
                        {
                            string prms = string.Join(", ", method.OriginalDefinition.Parameters.Select(p => p.Type.ToMinimalString() + " " + p.Name).ToArray());

                            if (method.IsExtensionMethod)
                                args = $"(this {prms})";
                            else
                                args = $"({prms})";
                        }

                        string constrains = type.TypeParameters.GetConstrains();

                        if (method.MethodKind == MethodKind.Constructor)
                            code.Append($"{modifiers.Trim()} {method.ContainingType.Name}{args};");
                        else
                            code.Append($"{modifiers.Trim()} {returnType} {name}{args};");

                        break;
                    }
                case SymbolKind.NamedType:
                    {
                        var type = (symbol as INamedTypeSymbol);

                        if (type.IsAbstract && !symbol.IsStatic)
                            modifiers += "abstract ";
                        if (!type.IsEnum() && type.IsSealed)
                            modifiers += "sealed ";

                        string kind = type.IsReferenceType ? "class" :
                                      type.IsEnum() ? "enum" :
                                      "struct";

                        code.Append($"{modifiers.Trim()} {kind} {type.Name}")   // public class Filter
                            .Append(type.TypeParameters.ToDeclarationString())  // <T, T2>
                            .Append(type.ToInheritanceString())                 // : IList<int>
                            .Append(type.TypeParameters.GetConstrains());       // where T: class

                        break;
                    }
                default:
                    break;
            }

            if (code.Length == 0)
                code.AppendLine($"{symbol.ToDisplayKind()}: {symbol.ToDisplayString()}");

            return code.ToString();
        }

        public static string ToInheritanceString(this INamedTypeSymbol type)
        {
            var code = new StringBuilder();

            string baseType = null;

            if (type.BaseType != null && type.BaseType.GetFullName().OneOf("System.ValueType", "System.Object"))
                type.BaseType.ToMinimalString();

            if (baseType.HasAny())
                code.Append(" : " + baseType);
            if (type.AllInterfaces.HasAny())
            {
                if (baseType.HasAny())
                    code.Append(", ");
                else
                    code.Append(": ");

                code.Append(type.AllInterfaces.Select(x => x.Name).JoinBy(", "));
            }
            return code.ToString();
        }

        public static string ToDeclarationString(this IEnumerable<ITypeParameterSymbol> typeParameters)
        {
            if (typeParameters.HasAny())
            {
                string prms = typeParameters.Select(x => x.Name).JoinBy(", ");
                return $"<{prms}>";
            }

            return "";
        }

        public static string GetConstrains(this IEnumerable<ITypeParameterSymbol> typeParameters)
        {
            var code = new StringBuilder();

            if (typeParameters.HasAny())
                foreach (var item in typeParameters)
                {
                    var constrains = item.GetConstrains();
                    if (constrains.HasAny())
                        code.Append(Environment.NewLine + "    " + constrains);
                }
            return code.ToString();
        }

        public static string GetConstrains(this ITypeParameterSymbol symbol)
        {
            var items = new List<string>();

            if (symbol.HasValueTypeConstraint) items.Add("struct");
            if (symbol.HasReferenceTypeConstraint) items.Add("class");
            if (symbol.HasConstructorConstraint) items.Add("new()");

            items.AddRange(symbol.ConstraintTypes.Select(x => x.ToMinimalString()));

            if (items.Any())
                return $"where {symbol.Name}: {items.JoinBy(", ")}";

            else
                return "";
        }

        public static bool IsEnum(this INamedTypeSymbol symbol)
        {
            return symbol.BaseType != null && symbol.BaseType.GetFullName() == "System.Enum";
        }

        public static string JoinBy(this IEnumerable<string> items, string separator = "")
        {
            return string.Join(separator, items.ToArray());
        }

        public static bool OneOf(this string text, params string[] items)
        {
            return items.Any(x => x == text);
        }

        public static string ToTooltip(this ISymbol symbol)
        {
            string symbolDoc = "";

            switch (symbol.Kind)
            {
                case SymbolKind.Property:
                    {
                        var member = (IPropertySymbol) symbol;
                        var type = member.Type.ToMinimalString();
                        var name = $"{member.ContainingType.ToMinimalString()}.{member.Name}";

                        string body = "{ }";
                        if (member.GetMethod == null)
                            body = "{ set; }";
                        else if (member.SetMethod == null)
                            body = "{ get; }";
                        else
                            body = "{ get; set; }";

                        symbolDoc = $"Property: {type} {name} {body}";

                        break;
                    }
                case SymbolKind.Field:
                    {
                        var member = (IFieldSymbol) symbol;
                        var type = member.Type.ToMinimalString();
                        var name = $"{member.ContainingType.ToMinimalString()}.{member.Name}";
                        symbolDoc = $"Field: {type} {name}";
                        break;
                    }
                case SymbolKind.Event:
                    {
                        var member = (IEventSymbol) symbol;
                        var type = member.Type.ToMinimalString();
                        var name = $"{member.ContainingType.ToMinimalString()}.{member.Name}";
                        symbolDoc = $"Event: {type} {name}";
                        break;
                    }
                case SymbolKind.Method:
                    {
                        var method = (symbol as IMethodSymbol);

                        var returnType = method.ReturnType.ToMinimalString();

                        var name = $"{method.ReceiverType.ToMinimalString()}.{method.Name}";
                        if (method.TypeArguments.HasAny())
                        {
                            string prms = string.Join(", ", method.TypeArguments.Select(p => p.ToMinimalString()).ToArray());
                            name = $"{name}<{prms}>";
                        }

                        var args = "()";
                        if (method.Parameters.HasAny())
                        {
                            string prms = string.Join(", ", method.Parameters.Select(p => p.Type.ToMinimalString() + " " + p.Name).ToArray());
                            args = $"({prms})";
                        }

                        var kind = "Method";
                        if (method.IsExtensionMethod)
                            kind += " (extension)";

                        symbolDoc = $"{kind}: {returnType} {name}{args}";

                        int overloads = symbol.ContainingType.GetMembers(method.Name).OfType<IMethodSymbol>().Count() - 1;

                        if (overloads > 0)
                            symbolDoc += $" (+ {overloads} overloads)";

                        break;
                    }
                case SymbolKind.ArrayType:
                case SymbolKind.Local:
                case SymbolKind.NamedType:
                case SymbolKind.Parameter:
                    break;

                default:
                    break;
            }

            if (!symbolDoc.HasText())
                symbolDoc = $"{symbol.ToDisplayKind()}: {symbol.ToDisplayString()}";

            var xmlDoc = symbol.GetDocumentationCommentXml();
            if (xmlDoc.HasText())
                symbolDoc += "\r\n" + xmlDoc.XmlToPlainText();

            return symbolDoc;
        }

        public static ICompletionData ToCompletionData(this ISymbol symbol)
        {
            if (symbol.Kind == SymbolKind.Event ||
                symbol.Kind == SymbolKind.Property ||
                symbol.Kind == SymbolKind.Field ||
                symbol.Kind == SymbolKind.Method)
            {
                // Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE.PEMethodSymbol is internal so we cannot explore it
                var invokeParams = new List<string>();
                string invokeReturn = null;
                try
                {
                    if (symbol.Kind == SymbolKind.Method && symbol is IMethodSymbol)
                    {
                        var method = (IMethodSymbol) symbol;
                        foreach (var item in method.Parameters)
                        {
                            invokeParams.Add(item.Type.ToDecoratedName() + " " + item.Name);
                        }
                        invokeReturn = method.ReturnType.ToDecoratedName();
                    }
                    else if (symbol.Kind == SymbolKind.Event && symbol is IEventSymbol)
                    {
                        var method = symbol.To<IEventSymbol>()
                                           .Type.To<INamedTypeSymbol>()
                                           .DelegateInvokeMethod.To<IMethodSymbol>();

                        foreach (var item in method.Parameters)
                        {
                            invokeParams.Add(item.Type.ToDecoratedName() + " " + item.Name);
                        }
                        invokeReturn = method.ReturnType.ToDecoratedName();
                    }
                }
                catch { }

                return new EntityCompletionData
                {
                    DisplayText = symbol.Name,
                    CompletionText = symbol.Name,
                    CompletionType = symbol.Kind.ToCompletionType(),
                    RawData = symbol,
                    InvokeParameters = invokeParams,
                    InvokeParametersSet = true,
                    InvokeReturn = invokeReturn
                };
            }
            else
            {
                return new CompletionData
                {
                    DisplayText = symbol.Name,
                    CompletionText = symbol.Name,
                    CompletionType = symbol.Kind.ToCompletionType(),
                    RawData = symbol
                };
            }
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

        public static string[] GetUsingNamespace(this SyntaxNode syntax, string code)
        {
            var t = syntax.SyntaxTree.Options.DocumentationMode;

            var namespaces = syntax.DescendantNodes()
                               .OfType<UsingDirectiveSyntax>()
                               .Select(x => x.GetText().ToString()
                                             .Replace("using", "")
                                             .Replace(";", "")
                                             .Trim())
                               .ToArray();
            return namespaces;
        }

        //public static string GetXmlDocFor(this SyntaxNode syntax, string docId)
        //{
        //    if (docId.StartsWith("T"))
        //    {
        //        var docContent = syntax.DescendantNodes()
        //                               .Where(x => x is ClassDeclarationSyntax || x is InterfaceDeclarationSyntax || x is StructDeclarationSyntax || x is DelegateDeclarationSyntax)
        //                               .FirstOrDefault()
        //                               .GetXmlDocString();
        //        return docContent;
        //    }
        //    else
        //    {
        //        var parts = docId.Split('.');
        //        //var member = parts.
        //        var type = string.Join(".", parts.Take(parts.Length - 1).ToArray());
        //        var typeNode = syntax.DescendantNodes()
        //                             .OfType<TypeDeclarationSyntax>()
        //                             .FirstOrDefault();

        //        // string xmlText = DocumentationCommentCompiler.GetDocumentationCommentXml(symbol, expandIncludes, default(CancellationToken));
        //        //T:F:M:P
        //        //var typeNode = syntax.DescendantNodes()
        //        //    .Where(x=>x.IsKind(SyntaxKind.t))

        //        var docContent = syntax.DescendantNodes()
        //                               //.OfType<ClassDeclaration SingleLineDocumentationCommentTrivia>()
        //                               //.Where(x=>x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
        //                               //.Select(x => x.GetText().ToString()
        //                               //              .Replace("using", "")
        //                               //              .Replace(";", "")
        //                               //              .Trim())
        //                               .ToArray();

        //        var ttt = docContent.FirstOrDefault(x=> x is INamespaceOrTypeSymbol) as INamespaceOrTypeSymbol;
        //        //var r = ttt.GetFullMetadataName();
        //        //%
        //        var tt22t = docContent[3].ToString();
        //        return docContent[3].GetXmlDocString();
        //    }
        //}

        //public static string GetFullName(this TypeDeclarationSyntax symbol)
        //{
        //    return null;
        //    var sb = new StringBuilder(symbol.ToFullString());

        //    var last = s;
        //    s = s.ContainingSymbol;
        //    while (!IsRootNamespace(s))
        //    {
        //        if (s is ITypeSymbol && last is ITypeSymbol)
        //        {
        //            sb.Insert(0, '+');
        //        }
        //        else
        //        {
        //            sb.Insert(0, '.');
        //        }
        //        sb.Insert(0, s.MetadataName);
        //        s = s.ContainingSymbol;
        //    }

        //    return sb.ToString();
        //}

        //private static bool IsRootNamespace(ISymbol s)
        //{
        //    return s is INamespaceSymbol && ((INamespaceSymbol) s).IsGlobalNamespace;
        //}

        //static string GetTypeName(string docId)
        //{
        //    switch (docId[0])
        //    {
        //        case 'M':
        //        case 'E':
        //        case 'F':
        //        case 'P':
        //            {
        //                var parts = docId.Split('.');

        //                return string.Join(".", parts.Take(parts.Length-1).ToArray());
        //            }
        //        case 'T': return docId;
        //    }
        //    return "";
        //}

        //static string GetXmlDocString(this SyntaxNode node)
        //{
        //    if (node == null)
        //        return "";

        //    var xmlTrivia = node.GetLeadingTrivia()
        //        .Select(i => i.GetStructure())
        //        .OfType<DocumentationCommentTriviaSyntax>()
        //        .FirstOrDefault();

        //    if (xmlTrivia != null)
        //        return xmlTrivia.ToString().Replace("///", "");

        //    return "";
        //}

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

    public static class CompletionStringExtensions
    {
        public static bool CanComplete(this string completion, string partial)
        {
            return completion.StartsWith(partial) || completion.IsSubsequenceMatch(partial);
        }

        public static bool IsCamelCaseMatch(this string completion, string partialWord)
        {
            return new string(completion.Where(c => char.IsLetter(c) && char.IsUpper(c)).ToArray()).StartsWith(partialWord, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSubsequenceMatch(this string completion, string partial)
        {
            if (!partial.HasText())
                return true;

            if (char.ToUpperInvariant(completion[0]) != char.ToUpperInvariant(partial[0]))
                return false;
            else
                return string.Compare(new string(completion.ToUpper().Intersect(partial.ToUpper()).ToArray()),
                                      partial,
                                      StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}