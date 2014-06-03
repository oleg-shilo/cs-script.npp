using CSScriptLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace CSScriptNpp
{
    public class CSScriptHelper
    {
        private static string consoleHostPath;
        private static string nppScriptsDir;

        private static string scriptDir;

        private static string vsDir;

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

        public static string VsDir
        {
            get
            {
                if (vsDir == null)
                    vsDir = Path.Combine(CSScript.GetScriptTempDir(), "NppScripts");
                return vsDir;
            }
        }

        static internal bool RunningAsAdmin
        {
            get
            {
                var p = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                return p.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static string ConsoleHostPath
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

        private static string NppScriptsDir
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

        static public void ClearVSDir()
        {
            try
            {
                string excludeDirPrefix = Path.Combine(VsDir, Process.GetCurrentProcess().Id.ToString()) + "-";

                if (Directory.Exists(VsDir))
                    foreach (string projectDir in Directory.GetDirectories(VsDir))
                    {
                        if (projectDir.StartsWith(excludeDirPrefix))
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

        static public void Execute(string scriptFile, Action<Process> onStart = null, Action<string> onStdOut = null)
        {
            string outputFile = null;
            try
            {
                var p = new Process();
                p.StartInfo.FileName = Path.Combine(Plugin.PluginDir, "cscs.exe");
                p.StartInfo.Arguments = "/nl /l /dbg " + GenerateProbingDirArg() + " \"" + scriptFile + "\"";

                bool needsElevation = !RunningAsAdmin && IsAsAdminScriptFile(scriptFile);
                bool useFileRedirection = false;

                if (needsElevation)
                {
                    p.StartInfo.Verb = "runas";
                    useFileRedirection = (onStdOut != null);

                    if (useFileRedirection)
                        onStdOut("WARNING: StdOut interception is impossible for elevated scripts. The whole output will be displayed at the end of the execution instead. Alternatively you can elevate Notepad++ process.");
                }

                if (onStdOut != null)
                {
                    if (useFileRedirection)
                    {
                        outputFile = Path.GetTempFileName();

                        string cmdText = string.Format("\"{0}\" {1} > \"{2}\"", p.StartInfo.FileName, p.StartInfo.Arguments, outputFile);

                        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = string.Format("/C \"{0}\"", cmdText);
                    }
                    else
                    {
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    }
                }

                p.Start();

                if (onStart != null)
                    onStart(p);

                var output = new StringBuilder();

                if (onStdOut != null && !useFileRedirection)
                {
                    string line = null;
                    while (null != (line = p.StandardOutput.ReadLine()))
                    {
                        output.AppendLine(line);
                        onStdOut(line);
                    }
                }
                p.WaitForExit();

                if (onStdOut != null && useFileRedirection)
                {
                    try
                    {
                        string outputText = File.ReadAllText(outputFile);
                        output.Append(outputText);
                        onStdOut(outputText);
                    }
                    catch { }
                }

                if (output.Length > 0 && output.ToString().StartsWith("Error: Specified file could not be compiled."))
                    throw new ApplicationException(output.ToString().Replace("csscript.CompilerException: ", ""));
            }
            finally
            {
                try
                {
                    if (outputFile != null && File.Exists(outputFile))
                        File.Delete(outputFile);
                }
                catch { }
            }
        }

        static public void ExecuteAsynch(string scriptFile)
        {
            string cscs = "\"" + Path.Combine(Plugin.PluginDir, "cscs.exe") + "\"";
            string script = "\"" + scriptFile + "\"";
            string args = string.Format("{0} /nl /l {1} {2}", cscs, GenerateProbingDirArg(), script);

            if (!RunningAsAdmin)
                ProcessStart(ConsoleHostPath, args, IsAsAdminScriptFile(scriptFile));
            else
                ProcessStart(ConsoleHostPath, args);
        }

        static public void ExecuteDebug(string scriptFileCmd)
        {
            ProcessStart("cmd.exe", "/K \"\"" + Path.Combine(Plugin.PluginDir, "cscs.exe") + "\" /nl /l /dbg " + GenerateProbingDirArg() + " \"" + scriptFileCmd + "\" //x\"");
        }

        static public Project GenerateProjectFor(string script)
        {
            var retval = new Project { PrimaryScript = script };

            var searchDirs = new List<string> { Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) };

            var parser = new ScriptParser(script, searchDirs.ToArray(), false);
            searchDirs.AddRange(parser.SearchDirs);        //search dirs could be also defined n the script
            searchDirs.AddRange(GetGlobalSearchDirs());

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

        static public string GetDbgInfoFile(string script, bool create = false)
        {
            string uniqueScriptHash = Path.GetFullPath(script).ToLower() //Win is not case-sensitive
                                                              .GetHashCode()
                                                              .ToString();

            var file = Path.Combine(CSScript.GetScriptTempDir(), "CSScriptNpp\\" + uniqueScriptHash + "\\" + Path.GetFileName(script) + ".dbg");

            if (create)
            {
                var dir = Path.GetDirectoryName(file);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(file))
                    File.WriteAllText(file, "");
            }

            return file;
        }

        public static DecorationInfo GetDecorationInfo(string file)
        {
            DecorationInfo retval = null;

            try
            {
                if (IsAutoGenFile(file))
                {
                    string originalScript = CSScriptHelper.GetOriginalFileName(file);
                    if (File.Exists(originalScript))
                        retval = new DecorationInfo
                        {
                            ScriptFile = originalScript,
                            AutoGenFile = file
                        };
                }
                else
                {
                    string entryFile = CSScriptHelper.GetEntryFileName(file);
                    if (IsAutoGenFile(entryFile))
                        retval = new DecorationInfo
                        {
                            ScriptFile = file,
                            AutoGenFile = entryFile
                        };
                }

                if (retval != null)
                {
                    string code = File.ReadAllText(retval.AutoGenFile);
                    Tuple<int, int> info = CSScriptIntellisense.CSScriptHelper.GetDecorationInfo(code);
                    retval.IngecionStart = info.Item1;
                    retval.IngecionLength = info.Item2;
                    retval.InjectedLineNumber = code.Substring(0, retval.IngecionStart).Split('\n').Count(); ;
                }
            }
            catch { retval = null; }

            return retval;
        }

        static public string GetEntryFileName(string scriptFile)
        {
            if (IsAutoClassScriptFile(scriptFile))
            {
                string cacheDir = Path.GetDirectoryName(CSScript.GetCachedScriptPath(scriptFile));
                return Path.Combine(cacheDir, Path.GetFileNameWithoutExtension(scriptFile) + ".g" + Path.GetExtension(scriptFile));
            }

            return scriptFile;
        }

        static public string GetLatestAvailableDistro(string version, string distroExtension, Action<long, long> onProgress)
        {
            try
            {
                string downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                string destFile = Path.Combine(downloadDir, "CSScriptNpp." + version + distroExtension);

                int numOfAlreadyDownloaded = Directory.GetFiles(downloadDir, "CSScriptNpp." + version + "*" + distroExtension).Count();
                if (numOfAlreadyDownloaded > 0)
                    destFile = Path.Combine(downloadDir, "CSScriptNpp." + version + " (" + (numOfAlreadyDownloaded + 1) + ")" + distroExtension);

                DownloadBinary("http://csscript.net/npp/CSScriptNpp." + version + distroExtension, destFile, onProgress);

                return destFile;
            }
            catch
            {
                return null;
            }
        }

        static public string GetLatestAvailableVersion()
        {
            try
            {
                string url = "http://csscript.net/npp/latest_version.txt";
#if DEBUG
                url = "http://csscript.net/npp/latest_version_dbg.txt";
#endif
                return DownloadText(url);
            }
            catch
            {
                return null;
            }
        }

        static public string GetOriginalFileName(string autogenFile)
        {
            string infoFile = Path.Combine(Path.GetDirectoryName(autogenFile), "css_info.txt");
            if (File.Exists(infoFile))
            {
                string originalDir = File.ReadAllLines(infoFile)[1];
                string originalFileName = Path.GetFileName(autogenFile.Substring(0, autogenFile.Length - ".g.cs".Length) + ".cs");
                return Path.Combine(originalDir, originalFileName);
            }

            return null;
        }

        static public bool IsAsAdminScriptFile(string file)
        {
            return HasDirective(file, IsAsAdminScriptDirective);
        }

        static public bool IsAutoClassScriptFile(string file)
        {
            return HasDirective(file, IsAutoclassScriptDirective);
        }

        static public bool HasDirective(string scriptFile, Predicate<string> lineChecker)
        {
            using (var file = new StreamReader(scriptFile))
            {
                string line = null;
                int count = 0;
                while ((line = file.ReadLine()) != null && count++ < 5) //5 lines should be enough
                    if (lineChecker(line))
                        return true;
                return false;
            }
        }

        static char[] lineWordDelimiters = new[] { ' ', '\t' }; //for singleLine

        static bool IsAutoclassScriptDirective(string text)
        {
            string[] words = text.Split(lineWordDelimiters, StringSplitOptions.RemoveEmptyEntries);
            return words.Any() && words.First() == "//css_args" && (words.Contains("/ac") || words.Contains("/ac,") || words.Contains("/ac;"));
        }

        static bool IsAsAdminScriptDirective(string text)
        {
            string[] words = text.Split(lineWordDelimiters, StringSplitOptions.RemoveEmptyEntries);
            return words.Any() && words.First() == "//css_npp" && (words.Contains("asadmin") || words.Contains("asadmin,") || words.Contains("asadmin;"));
        }


        static public string Isolate(string scriptFile, bool asScript, string targerRuntimeVersion)
        {
            string dir = Path.Combine(Path.GetDirectoryName(scriptFile), Path.GetFileNameWithoutExtension(scriptFile));

            EnsureCleanDirectory(dir); ;

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

        static public void OpenAsVSProjectFor(string script)
        {
            var searchDirs = new List<string> { ScriptsDir, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) };

            var parser = new ScriptParser(script, searchDirs.ToArray(), false);
            searchDirs.AddRange(parser.SearchDirs);        //search dirs could be also defined n the script
            searchDirs.AddRange(GetGlobalSearchDirs());

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

        static public string ScriptEngineVersion()
        {
            return FileVersionInfo.GetVersionInfo(Path.Combine(Plugin.PluginDir, "cscs.exe")).FileVersion.ToString();
        }

        static public bool Verify(string scriptFile)
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(Plugin.PluginDir, "cscs.exe");
            p.StartInfo.Arguments = "/nl /l /ca " + GenerateProbingDirArg() + " \"" + scriptFile + "\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            p.Start();

            var output = new StringBuilder();

            string line = null;
            while (null != (line = p.StandardOutput.ReadLine()))
            {
                output.AppendLine(line);
            }
            p.WaitForExit();

            string stdOutput = output.ToString();

            if (stdOutput.Contains("Error: Specified file could not be compiled."))
                return false;
            else
                return true;
        }

        private static void DownloadBinary(string url, string destinationPath, Action<long, long> onProgress = null, string proxyUser = null, string proxyPw = null)
        {
            var sb = new StringBuilder();
            byte[] buf = new byte[1024 * 4];

            if (proxyUser != null)
                WebRequest.DefaultWebProxy.Credentials = new NetworkCredential(proxyUser, proxyPw);

            var request = WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            using (var destStream = new FileStream(destinationPath, FileMode.CreateNew))
            using (var resStream = response.GetResponseStream())
            {
                int totalCount = 0;
                int count = 0;

                while (0 < (count = resStream.Read(buf, 0, buf.Length)))
                {
                    destStream.Write(buf, 0, count);

                    totalCount += count;
                    if (onProgress != null)
                        onProgress(totalCount, response.ContentLength);
                }
            }
        }

        private static string DownloadText(string url, string proxyUser = null, string proxyPw = null)
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

        //public static bool GetAutogeneratedScriptsMapping(ref string file, ref int line)
        //{
        //    string entryFile = CSScriptHelper.GetEntryFileName(file);
        private static void EnsureCleanDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }

                try { Directory.Delete(dir, true); }
                catch { } //OK if cannot delete as it is already empty.
            }
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private static string GenerateProbingDirArg()
        {
            string probingDirArg = "";

            if (NppScriptsDir != null)
                probingDirArg = "\"/dir:" + NppScriptsDir + "\"";

            return probingDirArg;
        }

        private static string[] GetGlobalSearchDirs()
        {
            var csscriptDir = Environment.GetEnvironmentVariable("CSSCRIPT_DIR");
            if (csscriptDir != null)
            {
                try
                {
                    var configFile = Path.Combine(csscriptDir, "css_config.xml");

                    if (File.Exists(configFile))
                    {
                        var doc = new XmlDocument();
                        doc.Load(configFile);

                        return doc.FirstChild.SelectSingleNode("searchDirs").InnerText.Split(';');
                    }
                }
                catch { }
            }
            return new string[0];
        }

        private static bool IsAutoGenFile(string file)
        {
            //auto-generated file in cache directory
            return file.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) && file.IndexOf(@"\CSSCRIPT\Cache\") != -1;
        }

        private static Process ProcessStart(string app, string args, bool elevated = false)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = app;
            startInfo.Arguments = args;

            if (elevated && !RunningAsAdmin)
                startInfo.Verb = "runas";

            return Process.Start(startInfo);
        }

        public class DecorationInfo
        {
            public string AutoGenFile;
            public int IngecionLength;
            public int IngecionStart;
            public int InjectedLineNumber;
            public string ScriptFile;
        }
    }

    public class Project
    {
        public string[] Assemblies;
        public string PrimaryScript;
        public string[] SourceFiles;
    }
}