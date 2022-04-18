using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSScriptIntellisense;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;

//The same as UltraSharp.Reflector but based on Cecil
namespace UltraSharp.Cecil
{
    public class Reflector
    {
        static public Func<string, string[]> GetCodeCompileOutput;

        static public string[] GetCodeUsings(string code)
        {
            try
            {
                var syntaxTree = new CSharpParser().Parse(code, "demo.cs");

                return syntaxTree.Children.DeepAll(x => x is UsingDeclaration)
                                          .Cast<UsingDeclaration>()
                                          .Select(x => x.Namespace)
                                          .ToArray();
            }
            catch
            {
                return new string[0];
            }
        }
    }

    public static class ReflectorExtensions
    {
        public static bool IgnoreDocumentationExceptions = false;

        public static string GetTypeName(this ITypeReference type, string[] usedNamespaces)
        {
            string retval;

            if (type is ParameterizedTypeReference)
                retval = (type as ParameterizedTypeReference).GetTypeName(usedNamespaces); //generic type
            else if (type is ArrayTypeReference)
                retval = (type as ArrayTypeReference).GetTypeName(usedNamespaces);
            else if (type is DefaultUnresolvedTypeDefinition)
                retval = (type as DefaultUnresolvedTypeDefinition).FullName.NormaliseClrName(usedNamespaces);
            else if (type is PointerTypeReference)
                retval = (type as PointerTypeReference).GetTypeName(usedNamespaces);
            else if (type is DefaultResolvedTypeDefinition)
                retval = (type as DefaultResolvedTypeDefinition).GetTypeName(usedNamespaces);
            else if (type is ByReferenceTypeReference)
                retval = (type as ByReferenceTypeReference).ElementType.GetTypeName(usedNamespaces);
            else if (type is GetClassTypeReference)
                retval = (type as GetClassTypeReference).GetTypeName(usedNamespaces);
            else
                retval = type.ToString();

            return retval.NormaliseClrName(usedNamespaces);
        }

        static public string NormaliseClrName(this string text, string[] namespacesToHide)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            else
            {
                var retval = text.ReplaceClrAliaces();

                foreach (string ns in namespacesToHide)
                    if (retval.StartsWith(ns) && retval.IndexOf('.', ns.Length + 1) == -1)
                        return retval.Substring(ns.Length + 1);

                return retval;
            }
        }

        public static string ResolveTypeParameters(this string code, List<string> typeParamsList)
        {
            //This is NRefactory naming convention for the Generic type parameters !!0->TSource, !!1->TDestination

            for (int i = 0; i < typeParamsList.Count; i++)
                code = code.Replace("!!" + i, typeParamsList[i]);

            return code;
        }

        public static string GetTypeName(this ParameterizedTypeReference obj, string[] usedNamespaces)
        {
            string retval = "";

            if (obj.GenericType is DefaultUnresolvedTypeDefinition)
                retval = (obj.GenericType as DefaultUnresolvedTypeDefinition).GetTypeName(usedNamespaces);
            else if (obj.GenericType is GetClassTypeReference)
                retval = (obj.GenericType as GetClassTypeReference).GetTypeName(usedNamespaces);
            else
                retval = obj.GenericType.ToString();

            bool isNullable = (retval == "System.Nullable" || retval == "Nullable");
            if (isNullable)
                retval = "";
            else
                retval += "<";

            foreach (ITypeReference arg in obj.TypeArguments)
            {
                if (arg != obj.TypeArguments.First())
                    retval += ", ";

                string typeName;

                if (arg is ParameterizedTypeReference)
                    typeName = (arg as ParameterizedTypeReference).GetTypeName(usedNamespaces);
                else if (arg is DefaultUnresolvedTypeDefinition)
                    typeName = (arg as DefaultUnresolvedTypeDefinition).FullName.NormaliseClrName(usedNamespaces);
                else if (arg is TypeParameterReference)
                    typeName = (arg as TypeParameterReference).ToString();
                else if (arg is GetClassTypeReference)
                    typeName = (arg as GetClassTypeReference).GetTypeName(usedNamespaces);
                else
                    typeName = arg.ToString();

                retval += typeName;
            }
            if (isNullable)
                retval += "?";
            else
                retval += ">";

            retval = retval.NormaliseClrName(usedNamespaces);

            return retval;
        }

        public static string GetTypeName(this GetClassTypeReference obj, string[] usedNamespaces)
        {
            string retval;

            if (string.IsNullOrEmpty(obj.Namespace))
                retval = obj.Name;
            else
                retval = obj.Namespace + "." + obj.Name;

            return retval.NormaliseClrName(usedNamespaces);
        }

        public static string GetTypeName(this PointerTypeReference obj, string[] usedNamespaces)
        {
            string retval = obj.ElementType.GetTypeName(usedNamespaces) + "*";

            return retval.NormaliseClrName(usedNamespaces);
        }

        public static string GetTypeName(this ArrayTypeReference obj, string[] usedNamespaces)
        {
            string dimentions = new string(',', obj.Dimensions - 1);
            string retval = obj.ElementType.GetTypeName(usedNamespaces) + "[" + dimentions + "]";

            return retval.NormaliseClrName(usedNamespaces);
        }

        public static string GetTypeName(this ITypeDefinition typeDef, string[] usedNamespaces)
        {
            var info = typeDef as DefaultResolvedTypeDefinition;

            return info.FullName.NormaliseClrName(usedNamespaces);
        }

        public static bool IsNullOrEmpty(this string obj)
        {
            return string.IsNullOrEmpty(obj);
        }

        public static bool IsNotEmpty(this string obj)
        {
            return !string.IsNullOrEmpty(obj);
        }
    }
}