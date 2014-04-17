using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CSScriptLibrary;
using csscript;
using ICSharpCode.NRefactory.Editor;

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

        static public Tuple<string[], string[]> GetProjectFiles(string script)
        {
            var searchDirs = new List<string> { Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ScriptsDir, };

            var parser = new ScriptParser(script, searchDirs.ToArray(), false);
            searchDirs.AddRange(parser.SearchDirs);        //search dirs could be also defined n the script

            IList<string> sourceFiles = parser.SaveImportedScripts().ToList(); //this will also generate auto-scripts and save them
            sourceFiles.Add(script);

            //some assemblies are referenced from code and some will need to be resolved from the namespaces
            var refAsms = parser.ReferencedNamespaces
                                .Where(name => !parser.IgnoreNamespaces.Contains(name))
                                .SelectMany(name => AssemblyResolver.FindAssembly(name, searchDirs.ToArray()))
                                .Union(parser.ReferencedAssemblies
                                             .SelectMany(asm => AssemblyResolver.FindAssembly(asm, searchDirs.ToArray())))
                                .Distinct()
                                .ToList();

            if (NppScriptsAsm != null)
                refAsms.Add(NppScriptsAsm);

            return new Tuple<string[], string[]>(sourceFiles.ToArray(), refAsms.ToArray());
        }

        static public bool NeedsAutoclassWrapper(string text)
        {
            return Regex.Matches(text, @"\s?//css_args\s+/ac(,|\s+)").Count != 0;
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
                string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string scriptDir = Path.Combine(rootDir, "NppScripts");

                if (!Directory.Exists(scriptDir))
                    Directory.CreateDirectory(scriptDir);

                return scriptDir;
            }
        }
    }
}
