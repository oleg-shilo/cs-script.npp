using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.NRefactory.Editor;
using Intellisense.Common;

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
    }
}