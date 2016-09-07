using Intellisense.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSScriptIntellisense
{
    public static class SimpleCodeCompletion
    {
        public static void Init()
        {
            MonoCompletionEngine.Init();
            if (Config.Instance.RoslynIntellisense)
                RoslynCompletionEngine.Init();
        }

        static IEngine MonoEngine = new MonoCompletionEngine();

        static IEngine RoslynEngine
        {
            get
            {
                return RoslynCompletionEngine.GetInstance();
            }
        }

        public static char[] Delimiters = "\\\t\n\r .,:;'\"=[]{}()+-/!?@$%^&*«»><#|~`".ToCharArray();
        public static char[] CSS_Delimiters = "\\\t\n\r .,:;'\"=[]{}()-!?@$%^&*«»><#|~`".ToCharArray();
        static char[] lineDelimiters = new char[] { '\n', '\r' };

        static IEnumerable<ICompletionData> GetCSharpScriptCompletionData(string editorText, int offset)
        {
            var directiveLine = GetCSharpScriptDirectiveLine(editorText, offset);

            if (directiveLine.StartsWith("//css_")) //e.g. '//css_ref'
            {
                var word = Npp.GetWordAtPosition(offset); //e.g. 'css_ref'

                if (word.StartsWith("css_")) //directive itself
                {
                    return CssCompletionData.AllDirectives;
                }
                else //directive is complete and user is typing the next word (directive argument)
                {
                    if (directiveLine.StartsWith("//css_ref"))
                        return CssCompletionData.DefaultRefAsms;
                }
            }

            return null;
        }

        public static IEnumerable<ICompletionData> GetCompletionData(string editorText, int offset, string fileName, bool isControlSpace = true) // not the best way to put in the whole string every time
        {
            try
            {
                if (string.IsNullOrEmpty(editorText))
                    return new ICompletionData[0];

                var data = (GetCSharpScriptCompletionData(editorText, offset) ??
                            (Config.Instance.RoslynIntellisense ?
                            RoslynEngine.GetCompletionData(editorText, offset, fileName, isControlSpace) :
                            MonoEngine.GetCompletionData(editorText, offset, fileName, isControlSpace))
                            ).ToList();

                //suggest default CS-Script usings as well
                var extraItems = new List<ICompletionData>();
                var line = Npp.GetLine(Npp.GetLineNumber(offset)).Trim();

                bool isUsing = (line == "using");

                if (isUsing)
                {
                    extraItems.AddRange(CssCompletionData.DefaultNamespaces);
                    extraItems.ForEach(x => x.CompletionText = x.CompletionText + ";");

                }

                int length = Math.Min(editorText.Length - offset, 20);
                string rightHalfOfLine = editorText.Substring(offset, length)
                                                   .Split(new[] { '\n' }, 2).FirstOrDefault();

                data.ForEach(x =>
                {
                    if (isUsing)
                    {
                        x.CompletionText = x.CompletionText + ";";
                    }
                    else if (Config.Instance.UseMethodBrackets)
                    {
                        if (x.CompletionType == CompletionType.method || x.CompletionType == CompletionType.extension_method)
                        {
                            //"Console.WriteLi| " but not "Console.Write|(" 
                            if (rightHalfOfLine == null || rightHalfOfLine.StartsWith(" ") || rightHalfOfLine.StartsWith("\r") || rightHalfOfLine.StartsWith("\n"))
                            {
                                x.CompletionText += "(";
                                if (x.InvokeParametersSet)
                                {
                                    if (x.InvokeParameters.Count() == 0 || (x.InvokeParameters.Count() == 1 && x.CompletionType == CompletionType.extension_method))
                                        x.CompletionText += ")"; //like .Clone()
                                }
                            }
                        }
                    }
                });

                return data.Concat(extraItems);
            }
            catch (Exception e)
            {
                e.LogAsDebug();
                return new ICompletionData[0]; //the exception can happens even for the internal NRefactor-related reasons
            }
        }

        //----------------------------------
        public static void ResetProject(Tuple<string, string>[] sourceFiles = null, params string[] assemblies)
        {
            if (Config.Instance.RoslynIntellisense)
                RoslynEngine.ResetProject(sourceFiles, assemblies);
            else
                MonoEngine.ResetProject(sourceFiles, assemblies); //at this stage always reset mono project as it is always used for other then autocomplete operations
        }

        //----------------------------------
        public static CodeMapItem[] GetMapOf(string code)
        {
            bool injected = CSScriptHelper.DecorateIfRequired(ref code);
            CodeMapItem[] map;

            if (Config.Instance.RoslynIntellisense)
                map = RoslynEngine.GetMapOf(code, injected);
            else
                map = MonoEngine.GetMapOf(code, injected);

            if (injected)
            {
                int injectedLineOffset = CSScriptHelper.GetDecorationInfo(code).Item1;
                int injectedLineNumber = code.Substring(0, injectedLineOffset).Split('\n').Count();

                map = map.Where(i => i.Line != injectedLineNumber).ToArray();

                foreach (CodeMapItem item in map)
                {
                    if (item.Line >= injectedLineNumber)
                        item.Line -= 1;
                }
            }
            return map;
        }
        //----------------------------------
        public static IEnumerable<Intellisense.Common.TypeInfo> GetMissingUsings(string editorText, int offset, string fileName) // not the best way to put in the whole string every time
        {
            string nameToResolve = GetWordAt(editorText, offset);
            return GetPossibleNamespaces(editorText, nameToResolve, fileName);
        }

        internal static IEnumerable<Intellisense.Common.TypeInfo> GetPossibleNamespaces(string editorText, string nameToResolve, string fileName) // not the best way to put in the whole string every time
        {
            if (Config.Instance.RoslynIntellisense)
                return RoslynEngine.GetPossibleNamespaces(editorText, nameToResolve, fileName);
            else
                return MonoEngine.GetPossibleNamespaces(editorText, nameToResolve, fileName);
        }

        //----------------------------------
        public static string[] GetMemberInfo(string editorText, int offset, string fileName, bool collapseOverloads)
        {
            int methodStartPos;
            return GetMemberInfo(editorText, offset, fileName, collapseOverloads, out methodStartPos);
        }

        public static string[] GetMemberInfo(string editorText, int offset, string fileName, bool collapseOverloads, out int methodStartPos)
        {
            if (Config.Instance.RoslynIntellisense)
                return RoslynEngine.GetMemberInfo(editorText, offset, fileName, collapseOverloads, out methodStartPos);
            else
                return MonoEngine.GetMemberInfo(editorText, offset, fileName, collapseOverloads, out methodStartPos);
        }

        //----------------------------------
        static public string[] FindReferences(string editorText, int offset, string fileName)
        {
            if (Config.Instance.RoslynIntellisense)
                return RoslynEngine.FindReferences(editorText, offset, fileName);
            else
                return MonoEngine.FindReferences(editorText, offset, fileName);
        }

        //----------------------------------
        static public DomRegion ResolveMember(string editorText, int offset, string fileName)
        {
            return ResolveCSharpScriptMember(editorText, offset) ?? ResolveCSharpMember(editorText, offset, fileName);
        }

        static DomRegion? ResolveCSharpScriptMember(string editorText, int offset)
        {
            var directiveLine = GetCSharpScriptDirectiveLine(editorText, offset);

            if (directiveLine.StartsWith("//css_"))
            {
                var css_directive = directiveLine.Split(SimpleCodeCompletion.CSS_Delimiters).FirstOrDefault();
                return CssCompletionData.ResolveDefinition(css_directive);
            }
            else
                return null;
        }

        static DomRegion ResolveCSharpMember(string editorText, int offset, string fileName)
        {
            if (Config.Instance.RoslynIntellisense)
                return  RoslynEngine.ResolveCSharpMember(editorText, offset, fileName);
            else
                return MonoEngine.ResolveCSharpMember(editorText, offset, fileName);
        }

        //----------------------------------

        internal static string GetWordAt(string editorText, int offset)
        {
            string retval = "";

            if (offset > 0 && editorText[offset - 1] != '.') //avoid "type.|"
            {
                //following VS default practice:  "type|."
                for (int i = offset - 1; i >= 0; i--)
                    if (SimpleCodeCompletion.Delimiters.Contains(editorText[i]))
                    {
                        retval = editorText.Substring(i + 1, offset - i - 1);
                        break;
                    }

                //extend the VS practice with the partial word support
                for (int i = offset; i < editorText.Length; i++)
                    if (SimpleCodeCompletion.Delimiters.Contains(editorText[i]))
                        break;
                    else
                        retval += editorText[i];
            }
            return retval;
        }

        internal static string GetPrevWordAt(string editorText, int offset)
        {
            string retval = "";

            int primaryWordStart = -1;

            if (offset > 0 && editorText[offset - 1] != '.') //avoid "type.|"
            {
                //following VS default practice:  "type|."
                for (int i = offset - 1; i >= 0; i--)
                    if (SimpleCodeCompletion.Delimiters.Contains(editorText[i]))
                    {
                        if (primaryWordStart == -1)
                        {
                            primaryWordStart = i;
                        }
                        else
                        {
                            retval = editorText.Substring(i + 1, primaryWordStart - i - 1).Trim();
                            break;
                        }
                    }
            }
            return retval;
        }

        static string GetCSharpScriptDirectiveLine(string editorText, int offset)
        {
            int i = 0;

            //need to allow 'space' as we are looking for a CS-Script line not a token
            var delimiters = SimpleCodeCompletion.CSS_Delimiters.Where(x => x != ' ');

            if (editorText[offset-1] != '.') //we may be at the partially complete word
                for (i = offset - 1; i >= 0; i--)
                    if (delimiters.Contains(editorText[i]))
                    {
                        offset = i + 1;
                        break;
                    }

            if (i == -1)
                offset = 0;

            var textOnRight = editorText.Substring(offset);
            var endPos = textOnRight.IndexOf('\n');
            if (endPos != -1)
            {
                if (endPos == 0)
                    return "";

                textOnRight = textOnRight.Substring(0, endPos - 1).TrimEnd('\r');
            }
            return textOnRight;
        }
    }
}