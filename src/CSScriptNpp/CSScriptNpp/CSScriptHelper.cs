using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using CSScriptLibrary;

namespace CSScriptNpp
{
    public class Project
    {
        public string[] Assemblies;
        public string[] SourceFiles;
        public string PrimaryScript;
    }

    public class CSScriptHelper
    {
        static string nppScriptsDir;

        static string NppScriptsDir
        {
            get
            {
                if (nppScriptsDir == null)
                    nppScriptsDir = AppDomain.CurrentDomain.GetAssemblies()
                                                           .Where(x => x.FullName.StartsWith("NppScripts,"))
                                                           .Select(x => Path.GetDirectoryName(x.Location))
                                                           .FirstOrDefault();
                return nppScriptsDir;
            }
        }

        static string GenerateProbingDirArg()
        {
            string probingDirArg = "";

            if (NppScriptsDir != null)
                probingDirArg = "\"/dir:" + NppScriptsDir + "\"";

            return probingDirArg;
        }

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

        static public string ScriptEngineVersion()
        {
            return FileVersionInfo.GetVersionInfo(Path.Combine(Plugin.PluginDir, "cscs.exe")).FileVersion.ToString();
        }

        static public void Execute(string scriptFileCmd, Action<Process> onStart = null, Action<string> onStdOut = null)
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(Plugin.PluginDir, "cscs.exe");
            p.StartInfo.Arguments = "/nl /l /dbg " + GenerateProbingDirArg() + " \"" + scriptFileCmd + "\"";

            //p.StartInfo.WorkingDirectory = Path.GetDirectoryName(scriptFileCmd);

            if (onStdOut != null)
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                //p.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentUICulture.TextInfo.OEMCodePage);
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

        static public string Isolate(string scriptFile, bool asScript, string targerRuntimeVersion)
        {
            string dir = Path.Combine(Path.GetDirectoryName(scriptFile), Path.GetFileNameWithoutExtension(scriptFile));
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);

            string engineFileName;
            if (targerRuntimeVersion == "v4.0.30319")
                engineFileName = "cscs.exe";
            else if (targerRuntimeVersion == "v2.0.50727")
                engineFileName = "cscs.v3.5.exe";
            else
                throw new Exception("The requested Target Runtime version (" + targerRuntimeVersion + ") is not supported.");

            if (asScript)
            {
                Project proj = GenerateProjectFor(scriptFile);

                string engine = Path.Combine(Plugin.PluginDir, engineFileName);

                string batchFile = Path.Combine(dir, "run.cmd");

                var files = proj.Assemblies.Where(a => !a.Contains("GAC_MSIL"))
                                .Concat(proj.SourceFiles);

                Directory.CreateDirectory(dir);

                Action<string, string> copy = (file, directory) => File.Copy(file, Path.Combine(directory, Path.GetFileName(file)), true);

                foreach (string file in files)
                    copy(file, dir);

                File.Copy(engine, Path.Combine(dir, "cscs.exe"), true);

                File.WriteAllText(batchFile, "cscs.exe \"" + Path.GetFileName(scriptFile) + "\"\r\npause");

                return dir;
            }
            else
            {
                string srcExe = Path.ChangeExtension(scriptFile, ".exe");
                string destExe = Path.Combine(dir, Path.GetFileName(srcExe));

                string script = "\"" + scriptFile + "\"";

                string cscs = Path.Combine(Plugin.PluginDir, engineFileName);
                string args = string.Format("/e  {0} {1}", GenerateProbingDirArg(), script);

                var p = new Process();
                p.StartInfo.FileName = cscs;
                p.StartInfo.Arguments = args;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();

                if (File.Exists(srcExe))
                {
                    Directory.CreateDirectory(dir);

                    File.Copy(srcExe, destExe, true);
                    File.Delete(srcExe);
                    return dir;
                }
                else
                {
                    //just show why building has failed
                    cscs = "\"" + cscs + "\"";
                    args = string.Format("{0} /e  {1} {2}", cscs, GenerateProbingDirArg(), script);
                    Process.Start(ConsoleHostPath, args);
                    return null;
                }
            }
        }

        static public void Build(string scriptFileCmd)
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(Plugin.PluginDir, "cscs.exe");
            p.StartInfo.Arguments = "/nl /ca " + GenerateProbingDirArg() + " \"" + scriptFileCmd + "\"";

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8;

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

