using csscript;
using CSScriptLibrary;
using ICSharpCode.NRefactory.Editor;
using Intellisense.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace CSScriptIntellisense
{
    public static class CSScriptHelper
    {
        static string nppScriptsAsm;

        static string NppScriptsAsm
        {
            get
            {
                if (nppScriptsAsm == null)
                    nppScriptsAsm = AppDomain.CurrentDomain.GetAssemblies()
                                                           .Where(x => x.FullName.StartsWith("NppScripts,"))
                                                           .Select(x => x.Location)
                                                           .FirstOrDefault();
                return nppScriptsAsm;
            }
        }

        static public string GetGlobalCSSConfig()
        {
            try
            {
                string cscs_exe = GetEngineExe();
                if (cscs_exe != null)
                {
                    var file = Path.Combine(Path.GetDirectoryName(cscs_exe), "css_config.xml");
                    if (File.Exists(file))
                        return file;
                }
            }
            catch { }
            return null;

            //var csscriptDir = Environment.GetEnvironmentVariable("CSSCRIPT_DIR");
            //if (csscriptDir != null)
            //    return Environment.ExpandEnvironmentVariables("%CSSCRIPT_DIR%\\css_config.xml");
            //else
            //    return null;
        }

        public static Func<string> GetEngineExe = () => null;

        static public Tuple<string[], string[], string[]> GetGlobalConfigItems()
        {
            var dirs = new List<string>();
            var asms = new List<string>();
            var namespaces = new List<string>();

            Func<string, string[]> splitPathItems = text => text.Split('|', ';')
                                                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                                                .Select(x => Environment.ExpandEnvironmentVariables(x.Trim()))
                                                                .ToArray();
            try
            {
                dirs.AddRange(splitPathItems(Config.Instance.DefaultSearchDirs ?? ""));
                asms.AddRange(splitPathItems(Config.Instance.DefaultRefAsms ?? ""));
                namespaces.AddRange(splitPathItems(Config.Instance.DefaultNamespaces ?? ""));

                var configFile = GetGlobalCSSConfig();
                if (configFile != null && File.Exists(configFile))
                {
                    var doc = new XmlDocument();
                    doc.Load(configFile);

                    dirs.AddRange(splitPathItems((doc.FirstChild.SelectSingleNode("searchDirs") ??
                                                  doc.FirstChild.SelectSingleNode("SearchDirs")).InnerText));
                    dirs.Add(Path.Combine(Path.GetDirectoryName(configFile), "Lib"));

                    asms.AddRange(splitPathItems((doc.FirstChild.SelectSingleNode("defaultRefAssemblies") ??
                                                  doc.FirstChild.SelectSingleNode("DefaultRefAssemblies"))
                                                  .InnerText));
                }
            }
            catch { }

            return new Tuple<string[], string[], string[]>(dirs.Distinct().ToArray(), asms.ToArray(), namespaces.ToArray());
        }

        static public List<string> RemoveEmptyAndDulicated(this List<string> collection)
        {
            collection.RemoveAll(x => string.IsNullOrEmpty(x));
            var distinct = collection.Distinct().ToArray();
            collection.Clear();
            collection.AddRange(distinct);
            return collection;
        }

        static public List<string> AgregateReferences(this ScriptParser parser, IEnumerable<string> searchDirs, IEnumerable<string> defaultRefAsms, IEnumerable<string> defaultNamespacess)
        {
            var probingDirs = searchDirs.ToArray();

            var refPkAsms = parser.ResolvePackages(suppressDownloading: true);

            var refCodeAsms = parser.ReferencedAssemblies
                                    .SelectMany(asm => AssemblyResolver.FindAssembly(asm.Replace("\"", ""), probingDirs));

            var refAsms = refPkAsms.Union(refPkAsms)
                                   .Union(refCodeAsms)
                                   .Union(defaultRefAsms.SelectMany(name => AssemblyResolver.FindAssembly(name, probingDirs)))
                                   .Distinct()
                                   .ToArray();

            //some assemblies are referenced from code and some will need to be resolved from the namespaces
            bool disableNamespaceResolving = (parser.IgnoreNamespaces.Count() == 1 && parser.IgnoreNamespaces[0] == "*");

            if (!disableNamespaceResolving)
            {
                var asmNames = refAsms.Select(x => Path.GetFileNameWithoutExtension(x).ToUpper()).ToArray();

                var refNsAsms = parser.ReferencedNamespaces
                                      .Union(defaultNamespacess)
                                      .Where(name => !string.IsNullOrEmpty(name))
                                      .Where(name => !parser.IgnoreNamespaces.Contains(name))
                                      .Where(name => !asmNames.Contains(name.ToUpper()))
                                      .Distinct()
                                      .SelectMany(name =>
                                      {
                                          var asms = AssemblyResolver.FindAssembly(name, probingDirs);
                                          return asms;
                                      })
                                      .ToArray();

                refAsms = refAsms.Union(refNsAsms).ToArray();
            }

            refAsms = FilterDuplicatedAssembliesByFileName(refAsms);
            //refAsms = FilterDuplicatedAssembliesWithReflection(refAsms); //for possible more comprehensive filtering in future
            return refAsms.ToList();
        }

        static public T CreateInstanceFromAndUnwrap<T>(this AppDomain domain)
        {
            Type type = typeof(T);
            return (T)domain.CreateInstanceFromAndUnwrap(type.Assembly.Location, type.ToString());
        }

        class RemoteResolver : MarshalByRefObject
        {
            //Must be done remotely to avoid loading collisions like below:
            //"Additional information: API restriction: The assembly 'file:///...\CSScriptLibrary.dll' has
            //already loaded from a different location. It cannot be loaded from a new location within the same appdomain."
            public string[] Filter(string[] assemblies)
            {
                var uniqueAsms = new List<string>();
                var asmNames = new List<string>();
                foreach (var item in assemblies)
                {
                    try
                    {
                        string name = Assembly.ReflectionOnlyLoadFrom(item).GetName().Name;
                        if (!asmNames.Contains(name))
                        {
                            uniqueAsms.Add(item);
                            asmNames.Add(name);
                        }
                    }
                    catch { }
                }
                return uniqueAsms.ToArray();
            }
        }

        //not in use yet
        static string[] FilterDuplicatedAssembliesWithReflection(string[] assemblies)
        {
            var tempDomain = AppDomain.CurrentDomain.Clone();

            var resolver = tempDomain.CreateInstanceFromAndUnwrap<RemoteResolver>();
            var newAsms = resolver.Filter(assemblies);

            tempDomain.Unload();

            return newAsms;
        }

        static string[] FilterDuplicatedAssembliesByFileName(string[] assemblies)
        {
            var uniqueAsms = new List<string>();
            var asmNames = new List<string>();
            foreach (var item in assemblies)
            {
                try
                {
                    string name = Path.GetFileNameWithoutExtension(item);
                    if (!asmNames.Contains(name))
                    {
                        uniqueAsms.Add(item);
                        asmNames.Add(name);
                    }
                }
                catch { }
            }
            return uniqueAsms.ToArray();
        }

        static public Tuple<string[], string[]> GetProjectFiles(string script)
        {
            var globalConfig = GetGlobalConfigItems();
            string[] defaultSearchDirs = globalConfig.Item1;
            string[] defaultRefAsms = globalConfig.Item2;
            string[] defaultNamespaces = globalConfig.Item3;

            var searchDirs = new List<string>();
            searchDirs.Add(Path.GetDirectoryName(script));
            searchDirs.AddRange(defaultSearchDirs);

            var parser = new ScriptParser(script, searchDirs.ToArray(), false);

            searchDirs.AddRange(parser.SearchDirs);        //search dirs could be also defined n the script
            searchDirs.Add(ScriptsDir);
            searchDirs.RemoveEmptyAndDulicated();

            IList<string> sourceFiles = parser.SaveImportedScripts().ToList(); //this will also generate auto-scripts and save them
            sourceFiles.Add(script);
            sourceFiles = sourceFiles.Distinct().ToList();

            //some assemblies are referenced from code and some will need to be resolved from the namespaces
            var refAsms = parser.AgregateReferences(searchDirs, defaultRefAsms, defaultNamespaces);

            if (NppScriptsAsm != null)
                refAsms.Add(NppScriptsAsm);

            return new Tuple<string[], string[]>(sourceFiles.ToArray(), refAsms.ToArray());
        }

        static public bool NeedsAutoclassWrapper(string text)
        {
            return Regex.Matches(text, @"\s?//css_args\s+/ac(,|;|\s+)").Count != 0
                || Regex.Matches(text, @"\s?//css_autoclass\s+").Count != 0
                || Regex.Matches(text, @"\s?//css_ac\s+").Count != 0;
        }

        static public string GenerateAutoclassWrapper(string text, ref int position)
        {
            return AutoclassGenerator.Process(text, ref position);
        }

        static public bool DecorateIfRequired(ref string text)
        {
            int dummy = 0;
            return DecorateIfRequired(ref text, ref dummy);
        }

        static public Tuple<int, int> GetDecorationInfo(string code)
        {
            int pos = code.IndexOf("///CS-Script auto-class generation");
            if (pos != -1)
            {
                var injectedLine = new ReadOnlyDocument(code).GetLineByOffset(pos);
                return new Tuple<int, int>(injectedLine.Offset, injectedLine.Length + Environment.NewLine.Length);
            }
            else
                return new Tuple<int, int>(-1, 0);
        }

        static public void Undecorate(string text, ref DomRegion region)
        {
            int pos = text.IndexOf("///CS-Script auto-class generation");
            if (pos != -1)
            {
                var injectedLine = new ReadOnlyDocument(text).GetLineByOffset(pos);
                if (injectedLine.LineNumber < region.BeginLine)
                {
                    region.BeginLine--;
                    region.EndLine--;
                }
            }
        }

        static public bool DecorateIfRequired(ref string text, ref int currentPos)
        {
            if (NeedsAutoclassWrapper(text))
            {
                text = GenerateAutoclassWrapper(text, ref currentPos);
                return true;
            }
            else
                return false;
        }

        static public string ScriptsDir
        {
            get
            {
                string scriptDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NppScripts");
                if (!Directory.Exists(scriptDir))
                    Directory.CreateDirectory(scriptDir);
                return scriptDir;
            }
        }
    }
}