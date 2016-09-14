using Intellisense.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RoslynIntellisense
{
    public class Engine : IEngine
    {
        public string Language
        {
            get
            {
                return Autocompleter.Language;
            }
        
            set
            {
                Autocompleter.Language = value;
            }
        }

#pragma warning disable 4014

        public void Preload()
        {
            lock (typeof(Autocompleter))
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
        }

        Dictionary<string, object> options = new Dictionary<string, object>();
        public void SetOption(string name, object value)
        {
            if (name == "ReflectionOutDir")
                Autocompleter.OutputDir = value.ToString();
            else
                options[name] = value;
        }

        string GetLanguageFor(string file)
        {
            return file.IsVbFile() ? "VB" : "C#";
        }

        public string[] FindReferences(string editorText, int offset, string fileName)
        {
            lock (typeof(Autocompleter))
            {
                Autocompleter.Language = GetLanguageFor(fileName);
                return Autocompleter.FindReferencess(editorText, offset, fileName,
                                                     assemblies.ToArray(),
                                                     sources.Where(x => x.Item2 != fileName));
            }
        }

        public IEnumerable<ICompletionData> GetCompletionData(string editorText, int offset, string fileName, bool isControlSpace = true)
        {
            lock (typeof(Autocompleter))
            {
                Autocompleter.Language = GetLanguageFor(fileName);
                return Autocompleter.GetAutocompletionFor(editorText, offset,
                                                      assemblies.ToArray(),
                                                      sources.Where(x => x.Item2 != fileName))
                                                      .Result;
            }
        }

        public string[] GetMemberInfo(string editorText, int offset, string fileName, bool collapseOverloads, out int methodStartPos)
        {
            lock (typeof(Autocompleter))
            {
                Autocompleter.Language = GetLanguageFor(fileName);

                methodStartPos = offset;
                return Autocompleter.GetMemberInfo(editorText, offset, out methodStartPos,
                                                   assemblies.ToArray(),
                                                   sources.Where(x => x.Item2 != fileName),
                                                   includeOverloads: !collapseOverloads)
                                                   .ToArray();
            }
        }

        public IEnumerable<Intellisense.Common.TypeInfo> GetPossibleNamespaces(string editorText, string nameToResolve, string fileName)
        {
            lock (typeof(Autocompleter))
            {
                Autocompleter.Language = GetLanguageFor(fileName);

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
        }

        public CodeMapItem[] GetMapOf(string code, bool decorated)
        {
            lock (typeof(Autocompleter))
            {
                return Autocompleter.GetMapOf(code, decorated);
            }
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
            lock (typeof(Autocompleter))
            {
                Autocompleter.Language = GetLanguageFor(fileName);
                return Autocompleter.ResolveSymbol(editorText, offset, fileName,
                                               assemblies.ToArray(),
                                               sources.Where(x => x.Item2 != fileName));
            }
        }
    }
}