        static public void ExecuteAsynch(string scriptFile)
        {
            string cscs = "\"" + Path.Combine(Plugin.PluginDir, "cscs.exe") + "\"";
            string script = "\"" + scriptFile + "\"";
            string args = string.Format("{0} /nl /l {1} {2}", cscs, GenerateProbingDirArg(), script);
            Process.Start(ConsoleHostPath, args);
        }

        static public void ExecuteDebug(string scriptFileCmd)
        {
            Process.Start("cmd.exe", "/K \"\"" + Path.Combine(Plugin.PluginDir, "cscs.exe") + "\" /nl /l /dbg " + GenerateProbingDirArg() + " \"" + scriptFileCmd + "\" //x\"");
        }

        static public void OpenAsVSProjectFor(string script)
        {
            var searchDirs = new List<string> { ScriptsDir, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) };

            var parser = new ScriptParser(script, searchDirs.ToArray(), false);
            searchDirs.AddRange(parser.SearchDirs);        //search dirs could be also defined n the script

            if (NppScriptsDir != null)
                searchDirs.Add(NppScriptsDir);

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
                //if (consoleHostPath == null || !File.Exists(consoleHostPath) || !Utils.IsSameTimestamp(Assembly.GetExecutingAssembly().Location, consoleHostPath))
                if (consoleHostPath == null || !File.Exists(consoleHostPath))
                {
                    consoleHostPath = Path.Combine(CSScript.GetScriptTempDir(), "CSScriptNpp\\ConsoleHost.exe");
                    try
                    {
                        var dir = Path.GetDirectoryName(consoleHostPath);

                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        File.WriteAllBytes(consoleHostPath, Resources.Resources.ConsoleHost); //always try to override existing to ensure the latest version
                        //Utils.SetSameTimestamp(Assembly.GetExecutingAssembly().Location, consoleHostPath);
                    }
                    catch { } //it can be already locked (running)
                }
                return consoleHostPath;
            }
        }

        static public string GetLatestAvailableVersion()
        {
            try
            {
                return DownloadText("http://csscript.net/npp/latest_version.txt");
            }
            catch
            {
                return null;
            }
        }

        static public string GetLatestAvailableMsi(string version)
        {
            try
            {
                string downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                string destFile = Path.Combine(downloadDir, "CSScriptNpp." + version + ".msi");

                int numOfAlreadyDownloaded = Directory.GetFiles(downloadDir, "CSScriptNpp." + version + "*.msi").Count();
                if (numOfAlreadyDownloaded > 0)
                    destFile = Path.Combine(downloadDir, "CSScriptNpp." + version + " (" + (numOfAlreadyDownloaded + 1) + ").msi");

                DownloadBinary("http://csscript.net/npp/CSScriptNpp." + version + ".msi", destFile);

                return destFile;
            }
            catch
            {
                return null;
            }
        }

        static void DownloadBinary(string url, string destinationPath, string proxyUser = null, string proxyPw = null)
        {
            var sb = new StringBuilder();
            byte[] buf = new byte[1024 * 4];

            if (proxyUser != null)
                WebRequest.DefaultWebProxy.Credentials = new NetworkCredential(proxyUser, proxyPw);

            var request = WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            using (var destStream = new FileStream(destinationPath, FileMode.CreateNew))
            using (var resStream = response.GetResponseStream())
            {
                int count = 0;
                while (0 < (count = resStream.Read(buf, 0, buf.Length)))
                {
                    destStream.Write(buf, 0, count);
                }
            }
        }

        static string DownloadText(string url, string proxyUser = null, string proxyPw = null)
        {
            var sb = new StringBuilder();
            byte[] buf = new byte[1024 * 4];

            if (proxyUser != null)
                WebRequest.DefaultWebProxy.Credentials = new NetworkCredential(proxyUser, proxyPw);

            var request = WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            using (var resStream = response.GetResponseStream())
            {
                string tempString = null;
                int count = 0;

                while (0 < (count = resStream.Read(buf, 0, buf.Length)))
                {
                    tempString = Encoding.ASCII.GetString(buf, 0, count);
                    sb.Append(tempString);
                }
                return sb.ToString();
            }
        }
    }
}