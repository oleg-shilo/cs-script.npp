using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CSScriptLibrary;
using CSScriptNpp.Properties;

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

        static public void ExecuteAsynch(string scriptFile)
        {
            string cscs = "\"" + Path.Combine(Plugin.PluginDir, "cscs.exe") + "\"";
            string script = "\"" + scriptFile + "\"";
            string args = string.Format("{0} /nl {1}", cscs, script);
            Process.Start(ConsoleHostPath, args);
        }

        static public void Execute(string scriptFileCmd, Action<Process> onStart = null, Action<string> onStdOut = null)
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

            if (onStart != null)
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
                throw new ApplicationException(output.ToString().Replace("csscript.CompilerException: ", ""));
        }
       
        static public void Build(string scriptFileCmd)
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(Plugin.PluginDir, "cscs.exe");
            p.StartInfo.Arguments = "/nl /ca \"" + scriptFileCmd + "\"";

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;

            p.Start();

            var output = new StringBuilder();

            bool error = false;
            string line = null;
            while (null != (line = p.StandardOutput.ReadLine()))
            {
                if (line.ToString().StartsWith("Error: Specified file could not be compiled."))
                {
                    error = true;
                    continue;
                }

                if (!string.IsNullOrEmpty(line) && !line.Contains("at csscript.CSExecutor."))
                    output.AppendLine(line);
            }
            p.WaitForExit();

            if (error)
                throw new ApplicationException(output.ToString().Replace("csscript.CompilerException: ", "")); //for a better appearance remove CS-Script related stuff
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

        static string consoleHostPath;
        static string ConsoleHostPath
        {
            get
            {
                if (consoleHostPath == null || !File.Exists(consoleHostPath))
                {
                    consoleHostPath = Path.Combine(CSScript.GetScriptTempDir(), "CSScriptNpp\\ConsoleHost.exe");
                    try
                    {
                        var dir = Path.GetDirectoryName(consoleHostPath);

                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        File.WriteAllBytes(consoleHostPath, Resources.Resources.ConsoleHost); //always try to override existing to ensure the latest version
                    }
                    catch { } //it can be already locked (running)
                }
                return consoleHostPath;
            }

        }
    }
}