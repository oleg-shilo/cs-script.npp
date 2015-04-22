using CSScriptIntellisense;
using CSScriptLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace CSScriptNpp
{
    public class CSScriptHelper
    {
        static string consoleHostPath;
        static string vsDir;
        static string scriptsDirectory;

        public static string ScriptsDir
        {
            get
            {
                if (scriptsDirectory == null)
                    scriptsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "C# Scripts");

                if (!Directory.Exists(scriptsDirectory))
                    Directory.CreateDirectory(scriptsDirectory);

                return scriptsDirectory;
            }
        }

        static string nppScriptsScriptsDir;

        public static string NppScripts_ScriptsDir  //NppScripts is another CS-Script related plugin 
        {
            get
            {
                if (nppScriptsScriptsDir == null)
                {
                    string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    nppScriptsScriptsDir = Path.Combine(rootDir, "NppScripts");
                }
                return nppScriptsScriptsDir;
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

        public static string CSScriptTempDir
        {
            get
            {
                if (vsDir == null)
                    vsDir = Path.Combine(CSScript.GetScriptTempDir(), "CSScriptNpp");
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

        static public void Build(string scriptFileCmd)
        {
            string compilerOutput;
            bool success = Build(scriptFileCmd, out compilerOutput);
            if (!success)
                throw new ApplicationException(compilerOutput.Replace("csscript.CompilerException: ", "")); //for a better appearance remove CS-Script related stuff
        }

        static public string[] GetCodeCompileOutput(string scriptFile)
        {
            string compilerOutput;
            bool success = Build(scriptFile, out compilerOutput);
            if (!success)
                return compilerOutput.Replace("csscript.CompilerException: ", "").Split('\n');
            else
                return new string[0];
        }

        static bool Build(string scriptFileCmd, out string compilerOutput)
        {
            string oldNotificationMessage = null;
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                var p = new Process();
                p.StartInfo.FileName = Path.Combine(Plugin.PluginDir, "cscs.exe");
                p.StartInfo.Arguments = "/nl /ca " + GenerateDefaultArgs() + " \"" + scriptFileCmd + "\"";

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
                    if (line.Contains("NuGet") && NotifyClient != null)
                    {
                        oldNotificationMessage = NotifyClient("Processing NuGet packages...");
                    }

                    if (line.ToString().StartsWith("Error: Specified file could not be compiled."))
                    {
                        error = true;
                        continue;
                    }

                    if (!string.IsNullOrEmpty(line) && !line.Contains("at csscript.CSExecutor."))
                        output.AppendLine(line);
                }
                p.WaitForExit();

                compilerOutput = output.ToString();
                return !error;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                if (oldNotificationMessage != null && NotifyClient != null)
                    NotifyClient(oldNotificationMessage);
            }
        }

        static public void ClearVSDir()
        {
            try
            {
                string excludeDirPrefix = Path.Combine(VsDir, Process.GetCurrentProcess().Id.ToString()) + "-";

                if (Directory.Exists(VsDir))
                {
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

                    if (Directory.Exists(CSScriptTempDir))
                    {
                        foreach (string dir in Directory.GetDirectories(CSScriptTempDir))
                        {
                            foreach (string file in Directory.GetFiles(dir, "*.cs.dbg"))
                            {
                                try
                                {
                                    //#script: E:\Dropbox\Public\Support\test.cs
                                    //E:\Dropbox\Public\Support\test.cs|20

                                    string sourceFile = File.ReadAllLines(file).First().Split(new[] { ':' }, 2).First();
                                    if (!File.Exists(sourceFile))
                                        File.Delete(file);
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        static public void Execute(string scriptFile, Action<Process> onStart = null, Action<char[]> onStdOut = null)
        {
            string outputFile = null;
            try
            {
                var p = new Process();
                p.StartInfo.FileName = Path.Combine(Plugin.PluginDir, "cscs.exe");
                p.StartInfo.Arguments = "/nl /l /dbg " + GenerateDefaultArgs() + " \"" + scriptFile + "\"";

                bool needsElevation = !RunningAsAdmin && IsAsAdminScriptFile(scriptFile);
                bool useFileRedirection = false;

                if (needsElevation)
                {
                    p.StartInfo.Verb = "runas";
                    useFileRedirection = (onStdOut != null);

                    if (useFileRedirection)
                        onStdOut("WARNING: StdOut interception is impossible for elevated scripts. The whole output will be displayed at the end of the execution instead. Alternatively you can elevate Notepad++ process.".ToCharArray());
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
                    char[] buf = new char[1];
                    while (0 != p.StandardOutput.Read(buf, 0, 1))
                    {
                        output.Append(buf[0]);
                        onStdOut(buf);
                    }
                }
                p.WaitForExit();

                if (onStdOut != null && useFileRedirection)
                {
                    try
                    {
                        string outputText = File.ReadAllText(outputFile);
                        output.Append(outputText);
                        onStdOut(outputText.ToCharArray());
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
            string debugFlag = "/dbg ";
            if (!Config.Instance.RunExternalInDebugMode)
                debugFlag = " ";

            string args = string.Format("{0} /nl {3}/l {1} {2}", cscs, GenerateDefaultArgs(), script, debugFlag);

            if (!RunningAsAdmin)
                ProcessStart(ConsoleHostPath, args, IsAsAdminScriptFile(scriptFile));
            else
                ProcessStart(ConsoleHostPath, args);
        }

        static public void ExecuteDebug(string scriptFileCmd)
        {
            ProcessStart("cmd.exe", "/K \"\"" + Path.Combine(Plugin.PluginDir, "cscs.exe") + "\" /nl /l /dbg " + GenerateDefaultArgs() + " \"" + scriptFileCmd + "\" //x\"");
        }

        static public Func<string, string> NotifyClient;

        static public Project GenerateProjectFor(string script)
        {
            string oldNotificationMessage = null;

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var retval = new Project { PrimaryScript = script };

                var searchDirs = new List<string>();
                //searchDirs.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                searchDirs.AddRange(CSScriptIntellisense.CSScriptHelper.GetGlobalSearchDirs());

                var parser = new ScriptParser(script, searchDirs.ToArray(), false);

                searchDirs.AddRange(parser.SearchDirs);        //search dirs could be also defined n the script
                searchDirs.RemoveEmptyAndDulicated();

                var sources = parser.SaveImportedScripts().ToList(); //this will also generate auto-scripts and save them
                sources.Insert(0, script);
                retval.SourceFiles = sources.ToArray();

                if (parser.Packages.Any() && NotifyClient != null)
                {
                    oldNotificationMessage = NotifyClient("Processing NuGet packages...");
                }

                retval.Assemblies = parser.AgregateReferences(searchDirs).ToArray();
                return retval;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                if (oldNotificationMessage != null && NotifyClient != null)
                    NotifyClient(oldNotificationMessage);
            }
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
                if (file != null)
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

        static public bool IsSurrogateHosted(string file, ref bool isX86)
        {
            bool isPlatform86 = false;

            bool result = HasDirective(file, line =>
                {
                    if (line.Trim().StartsWith("//css_host "))
                    {
                        isPlatform86 = line.Contains("/platform:x86");
                        return true;
                    }
                    else
                        return false;
                });
            isX86 = isPlatform86;
            return result;
        }

        static public bool HasDirective(string scriptFile, Predicate<string> lineChecker)
        {
            using (var file = new StreamReader(scriptFile))
            {
                string line = null;
                int count = 0;
                while ((line = file.ReadLine()) != null && count++ < 15) //lines should be enough
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

        static public string Isolate(string scriptFile, bool asScript, string targerRuntimeVersion, bool windowApp)
        {
            string dir = Path.Combine(Path.GetDirectoryName(scriptFile), Path.GetFileNameWithoutExtension(scriptFile));

            EnsureCleanDirectory(dir);

            string engineFileName;
            if (targerRuntimeVersion == "v4.0.30319")
                engineFileName = "cscs.exe";
            else if (targerRuntimeVersion == "v2.0.50727")
                engineFileName = "cscs.v3.5.exe";
            else
                throw new Exception("The requested Target Runtime version (" + targerRuntimeVersion + ") is not supported.");

            Project proj = GenerateProjectFor(scriptFile);
            var assemblies = proj.Assemblies.Where(a => !a.Contains("GAC_MSIL")).ToArray();
            Action<string, string> copy = (file, directory) => File.Copy(file, Path.Combine(directory, Path.GetFileName(file)), true);

            if (asScript)
            {
                string engine = Path.Combine(Plugin.PluginDir, engineFileName);

                proj.SourceFiles.Concat(assemblies)
                                .Concat(engine)
                                .ForEach(file => copy(file, dir));

                string batchFile = Path.Combine(dir, "run.cmd");
                File.WriteAllText(batchFile, "cscs.exe \"" + Path.GetFileName(scriptFile) + "\"\r\npause");

                return dir;
            }
            else
            {
                string srcExe = Path.ChangeExtension(scriptFile, ".exe");
                string destExe = Path.Combine(dir, Path.GetFileName(srcExe));

                string script = "\"" + scriptFile + "\"";

                string cscs = Path.Combine(Plugin.PluginDir, engineFileName);
                string args = string.Format("/e{2} {0} {1}", GenerateDefaultArgs(), script, (windowApp ? "w" : ""));

                var p = new Process();
                p.StartInfo.FileName = cscs;
                p.StartInfo.Arguments = args;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();

                if (File.Exists(srcExe))
                {
                    assemblies.Concat(srcExe)
                              .ForEach(file => copy(file, dir));

                    File.Delete(srcExe);
                    return dir;
                }
                else
                {
                    //just show why building has failed
                    cscs = "\"" + cscs + "\"";
                    args = string.Format("{0} /e  {1} {2}", cscs, GenerateDefaultArgs(), script);
                    Process.Start(ConsoleHostPath, args);
                    return null;
                }
            }
        }

        static public void OpenAsVSProjectFor(string script)
        {
            var searchDirs = new List<string>();
            //searchDirs.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            searchDirs.AddRange(CSScriptIntellisense.CSScriptHelper.GetGlobalSearchDirs());

            var parser = new ScriptParser(script, searchDirs.ToArray(), false);
            searchDirs.AddRange(parser.SearchDirs);        //search dirs could be also defined in the script

            if (NppScripts_ScriptsDir != null)
                searchDirs.Add(NppScripts_ScriptsDir);

            IList<string> sourceFiles = parser.SaveImportedScripts().ToList(); //this will also generate auto-scripts and save them
            sourceFiles.Add(script);

            //some assemblies are referenced from code and some will need to be resolved from the namespaces
            var refAsms = parser.ReferencedNamespaces
                                .Where(name => !parser.IgnoreNamespaces.Contains(name))
                                .SelectMany(name => AssemblyResolver.FindAssembly(name, searchDirs.ToArray()))
                                .Union(parser.ResolvePackages(suppressDownloading: true)) //it is not the first time we are loading the script so we already tried to download the packages
                                .Union(parser.ReferencedAssemblies
                                             .SelectMany(asm => AssemblyResolver.FindAssembly(asm.Replace("\"", ""), searchDirs.ToArray())))
                                .Distinct()
                                .ToArray();

            string dir = Path.Combine(VsDir, Process.GetCurrentProcess().Id.ToString()) + "-" + script.GetHashCode();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string projectName = Path.GetFileNameWithoutExtension(script);
            string projectFile = Path.Combine(dir, projectName + ".csproj");
            string solutionFile = Path.Combine(dir, projectName + ".sln");

            string sourceFilesXml = string.Join(Environment.NewLine,
                                                sourceFiles.Select(file => "<Compile Include=\"" + file + "\" />").ToArray());

            string refAsmsXml = string.Join(Environment.NewLine,
                                            refAsms.Select(file => "<Reference Include=\"" + Path.GetFileNameWithoutExtension(file) + "\"><HintPath>" + file + "</HintPath></Reference>").ToArray());

            File.WriteAllText(projectFile,
                              Resources.Resources.VS2012ProjectTemplate
                                                 .Replace("<UseVSHostingProcess>false", "<UseVSHostingProcess>true") // to ensure *.vshost.exe is created
                                                 .Replace("<OutputType>Library", "<OutputType>Exe")
                                                 .Replace("{$SOURCES}", sourceFilesXml)
                                                 .Replace("{$REFERENCES}", refAsmsXml));

            File.WriteAllText(solutionFile,
                              Encoding.UTF8.GetString(Resources.Resources.VS2012SolutionTemplate)
                                      .Replace("{$PROJECTNAME}", projectName));

            Process.Start(solutionFile);
        }

        static public string ScriptEngineVersion()
        {
            return FileVersionInfo.GetVersionInfo(Path.Combine(Plugin.PluginDir, "cscs.exe")).FileVersion.ToString();
        }

        static public bool Verify(string scriptFile)
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(Plugin.PluginDir, "cscs.exe");
            //NOTE: it is important to always pass /dbg otherwise NPP will not be able to debug.
            //particularly important to do for //css_host scripts
            p.StartInfo.Arguments = "/nl /l /dbg /ca " + GenerateDefaultArgs() + " \"" + scriptFile + "\"";
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

        static void DownloadBinary(string url, string destinationPath, Action<long, long> onProgress = null, string proxyUser = null, string proxyPw = null)
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

        static void EnsureCleanDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }
            }
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        internal static string GenerateDefaultArgs()
        {
            return GenerateConfigFileExecutionArg() + GenerateProbingDirArg();
        }

        static string GenerateConfigFileExecutionArg()
        {
            string result = "";
            string globalConfig = CSScriptIntellisense.CSScriptHelper.GetGlobalCSSConfig();
            if (globalConfig != null)
                result = " \"/noconfig:" + globalConfig + "\"";

            return result;
        }

        static string GenerateProbingDirArg()
        {
            string probingDirArg = "";

            if (NppScripts_ScriptsDir != null)
                probingDirArg = " \"/dir:" + NppScripts_ScriptsDir + "\"";

            return probingDirArg;
        }

        static string[] GetGlobalSearchDirs_tobe_removed()
        {
            var csscriptDir = Environment.GetEnvironmentVariable("CSSCRIPT_DIR");
            if (csscriptDir != null)
            {
                var dirs = new List<string>();
                dirs.Add(Environment.ExpandEnvironmentVariables("%CSSCRIPT_DIR%\\Lib"));

                try
                {
                    var configFile = Path.Combine(csscriptDir, "css_config.xml");

                    if (File.Exists(configFile))
                    {
                        var doc = new XmlDocument();
                        doc.Load(configFile);
                        dirs.AddRange(doc.FirstChild
                                         .SelectSingleNode("searchDirs")
                                         .InnerText.Split(';')
                                         .Select(x => Environment.ExpandEnvironmentVariables(x)));
                    }
                }
                catch { }
                return dirs.ToArray();
            }
            return new string[0];
        }

        static bool IsAutoGenFile(string file)
        {
            //auto-generated file in cache directory
            return file.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) && file.IndexOf(@"\CSSCRIPT\Cache\") != -1;
        }

        static Process ProcessStart(string app, string args, bool elevated = false)
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

    static class GenericExtensions
    {
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> collection, T item)
        {
            return collection.Concat(new[] { item });
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
            return collection;
        }
    }

    public class Project
    {
        public string[] Assemblies;
        public string PrimaryScript;
        public string[] SourceFiles;
    }
}
