using Intellisense.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            catch
            {
                throw;
            }
        }

        public string[] FindReferences(string editorText, int offset, string fileName)
        {
            return Autocompleter.FindReferencess(editorText, offset, fileName,
                                                 assemblies.ToArray(),
                                                 sources.Where(x => x.Item2 != fileName));
        }

        public IEnumerable<ICompletionData> GetCompletionData(string editorText, int offset, string fileName, bool isControlSpace = true)
        {
            return Autocompleter.GetAutocompletionFor(editorText, offset,
                                                      assemblies.ToArray(),
                                                      sources.Where(x => x.Item2 != fileName))
                                                      .Result;
        }

        public string[] GetMemberInfo(string editorText, int offset, string fileName, bool collapseOverloads, out int methodStartPos)
        {
            methodStartPos = offset;
            return Autocompleter.GetMemberInfo(editorText, offset, out methodStartPos,
                                               assemblies.ToArray(),
                                               sources.Where(x => x.Item2 != fileName),
                                               includeOverloads: !collapseOverloads)
                                               .ToArray();
        }

        public IEnumerable<Intellisense.Common.TypeInfo> GetPossibleNamespaces(string editorText, string nameToResolve, string fileName)
        {
            //var sw = new Stopwatch();
            //sw.Start();
            var result = Autocompleter.GetNamespacesFor(editorText, nameToResolve,
                                                        assemblies.ToArray(),
                                                        sources.Where(x => x.Item2 != fileName))
                                                        .Result;

            //sw.Stop();
            //Console.WriteLine("GetPossibleNamespaces " + sw.ElapsedMilliseconds);

            return result;
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
            return Autocompleter.ResolveSymbol(editorText, offset, fileName,
                                               assemblies.ToArray(),
                                               sources.Where(x => x.Item2 != fileName));
        }
    }
}