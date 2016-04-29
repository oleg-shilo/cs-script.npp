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
using System.Diagnostics;

namespace RoslynIntellisense
{
    public static class Autocompleter
    {
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

            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, LanguageNames.CSharp, metadataReferences: refs);
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
                var doc = InitWorkspace(workspace, code, file, references, includes);

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

                                              var hint = File.ReadAllText(filePath).Substring(x.SourceSpan.Start).Split('\n').First().Trim();

                                              var fileContent = x.SourceTree.GetText().ToString();
                                              int pos = fileContent.IndexOf("///CS-Script auto-class generation");
                                              if (pos != 1)
                                              {
                                                  if (pos < x.SourceSpan.Start)
                                                      line--;
                                              }
                                              return $"{filePath}({line},{column}): {hint}";
                                          }).ToArray();

                return locations;
            }
            catch { } //failed, no need to report, as auto-completion is expected to fail sometimes 
            return new string[0];
        }


        public static DomRegion ResolveSymbol(string code, int position, string file, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            try
            {
                var workspace = new AdhocWorkspace();
                var doc = InitWorkspace(workspace, code, file, references, includes);

                ISymbol symbol = SymbolFinder.FindSymbolAtPositionAsync(doc, position).Result;

                if (symbol == null)
                    return DomRegion.Empty;

                var location = symbol.Locations.First();

                if (location.IsInSource)
                {
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

                    // Will not work if the asm name (Identity.Name) is not the same as the file name.
                    // But unfortunately location.MetadataModule doesn't preserve the asm file name
                    var asmName = symbol.ContainingAssembly.Identity.Name;
                    var asmFile = doc.Project.MetadataReferences
                                             .Select(x => x.Display)
                                             .Where(x => string.Compare(Path.GetFileNameWithoutExtension(x), asmName, true) == 0)
                                             .FirstOrDefault();

                    string typeName, memberName;

                    if (symbol.Kind == SymbolKind.NamedType)
                    {
                        typeName =
                        memberName = symbol.ToString();
                    }
                    else
                    {
                        int pos;
                        string reconstructed = symbol.Reconstruct(out pos);


                        //Temporary work around: making it Reflector.Cecil compatible
                        //Cannot use symbol.ContainingType.ToString() as it will use aliases for built-in types
                        //Reflector.Cecil require aliased params but proper member names 
                        string displayName = symbol.ToString();
                        typeName = symbol.ContainingType.GetFullName();
                        //Need to generate: 'System.String.Replace(string, string)'
                        if (displayName.Contains("(")) //method
                            memberName = typeName + "." + symbol.Name + "(" + displayName.Split('(').Last();
                        else
                            memberName = typeName + "." + symbol.Name;
                    }
                    return new DomRegion
                    {
                        FileName = asmFile,
                        BeginLine = 0,
                        EndLine = 0,
                        BeginColumn = 0,
                        Hint = $"asm|{typeName}|{memberName}"
                    };
                }
            }
            catch { } //failed, no need to report, as auto-completion is expected to fail sometimes 
            return DomRegion.Empty;
        }

        public static IEnumerable<string> GetMemberInfo(string code, int position, out int methodStartPos, string[] references = null, IEnumerable<Tuple<string, string>> includes = null, bool includeOverloads = false)
        {
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
                var doc = InitWorkspace(workspace, code, null, references, includes);

                var symbol = SymbolFinder.FindSymbolAtPositionAsync(doc, actualPosition).Result;

                if (symbol != null)
                {
                    result.Add(symbol.ToTooltip());

                    if (includeOverloads)
                    {
                        result.AddRange(symbol.GetOverloads().Select(s => s.ToTooltip()));
                    }

                    return result;
                }
            }
            catch { } //failed, no need to report, as auto-completion is expected to fail sometimes 
            return new string[0];
        }

        static IEnumerable<ISymbol> GetOverloads(this ISymbol symbol)
        {
            return symbol.ContainingType.GetMembers(symbol.Name).Where(x => x != symbol);
        }

        //position is zero-based
        public async static Task<IEnumerable<ICompletionData>> GetAutocompletionFor(string code, int position, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            string opContext;

            string partialWord;
            int logicalPosition;

            GetWordFromCaret(code, position, out logicalPosition, out partialWord, out opContext);

            var completions = await Resolve(code, logicalPosition, references, includes);

            try
            {
                var result = completions.Where(s => s.DisplayText.CanComplete(partialWord))
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

        public async static Task<IEnumerable<ICompletionData>> Resolve(string code, int position, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            var completions = new List<ICompletionData>();

            var pos = position - 1;
            var workspace = new AdhocWorkspace();
            var asms = defaultRefs;

            if (references != null)
                asms = asms.Concat(references).ToArray();

            var document = InitWorkspace(workspace, code, null, asms.ToArray(), includes);
            var model = await document.GetSemanticModelAsync();
            var symbols = Recommender.GetRecommendedSymbolsAtPosition(model, position, workspace).ToArray();

            var data = symbols.Select(s => s.ToCompletionData()).ToArray();

            foreach (var group in data.GroupBy(x => x.DisplayText))
            {
                var item = group.First();

                if (group.Count() > 1)
                    foreach (var overload in group)
                        item.AddOverload(overload);

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

        public async static Task<IEnumerable<Intellisense.Common.TypeInfo>> GetNamespacesFor(string editorText, string nameToResolve, string[] references = null, IEnumerable<Tuple<string, string>> includes = null)
        {
            var suggestions = new List<Intellisense.Common.TypeInfo>();

            var workspace = new AdhocWorkspace();

            var doc = InitWorkspace(workspace, editorText, null, references, includes);

            IEnumerable<ISymbol> result = await SymbolFinder.FindDeclarationsAsync(doc.Project, nameToResolve, ignoreCase: false);
            foreach (ISymbol declaration in result)
            {
                if (declaration.Kind != SymbolKind.NamedType)
                    continue; //limit to 

                var name = declaration.Name;
                var nmspace = declaration.GetNamespace();
                if (!string.IsNullOrEmpty(nmspace))
                    name = nmspace + "." + name;

                var info = new Intellisense.Common.TypeInfo { Namespace = nmspace, FullName = name };
                suggestions.Add(info);
            }

            return suggestions;
        }
    }

    //for member info SemanticModel.LookupSymbols can be tried
}