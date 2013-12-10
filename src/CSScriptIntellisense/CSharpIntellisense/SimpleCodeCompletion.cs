using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.Documentation;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Mono.Cecil;
using UltraSharp.Cecil;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CSScriptIntellisense
{
    public static class SimpleCodeCompletion
    {
        static Lazy<IList<IUnresolvedAssembly>> builtInLibs = new Lazy<IList<IUnresolvedAssembly>>(
          delegate
          {
              Assembly[] assemblies = {
					typeof(object).Assembly,                    // mscorlib.dll
					typeof(Uri).Assembly,                       // System.dll
					typeof(Form).Assembly,                      // System.Windows.Forms.dll
					typeof(System.Linq.Enumerable).Assembly,    // System.Core.dll
					typeof(System.Xml.XmlDocument).Assembly,    // System.Xml.dll
					typeof(System.Drawing.Bitmap).Assembly,     // System.Drawing.dll
				};

              var projectContents = new IUnresolvedAssembly[assemblies.Length];
              Parallel.For(0, assemblies.Length, i =>
                  {
                      projectContents[i] = new CecilLoader { DocumentationProvider = GetXmlDocumentation(assemblies[i].Location) }.LoadAssemblyFile(assemblies[i].Location);
                  });
              return projectContents;
          });

        public static void Init()
        {
            builtInLibs.Value.ToString(); //anything just to de-reference it
        }

        public static char[] Delimiters = new[] { '\n', '\r', '\t', '!', '"', '#', ' ', '%', '&', '\'', '(', ')', '*', ',', '-', '.', '/', ':', ';', '?', '@', '[', '\\', ']', '{', '}', '¡', '«', '­', '·', '»', '>', '<' };

        static public XmlDocumentationProvider GetXmlDocumentation(string dllPath)
        {
            if (string.IsNullOrEmpty(dllPath))
                return null;

            var xmlFileName = Path.GetFileNameWithoutExtension(dllPath) + ".xml";
            var localPath = Path.Combine(Path.GetDirectoryName(dllPath), xmlFileName);
            if (File.Exists(localPath))
                return new XmlDocumentationProvider(localPath);

            //if it's a .NET framework assembly it's in one of following folders

            var netPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\" + xmlFileName);
            if (File.Exists(netPath))
                return new XmlDocumentationProvider(netPath);

            return null;
        }

        public static void ResetProject(Tuple<string, string>[] sourceFiles = null, params string[] assemblies)
        {
            Project = null;

            if (sourceFiles == null)
                sourceFiles = new Tuple<string, string>[0]; //will happen only during the testing

            var projectContents = new IUnresolvedAssembly[assemblies.Length];
            Parallel.For(0, assemblies.Length, i =>
                         {
                             projectContents[i] = new CecilLoader { DocumentationProvider = GetXmlDocumentation(assemblies[i]) }.LoadAssemblyFile(assemblies[i]);
                         });

            var unresolvedAsms = builtInLibs.Value.Concat(projectContents);
            var unresolvedFiles = new IUnresolvedFile[sourceFiles.Length];
            Parallel.For(0, unresolvedFiles.Length, i =>
                         {
                             var pair = sourceFiles[i];
                             var syntaxTree = new CSharpParser().Parse(pair.Item1, pair.Item2);
                             syntaxTree.Freeze();
                             unresolvedFiles[i] = syntaxTree.ToTypeSystem();
                         });

            IProjectContent project = new CSharpProjectContent();
            project = project.AddAssemblyReferences(unresolvedAsms);
            project = project.AddOrUpdateFiles(unresolvedFiles);
            Project = project;
        }

        static IProjectContent Project;



        public static IEnumerable<ICompletionData> GetCompletionData(string editorText, int offset, string fileName, bool isControlSpace = true, bool prepareForDisplay = true) // not the best way to put in the whole string every time
        {


            try
            {
                if (Project == null || string.IsNullOrEmpty(editorText))
                    return new ICompletionData[0];

                var doc = new ReadOnlyDocument(editorText);

                if (editorText[offset] != '.') //we may be at the partially complete word
                    for (int i = offset - 1; i >= 0; i--)
                        if (SimpleCodeCompletion.Delimiters.Contains(editorText[i]))
                        {
                            offset = i + 1;
                            break;
                        }

                var location = doc.GetLocation(offset);

                var syntaxTree = new CSharpParser().Parse(editorText, fileName);
                syntaxTree.Freeze();
                var unresolvedFile = syntaxTree.ToTypeSystem();

                Project = Project.AddOrUpdateFiles(unresolvedFile);


                //note project should be reassigned/recreated every time we add asms or file

                //IProjectContent project = new CSharpProjectContent();
                //project = project.AddAssemblyReferences(builtInLibs.Value);
                //project = project.AddOrUpdateFiles(unresolvedFile);

                //IProjectContent project = new CSharpProjectContent().AddAssemblyReferences(builtInLibs.Value).AddOrUpdateFiles(unresolvedFile);

                var completionContextProvider = new DefaultCompletionContextProvider(doc, unresolvedFile);

                var compilation = Project.CreateCompilation();
                var resolver = unresolvedFile.GetResolver(compilation, location);

                var engine = new CSharpCompletionEngine(doc,
                                                        completionContextProvider,
                                                        new SimpleCompletionDataFactory(resolver),
                                                        Project,
                                                        resolver.CurrentTypeResolveContext);

                var data = engine.GetCompletionData(offset, isControlSpace);

                if (prepareForDisplay)
                    return data.PrepareForDisplay();
                else
                    return data;
            }
            catch
            {
                return new ICompletionData[0]; //the exception can happens even for the internal NRefactor-related reasons 
            }
        }

        public static IEnumerable<TypeInfo> GetMissingUsings(string editorText, int offset, string fileName) // not the best way to put in the whole string every time
        {
            string nameToResolve = GetWordAt(editorText, offset);
            return GetPossibleNamespaces(editorText, nameToResolve, fileName);
        }

        public static IEnumerable<TypeInfo> GetPossibleNamespaces(string editorText, string nameToResolve, string fileName) // not the best way to put in the whole string every time
        {
            try
            {
                if (Project == null && string.IsNullOrEmpty(nameToResolve))
                    return new TypeInfo[0];

                var syntaxTree = new CSharpParser().Parse(editorText, fileName);
                syntaxTree.Freeze();
                var unresolvedFile = syntaxTree.ToTypeSystem();

                Project = Project.AddOrUpdateFiles(unresolvedFile);

                var srcNamespaces = Project.Files
                                           .SelectMany(x => x.TopLevelTypeDefinitions)
                                           .Union(Project.Files
                                                          .SelectMany(x => x.TopLevelTypeDefinitions)
                                                          .SelectMany(x => x.NestedTypes))
                                           .Where(x => x.Name == nameToResolve)
                                           .Distinct()
                                           .Select(x => new TypeInfo { Namespace = x.Namespace, FullName = x.FullName });

                var asmNamespaces = Project.AssemblyReferences
                                           .SelectMany(x => ((DefaultUnresolvedAssembly)x).TopLevelTypeDefinitions)
                                           .Union(Project.AssemblyReferences
                                                         .SelectMany(x => ((DefaultUnresolvedAssembly)x).TopLevelTypeDefinitions)
                                                         .SelectMany(x => x.NestedTypes))
                                           .Where(x => x.Name == nameToResolve)
                                           .Distinct()
                                           .Select(x => new TypeInfo { Namespace = x.Namespace, FullName = x.FullName });

                return srcNamespaces.Union(asmNamespaces)
                                    .OrderBy(x => x.Namespace)
                                    .DistinctBy(x => x.FullName)
                                    .ToArray();
            }
            catch
            {
                return new TypeInfo[0]; //the exception can happens even for the internal NRefactor-related reasons 
            }
        }

        static char[] lineDelimiters = new char[] { '\n', '\r' };

        public static string[] GetMemberInfo(string editorText, int offset, string fileName, bool collapseOverloads)
        {
            int methodStartPos;
            return GetMemberInfo(editorText, offset, fileName, collapseOverloads, out methodStartPos);
        }

        public static string[] GetMemberInfo(string editorText, int offset, string fileName, bool collapseOverloads, out int methodStartPos)
        {
            methodStartPos = offset;

            try
            {
                string nameToResolve = "";
                if (collapseOverloads) //simple resolving from the position
                {
                    nameToResolve = GetWordAt(editorText, offset);
                }
                else
                {
                    int pos = editorText.LastIndexOf('(', offset - 1);
                    if (pos == -1)
                        return new string[0];
                    else
                        offset = pos;

                    nameToResolve = GetWordAt(editorText, offset);
                }

                methodStartPos = offset;

                IEnumerable<ICompletionData> data = GetCompletionData(editorText, offset, fileName, true, false);

                string[] usedNamespaces = new CSharpParser().Parse(editorText, fileName)
                                                            .GetUsingNamepseces();

                var match = data.Where(x => x.DisplayText == nameToResolve).FirstOrDefault();//it will be either one or no records

                if (match != null)
                {
                    bool fullInfo = collapseOverloads;
                    //Note x.OverloadedData includes all instances of the same-name member. Thus '-1' should be applied
                    if (collapseOverloads)
                    {
                        string[] infoParts = match.GetDisplayInfo(fullInfo).HideKnownNamespaces(usedNamespaces).GetLines(2);
                        string desctription = infoParts.First();
                        string documentation = (infoParts.Length > 1 ? "\r\n" + infoParts[1] : "");

                        string info = desctription + (match.HasOverloads ? (" (+ " + (match.OverloadedData.Count() - 1) + " overload(s))") : "") + documentation;

                        if (!string.IsNullOrEmpty(info))
                            return new string[] { info };
                    }
                    else
                    {
                        if (match.HasOverloads)
                        {
                            return match.OverloadedData
                                       .Select(d => d as IEntityCompletionData)
                                       .Select(d => d.Entity.ToTooltip(fullInfo).HideKnownNamespaces(usedNamespaces))
                                       .Where(x => !string.IsNullOrEmpty(x))
                                       .ToArray();
                        }
                        else
                        {
                            string info = match.GetDisplayInfo(fullInfo).HideKnownNamespaces(usedNamespaces);
                            if (!string.IsNullOrEmpty(info))
                                return new string[] { info };
                        }
                    }
                }
            }
            catch
            {
                //the exception can happens even for the internal NRefactor-related reasons 
            }
            return new string[0];
        }

        public static string GetWordAt(string editorText, int offset)
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

        static public IEnumerable<string> FindReferences(string editorText, string pattern, ResolveResult target, string fileName)
        {
            var references = new List<string>();

            IDocument document = null;

            Type patternType = target.GetType();

            string targetReflectionName = target.ToString();
            if (target is MemberResolveResult) //the caret is on the member implementation
            {
                targetReflectionName = targetReflectionName.Replace("MemberResolveResult", "CSharpInvocationResolveResult");
                patternType = typeof(CSharpInvocationResolveResult);
            }

            //we are looking for the member invocation code.
            //For example: "[CSharpInvocationResolveResult [Method Test.Who():System.Void]]"
            foreach (Match m in Regex.Matches(editorText, pattern))
            {
                var match = ResolveFromPosition(editorText, m.Index, fileName);
                if (match != null && match.GetType() == patternType && match.ToString() == targetReflectionName)
                {
                    if (document == null)
                        document = new ReadOnlyDocument(editorText);

                    int position = m.Index;
                    var location = document.GetLocation(position);
                    var line = document.GetLineByOffset(position);
                    string lineText = editorText.GetTextOf(line);

                    Tuple<int, int> decoration = CSScriptHelper.GetDecorationInfo(editorText);

                    if (decoration.Item1 != -1) //the file content is no the actual one but an auto-generated (decorated) 
                    {
                        if (position > decoration.Item1)
                        {
                            position -= decoration.Item2;

                            string actualText = File.ReadAllText(fileName);
                            if (actualText != editorText)
                            {
                                document = new ReadOnlyDocument(actualText);
                                location = document.GetLocation(position);
                                line = document.GetLineByOffset(position);
                                lineText = actualText.GetTextOf(line);
                            }
                        }
                    }

                    references.Add(string.Format("{0}({1},{2}): {3}", fileName, location.Line, location.Column, lineText.Trim()));
                }
            }

            return references;
        }

        static public string[] FindReferences(string editorText, int offset, string fileName)
        {
            var retval = new List<string>();

            if (Project != null)
            {
                string pattern = GetWordAt(editorText, offset);
                var target = ResolveFromPosition(editorText, offset, fileName);

                if (target != null && !string.IsNullOrEmpty(pattern))
                {
                    var references = new List<string>();

                    foreach (string sourceFile in Project.Files.Select(x => x.FileName))
                    {
                        bool isEditedFile = (sourceFile == fileName);
                        string text = (isEditedFile ? editorText : File.ReadAllText(sourceFile));

                        retval.AddRange(FindReferences(text, pattern, target, sourceFile));
                    }
                }
            }
            return retval.ToArray();
        }

        static public ResolveResult ResolveFromPosition(string editorText, int offset, string fileName)
        {
            if (Project == null)
                return null;

            var location = new ReadOnlyDocument(editorText).GetLocation(offset);

            var syntaxTree = new CSharpParser().Parse(editorText, fileName);
            syntaxTree.Freeze();
            var unresolvedFile = syntaxTree.ToTypeSystem();

            Project = Project.AddOrUpdateFiles(unresolvedFile);

            var compilation = Project.CreateCompilation();

            return ResolveAtLocation.Resolve(compilation, unresolvedFile, syntaxTree, location);
        }

        static public DomRegion ResolveMember(string editorText, int offset, string fileName)
        {
            if (Project == null)
                return DomRegion.Empty;

            ResolveResult result = ResolveFromPosition(editorText, offset, fileName);

            DomRegion region = DomRegion.Empty;


            if (result is TypeResolveResult)
            {
                var type = (result as TypeResolveResult).Type as DefaultResolvedTypeDefinition;
                if (type != null)
                {
                    var asm = type.ParentAssembly;
                    if (asm.UnresolvedAssembly is CSharpProjectContent) //source code
                    {
                        //note NRefactory for unknown reason has body start point at the start of the definition but the declaration
                        return type.BodyRegion;
                    }
                    else if (asm.UnresolvedAssembly is IUnresolvedAssembly) //referenced assembly
                    {
                        FileLocation document = new Reflector().ReconstructToFile(asm, type);
                        return document.ToDomRegion();
                    }
                }
            }
            else if (result is MemberResolveResult)
            {
                var member = (result as MemberResolveResult).Member;

                var asm = member.ParentAssembly;
                if (asm.UnresolvedAssembly is CSharpProjectContent) //source code
                {
                    return member.BodyRegion;
                }
                else if (asm.UnresolvedAssembly is IUnresolvedAssembly) //referenced assembly
                {
                    FileLocation document = new Reflector().ReconstructToFile(asm, member.DeclaringType, member);
                    return document.ToDomRegion();
                }
            }
            else if (result is MethodGroupResolveResult)
            {
                var method = (result as MethodGroupResolveResult).Methods.FirstOrDefault();
                if (method != null)
                    return method.BodyRegion;
            }

            if (result != null)
            {
                string retval = result.ToString();
                if (result is TypeResolveResult)
                    retval = result.Type.ToString(); //[TypeResolveResult System.Console]
                else if (result is LocalResolveResult)
                    retval = (result as LocalResolveResult).Variable.ToString(); //[LocalResolveResult args:System.String[]]
                else if (result is CSharpInvocationResolveResult)
                    retval = (result as CSharpInvocationResolveResult).Member.ToString(); //[CSharpInvocationResolveResult [Method System.Console.WriteLine(value:System.String):System.Void]]
            }

            return DomRegion.Empty;
        }

        public static IEnumerable<ICompletionData> PrepareForDisplay(this IEnumerable<ICompletionData> data)
        {
            foreach (ICompletionData item in data)
            {
                if (!(item is IEntityCompletionData))
                    continue;

                IEntity entity = (item as IEntityCompletionData).Entity;
                switch (entity.EntityType)
                {
                    case EntityType.Constructor:
                    case EntityType.Destructor:
                    case EntityType.Method:
                        if (Config.Instance.UseMethodBrackets)
                            item.CompletionText += "(";
                        break;

                    case EntityType.Indexer:
                        if (Config.Instance.UseMethodBrackets)
                            item.CompletionText += "[";
                        break;

                    case EntityType.Event:
                        break;

                    case EntityType.Field:
                        break;

                    case EntityType.Accessor:
                        break;

                    case EntityType.None:
                        break;

                    case EntityType.Operator:
                        break;

                    case EntityType.Property:
                        break;

                    case EntityType.TypeDefinition:
                        break;

                    default:
                        break;
                }
            }

            return data.OrderBy(item => item.DisplayText);
        }
    }
}