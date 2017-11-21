using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Intellisense.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;
using System.Windows.Forms;
using VB = Microsoft.CodeAnalysis.VisualBasic;
using System.Diagnostics;

namespace RoslynIntellisense
{
    internal class AppDomainHelper
    {
        static bool initialized;
        static string probingDir;

        static public void Init()
        {
            if (!initialized)
            {
                //Debug.Assert(false);
                initialized = true;

                probingDir = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(probingDir))
                    probingDir = Assembly.GetCallingAssembly().Location;
                if (string.IsNullOrEmpty(probingDir))
                    probingDir = Assembly.GetEntryAssembly().Location;
                probingDir = probingDir.GetDirName();

                if (!File.Exists(probingDir.PathJoin("Microsoft.CodeAnalysis.dll")))
                {
                    probingDir = probingDir.PathJoin("Roslyn");
                }

                // if (!File.Exists(probingDir.PathJoin("Microsoft.CodeAnalysis.dll")))
                //     Debug.Assert(false);

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var file = probingDir.PathJoin(args.Name.Split(',').First() + ".dll");
            if (File.Exists(file))
                return Assembly.LoadFrom(file);
            return null;
        }
    }

    public static class Autocompleter
    {
        static Autocompleter()
        {
            AppDomainHelper.Init();
        }

        static internal Lazy<MetadataReference[]> builtInLibs = new Lazy<MetadataReference[]>(
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

             return assemblies.Select(a =>
                {
                    return MetadataReference.CreateFromFile(a.Location, documentation: NppDocumentationProvider.NewFor(a.Location));
                }).ToArray();
         });

        public static Document WithWorkspace(string code, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            var workspace = new AdhocWorkspace();
            return InitWorkspace(workspace, code, null, references, includes);
        }

        public static Document InitWorkspace(AdhocWorkspace workspace, string code, string file = null, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            string projName = "NewProject";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            string docName = file ?? "current_code.cs";

            var refs = new List<MetadataReference>(builtInLibs.Value);

            if (references != null)
                refs.AddRange(references.Select(a => MetadataReference.CreateFromFile(a, documentation: NppDocumentationProvider.NewFor(a))));

            var lng = Autocompleter.Language == "C#" ? LanguageNames.CSharp : LanguageNames.VisualBasic;
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, lng, metadataReferences: refs);

            var newProject = workspace.AddProject(projectInfo);
            var newDocument = workspace.AddDocument(newProject.Id, docName, SourceText.From(code));

            if (includes != null)
                foreach (var item in includes)
                    workspace.AddDocument(newProject.Id, item.Item2, SourceText.From(item.Item1));

            var proj = workspace.CurrentSolution.Projects.Single();

