using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common = Intellisense.Common;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.Documentation;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using UltraSharp.Cecil;

namespace CSScriptIntellisense
{
    public class MonoCompletionEngine : Common.IEngine
    {
        public string Language { get; set; } = "C#";

        IProjectContent Project;

        static internal Lazy<IList<IUnresolvedAssembly>> builtInLibs = new Lazy<IList<IUnresolvedAssembly>>(
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

        public void SetOption(string name, object value)
        {
        }

        public void Preload()
        {
        }

        internal static XmlDocumentationProvider GetXmlDocumentation(string dllPath)
        {
            if (string.IsNullOrEmpty(dllPath))
                return null;

            if (Path.GetDirectoryName(dllPath).EndsWith("CSScriptNpp", StringComparison.OrdinalIgnoreCase))
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

        public void ResetProject(Tuple<string, string>[] sourceFiles = null, params string[] assemblies)
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

        IEnumerable<Common.ICompletionData> GetCSharpCompletionData(ReadOnlyDocument doc, string editorText, int offset, string fileName, bool isControlSpace = true) // not the best way to put in the whole string every time
        {
            if (editorText[offset] != '.') //we may be at the partially complete word
                for (int i = offset - 1; i >= 0; i--)
                    if (SimpleCodeCompletion.Delimiters.Contains(editorText[i]))
                    {
                        offset = i + 1;
                        break;
                    }

            //test for C# completion
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

            return data.PrepareForDisplay().ToCommon();
        }

        public Common.CodeMapItem[] GetMapOf(string code, bool decorated, string codeFile)
        {
            return Reflector.GetMapOfImpl(code, decorated);
        }

        public IEnumerable<Common.ICompletionData> GetCompletionData(string editorText, int offset, string fileName, bool isControlSpace = true) // not the best way to put in the whole string every time
        {
            try
            {
                if (Project == null || string.IsNullOrEmpty(editorText))
                    return new Common.ICompletionData[0];

                var doc = new ReadOnlyDocument(editorText);

                if (editorText.Length <= offset)
                    offset = editorText.Length - 1;

                return GetCSharpCompletionData(doc, editorText, offset, fileName, isControlSpace);
            }
            catch
            {
                return new Common.ICompletionData[0]; //the exception can happens even for the internal NRefactor-related reasons
            }
        }

        public IEnumerable<Common.TypeInfo> GetPossibleNamespaces(string editorText, string nameToResolve, string fileName) // not the best way to put in the whole string every time
        {
            try
            {
                if (Project == null && string.IsNullOrEmpty(nameToResolve))
                    return new Common.TypeInfo[0];

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
                                           .Select(x => new Common.TypeInfo { Namespace = x.Namespace, FullName = x.FullName });

                var asmNamespaces = Project.AssemblyReferences
                                           .SelectMany(x => ((DefaultUnresolvedAssembly) x).TopLevelTypeDefinitions)
                                           .Union(Project.AssemblyReferences
                                                         .SelectMany(x => ((DefaultUnresolvedAssembly) x).TopLevelTypeDefinitions)
                                                         .SelectMany(x => x.NestedTypes))
                                           .Where(x => x.Name == nameToResolve)
                                           .Distinct()
                                           .Select(x => new Common.TypeInfo { Namespace = x.Namespace, FullName = x.FullName });

                return srcNamespaces.Union(asmNamespaces)
                                    .OrderBy(x => x.Namespace)
                                    .DistinctBy(x => x.FullName)
                                    .ToArray();
            }
            catch
            {
                return new Common.TypeInfo[0]; //the exception can happens even for the internal NRefactor-related reasons
            }
        }

        public string[] FindReferences(string editorText, int offset, string fileName)
        {
            var retval = new List<string>();

            if (Project != null)
            {
                string pattern = SimpleCodeCompletion.GetWordAt(editorText, offset);
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

        public Common.DomRegion ResolveTypeByName(string typeName, string typeMemberName)
        {
            if (Project == null)
                return Common.DomRegion.Empty;

            var code = $"class dummy11111 {{ System.Type t = typeof({typeName}); }}";
            var location = new ReadOnlyDocument(code).GetLocation(code.Length-6);

            var syntaxTree = new CSharpParser().Parse(code, "dummy11111.cs");
            syntaxTree.Freeze();
            var unresolvedFile = syntaxTree.ToTypeSystem();

            Project = Project.AddOrUpdateFiles(unresolvedFile);

            var compilation = Project.CreateCompilation();

            var result = ResolveAtLocation.Resolve(compilation, unresolvedFile, syntaxTree, location);

            var type = (result as TypeResolveResult).Type as DefaultResolvedTypeDefinition;
            if (type != null)
            {
                var asm = type.ParentAssembly;
                if (asm.UnresolvedAssembly is IUnresolvedAssembly) //referenced assembly
                {
                    FileLocation document = new Reflector().ReconstructToFile(asm, type, memberName: typeMemberName);
                    return document.ToDomRegion().ToCommon();
                }
            }

            return Common.DomRegion.Empty;
        }


        ResolveResult ResolveFromPosition(string editorText, int offset, string fileName)
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

        public IEnumerable<string> FindReferences(string editorText, string pattern, ResolveResult target, string fileName)
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

        public string[] GetMemberInfo(string editorText, int offset, string fileName, bool collapseOverloads, out int methodStartPos)
        {
            methodStartPos = offset;

            bool isInstantiation = false;

            try
            {
                string nameToResolve = "";
                if (collapseOverloads) //simple resolving from the position
                {
                    isInstantiation = SimpleCodeCompletion.GetPrevWordAt(editorText, offset) == "new";
                    nameToResolve = SimpleCodeCompletion.GetWordAt(editorText, offset);
                }
                else
                {
                    int pos = editorText.LastIndexOf('(', offset - 1);
                    if (pos == -1)
                        return new string[0];
                    else
                        offset = pos;

                    nameToResolve = SimpleCodeCompletion.GetWordAt(editorText, offset);
                }

                methodStartPos = offset;

                IEnumerable<ICompletionData> data = GetCompletionData(editorText, offset, fileName, true).ToNRef();

                string[] usedNamespaces = new CSharpParser().Parse(editorText, fileName)
                                                            .GetUsingNamepseces();

                var match = data.Where(x => x.matchesToken(nameToResolve)).FirstOrDefault();//it will be either one or no records

                if (match != null)
                {
                    bool fullInfo = !collapseOverloads;
                    //Note x.OverloadedData includes all instances of the same-name member. Thus '-1' should be applied
                    if (collapseOverloads)
                    {
                        string[] infoParts = match.GetDisplayInfo(fullInfo, isInstantiation).HideKnownNamespaces(usedNamespaces).GetLines(2);
                        string desctription = infoParts.First();
                        string documentation = (infoParts.Length > 1 ? "\r\n" + infoParts[1] : "");

                        string info = desctription;

                        int overloadsCount = 0;

                        if (match.HasOverloads)
                            overloadsCount = match.OverloadedData.Count() - 1;

                        if (match is TypeCompletionData)
                        {
                            if (isInstantiation)
                                info += "()";
                            else
                                overloadsCount = 0;
                        }

                        info += (overloadsCount > 0 ? (" (+ " + overloadsCount + " overload(s))") : "") + documentation;

                        if (!string.IsNullOrEmpty(info))
                            return new string[] { info };
                    }
                    else
                    {
                        if (match.HasOverloads)
                        {
                            return match.OverloadedData
                                        .Where(d => d is IEntityCompletionData)
                                        .Cast<IEntityCompletionData>()
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

        public Common.DomRegion ResolveCSharpMember(string editorText, int offset, string fileName)
        {
            if (Project == null)
                return Common.DomRegion.Empty;

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
                        return type.BodyRegion.ToCommon();
                    }
                    else if (asm.UnresolvedAssembly is IUnresolvedAssembly) //referenced assembly
                    {
                        FileLocation document = new Reflector().ReconstructToFile(asm, type);
                        return document.ToDomRegion().ToCommon();
                    }
                }
            }
            else if (result is InvocationResolveResult)
            {
                var member = (result as InvocationResolveResult).Member;

                var asm = member.ParentAssembly;
                if (asm.UnresolvedAssembly is CSharpProjectContent) //source code
                {
                    //constructors are not always resolved (e.g. implicit default constructor in absence of other constructors)
                    //thus point to the definition instead

                    var method = member as DefaultResolvedMethod;
                    var type = result.Type as DefaultResolvedTypeDefinition;

                    if (member.BodyRegion.FileName == null && type != null && method != null && method.IsConstructor)
                        return type.BodyRegion.ToCommon();
                    else
                        return member.BodyRegion.ToCommon();
                }
                else if (asm.UnresolvedAssembly is IUnresolvedAssembly) //referenced assembly
                {
                    FileLocation document = new Reflector().ReconstructToFile(asm, member.DeclaringType, member);
                    return document.ToDomRegion().ToCommon();
                }
            }
            else if (result is MemberResolveResult)
            {
                var member = (result as MemberResolveResult).Member;

                var asm = member.ParentAssembly;
                if (asm.UnresolvedAssembly is CSharpProjectContent) //source code
                {
                    return member.BodyRegion.ToCommon();
                }
                else if (asm.UnresolvedAssembly is IUnresolvedAssembly) //referenced assembly
                {
                    FileLocation document = new Reflector().ReconstructToFile(asm, member.DeclaringType, member);
                    return document.ToDomRegion().ToCommon();
                }
            }
            else if (result is MethodGroupResolveResult)
            {
                var method = (result as MethodGroupResolveResult).Methods.FirstOrDefault();
                if (method != null)
                    return method.BodyRegion.ToCommon();
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

            return Common.DomRegion.Empty;
        }

        //internal class DomRegionProxy : Common.DomRegion
        //{
        //    public ICSharpCode.NRefactory.TypeSystem.DomRegion obj;
        //    public DomRegionProxy(ICSharpCode.NRefactory.Completion.DomRegion obj)
        //    {
        //        this.obj = obj;
        //    }

        //    public new string DisplayText { get { return obj.DisplayText; } set { obj.DisplayText = value; } }
        //    public new string Icon { get { return obj.Icon; } set { obj.Icon = value; } }
        //}

        internal class CompletionCategoryProxy : Common.CompletionCategory
        {
            public ICSharpCode.NRefactory.Completion.CompletionCategory obj;
            public CompletionCategoryProxy(ICSharpCode.NRefactory.Completion.CompletionCategory obj)
            {
                this.obj = obj;
            }

            public new string DisplayText { get { return obj.DisplayText; } set { obj.DisplayText = value; } }
            public new string Icon { get { return obj.Icon; } set { obj.Icon = value; } }
        }

        internal class CompletionDataProxy : Intellisense.Common.ICompletionData
        {
            public ICSharpCode.NRefactory.Completion.ICompletionData obj;
            public CompletionDataProxy(ICSharpCode.NRefactory.Completion.ICompletionData obj)
            {
                this.obj = obj;
            }

            public Common.CompletionCategory CompletionCategory { get { return new CompletionCategoryProxy(obj.CompletionCategory); } set { throw new NotImplementedException(); } }

            public string CompletionText { get { return obj.CompletionText; } set { obj.CompletionText = value; } }

            public string Description { get { return obj.Description; } set { obj.Description = value; } }

            public Common.DisplayFlags DisplayFlags { get { return (Common.DisplayFlags) obj.DisplayFlags; } set { obj.DisplayFlags = (DisplayFlags) value; } }

            public string DisplayText { get { return obj.DisplayText; } set { obj.DisplayText = value; } }

            public string OperationContext { get; set; }

            public object Tag { get; set; }

            public Common.CompletionType CompletionType
            {
                get
                {

                    var itemType = EntityType.None;
                    var declarationType = DeclarationType.None;
                    bool isExtensionMethod = false;

                    if (obj is IEntityCompletionData)
                    {
                        itemType = (obj as IEntityCompletionData).Entity.EntityType;
                    }

                    if (obj is CompletionData)
                    {
                        var data = (obj as CompletionData);
                        declarationType = data.DeclarationType;
                        isExtensionMethod = data.IsExtensionMethod;
                    }

                    switch (itemType)
                    {
                        case EntityType.Constructor:
                        case EntityType.Destructor:
                            return Common.CompletionType.constructor;

                        case EntityType.Method:
                            {
                                if (isExtensionMethod)
                                    return Common.CompletionType.extension_method;
                                else
                                    return Common.CompletionType.method;
                            }
                        case EntityType.Event:
                            return Common.CompletionType._event;

                        case EntityType.Field:
                            return Common.CompletionType.field;

                        case EntityType.Property:
                            return Common.CompletionType.property;

                        default:
                            break;
                    }

                    switch (declarationType)
                    {
                        case DeclarationType.None:
                            break;

                        case DeclarationType.Namespace:
                            return Common.CompletionType._namespace;

                        case DeclarationType.Type:
                            return Common.CompletionType.constructor;

                        case DeclarationType.Variable:
                        case DeclarationType.Parameter:
                            return Common.CompletionType.field;

                        case DeclarationType.Event:
                            return Common.CompletionType._event;

                        case DeclarationType.Unresolved:
                            return Common.CompletionType.unresolved;

                        default:
                            break;
                    }

                    return Common.CompletionType.none;
                }

                set { }
            }

            public bool HasOverloads { get { return obj.HasOverloads; } }

            public IEnumerable<Common.ICompletionData> OverloadedData
            {
                get { return obj.OverloadedData.Select(x => new CompletionDataProxy(x)); }
            }

            public IEnumerable<string> InvokeParameters
            {
                get { return null; }
            }

            public bool InvokeParametersSet { get; set; }
            public string InvokeReturn { get; set; }

            public void AddOverload(Common.ICompletionData data)
            {
                throw new NotImplementedException();
            }
        }

    }

    static class MonoCompletionExtensions
    {
        public static Common.DomRegion ToCommon(this DomRegion region)
        {
            return new Common.DomRegion
            {
                BeginColumn = region.BeginColumn,
                BeginLine = region.BeginLine,
                EndLine = region.EndLine,
                FileName = region.FileName,
                IsEmpty = region == DomRegion.Empty
            };
        }

        public static IEnumerable<Intellisense.Common.ICompletionData> ToCommon(this IEnumerable<ICSharpCode.NRefactory.Completion.ICompletionData> collection)
        {
            return collection.Select(x => (Intellisense.Common.ICompletionData) new MonoCompletionEngine.CompletionDataProxy(x));
        }

        public static IEnumerable<ICSharpCode.NRefactory.Completion.ICompletionData> ToNRef(this IEnumerable<Common.ICompletionData> collection)
        {
            return collection.Select(x => (x as MonoCompletionEngine.CompletionDataProxy).obj);
        }

        internal static IEnumerable<ICompletionData> PrepareForDisplay(this IEnumerable<ICompletionData> data)
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
                        //if (Config.Instance.UseMethodBrackets)
                        //    item.CompletionText += "(";
                        break;

                    case EntityType.Indexer:
                        //if (Config.Instance.UseMethodBrackets)
                        //    item.CompletionText += "[";
                        break;

                    default:
                        break;
                }
            }

            return data.OrderBy(item => item.DisplayText);
        }
    }
}