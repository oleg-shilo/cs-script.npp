using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Intellisense.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;
using System.Windows.Forms;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace RoslynIntellisense
{
    public class Engine : IEngine
    {
#pragma warning disable 4014
        public void Preload()
        {
            try
            {
                if (Environment.GetEnvironmentVariable("suppress_roslyn_preloading") == null)
                {
                    var code = @"class Script
                             {
                                 static void Main()
                                 {
                                     var test = ""ttt"";
                                     System.Console.WriteLine($""Hello World!{test.Ends";

                    Autocompleter.GetAutocompletionFor(code, 132);
                }
            }
            catch { }

        }
        public string[] FindReferences(string editorText, int offset, string fileName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICompletionData> GetCompletionData(string editorText, int offset, string fileName, bool isControlSpace = true)
        {
            return Autocompleter.GetAutocompletionFor(editorText, offset, assemblies.ToArray(), sources.Select(x => x.Item1).ToArray()).Result;
        }

        public string[] GetMemberInfo(string editorText, int offset, string fileName, bool collapseOverloads, out int methodStartPos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Intellisense.Common.TypeInfo> GetPossibleNamespaces(string editorText, string nameToResolve, string fileName)
        {
            throw new NotImplementedException();
        }

        List<Tuple<string, string>> sources = new List<Tuple<string, string>>();
        List<string> assemblies = new List<string>();

        public void ResetProject(Tuple<string, string>[] sourceFiles = null, params string[] assemblies)
        {
            this.sources.Clear();
            this.assemblies.Clear();
            if (sourceFiles != null)
                this.sources.AddRange(sourceFiles);
            this.assemblies.AddRange(assemblies);
        }

        public DomRegion ResolveCSharpMember(string editorText, int offset, string fileName)
        {
            throw new NotImplementedException();
        }
    }

    public static class Autocompleter
    {
        public static Document InitWorkspace(AdhocWorkspace workspace, string code, string[] references = null, string[] includes = null)
        {
            string projName = "NewProject";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();

            var refs = new List<MetadataReference>();
            if (references != null)
                refs.AddRange(references.Select(a => MetadataReference.CreateFromFile(a)));

            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, LanguageNames.CSharp, metadataReferences: refs);
            var newProject = workspace.AddProject(projectInfo);

            var newDocument = workspace.AddDocument(newProject.Id, "code.cs", SourceText.From(code));

            if (includes != null)
            {
                int index = 1;

                foreach (var item in includes)
                    workspace.AddDocument(newProject.Id, $"code{index++}.cs", SourceText.From(item));
            }

            return newDocument;
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

        //position is zero-based
        public async static Task<IEnumerable<ICompletionData>> GetAutocompletionFor(string code, int position, string[] references = null, string[] includes = null)
        {
            string opContext;

            string partialWord;
            int logicalPosition;

            GetWordFromCaret(code, position, out logicalPosition, out partialWord, out opContext);

            var completions = await Resolve(code, logicalPosition, references, includes);

            try
            {
                var result = completions.Where(s => s.DisplayText.IsValidCompletionFor(partialWord))
                                        .OrderByDescending(c => c.DisplayText.IsValidCompletionStartsWithExactCase(partialWord))
                                        .ThenByDescending(c => c.DisplayText.IsValidCompletionStartsWithIgnoreCase(partialWord))
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

        public async static Task<IEnumerable<ICompletionData>> Resolve(string code, int position, string[] references = null, string[] includes = null)
        {
            var completions = new List<ICompletionData>();

            var pos = position - 1;
            var workspace = new AdhocWorkspace();
            var asms = defaultRefs;

            if (references != null)
                asms = asms.Concat(references).ToArray();

            var document = InitWorkspace(workspace, code, asms.ToArray(), includes);
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
    }

    public static class GenericExtensions
    {
        public static T To<T>(this object obj)
        {
            return (T) obj;
        }

        public static CompletionType ToCompletionType(this SymbolKind kind)
        {
            switch (kind)
            {
                case SymbolKind.Method: return CompletionType.method;
                case SymbolKind.Event: return CompletionType._event;
                case SymbolKind.Field: return CompletionType.field;
                case SymbolKind.Property: return CompletionType.property;
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

            string nmspace = "";
            if (type.ContainingNamespace != null)
                nmspace = "{" + type.ContainingNamespace + "}.";
            return (nmspace + type.Name);
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
                        var method = (IMethodSymbol) symbol;
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

        public static int GetWordStartOf(this string text, int offset)
        {
            if (text[offset] != '.') //we may be at the partially complete word
                for (int i = offset - 1; i >= 0; i--)
                    if (Autocompleter.Delimiters.Contains(text[i]))
                        return i + 1;
            return offset;
        }

        public static object GetProp(this object obj, string name)
        {
            var property = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                throw new Exception("ReflectionExtensions: cannot find property " + name);
            return property.GetValue(obj, null);
        }

        public static string[] GetUsingNamespace(this SyntaxNode syntax, string code)
        {
            var namespaces = syntax.DescendantNodes()
                                   .OfType<UsingDirectiveSyntax>()
                                   .Select(x => x.GetText().ToString()
                                                 .Replace("using", "")
                                                 .Replace(";", "")
                                                 .Trim())
                                   .ToArray();
            return namespaces;
        }

        public static string ShrinkNamespaces(this string statement, params string[] knownNamespaces)
        {
            //statement format: "{<namespace_name>}.type"
            string result = statement;

            foreach (var item in knownNamespaces.Select(x => "{" + x + "}."))
                result = result.Replace(item, "");

            return result.Replace("{", "")
                         .Replace("}", "");
        }
    }

    public static class OmnyRoslynStringExtensions
    {
        public static bool IsValidCompletionFor(this string completion, string partial)
        {
            return completion.IsValidCompletionStartsWithIgnoreCase(partial) || completion.IsSubsequenceMatch(partial);
        }

        public static bool IsValidCompletionStartsWithExactCase(this string completion, string partial)
        {
            return completion.StartsWith(partial);
        }

        public static bool IsValidCompletionStartsWithIgnoreCase(this string completion, string partial)
        {
            return completion.ToLower().StartsWith(partial.ToLower());
        }

        public static bool IsCamelCaseMatch(this string completion, string partial)
        {
            return new string(completion.Where(c => c >= 'A' && c <= 'Z').ToArray()).StartsWith(partial.ToUpper());
        }

        public static bool IsSubsequenceMatch(this string completion, string partial)
        {
            if (partial == string.Empty)
            {
                return true;
            }

            // Limit the number of results returned by making sure
            // at least the first characters match.
            // We can get far too many results back otherwise.
            if (!FirstLetterMatches(partial, completion))
            {
                return false;
            }

            return new string(completion.ToUpper().Intersect(partial.ToUpper()).ToArray()) == partial.ToUpper();
        }

        static bool FirstLetterMatches(string word, string match)
        {
            if (string.IsNullOrEmpty(match))
            {
                return false;
            }

            return char.ToLowerInvariant(word.First()) == char.ToLowerInvariant(match.First());
        }
    }
}