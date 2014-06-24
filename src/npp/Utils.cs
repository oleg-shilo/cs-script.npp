using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Tools.Mdbg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace npp
{
    static class Utils
    {
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

        static public string ReplaceWholeWord(this string text, string pattern, string replacement)
        {
            return Regex.Replace(text, @"\b(" + pattern + @")\b", replacement);
        }

        public static string Join(this IEnumerable<string> items, string delimiter = "")
        {
            return string.Join(delimiter, items.ToArray());
        }

        static public string[] GetCodeUsings(string csFile)
        {
            return GetCodeUsingsSimple(csFile);
            //return GetCodeUsingsNRefactory(csFile);
        }

        static public string[] GetCodeUsingsSimple(string csFile)
        {
            //Very simplistic implementation with no dependency

            var result = new List<string>();
            try
            {
                using (var reader = new StreamReader(csFile))
                {
                    int blockCommantLevel = 0;

                    string line;
                    while (null != (line = reader.ReadLine()))
                    {
                        line = line.Trim();

                        if (blockCommantLevel == 0)
                        {
                            if (Regex.Matches(line, @"(namespace\s*|class\s*|delegate\s*|public\s*|private\s*|protected\s*|internal\s*|static\s*)").Count != 0)
                            {
                                break; //end of 'usings' header 
                            }
                            else if (line.StartsWith("//"))
                            {
                                continue;
                            }
                            else if (line.StartsWith("using "))
                            {
                                foreach (string statement in line.Split(';'))
                                {
                                    if (statement.StartsWith("using "))
                                    {
                                        string @namespace = statement.Substring("using ".Length);
                                        if (!string.IsNullOrWhiteSpace(@namespace) && !@namespace.Contains(' '))
                                            result.Add(@namespace);
                                    }
                                }
                            }
                        }

                        int opening = Regex.Matches(line, @"/\*").Count;
                        int closing = Regex.Matches(line, @"\*/").Count;

                        blockCommantLevel += (opening - closing);
                    }
                }

            }
            catch { }
            return result.ToArray();
        }

        static public string[] GetCodeUsingsNRefactory(string csFile)
        {
            //very reliable and accurate but with a huge dependency cost: ICSharpCode.NRefactory.dll, ICSharpCode.NRefactory.CSharp.dll
            //var syntaxTree = new CSharpParser().Parse(code, "demo.cs");

            //return syntaxTree.Children.DeepAll(x => x is UsingDeclaration)
            //                          .Cast<UsingDeclaration>()
            //                          .Select(x => x.Namespace)
            //                          .ToArray();

            throw new NotImplementedException();
        }

        public static CorValue CreateCorValue(this MDbgProcess process, string value)
        {
            CorEval eval = process.Threads.Active.CorThread.CreateEval();
            eval.NewString(value);
            process.Go().WaitOne();
            return (process.StopReason as EvalCompleteStopReason).Eval.Result;
        }

        //    return value.TypeName.StartsWith("System.Collections.Generic.List<");
        //}

        public static MDbgValue[] GenerateListItems(this MDbgValue value, int maxCount = int.MaxValue)
        {
            try
            {
                return value.GetListItems(maxCount);
            }
            catch
            {
                return new[] { new MDbgValue(value.Process, "{items}", value.Process.CreateCorValue("Cannot retrieve items")) };
            }
        }

        public static MDbgValue[] GenerateDictionaryItems(this MDbgValue value, int maxCount = int.MaxValue)
        {
            try
            {
                return value.GetDictionaryItems(maxCount);
            }
            catch
            {
                return new[] { new MDbgValue(value.Process, "{items}", value.Process.CreateCorValue("Cannot retrieve items")) };
            }
        }

        //the following class is for reference only as it is moved to the Microsoft.Samples.Debugging.MdbgEngine.Extensions class 
        internal static class Extensions
        {
            public static bool IsSet(CorDebugUserState value, CorDebugUserState bitValue)
            {
                return (value | bitValue) == value;
            }

            public static bool IsEvalSafe(MDbgProcess proc)
            {
                ICorDebugThread dbgThred = proc.Threads.Active.CorThread.Raw;

                ///http://blogs.msdn.com/b/jmstall/archive/2005/11/15/funceval-rules.aspx
                ///Dangers of Eval: http://blogs.msdn.com/b/jmstall/archive/2005/03/23/400794.aspx
                CorDebugUserState pState;
                dbgThred.GetUserState(out pState);
                //CorDebugUserState.USER_NONE was identified by the experimenting
                if ((!pState.IsSet(CorDebugUserState.USER_UNSAFE_POINT) && pState.IsSet(CorDebugUserState.USER_STOPPED)) || pState == CorDebugUserState.USER_NONE)
                    return true;
                else
                    return false;
            }
        }
    }
}