            //EXTREMELY IMPORTANT: "return newDocument;" will not return the correct instance of the document but lookup will
            return workspace.CurrentSolution.Projects.Single().Documents.Single(x => x.Name == docName);
        }

        //mscorlib, systemCore
        public static string[] defaultRefs = new string[]
        {
                    typeof(object).Assembly.Location,                    // mscorlib.dll
                    typeof(Uri).Assembly.Location,                       // System.dll
                    typeof(Form).Assembly.Location,                      // System.Windows.Forms.dll
                    typeof(System.Linq.Enumerable).Assembly.Location,    // System.Core.dll
                    typeof(System.Xml.XmlDocument).Assembly.Location,    // System.Xml.dll
                    typeof(System.Drawing.Bitmap).Assembly.Location      // System.Drawing.dll
        };

        public static char[] Delimiters = "\\\t\n\r .,:;'\"=[]{}()+-/!?@$%^&*«»><#|~`".ToCharArray();

        static void GetWordFromCaret(string code, int position, out int logicalPosition, out string partialWord, out string opContext)
        {
            //check if it is variable assignment or 'edd even' declaration
            if (position > 5) //you need at least a few chars to declare the event handler adding: a.b+=
            {
                int start = Math.Max(0, position - 300);
                string leftSide = code.Substring(start, position - start); //max 300 chars from left
                string leftSideText = leftSide.TrimEnd();

                if (leftSideText.EndsWith("-=")) //it is 'remove event' declaration: this.Load -= |
                {
                    opContext = "-=";
                    //not supported
                }
                else if (leftSideText.EndsWith("=")) //it is 'add event' declaration: this.Load += |
                {
                    int pos = leftSide.LastIndexOf('='); //this.Load |+=
                    pos--;

                    if (leftSideText.EndsWith("+="))
                    {
                        opContext = "+=";
                        pos--;
                    }
                    else
                    {
                        opContext = "=";
                    }

                    bool started = false;
                    for (; pos >= 0; pos--) //this.|Load +=
                    {
                        if (!started)
                        {
                            if (!char.IsWhiteSpace(leftSide[pos]))
                                started = true;
                        }
                        else if (Delimiters.Contains(leftSide[pos]))
                            break;
                    }
                    var startOfName = pos + 1;

                    partialWord = leftSide.Substring(startOfName).Split(Delimiters).FirstOrDefault();
                    logicalPosition = startOfName + partialWord.Length;
                    return;
                }
            }
            int wordStart = code.GetWordStartOf(position);
            opContext = null;
            partialWord = code.Substring(wordStart, position - wordStart);
            logicalPosition = position;
        }

        public static string[] FindReferencess(string code, int position, string file, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            //SymbolFinder.FindReferencesAsync
            try
            {
                var workspace = new AdhocWorkspace();
                var doc = InitWorkspace(workspace, code, file, AgregateRefs(references), includes);

                ISymbol symbol = SymbolFinder.FindSymbolAtPositionAsync(doc, position).Result;

                if (symbol == null)
                    return new string[0];

                var refsGroups = SymbolFinder.FindReferencesAsync(symbol, doc.Project.Solution).Result;

                //good ref doc https://joshvarty.wordpress.com/roslynfaq/csharp/
                var locations = refsGroups.FirstOrDefault()
                                          .Locations
                                          .Select(x => x.Location)
                                          .OrderBy(x => x.SourceSpan.Start)
                                          .Select(x =>
                                          {
                                              var start = x.GetLineSpan().StartLinePosition;

                                              var line = start.Line + 1;
                                              var column = start.Character + 1;
                                              var filePath = x.SourceTree.FilePath;

                                              var fileContent = x.SourceTree.GetText().ToString();
                                              int pos = fileContent.IndexOf("///CS-Script auto-class generation");
                                              if (pos != -1)
                                              {
                                                  if (pos < x.SourceSpan.Start)
                                                      line--;
                                              }

                                              var hint = fileContent.Substring(x.SourceSpan.Start).Split('\n').First().Trim();

                                              return $"{filePath}({line},{column}): {hint}";
                                          }).ToArray();

                return locations;
            }
            catch { } //failed, no need to report, as auto-completion is expected to fail sometimes
            return new string[0];
        }

        public static DomRegion ResolveType(string typeName, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            var code = $"class Test {{  void Init() {{ var t = typeof({typeName}|); }} }}";
            int position = code.IndexOf("|") - 1;
            code = code.Replace("|", "");

            return ResolveSymbol(code, position, "dummy.cs", references, includes);
        }

        public static DomRegion ResolveSymbol(string code, int position, string file, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            try
            {
                var workspace = new AdhocWorkspace();
                var doc = InitWorkspace(workspace, code, file, AgregateRefs(references), includes);

                ISymbol symbol = SymbolFinder.FindSymbolAtPositionAsync(doc, position).Result;

                if (symbol == null)
                    return DomRegion.Empty;

                var location = symbol.Locations.First();

                if (location.IsInSource)
                {
                    if (file.EndsWith(".r.cs")) //reflected file
                    {
                        var text = code.GetLineAt(location.SourceSpan.Start);

                        bool isPlaceholder = text.Contains("{ /*hidden*/ }");
                        if (isPlaceholder)
                            return ResolveType(symbol.GetFullName(), references, includes);
                    }

                    var linePosition = location.GetLineSpan().StartLinePosition;

                    //DomRegion is 1-based Editor friendly struct
                    return new DomRegion
                    {
                        FileName = location.SourceTree.FilePath,
                        BeginLine = linePosition.Line + 1,
                        EndLine = linePosition.Line + 1,
                        BeginColumn = linePosition.Character + 1,
                    };
                }
                else if (location.IsInMetadata)
                {
                    var result = new DomRegion();

                    // Will not work if the asm name (Identity.Name) is not the same as the file name.
                    // But unfortunately location.MetadataModule doesn't preserve the asm file name
                    var asmName = symbol.ContainingAssembly.Identity.Name;
                    var asmFile = doc.Project.MetadataReferences
                                             .Select(x => x.Display)
                                             .Where(x => string.Compare(Path.GetFileNameWithoutExtension(x), asmName, true) == 0)
                                             .FirstOrDefault();

                    string asmHeader = $"#region Assembly {symbol.ContainingAssembly.Identity}" + Environment.NewLine +
                                       $"// {asmFile}" + Environment.NewLine +
                                        "#endregion" + Environment.NewLine + Environment.NewLine;

                    int pos;
                    string reconstructed = symbol.Reconstruct(out pos, header: asmHeader);

                    result.FileName = SaveReflectedCode(symbol.GetRootType().GetFullName(), reconstructed);
                    result.BeginLine =
                    result.EndLine = reconstructed.LineNumberOf(pos);

                    return result;
                }
            }
            catch { } //failed, no need to report, as auto-completion is expected to fail sometimes
            return DomRegion.Empty;
        }

        static public string OutputDir = Path.Combine(Path.GetTempPath(), "Roslyn.Intellisense\\ReflctedTypes");
        static public string Language = "C#";

        static string SaveReflectedCode(string typeName, string code)
        {
            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);

            string file = Path.Combine(OutputDir, typeName.NormalizeAsPath());

            file += "." + code.GetHashCode() + ".r.cs"; //simple caching mechanism for avoiding overrating the file

            if (!File.Exists(file))
                File.WriteAllText(file, code);

            Directory.GetFiles(OutputDir, typeName.NormalizeAsPath() + ".*")
                     .Where(x => x != file).ToList()
                     .ForEach(x =>
                     {
                         try { File.Delete(x); } //delete old
                         catch { }
                     });

            return file;
        }

        static string[] AgregateRefs(string[] references)
        {
            var asms = defaultRefs;

            if (references != null)
                asms = asms.Concat(references.Where(File.Exists)).ToArray();
            return asms;
        }

        public static IEnumerable<string> GetMemberInfo(string code, int position, out int methodStartPos, string[] references = null, IEnumerable<Tuple<string, string>> includes = null, bool includeOverloads = false)
        {
            // Debug.Assert(false);

            int actualPosition = position;

            if (includeOverloads)  //Resolving method tooltips
            {
                int pos = code.LastIndexOf('(', position - 1);
                if (pos != -1)
                    actualPosition = pos;
            }

            methodStartPos = actualPosition;

            try
            {
                var result = new List<string>();

                var workspace = new AdhocWorkspace();

                var doc = InitWorkspace(workspace, code, null, AgregateRefs(references), includes);

                var symbol = SymbolFinder.FindSymbolAtPositionAsync(doc, actualPosition).Result;

                if (symbol != null)
                {
                    result.Add(symbol.ToTooltip(!includeOverloads));

                    if (includeOverloads)
                    {
                        result.AddRange(symbol.GetOverloads().Select(s => s.ToTooltip()));
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            // catch { } //failed, no need to report, as auto-completion is expected to fail sometimes
            return new string[0];
        }

        static IEnumerable<ISymbol> GetOverloads(this ISymbol symbol)
        {
            return symbol.ContainingType.GetMembers(symbol.Name).Where(x => x != symbol);
        }

        //position is zero-based
        //public async static Task<IEnumerable<ICompletionData>> GetAutocompletionFor(string code, int position, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        public static IEnumerable<ICompletionData> GetAutocompletionFor(string code, int position, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            //Debug.Assert(false);
            string opContext;

            string partialWord;
            int logicalPosition;

            GetWordFromCaret(code, position, out logicalPosition, out partialWord, out opContext);

            var completions = Resolve(code, logicalPosition, references, includes);

            try
            {
                var result = completions.Where(s => s.DisplayText.HasText() && s.DisplayText.CanComplete(partialWord))
                                        .OrderByDescending(c => c.DisplayText.StartsWith(partialWord))
                                        .ThenByDescending(c => c.DisplayText.StartsWith(partialWord, StringComparison.OrdinalIgnoreCase))
                                        .ThenByDescending(c => c.DisplayText.IsCamelCaseMatch(partialWord))
                                        .ThenByDescending(c => c.DisplayText.IsSubsequenceMatch(partialWord))
                                        .ThenBy(c => c.DisplayText)
                                        .ToList();

                if (opContext != null) //only if a single assignment/add is identified
                {
                    if (opContext == "+=")
                    {
                        result = result.Where(s => s.DisplayText == partialWord)
                                       .ToList();

                        if (result.Count() == 1)
                        {
                            var data = result.First();

                            data.OperationContext = opContext;
                            if (data.CompletionType == CompletionType._event)
                                ProcessEventCompletion(code, position, data, result);
                            else
                                result.Clear(); //only supporting auto completion on += for events
                        }
                    }
                    else if (opContext == "=")
                    {
                        result = result.Where(s => s.DisplayText == partialWord)
                                       .ToList();

                        if (result.Count() == 1)
                        {
                            var data = result.First();

                            data.OperationContext = opContext;
                            if (data.CompletionType == CompletionType.field || data.CompletionType == CompletionType.property)
                            {
                                if (!ProcessAsignmentCompletion(data, code))
                                    result.Clear();
                            }
                            else
                                result.Clear(); //only supporting auto completion on += for events
                        }
                    }
                };

                return result;
            }
            catch { } //failed, no need to report, as auto-completion is expected to fail sometimes
            return new ICompletionData[0];
        }

        static void ProcessEventCompletion(string code, int position, ICompletionData data, List<ICompletionData> result)
        {
            try
            {
                var lineStart = code.Substring(0, position).LastIndexOf('\n');
                if (lineStart == -1)
                    lineStart = 0;

                var leftPart = code.Substring(lineStart, position - lineStart).TrimEnd(); //line left part
                string indent = new string(' ', leftPart.Length);

                string eventName = data.DisplayText;

                // lambda event handler
                data.DisplayText = "On" + eventName + " - lambda";
                string handlerArgs = string.Join(", ", data.InvokeParameters.Select(p => p.Split(' ').LastOrDefault()).ToArray());

                var sb = new StringBuilder()
                        .AppendLine("(" + handlerArgs + ")=>")
                        .AppendLine(indent + "{")
                        .AppendLine(indent + "   $|$")
                        .AppendLine(indent + "};");

                data.CompletionText = sb.ToString();

                var root = CSharpSyntaxTree.ParseText(code)
                                           .GetRoot();

                string[] namespaces = root.GetUsingNamespace(code);

                // delegate event handler
                handlerArgs = string.Join(", ", data.InvokeParameters.Select(x => x.ShrinkNamespaces(namespaces)).ToArray());

                sb.Clear()
                  .AppendLine("delegate(" + handlerArgs + ")")
                  .AppendLine(indent + "{")
                  .AppendLine(indent + "   $|$")
                  .AppendLine(indent + "};");

                //add delegate version of the same event handler
                var delegateCompletion = new EntityCompletionData().CopyPropertiesFrom(data);
                delegateCompletion.CompletionText = sb.ToString();
                delegateCompletion.DisplayText = "On" + eventName + " - delegate";

                result.Add(delegateCompletion);

                // method event handler
                string handlerName = "On" + eventName;

                var nodes = root.DescendantNodes();

                var methodAtCursor = nodes.Where(x => x.Kind() == SyntaxKind.MethodDeclaration &&
                                                      x.FullSpan.End >= position)
                                          .OfType<MethodDeclarationSyntax>()
                                          .Select(x => new
                                          {
                                              End = x.FullSpan.End,
                                              Distance = x.FullSpan.End - position,
                                              Data = x,
                                          })
                                          .OrderBy(x => x.Distance)
                                          .FirstOrDefault();

                if (methodAtCursor != null)
                {
                    indent = new string(' ', methodAtCursor.Data.Span.Start - methodAtCursor.Data.FullSpan.Start);

                    var similarMethods = nodes.OfType<MethodDeclarationSyntax>()
                                              .Where(x => x.Parent == methodAtCursor.Data.Parent &&
                                                          x.Identifier.Text.StartsWith(handlerName));
                    if (similarMethods.Any())
                        handlerName = handlerName + (similarMethods.Count() + 1);

                    var modifier = methodAtCursor.Data
                                                 .Modifiers
                                                 .Where(x => x.Text == "static")
                                                 .Select(x => "static ")
                                                 .FirstOrDefault();

                    //add delegate version of the same event handler
                    var methodCompletion = new EntityCompletionData().CopyPropertiesFrom(data);
                    methodCompletion.CompletionText = handlerName + ";";
                    methodCompletion.DisplayText = "On" + eventName + " - method";

                    var returnType = delegateCompletion.InvokeReturn.ShrinkNamespaces(namespaces) + " ";

                    sb.Clear()
                      .AppendLine()
                      .AppendLine(indent + modifier + returnType + handlerName + "(" + handlerArgs + ")")
                      .AppendLine(indent + "{")
                      .AppendLine(indent + "    $|$")
                      .AppendLine(indent + "}");

                    methodCompletion.Tag = new Dictionary<string, object>()
                    {
                        { "insertionPos", methodAtCursor.End },
                        { "insertionContent", sb.ToString() },
                    };

                    result.Add(methodCompletion);
                }
            }
            catch { }
        }

        static bool ProcessAsignmentCompletion(ICompletionData data, string code)
        {
            var root = CSharpSyntaxTree.ParseText(code)
                                       .GetRoot();

            string[] namespaces = root.GetUsingNamespace(code);

            data.DisplayText += " - value";
            var rawData = data.To<EntityCompletionData>().RawData;

            ITypeSymbol type = null;
            if (rawData is IPropertySymbol)
                type = rawData.To<IPropertySymbol>().Type;
            else if (rawData is IFieldSymbol)
                type = rawData.To<IFieldSymbol>().Type;
            else
                return false;

            var typeName = type.ToDecoratedName().ShrinkNamespaces(namespaces);

            if (type.BaseType.ToDisplayString() == "System.Enum")
                data.CompletionText = typeName + ".";
            else if (type.IsReferenceType)
                data.CompletionText = "new " + typeName + "();";
            else
                data.CompletionText = typeName;
            return true;
        }

        public static IEnumerable<ICompletionData> Resolve(string code, int position, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            var completions = new List<ICompletionData>();

            var pos = position - 1;
            var workspace = new AdhocWorkspace();

            var document = InitWorkspace(workspace, code, null, AgregateRefs(references), includes);
            var model = document.GetSemanticModelAsync().Result;
            var symbols = Recommender.GetRecommendedSymbolsAtPositionAsync(model, position, workspace).Result.ToArray();

            var data = symbols.Select(s => s.ToCompletionData()).ToArray();

            foreach (var group in data.GroupBy(x => x.DisplayText))
            {
                var item = group.First();

                if (group.Count() > 1)
                    foreach (var overload in group)
                    {
                        item.AddOverload(overload);
                        //item.HasOverloads = true;
                    }

                completions.Add(item);
            }

            return completions;
        }

        static public void FindMissingUsingsCanonical()
        {
            var workspace = new AdhocWorkspace();
            var solutionInfo = SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create());
            var solution = workspace.AddSolution(solutionInfo);
            var project = workspace.AddProject("NewProj", "C#");

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            project = project.AddMetadataReference(mscorlib);
            workspace.TryApplyChanges(project.Solution);
            string text = @"class Test
            {
                void Foo()
                {
                    Console.Write();
                }
            }";

            var sourceText = SourceText.From(text);
            //Add document to project
            var doc = workspace.AddDocument(project.Id, "NewDoc", sourceText);
            var model = doc.GetSemanticModelAsync().Result;
            var unresolved = doc.GetSyntaxRootAsync().Result.DescendantNodes()
                                                     .OfType<IdentifierNameSyntax>()
                                                     .Where(x => model.GetSymbolInfo(x).Symbol == null)
                                                     .ToArray();
            foreach (var identifier in unresolved)
            {
                var candidateUsings = SymbolFinder.FindDeclarationsAsync(doc.Project, identifier.Identifier.ValueText, ignoreCase: false).Result;
            }
        }

        public static IEnumerable<Intellisense.Common.TypeInfo> GetNamespacesFor(string editorText, string nameToResolve, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            var suggestions = new List<Intellisense.Common.TypeInfo>();

            var workspace = new AdhocWorkspace();

            var doc = InitWorkspace(workspace, editorText, null, AgregateRefs(references), includes);

            IEnumerable<ISymbol> result = SymbolFinder.FindDeclarationsAsync(doc.Project, nameToResolve, ignoreCase: false).Result;
            foreach (ISymbol declaration in result)
            {
                if (declaration.Kind != SymbolKind.NamedType)
                    continue; //limit to

                var name = declaration.Name;
                var nmspace = declaration.GetNamespace();

                var info = new Intellisense.Common.TypeInfo { Namespace = nmspace, FullName = declaration.ToString() };
                suggestions.Add(info);
            }

            return suggestions;
        }

        public static CodeMapItem[] GetMapOf(string code, bool decorated)
        {
            if (Autocompleter.Language == "VB")
                return GetMapOfVB(code, decorated);
            else
                return GetMapOfCSharp(code, decorated);
        }

        static CodeMapItem[] GetMapOfCSharp(string code, bool decorated)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);

            var root = tree.GetRoot();

            var map = new List<CodeMapItem>();

            var nodes = root.DescendantNodes();

            var types = nodes.OfType<TypeDeclarationSyntax>()
                             .OrderBy(x => x.FullSpan.End)
                             .ToArray();

            foreach (EnumDeclarationSyntax type in nodes.OfType<EnumDeclarationSyntax>())
            {
                var parentType = type.Parent as BaseTypeDeclarationSyntax;

                map.Add(new CodeMapItem
                {
                    Line = type.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Column = type.GetLocation().GetLineSpan().StartLinePosition.Character,
                    DisplayName = type.Identifier.Text,
                    ParentDisplayName = parentType.GetFullName(),
                    MemberType = "Enum"
                });
            }

            foreach (TypeDeclarationSyntax type in types)
            {
                foreach (var member in type.ChildNodes().OfType<MemberDeclarationSyntax>())
                {
                    string parentKind = type.ToKindDisplayName();

                    if (member is MethodDeclarationSyntax)
                    {
                        var method = (member as MethodDeclarationSyntax);
                        map.Add(new CodeMapItem
                        {
                            Line = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                            Column = method.GetLocation().GetLineSpan().StartLinePosition.Character,
                            //DisplayName = method.Identifier.Text + method.ParameterList, //nicely prints all params with their types and names
                            DisplayName = method.Identifier.Text + "(" + new string(',', Math.Max(method.ParameterList.Parameters.Count - 1, 0)) + ")",
                            ParentDisplayName = type.GetFullName(),
                            ParentDisplayType = parentKind,
                            MemberType = "Method"
                        });
                    }
                    else if (member is PropertyDeclarationSyntax)
                    {
                        var prop = (member as PropertyDeclarationSyntax);
                        map.Add(new CodeMapItem
                        {
                            Line = prop.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                            Column = prop.GetLocation().GetLineSpan().StartLinePosition.Character,
                            DisplayName = prop.Identifier.ValueText,
                            ParentDisplayName = type.GetFullName(),
                            ParentDisplayType = parentKind,
                            MemberType = "Property"
                        });
                    }
                    else if (member is FieldDeclarationSyntax)
                    {
                        var field = (member as FieldDeclarationSyntax);
                        map.Add(new CodeMapItem
                        {
                            Line = field.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                            Column = field.GetLocation().GetLineSpan().StartLinePosition.Character,
                            DisplayName = field.Declaration.Variables.First().Identifier.Text,
                            ParentDisplayName = type.GetFullName(),
                            ParentDisplayType = parentKind,
                            MemberType = "Field"
                        });
                    }
                }
            }

            if (decorated && map.Any())
            {
                //string rootClassName = map.First().ParentDisplayName;
                string rootClassName = "ScriptClass";
                //foreach (var item in map.Skip(1))
                foreach (var item in map)
                {
                    if (item.ParentDisplayName == rootClassName)
                    {
                        item.ParentDisplayName = "<Global>";
                        item.ParentDisplayType = "";
                    }
                    else if (item.ParentDisplayName.StartsWith(rootClassName + "."))
                        item.ParentDisplayName = item.ParentDisplayName.Substring(rootClassName.Length + 1);
                }

                var customMain = map.FirstOrDefault(x => x.ParentDisplayName == "<Global>" && x.DisplayName == "main()");
                var canonicalMain = map.FirstOrDefault(x => x.ParentDisplayName == "<Global>" && x.DisplayName == "Main()");
                if (customMain != null && canonicalMain != null)
                    map = map.Where(x => x != canonicalMain).ToList();
            }

            return map.ToArray();
        }

        static CodeMapItem[] GetMapOfVB(string code, bool decorated)
        {
            throw new SyntaxErrorParsingException("VB syntax is not supported for Code Map.");

            // if enabled and fixed still need to be isolated in the separate class as otherwise it triggers probing VB Roslyn asms, which are not present on Linux (e.g. Omnisharp)

            // SyntaxTree tree = VB.VisualBasicSyntaxTree.ParseText(code);

            // var root = tree.GetRoot();

            // var map = new List<CodeMapItem>();

            // var nodes = root.DescendantNodes();

            // var types = nodes.OfType<VB.Syntax.TypeBlockSyntax>()
            //                  .OrderBy(x => x.FullSpan.End)
            //                  .ToArray();

            // //not sure what is the VB equivalent
            // //foreach (VB.Syntax.EnumBlockSyntax type in nodes.OfType<VB.Syntax.EnumBlockSyntax>())
            // //{
            // //    var parentType = type.Parent as VB.Syntax.DeclarationStatementSyntax;

            // //    map.Add(new CodeMapItem
            // //    {
            // //        Line = type.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            // //        Column = type.GetLocation().GetLineSpan().StartLinePosition.Character,
            // //        DisplayName = type.Identifier.Text,
            // //        ParentDisplayName = parentType.GetFullName(),
            // //        MemberType = "Enum"
            // //    });
            // //}

            // foreach (VB.Syntax.TypeBlockSyntax type in types)
            // {
            //     foreach (var member in type.ChildNodes().OfType<VB.Syntax.MethodBlockSyntax>())
            //     {
            //         //if (member is VB.Syntax.MethodBlockSyntax)
            //         //{
            //         //    var method = (member as VB.Syntax.MethodStatementSyntax);
            //         //    map.Add(new CodeMapItem
            //         //    {
            //         //        Line = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            //         //        Column = method.GetLocation().GetLineSpan().StartLinePosition.Character,
            //         //        //DisplayName = method.Identifier.Text + method.ParameterList, //nicely prints all params with their types and names
            //         //        DisplayName = method.Identifier.Text + "(" + new string(',', Math.Max(method.ParameterList.Parameters.Count - 1, 0)) + ")",
            //         //        ParentDisplayName = type.GetFullName(),
            //         //        MemberType = "Method"
            //         //    });
            //         //}
            //         //else if (member is PropertyDeclarationSyntax)
            //         //{
            //         //var prop = (member as PropertyDeclarationSyntax);
            //         //map.Add(new CodeMapItem
            //         //{
            //         //    Line = prop.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            //         //    Column = prop.GetLocation().GetLineSpan().StartLinePosition.Character,
            //         //    DisplayName = prop.Identifier.ValueText,
            //         //    ParentDisplayName = type.GetFullName(),
            //         //    MemberType = "Property"
            //         //});
            //         //}
            //         //else
            //         //if (member is FieldDeclarationSyntax)
            //         //{
            //         //var field = (member as FieldDeclarationSyntax);
            //         //map.Add(new CodeMapItem
            //         //{
            //         //    Line = field.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            //         //    Column = field.GetLocation().GetLineSpan().StartLinePosition.Character,
            //         //    DisplayName = field.Declaration.Variables.First().Identifier.Text,
            //         //    ParentDisplayName = type.GetFullName(),
            //         //    MemberType = "Field"
            //         //});
            //         //}
            //     }
            // }

            // //if (decorated && map.Any())
            // //{
            // //    string rootClassName = map.First().ParentDisplayName;
            // //    foreach (var item in map.Skip(1))
            // //    {
            // //        if (item.ParentDisplayName == rootClassName)
            // //            item.ParentDisplayName = "<Global>";
            // //        else if (item.ParentDisplayName.StartsWith(rootClassName + "."))
            // //            item.ParentDisplayName = item.ParentDisplayName.Substring(rootClassName.Length + 1);
            // //    }
            // //}

            // return map.ToArray();
        }
    }

    //for member info SemanticModel.LookupSymbols can be tried
}