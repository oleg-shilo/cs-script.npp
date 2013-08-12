using CSScriptLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSScriptNpp
{
    public class Project
    {
        public string[] Assemblies;
        public string[] SourceFiles;
        public string PrimaryScript;
    }

    public class CSSctiptHelper
    {
        static public Project GenerateProjectFor(string script)
        {
            var retval = new Project { PrimaryScript = script };

            var searchDirs = new List<string> { Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) };

            var parser = new ScriptParser(script, searchDirs.ToArray(), false);
            searchDirs.AddRange(parser.SearchDirs);        //search dirs could be also defined n the script

            var sources = parser.SaveImportedScripts().ToList(); //this will also generate auto-scripts and save them
            sources.Insert(0, script);

            retval.SourceFiles = sources.ToArray();

            //some assemblies are referenced from code and some will need to be resolved from the namespaces
            retval.Assemblies = parser.ReferencedNamespaces
                                      .Where(name => !parser.IgnoreNamespaces.Contains(name))
                                      .SelectMany(name => AssemblyResolver.FindAssembly(name, searchDirs.ToArray()))
                                      .Union(parser.ReferencedAssemblies
                                                   .SelectMany(asm => AssemblyResolver.FindAssembly(asm, searchDirs.ToArray())))
                                      .Distinct()
                                      .ToArray();
            return retval;
        }

        static public void Execute(string scriptFileCmd, Action<Process> onStart, Action<string> onStdOut = null)
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(Plugin.PluginDir, "cscs.exe");
            p.StartInfo.Arguments = "/nl /dbg \"" + scriptFileCmd + "\"";

            if (onStdOut != null)
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
            }

            p.Start();

            onStart(p);

            var output = new StringBuilder();

            string line = null;
            while (onStdOut != null && null != (line = p.StandardOutput.ReadLine()))
            {
                output.AppendLine(line);
                onStdOut(line);
            }
            p.WaitForExit();

            if (output.Length > 0 && output.ToString().StartsWith("Error: Specified file could not be compiled."))
                throw new ApplicationException(output.ToString());
        }

        static public void ExecuteDebug(string scriptFileCmd)
        {
            Process.Start("cmd.exe", "/K \"\"" + Path.Combine(Plugin.PluginDir, "cscs.exe") + "\" /nl /dbg \"" + scriptFileCmd + "\" //x\"");
        }

        static public void OpenAsVSProjectFor(string script)
        {
            var searchDirs = new List<string> { ScriptsDir, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) };

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
                                .ToArray();

            string dir = Path.Combine(VsDir, Process.GetCurrentProcess().Id.ToString()) + "-" + script.GetHashCode();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string projectFile = Path.Combine(dir, Path.GetFileNameWithoutExtension(script) + ".csproj");

            string sourceFilesXml = string.Join(Environment.NewLine,
                                                sourceFiles.Select(file => "<Compile Include=\"" + file + "\" />").ToArray());

            string refAsmsXml = string.Join(Environment.NewLine,
                                            refAsms.Select(file => "<Reference Include=\"" + Path.GetFileNameWithoutExtension(file) + "\"><HintPath>" + file + "</HintPath></Reference>").ToArray());

            File.WriteAllText(projectFile,
                              Resources.Resources.VS2010ProjectTemplate
                                                 .Replace("<UseVSHostingProcess>false", "<UseVSHostingProcess>true") // to ensure *.vshost.exe is created
                                                 .Replace("<OutputType>Library", "<OutputType>Exe")
                                                 .Replace("{$SOURCES}", sourceFilesXml)
                                                 .Replace("{$REFERENCES}", refAsmsXml));
            Process.Start(projectFile);
        }

        static public void ClearVSDir()
        {
            try
            {
                string excludeDirPreffix = Path.Combine(VsDir, Process.GetCurrentProcess().Id.ToString()) + "-";

                if (Directory.Exists(VsDir))
                    foreach (string projectDir in Directory.GetDirectories(VsDir))
                    {
                        if (projectDir.StartsWith(excludeDirPreffix))
                            continue;

                        //vshost.exe is the only file to be 100% times locked if VS has the project loaded
                        string hostFile = Directory.GetFiles(projectDir, "*.vshost.exe", SearchOption.AllDirectories).FirstOrDefault();

                        try
                        {
                            if (hostFile != null)
                                File.Delete(hostFile);

                            foreach (string file in Directory.GetFiles(projectDir, "*", SearchOption.AllDirectories))
                                File.Delete(file);
                            Directory.Delete(projectDir, true);
                        }
                        catch { }
                    }
            }
            catch { }
        }

        static string vsDir;

        public static string VsDir
        {
            get
            {
                if (vsDir == null)
                    vsDir = Path.Combine(CSScript.GetScriptTempDir(), "NppScripts");
                return vsDir;
            }
        }

        static string scriptDir;

        public static string ScriptsDir
        {
            get
            {
                if (scriptDir == null)
                {
                    string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    scriptDir = Path.Combine(rootDir, "NppScripts");
                }
                return scriptDir;
            }
        }
    }
}