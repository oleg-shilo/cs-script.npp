using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;

namespace CSScriptNpp.Roslyn
{
    //all Workspace interface is internal so need to subclass it
    //public class OmnisharpWorkspace : Workspace
    //{
    //    public void AddProject(ProjectInfo projectInfo)
    //    {
    //        OnProjectAdded(projectInfo);
    //    }

    //    public void AddDocument(DocumentInfo documentInfo)
    //    {
    //        OnDocumentAdded(documentInfo);
    //    }

    //    public Task<Workspace> AddProjectToWorkspace(Workspace workspace, string filePath, string[] frameworks, Dictionary<string, string> sourceFiles)
    //    {
    //        var versionStamp = VersionStamp.Create();
    //        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
    //        var systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
    //        var references = new[] { mscorlib, systemCore };

    //        foreach (var framework in frameworks)
    //        {
    //            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), versionStamp,
    //                                                 "OmniSharp", "AssemblyName",
    //                                                 LanguageNames.CSharp, filePath, metadataReferences: references);

    //            this.AddProject(projectInfo);
    //            foreach (var file in sourceFiles)
    //            {
    //                var document = DocumentInfo.Create(DocumentId.CreateNewId(projectInfo.Id), file.Key,
    //                                                   null, SourceCodeKind.Regular,
    //                                                   TextLoader.From(TextAndVersion.Create(SourceText.From(file.Value), versionStamp)), file.Key);

    //                this.AddDocument(document);
    //            }
    //        }

    //        return Task.FromResult(workspace);
    //    }
    //}

    public static class Autocompleter
    {
        public static void InitWorkspace(AdhocWorkspace workspace, string code, int position)
        {
            string projName = "NewProject";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
            var references = new[] { mscorlib, systemCore };
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, LanguageNames.CSharp, metadataReferences: references);
            var newProject = workspace.AddProject(projectInfo);
            var sourceText = SourceText.From(code);
            var newDocument = workspace.AddDocument(newProject.Id, "NewFile.cs", sourceText);
        }

        public async static Task<IEnumerable<object>> GetAutpocompletionFor(string code, int position)
        {
            //e:\dev\roslyn\omnisharp-roslyn\src\omnisharp.roslyn.csharp\services\intellisense\intellisenseservice.cs

            code = @"class Script
{
    static void Main()
    {
        var test = ""ttt"";
        System.Console.WriteLine($""Hello World!{test.Ends";
            position = 103;
            position = 128 + 4;

            var completions = new HashSet<string>();

            var workspace = new AdhocWorkspace();
            //var workspace1 = new AdhocWorkspace();
            //InitWorkspace(workspace, code, position);

            string projName = "NewProject";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
            var references = new[] { mscorlib, systemCore };
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, LanguageNames.CSharp, metadataReferences: references);
            var newProject = workspace.AddProject(projectInfo);
            var sourceText = SourceText.From(code);
            var newDocument = workspace.AddDocument(newProject.Id, "NewFile7.cs", sourceText);

            foreach (var project in workspace.CurrentSolution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    var model = document.GetSemanticModelAsync().Result;

                    var symbols = Recommender.GetRecommendedSymbolsAtPosition(model, position, workspace).ToArray();
                    foreach (var symbol in symbols.Where(s => s.Name.StartsWith("", StringComparison.OrdinalIgnoreCase)))
                    {
                        completions.Add(symbol.Name);
                    }
                }
            }

            var result = completions.Distinct().OrderBy(x=>x.ToLower());
            result.ToList().ForEach(x=> System.Console.WriteLine(x));
            return result;
            //return completions
            //    .OrderByDescending(c => c.CompletionText.IsValidCompletionStartsWithExactCase(wordToComplete))
            //    .ThenByDescending(c => c.CompletionText.IsValidCompletionStartsWithIgnoreCase(wordToComplete))
            //    .ThenByDescending(c => c.CompletionText.IsCamelCaseMatch(wordToComplete))
            //    .ThenByDescending(c => c.CompletionText.IsSubsequenceMatch(wordToComplete))
            //    .ThenBy(c => c.CompletionText); 
        }

        // public IEnumerable<AutoCompleteResponse> Handle(AutoCompleteRequest request)
        //{
        // var documents = _workspace.GetDocuments(request.FileName);
        //  var wordToComplete = request.WordToComplete;
        // var completions = new HashSet<AutoCompleteResponse>();

        //    foreach (var document in documents)
        //    {
        //        var sourceText = await document.GetTextAsync();
        //        var position = sourceText.Lines.GetPosition(new LinePosition(request.Line - 1, request.Column - 1));
        //        var model = await document.GetSemanticModelAsync();

        //        AddKeywords(completions, model, position, request.WantKind, wordToComplete);

        //        var symbols = Recommender.GetRecommendedSymbolsAtPosition(model, position, _workspace);

        //        foreach (var symbol in symbols.Where(s => s.Name.IsValidCompletionFor(wordToComplete)))
        //        {
        //            completions.Add(MakeAutoCompleteResponse(request, symbol));
        //        }
        //    }

        //    return completions
        //        .OrderByDescending(c => c.CompletionText.IsValidCompletionStartsWithExactCase(wordToComplete))
        //        .ThenByDescending(c => c.CompletionText.IsValidCompletionStartsWithIgnoreCase(wordToComplete))
        //        .ThenByDescending(c => c.CompletionText.IsCamelCaseMatch(wordToComplete))
        //        .ThenByDescending(c => c.CompletionText.IsSubsequenceMatch(wordToComplete))
        //        .ThenBy(c => c.CompletionText);
        //}
    }

    public static class StringExtensions
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