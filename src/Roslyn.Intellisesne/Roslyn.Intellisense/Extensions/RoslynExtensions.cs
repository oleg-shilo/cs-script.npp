using Intellisense.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace RoslynIntellisense
{
    public static class RoslynExtensions
    {
        public static string ToKindDisplayName(this TypeDeclarationSyntax type)
        {
            var kind = type.Kind();
            return kind == SyntaxKind.ClassDeclaration ? "class" :
                kind == SyntaxKind.StructDeclaration ? "struct" :
                kind == SyntaxKind.EnumDeclaration ? "enum" :
                "";
        }

        public static CompletionType ToCompletionType(this SymbolKind kind)
        {
            switch (kind)
            {
                case SymbolKind.Method: return CompletionType.method;
                case SymbolKind.Event: return CompletionType._event;
                case SymbolKind.Field: return CompletionType.field;
                case SymbolKind.Property: return CompletionType.property;
                case SymbolKind.NamedType: return CompletionType.type;
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

        public static string Reconstruct(this ISymbol symbol, bool includeDoc = true)
        {
            int pos;
            return symbol.Reconstruct(out pos, includeDoc);
        }

        public static string Reconstruct(this ISymbol symbol, out int startPosition, bool includeDoc = true, string header = "")
        {
            var code = new StringBuilder();
            code.Append(header);
            startPosition = -1;

            int indent = 0;

            INamedTypeSymbol rootType = symbol.GetRootType();  //itself if it is a type or containing type if a member

            var usedNamespaces = rootType.UsedNamespaces();
            if (usedNamespaces.HasAny())
            {
                code.AppendLine(usedNamespaces.Select(x => $"using {x};").JoinBy(Environment.NewLine));
                code.AppendLine();
            }

            string nmsp = rootType.GetNamespace();
            if (nmsp.HasAny())
            {
                code.AppendLine(("namespace " + nmsp).IndentBy(indent));
                code.AppendLine("{".IndentBy(indent++));
            }

            var parentClasses = rootType.GetParentClasses();
            foreach (var parent in parentClasses) //nested classes support
                code.AppendLine($"public {parent.TypeKind.ToString().ToLower()} ".IndentBy(indent) + parent.Name)
                    .AppendLine("{".IndentBy(indent++));

            startPosition = code.Length;
            var type = rootType.ToReflectedCode(includeDoc);
            type = type.IndentLinesBy(indent);
            code.AppendLine(type);

            //<doc>\r\n<declaration>
            startPosition += type.LastLineStart();

            if (!rootType.IsDelegate())
            {
                code.AppendLine("{".IndentBy(indent++));

                string currentGroup = null;

                var members = rootType.GetMembers()
                                      .OrderBy(x => x.GetDisplayGroup())
                                      .ThenByDescending(x => x.DeclaredAccessibility)
                                      .ThenBy(x => rootType.IsEnum() ? "" : x.Name);

                foreach (var item in members)
                {
                    string memberInfo = item.ToReflectedCode(includeDoc, usePartials: true);

                    if (memberInfo.HasAny())
                    {
                        if (currentGroup != null && item.GetDisplayGroup() != currentGroup)
                            code.AppendLine();
                        currentGroup = item.GetDisplayGroup();

                        var info = memberInfo.IndentLinesBy(indent);

                        if (symbol == item || (symbol is IMethodSymbol && (symbol as IMethodSymbol).ReducedFrom == item))
                            startPosition = code.Length + info.LastLineStart();

                        code.AppendLine(info);
                    }
                }

                code.AppendLine("}".IndentBy(--indent));
            }

            foreach (var item in parentClasses)
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

        static string ToTypeCode(this INamedTypeSymbol type, string modifiers, bool usePartials)
        {
            var code = new StringBuilder(150);

            if ((!type.IsEnum() && !type.IsDelegate()) && type.IsSealed)
                modifiers += "sealed ";

            string kind = type.IsInterface() ? "interface" :
                          type.IsReferenceType ? "class" :
                          type.IsEnum() ? "enum" :
                          "struct";

            if (type.IsDelegate())
            {
                IMethodSymbol invokeMethod = type.GetMethod("Invoke");

                code.Append($"{modifiers}delegate {invokeMethod.ReturnType.ToDisplayString()} {type.Name}")   // public delegate int GetIndexDlgt                                // public class Filter
                    .Append(type.TypeParameters.ToDeclarationString())                          // <T, T2>
                    .Append(invokeMethod.GetParametersString())                                 // (CustomIndex count, int? contextArg, T parent)
                    .Append(type.TypeParameters.GetConstrains(singleLine: type.IsDelegate()))   // where T: class
                    .Append(";");
            }
            else
            {
                //if (usePartials)
                //    kind = "partial " + kind;

                code.Append($"{modifiers}{kind} {type.Name}")                                   // public class Filter
                    .Append(type.TypeParameters.ToDeclarationString())                          // <T, T2>
                    .Append(type.IsEnum() ? "" :
                            type.ToInheritanceString())                                         // : IList<int>
                    .Append(type.TypeParameters.GetConstrains(singleLine: type.IsDelegate()));  // where T: class

                if (usePartials)
                    code.Append(" { /*hidden*/ }");
            }

            return code.ToString();
        }

        static string ToPropertyCode(this IPropertySymbol symbol, string modifiers)
        {
            var code = new StringBuilder(150);
            if (symbol.ContainingType.IsInterface())
                modifiers = "";

            string getter = "";
            string setter = "";
            if (symbol.GetMethod != null)
            {
                if (symbol.GetMethod.DeclaredAccessibility == Accessibility.Protected)
                    getter = "protected get; ";
                else if (symbol.GetMethod.DeclaredAccessibility == Accessibility.Public)
                    getter = "get; ";
            }
            if (symbol.SetMethod != null)
            {
                if (symbol.SetMethod.DeclaredAccessibility == Accessibility.Protected)
                    setter = "protected set; ";
                else if (symbol.SetMethod.DeclaredAccessibility == Accessibility.Public)
                    setter = "set; ";
            }

            var body = $"{{ {getter}{setter}}}";

            var type = symbol.OriginalDefinition.Type.ToMinimalString();

            //if (prop.IsReadOnly) modifiers += "readonly ";

            if (symbol.IsIndexer)
                code.Append($"{modifiers}{type} this{symbol.GetIndexerParametersString()} {body}");
            else
                code.Append($"{modifiers}{type} {symbol.Name} {body}");
            return code.ToString();
        }

        static string ToEventCode(this IEventSymbol symbol, string modifiers)
        {
            var code = new StringBuilder(150);
            if (symbol.ContainingType.IsInterface())
                modifiers = "";

            var type = symbol.OriginalDefinition.Type.ToMinimalString();
            code.Append($"{modifiers}event {type} {symbol.Name};");
            return code.ToString();
        }

        static string ToFieldCode(this IFieldSymbol symbol, string modifiers)
        {
            var code = new StringBuilder(150);

            if (symbol.ContainingType.IsEnum())
            {
                if (symbol.ConstantValue != null)
                    code.Append($"{symbol.Name} = {symbol.ConstantValue},");
                else
                    code.Append($"{symbol.Name},");
            }
            else
            {
                var type = symbol.OriginalDefinition.Type.ToMinimalString();

                if (symbol.IsConst)
                    modifiers += "const ";

                if (symbol.IsReadOnly)
                    modifiers += "readonly ";

                var val = "";
                if (symbol.ConstantValue != null)
                {
                    val = symbol.ConstantValue.ToString();
                    var typeFullName = symbol.OriginalDefinition.Type.GetFullName();
                    if (typeFullName == "System.Char")
                        val = $" = {symbol.ConstantValue.To<char>().ToLiteral()}";
                    else if (typeFullName == "System.String")
                        val = $" = {val.ToLiteral()}";
                    else
                        val = $" = {val}";
                }

                code.Append($"{modifiers}{type} {symbol.Name}{val};");
            }

            return code.ToString();
        }

        static bool HiddenFromUser(this ISymbol symbol)
        {
            if (symbol is IMethodSymbol)
            {
                var method = symbol as IMethodSymbol;

                if (method.ContainingType.IsEnum()) //Enum constructor is hidden from user
                    return true;

                if (method.MethodKind == MethodKind.PropertyGet ||
                    method.MethodKind == MethodKind.PropertySet ||
                    method.MethodKind == MethodKind.EventAdd ||
                    method.MethodKind == MethodKind.EventRemove)
                    return true; //getters, setters and so on are hidden from user

                if (method.IsConstructor())
                {
                    if (!method.Parameters.Any() && method.ContainingType.Constructors.Length == 1 && method.DeclaredAccessibility == Accessibility.Public)
                        return true; //hide default constructors if it is the only public constructor
                }
            }
            return false;
        }

        static string ToMethodCode(this IMethodSymbol symbol, string modifiers)
        {
            var code = new StringBuilder(150);

            if (symbol.ContainingType.IsInterface())
                modifiers = "";

            string returnTypeAndName;
            var returnType = symbol.OriginalDefinition.ReturnType.ToMinimalString();

            if (symbol.IsConstructor())
                returnTypeAndName = $"{symbol.ContainingType.Name}";        // Printer
            else if (symbol.IsDestructor())
                returnTypeAndName = $"~{symbol.ContainingType.Name}";       // ~Printer
            else if (symbol.IsOperator())
                returnTypeAndName = $"{returnType} {symbol.GetDisplayName()}";  // operator T? (T value);
            else if (symbol.IsConversion())
                returnTypeAndName = $"{symbol.GetDisplayName()} {returnType}";  //  implicit operator DBBool(bool x)
            else
                returnTypeAndName = $"{returnType} {symbol.Name}";              //int GetIndex

            code.Append($"{modifiers.Trim()} ")                                 // public static
                .Append(returnTypeAndName)                                      // int GetIndex
                .Append(symbol.TypeParameters.ToDeclarationString())            // <T, T2>
                .Append(symbol.GetParametersString())                           //(int position, int value)
                .Append(symbol.TypeParameters.GetConstrains(singleLine: true))  // where T: class
                .Append(";");

            return code.ToString();
        }

        public static string ToReflectedCode(this ISymbol symbol, bool includeDoc = true, bool usePartials = false)
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

            if (symbol.IsOverride) modifiers += "override ";
            if (symbol.IsStatic) modifiers += "static ";
            if (symbol.IsAbstract && !(symbol as INamedTypeSymbol).IsInterface())
                modifiers += "abstract ";
            if (symbol.IsVirtual) modifiers += "virtual ";

            switch (symbol.Kind)
            {
                case SymbolKind.Property:
                    {
                        var prop = (IPropertySymbol)symbol;
                        code.Append(prop.ToPropertyCode(modifiers));
                        break;
                    }
                case SymbolKind.Field:
                    {
                        var field = (IFieldSymbol)symbol;
                        code.Append(field.ToFieldCode(modifiers));
                        break;
                    }
                case SymbolKind.Event:
                    {
                        var @event = (IEventSymbol)symbol;
                        code.Append(@event.ToEventCode(modifiers));
                        break;
                    }
                case SymbolKind.Method:
                    {
                        if (symbol.HiddenFromUser())
                            return null;

                        var method = (symbol as IMethodSymbol);
                        code.Append(method.ToMethodCode(modifiers));
                        break;
                    }
                case SymbolKind.NamedType:
                    {
                        var type = (INamedTypeSymbol)symbol;
                        code.Append(type.ToTypeCode(modifiers, usePartials));
                        break;
                    }
            }

            if (code.Length == 0)
                code.AppendLine($"{symbol.ToDisplayKind()}: {symbol.ToDisplayString()};");

            return code.ToString();
        }

        public static string ToTooltip(this ISymbol symbol, bool showOverloadInfo = false)
        {
            string symbolDoc = "";

            switch (symbol.Kind)
            {
                case SymbolKind.Property:
                    {
                        var member = (IPropertySymbol)symbol;
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
                        var member = (IFieldSymbol)symbol;
                        var type = member.Type.ToMinimalString();
                        var name = $"{member.ContainingType.ToMinimalString()}.{member.Name}";
                        symbolDoc = $"Field: {type} {name}";
                        break;
                    }
                case SymbolKind.Event:
                    {
                        var member = (IEventSymbol)symbol;
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
                        if (method.IsConstructor())
                        {
                            kind = "Constructor";
                            name = name.Replace("..ctor", "");
                            symbolDoc = $"{kind}: {name}{args}";
                        }
                        else
                        {
                            if (method.IsExtensionMethod)
                                kind += " (extension)";
                            symbolDoc = $"{kind}: {returnType} {name}{args}";
                        }

                        int overloads = symbol.ContainingType.GetMembers(method.Name).OfType<IMethodSymbol>().Count() - 1;

                        if (overloads > 0 && showOverloadInfo)
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
                        var method = (IMethodSymbol)symbol;
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