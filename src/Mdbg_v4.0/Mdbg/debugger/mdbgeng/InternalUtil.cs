//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Text;

using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorMetadata;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using System.Text.RegularExpressions;


namespace Microsoft.Samples.Debugging.MdbgEngine
{
    // Class for some internal utility functions.

    /// <summary>
    /// InternalUtil - a place for utility functions.
    /// </summary>
    public static class InternalUtil
    {
        // Helper to append generic args from tyenum in pretty format.
        // This will add a string like '<int, Foo<string>>'
        internal static void AddGenericArgs(StringBuilder sb, MDbgProcess proc, IEnumerable tyenum)
        {
            int i = 0;
            foreach (CorType t1 in tyenum)
            {
                sb.Append((i == 0) ? '<' : ',');
                InternalUtil.PrintCorType(sb, proc, t1);
                i++;
            }
            if (i > 0)
            {
                sb.Append('>');
                //System.Collections.Generic.List`1 //zos; CSScript.Npp related changes
                sb.Replace("`" + i, "");
            }
        }


        // Return class as a string.
        /// <summary>
        /// Creates a string representation of CorType.
        /// </summary>
        /// <param name="proc">A debugged process.</param>
        /// <param name="ct">A CorType object representing a type in the debugged process.</param>
        /// <returns>String representaion of the passed in type.</returns>
        public static string PrintCorType(MDbgProcess proc, CorType ct)
        {
            StringBuilder sb = new StringBuilder();
            PrintCorType(sb, proc, ct);
            return sb.ToString();
        }

        //zos; CSScript.Npp related changes
        static internal T GetFieldValue<T>(this MDbgValue value, string name)
        {
            return (T)value.GetField(name).CorValue.CastToGenericValue().GetValue();
        }

        //zos; CSScript.Npp related changes
        static string ReplaceWholeWord(this string text, string pattern, string replacement)
        {
            return Regex.Replace(text, @"\b(" + pattern + @")\b", replacement);
        }

        //zos; CSScript.Npp related changes
        static string ReplaceClrAliaces(this string text)
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
                return retval;
            }

        }

        // Print CorType to the given string builder.
        // Will print generic info. 

        internal static void PrintCorType(StringBuilder sb, MDbgProcess proc, CorType ct)
        {
            switch (ct.Type)
            {
                case CorElementType.ELEMENT_TYPE_CLASS:
                case CorElementType.ELEMENT_TYPE_VALUETYPE:
                    // We need to get the name from the metadata. We can get a cached metadata importer
                    // from a MDbgModule, or we could get a new one from the CorModule directly.
                    // Is this hash lookup to get a MDbgModule cheaper than just re-querying for the importer?
                    CorClass cc = ct.Class;
                    MDbgModule m = proc.Modules.Lookup(cc.Module);
                    Type tn = m.Importer.GetType(cc.Token);

                    sb.Append(tn.FullName.ReplaceClrAliaces());
                    AddGenericArgs(sb, proc, ct.TypeParameters);
                    return;

                // Primitives
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    sb.Append("System.Boolean".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_CHAR:
                    sb.Append("System.Char".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_I1:
                    sb.Append("System.SByte.ReplaceClrAliaces()"); return;
                case CorElementType.ELEMENT_TYPE_U1:
                    sb.Append("System.Byte".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_I2:
                    sb.Append("System.Int16".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_U2:
                    sb.Append("System.UInt16".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_I4:
                    sb.Append("System.Int32".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_U4:
                    sb.Append("System.Uint32".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_I8:
                    sb.Append("System.Int64".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_U8:
                    sb.Append("System.UInt64".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_I:
                    sb.Append("System.IntPtr".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_U:
                    sb.Append("System.UIntPtr"); return;
                case CorElementType.ELEMENT_TYPE_R4:
                    sb.Append("System.Single".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_R8:
                    sb.Append("System.Double".ReplaceClrAliaces()); return;

                // Well known class-types.
                case CorElementType.ELEMENT_TYPE_OBJECT:
                    sb.Append("System.Object".ReplaceClrAliaces()); return;
                case CorElementType.ELEMENT_TYPE_STRING:
                    sb.Append("System.String".ReplaceClrAliaces()); return;


                // Special compound types. Based off first type-param
                case CorElementType.ELEMENT_TYPE_SZARRAY:
                case CorElementType.ELEMENT_TYPE_ARRAY:
                case CorElementType.ELEMENT_TYPE_BYREF:
                case CorElementType.ELEMENT_TYPE_PTR:
                    CorType t = ct.FirstTypeParameter;
                    PrintCorType(sb, proc, t);
                    switch (ct.Type)
                    {
                        case CorElementType.ELEMENT_TYPE_SZARRAY:
                            sb.Append("[]");
                            return;
                        case CorElementType.ELEMENT_TYPE_ARRAY:
                            int rank = ct.Rank;
                            sb.Append('[');
                            for (int i = 0; i < rank - 1; i++)
                            {

                                sb.Append(',');
                            }
                            sb.Append(']');
                            return;
                        case CorElementType.ELEMENT_TYPE_BYREF:
                            sb.Append("&");
                            return;
                        case CorElementType.ELEMENT_TYPE_PTR:
                            sb.Append("*");
                            return;
                    }
                    Debug.Assert(false); // shouldn't have gotten here.             
                    return;

                case CorElementType.ELEMENT_TYPE_FNPTR:
                    sb.Append("*(...)");
                    return;
                case CorElementType.ELEMENT_TYPE_TYPEDBYREF:
                    sb.Append("typedbyref");
                    return;
                default:
                    sb.Append("<unknown>");
                    return;
            }
        } // end PrintClass
    } // end class InternalUtil
}