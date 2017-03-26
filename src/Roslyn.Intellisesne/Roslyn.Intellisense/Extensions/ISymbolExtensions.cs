using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoslynIntellisense
{
    public static class ISymbolExtensions
    {
        public static INamedTypeSymbol GetRootType(this ISymbol symbol)
        {
            //itself if it is a type or containing type if a member
            if (symbol is INamedTypeSymbol)
                return (INamedTypeSymbol) symbol;
            else
                return symbol.ContainingType;
        }

        public static bool IsEnum(this INamedTypeSymbol symbol)
        {
            return symbol.BaseType != null && symbol.BaseType.GetFullName() == "System.Enum";
        }

        public static string GetDisplayGroup(this ISymbol symbol)
        {
            //<sorting>:<type>
            if (symbol.IsConstructor()) return "1:C";
            else if (symbol.IsDestructor()) return "2:D";
            else if (symbol.IsField()) return "3:F";
            else if (symbol.IsEvent()) return "4:E";
            else if (symbol.IsProperty()) return "5:P";
            else if (symbol.IsMethod()) return "6:M";
            else if (symbol.IsOperator())
            {
                if (symbol.Name == "op_True" || symbol.Name == "op_False") return "7:O.2";
                else return "7:O";
            }
            else if (symbol.IsConversion()) return "8:C";
            return "9:G"; //generic
        }

        public static bool IsField(this ISymbol symbol)
        {
            return symbol.Kind == SymbolKind.Field;
        }

        public static bool IsProperty(this ISymbol symbol)
        {
            return symbol.Kind == SymbolKind.Property;
        }

        public static bool IsEvent(this ISymbol symbol)
        {
            return symbol.Kind == SymbolKind.Event;
        }

        public static bool IsMethod(this ISymbol symbol)
        {
            return symbol.Kind == SymbolKind.Method && ((IMethodSymbol) symbol).MethodKind == MethodKind.Ordinary;
        }

        static Dictionary<string, string> opNamesMap = new Dictionary<string, string>
        {
            {"op_Addition", "operator +" },
            {"op_Subtraction", "operator -" },
            {"op_Multiply", "operator *" },
            {"op_Division", "operator /" },
            {"op_Modulus", "operator %" },
            {"op_BitwiseAnd", "operator &" },
            {"op_BitwiseOr", "operator |" },
            {"op_ExclusiveOr", "operator ^" },
            {"op_LeftShift", "operator <<" },
            {"op_RightShift", "operator >>" },
            {"op_LogicalNot", "operator !" },
            {"op_OnesComplement", "operator ~" },
            {"op_Decrement", "operator --" },
            {"op_Increment", "operator ++" },
            {"op_Equality", "operator ==" },
            {"op_Inequality", "operator !=" },
            {"op_LessThan", "operator <" },
            {"op_GreaterThan", "operator >" },
            {"op_LessThanOrEqual", "operator <=" },
            {"op_GreaterThanOrEqual", "operator >=" },
            {"op_True", "operator true" },
            {"op_False", "operator false" },
            {"op_Implicit", "implicit operator" },
            {"op_Explicit", "explicit operator" },
        };

        public static string GetDisplayName(this IMethodSymbol symbol)
        {
            if ((symbol.IsConversion() || symbol.IsOperator()) && opNamesMap.ContainsKey(symbol.Name))
                return opNamesMap[symbol.Name];
            else
                return symbol.Name;
        }

        public static bool IsConstructor(this ISymbol symbol)
        {
            if (symbol.Kind != SymbolKind.Method) return false;
            return symbol.Kind == SymbolKind.Method && ((IMethodSymbol) symbol).MethodKind == MethodKind.Constructor;
        }

        public static bool IsDestructor(this ISymbol symbol)
        {
            if (symbol.Kind != SymbolKind.Method) return false;
            return symbol.Kind == SymbolKind.Method && ((IMethodSymbol) symbol).MethodKind == MethodKind.Destructor;
        }

        public static bool IsInterface(this INamedTypeSymbol symbol)
        {
            return symbol != null && symbol.TypeKind == TypeKind.Interface;
        }
        public static bool IsDelegate(this INamedTypeSymbol symbol)
        {
            return symbol != null && symbol.TypeKind == TypeKind.Delegate;
        }

        public static bool IsOperator(this ISymbol symbol)
        {
            if (symbol.Kind != SymbolKind.Method) return false;
            var method = (IMethodSymbol) symbol;
            return symbol.Kind == SymbolKind.Method && method.MethodKind == MethodKind.UserDefinedOperator;
        }

        public static bool IsConversion(this ISymbol symbol)
        {
            if (symbol.Kind != SymbolKind.Method) return false;
            var method = (IMethodSymbol) symbol;
            return symbol.Kind == SymbolKind.Method && method.MethodKind == MethodKind.Conversion;
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

        public static string ToMinimalString(this ISymbol type)
        {
            if (type.ContainingSymbol != null && type.ContainingSymbol.Kind == SymbolKind.NamedType)
            {
                var nms = type.GetNamespace();
                return type.ToDisplayString().Replace(nms + ".", "");
            }
            else
            {
                //doesnt handle nested classes
                return type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            }
        }

        public static INamedTypeSymbol[] GetParentClasses(this ISymbol type)
        {
            var result = new List<INamedTypeSymbol>();

            var parent = type.ContainingType;
            while (parent != null)
            {
                result.Add(parent);
                parent = parent.ContainingType;
            }

            return result.ToArray();
        }

        public static string GetFullName(this ISymbol type)
        {
            if (type == null) return null;

            List<string> parts = new List<string>();
            var nm = type.ContainingSymbol; //important: ContainingSymbol will cover both namespaces and nested type's parents

            while (nm != null && nm.Name.HasText())
            {
                parts.Add(nm.Name);
                nm = nm.ContainingSymbol;
            }

            parts.Reverse();
            parts.Add(type.Name);
            return string.Join(".", parts.ToArray());
        }

        public static string GetFullName(this BaseTypeDeclarationSyntax token)
        {
            if (token == null)
                return "";

            dynamic node = token;
            if (node == null) return null;

            List<string> parts = new List<string>();
            parts.Add(node.Identifier.Text);
            var parent = node.Parent;

            while (parent != null && parent is BaseTypeDeclarationSyntax)
            {
                parts.Add(parent.Identifier.Text);
                parent = parent.Parent;
            }

            parts.Reverse();
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

        public static string ToDeclarationString(this IEnumerable<ITypeParameterSymbol> typeParameters)
        {
            if (typeParameters.HasAny())
            {
                string prms = typeParameters.Select(x => x.Name).JoinBy(", ");
                return $"<{prms}>";
            }

            return "";
        }

        public static string ToInheritanceString(this INamedTypeSymbol type)
        {
            var code = new StringBuilder();

            string baseType = null;

            if (type.BaseType != null && !type.BaseType.GetFullName().OneOf("System.ValueType", "System.Object"))
                baseType = type.BaseType.ToMinimalString();

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

        public static IMethodSymbol GetMethod(this INamedTypeSymbol type, string name)
        {
            return type.GetMembers(name).Cast<IMethodSymbol>().First();
        }

        public static string GetParametersString(this IMethodSymbol method)
        {
            var prms = "";
            if (method.Parameters.HasAny())
                prms = GetParametersString(method.OriginalDefinition.Parameters);

            if (prms.Any() && method.IsExtensionMethod)
                prms = "this " + prms;

            return $"({prms})";
        }

        public static string GetIndexerParametersString(this IPropertySymbol method)
        {
            var prms = "";
            if (method.Parameters.HasAny())
                prms = GetParametersString(method.OriginalDefinition.Parameters);

            return $"[{prms}]";
        }

        public static string GetParametersString(this IEnumerable<IParameterSymbol> parameters)
        {
            if (parameters.HasAny())
            {
                string prms = string.Join(", ", parameters
                                                      .Select(p =>
                                                      {
                                                          string refKind = "";

                                                          if (p.RefKind == RefKind.Ref)
                                                              refKind = "ref ";
                                                          else if (p.RefKind == RefKind.Out)
                                                              refKind = "out ";

                                                          if (p.IsParams)
                                                              refKind = "params " + refKind;

                                                          string defValue = "";
                                                          if (p.HasExplicitDefaultValue)
                                                          {
                                                              var val = p.ExplicitDefaultValue;

                                                              if (p.Type.GetFullName() == "System.Char")
                                                                  val = $"'{val}'";
                                                              else if (p.Type.GetFullName() == "System.String")
                                                                  val = $"\"{val}\"";

                                                              defValue = " = " + val;
                                                          }
                                                          return $"{refKind}{p.Type.ToMinimalString()} {p.Name}{defValue}";
                                                      })
                                                      .ToArray());

                return prms;
            }

            return "";
        }

        public static string GetConstrains(this IEnumerable<ITypeParameterSymbol> typeParameters, bool singleLine = false)
        {
            var code = new StringBuilder();

            if (typeParameters.HasAny())
                foreach (var item in typeParameters)
                {
                    var constrains = item.GetConstrains();
                    if (constrains.HasAny())
                        if (singleLine)
                            code.Append(" " + constrains);
                        else
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
    }
}