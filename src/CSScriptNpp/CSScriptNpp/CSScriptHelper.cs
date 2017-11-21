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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CSScriptNpp
{
    public class CSScriptHelper
    {
        static string consoleHostPath;
        static string vsDir;
        //static string scriptsDirectory;

        public static string ScriptsDir
        {
            get
            {
                string scriptDir = Config.Instance.ScriptsDir;
                try
                {
                    if (!Directory.Exists(scriptDir))
                        Directory.CreateDirectory(scriptDir);
                }
                catch
                {
                    scriptDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NppScripts");
                    if (!Directory.Exists(scriptDir))
                        Directory.CreateDirectory(scriptDir);
                    Config.Instance.ScriptsDir = scriptDir;
                    Config.Instance.Save();
                }
                return scriptDir;
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

        public static string SystemCSScriptDir
        {
            get
            {
                return Environment.GetEnvironmentVariable("CSSCRIPT_DIR");
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

        static public void Build(string scriptFile)
        {
            string compilerOutput;
            bool success = Build(scriptFile, out compilerOutput);
            if (!success)
                throw new ApplicationException(compilerOutput.RemoveNonUserCompilingInfo());
        }

        static public string[] GetCodeCompileOutput(string scriptFile)
        {
            string compilerOutput;
            bool success = Build(scriptFile, out compilerOutput);
            if (!success)
                return compilerOutput.RemoveNonUserCompilingInfo().Split('\n');
            else
                return new string[0];
        }

        internal static void SynchAutoclssDecorationSettings(bool supportCS6Syntax)
        {
            try
            {
                //AutoClass_DecorateAsCS6: False
                var output = Run(cscs_exe, "-config:get:AutoClass_DecorateAsCS6");
                bool injectingCS6_enabled = bool.Parse(output.Split(':').Last());
                if (supportCS6Syntax != injectingCS6_enabled)
                {
                    Call(cscs_exe, "-config:set:AutoClass_DecorateAsCS6:" + supportCS6Syntax, asAdmin: true);
                }
            }
            catch { } //failure is not critical as future script compile errors will be informative anyway
        }

        static string Run(string file, string args)
        {
            var p = new Process();
            p.StartInfo.FileName = file;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            p.Start();

            var output = new StringBuilder();

            string line = null;
            while (null != (line = p.StandardOutput.ReadLine()))
                output.AppendLine(line);

            p.WaitForExit();
            return output.ToString();
        }

        static void Call(string file, string args, bool asAdmin = false)
        {
            var p = new Process();
            p.StartInfo.FileName = file;
            p.StartInfo.Arguments = args;
            if (asAdmin)
            {
                p.StartInfo.Verb = "runas";
                p.StartInfo.UseShellExecute = true;
            }
            else
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
            }
            p.Start();
            p.WaitForExit();
        }

        static string ScriptEngineLocation
        {
            get
            {
                try
                {
                    if (!Config.Instance.UseEmbeddedEngine)
                    {
                        var dir = Config.Instance.UseCustomEngine;
                        if (dir.IsEmpty())
                            dir = "%CSSCRIPT_DIR%";

                        dir = Environment.ExpandEnvironmentVariables(dir);

                        if (Directory.Exists(dir))
                        {
                            return dir;
                        }
                        else
                        {
                            Config.Instance.UseEmbeddedEngine = true;
                            Config.Instance.Save();
                        }
                    }
                }
                catch { }
                return "";
            }
        }

        internal static string cscs_exe
        {
            get
            {
                string name = "cscs.exe";
                var file = Path.Combine(ScriptEngineLocation, name);
                if (File.Exists(file))
                    return file;
                else
                    return Path.Combine(Plugin.PluginDir, name);
            }
        }

        internal static string css_exe
        {
            get
            {
                string name = "css.exe";
                var file = Path.Combine(ScriptEngineLocation, name);
                if (File.Exists(file))
                    return file;
                else
                    return Path.Combine(Plugin.PluginDir, name);
            }
        }

        internal static string csws_exe
        {
            get
            {
                string name = "csws.exe";
                var file = Path.Combine(ScriptEngineLocation, name);
                if (File.Exists(file))
                    return file;
                else
                    return Path.Combine(Plugin.PluginDir, name);
            }
        }

        internal static string cscs_v35_exe
        {
            get
            {
                var file = Path.Combine(ScriptEngineLocation, @"lib\Bin\NET 3.5\cscs.exe");
                if (File.Exists(file))
                    return file;
                else
                    return Path.Combine(Plugin.PluginDir, "cscs.v3.5.exe");
            }
        }

        static bool Build(string scriptFile, out string compilerOutput)
        {
            string oldNotificationMessage = null;
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                var p = new Process();
                p.StartInfo.FileName = cscs_exe;
                p.StartInfo.Arguments = "-ca " + GenerateDefaultArgs(scriptFile) + " \"" + scriptFile + "\"";

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

                compilerOutput = output.ToString().RemoveNonUserCompilingInfo();

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
                p.StartInfo.FileName = cscs_exe;
                p.StartInfo.Arguments = "-l -d " + GenerateDefaultArgs(scriptFile) + " \"" + scriptFile + "\"";

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
                    //string line;
                    //while (null != (line=p.StandardOutput.ReadLine()))
                    //{
                    //    output.AppendLine(line);
                    //    onStdOut(line.ToCharArray());
                    //}
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
                    throw new ApplicationException(output.ToString().RemoveNonUserCompilingInfo());
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

        static System.Windows.Forms.Timer keepRoslynLoadedTimer;

        static public void InitRoslyn()
        {
            if (keepRoslynLoadedTimer == null)
            {
                LoadRoslyn();

                keepRoslynLoadedTimer = new Timer();
                keepRoslynLoadedTimer.Interval = 1000 * 60 * 9; //9 min
                keepRoslynLoadedTimer.Tick += (s, e) =>
                                                {
                                                    LoadRoslyn();
                                                };
                keepRoslynLoadedTimer.Enabled = true;
                keepRoslynLoadedTimer.Start();
            }
        }

        static public void LoadRoslyn()
        {
            //disabled as unreliable; it can even potentially crash csc.exe if MS CodeAnalysis asms are probed incorrectly
            try
            {
                string file = Path.Combine(Path.GetTempPath(), "load_roslyn.cs");

                File.WriteAllText(file,
@"
using System;
using System.Windows.Forms;

class Script
{
    [STAThread]
    static public void Main(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine(args[i]);
        }
    }
}");
                File.SetLastWriteTimeUtc(file, DateTime.Now.ToUniversalTime());

                string args = string.Format("-d -l {0} \"{1}\"", GenerateDefaultArgs("code.cs"), file);
                //Process.Start(csws_exe, args);
                var p = new Process();
                p.StartInfo.FileName = cscs_exe;
                p.StartInfo.Arguments = args;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
            }
            catch { }
        }

        static public void ExecuteAsynch(string scriptFile)
        {
            string cscs = "\"" + cscs_exe + "\"";
            string script = "\"" + scriptFile + "\"";
            string debugFlag = "-d ";
            if (!Config.Instance.RunExternalInDebugMode)
                debugFlag = " ";

            string args;
            string host;

            if (!Config.Instance.CustomAsyncHost.IsEmpty())
            {
                host = Config.Instance.CustomAsyncHost;
                if (!Path.IsPathRooted(host))
                    host = Path.Combine(Path.GetDirectoryName(cscs_exe), host);
                args = string.Format("/nl {2}/l {0} {1}", GenerateDefaultArgs(scriptFile), script, debugFlag);
            }
            else
            {
                host = cscs;
                args = string.Format("-wait /nl {2}/l {0} {1}", GenerateDefaultArgs(scriptFile), script, debugFlag);
                //host = ConsoleHostPath;
                //args = string.Format("{0} /nl {3}/l {1} {2}", cscs, GenerateDefaultArgs(scriptFile), script, debugFlag);
            }

            if (!RunningAsAdmin)
                ProcessStart(host, args, IsAsAdminScriptFile(scriptFile));
            else
                ProcessStart(host, args);
        }

        static public void ExecuteDebug(string scriptFileCmd)
        {
            ProcessStart("cmd.exe", "/K \"\"" + cscs_exe + "\" -l -d " + GenerateDefaultArgs(scriptFileCmd) + " \"" + scriptFileCmd + "\" //x\"");
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
                searchDirs.Add(Path.GetDirectoryName(script));

                var globalConfig = CSScriptIntellisense.CSScriptHelper.GetGlobalConfigItems();
                string[] defaultSearchDirs = globalConfig.Item1;
                string[] defaultRefAsms = globalConfig.Item2;
                string[] defaultNamespaces = globalConfig.Item3;

                searchDirs.AddRange(defaultSearchDirs);

                ScriptParser parser;
                string currDir = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = Path.GetDirectoryName(script);
                    parser = new ScriptParser(script, searchDirs.ToArray(), false);
                }
                finally
                {
                    Environment.CurrentDirectory = currDir;
                }

                searchDirs.AddRange(parser.SearchDirs);        //search dirs could be also defined n the script
                searchDirs.RemoveEmptyAndDulicated();

                var sources = parser.SaveImportedScripts().ToList(); //this will also generate auto-scripts and save them
                sources.Insert(0, script);
                retval.SourceFiles = sources.Distinct().ToArray();

                if (parser.Packages.Any() && NotifyClient != null)
                {
                    oldNotificationMessage = NotifyClient("Processing NuGet packages...");
                }

                if (Config.Instance.HideDevaultAssemblies)
                    retval.Assemblies = parser.AgregateReferences(searchDirs, new string[0], new string[0]).ToArray();
                else
                    retval.Assemblies = parser.AgregateReferences(searchDirs, defaultRefAsms, defaultNamespaces).ToArray();

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
                string downloadDir = KnownFolders.UserDownloads;

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

        static public string GetLatestReleaseInfo(string version)
        {
            try
            {
                string url = "http://csscript.net/npp/CSScriptNpp." + version + ".ReleaseInfo.txt";

                string text = DownloadText(url);
                if (text.Trim().StartsWith("<html>"))
                    text = "Complete release notes can be found here:\r\n\r\n" + url.Replace("ReleaseInfo", "ReleaseNotes");

                return text;
            }
            catch
            {
                return "";
            }
        }

        static public string GetLatestAvailableVersion()
        {
            bool update_always = Environment.GetEnvironmentVariable("CSSCRIPT_NPP_UPDATE_ALWAYS") != null;
            string url = Environment.GetEnvironmentVariable("CSSCRIPT_NPP_REPO_URL") ?? "http://csscript.net/npp/latest_version.txt";
#if DEBUG
            url = "http://csscript.net/npp/latest_version_dbg.txt";
#endif
            string stableVersion = GetLatestAvailableVersion(url);

            if (Config.Instance.CheckPrereleaseUpdates)
            {
                string prereleaseVersion = GetLatestAvailableVersion(url.Replace(".txt", ".pre.txt"));
                if (stableVersion.IsEmpty())
                {
                    return prereleaseVersion;
                }
                if (prereleaseVersion.IsEmpty())
                {
                    return stableVersion;
                }
                else
                {
                    try
                    {
                        if (Version.Parse(prereleaseVersion) > Version.Parse(stableVersion))
                            return prereleaseVersion;
                        else
                            return stableVersion;
                    }
                    catch { }
                }
            }

            return stableVersion;
        }

        static public string GetLatestAvailableVersion(string url)
        {
            try
            {
                return DownloadText(url).Trim();
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
            return words.Any() && words.First() == "//css_args" && (words.Contains("/ac") || words.Contains("/ac,") || words.Contains("/ac;") ||
                                                                    words.Contains("-ac") || words.Contains("-ac,") || words.Contains("-ac;"));
        }

        static bool IsAsAdminScriptDirective(string text)
        {
            string[] words = text.Split(lineWordDelimiters, StringSplitOptions.RemoveEmptyEntries);
            return words.Any() && words.First() == "//css_npp" && (words.Contains("asadmin") || words.Contains("asadmin,") || words.Contains("asadmin;"));
        }

        static public void CreateProviderWarningFileIfNeeded(string destDir, string scriptFile)
        {
            var executionArg = GenerateNppExecutionArg(scriptFile);

            var warning = new StringBuilder();
            if (scriptFile.IsVbFile())
            {
                warning.AppendLine("Your script requires a custom code compiler (Code Provider) for handling VB.NET syntax.");
                warning.AppendLine("It means that you will need to distribute the provider file(s) along with the script.");
            }
            else if (Config.Instance.UseRoslynProvider)
            {
                warning.AppendLine("Notepad++ CS-Script plugin is configured to use Roslyn as a compiler. This usually indicates that " +
                                   "your script execution requires Roslyn code provider (e.g. to handle C#6 syntax).");
                warning.AppendLine("If indeed it is case then you will need to distribute the provider file(s) along with the script.");
            }

            if (warning.Length > 0)
            {
                warning.AppendLine("You will also need to modify run.cmd as follows:");
                warning.AppendLine();
                warning.AppendLine($"cscs.exe {executionArg} \"{Path.GetFileName(scriptFile)}\"");
                warning.AppendLine();
                warning.AppendLine("Also make sure the provider's TargetRuntime is compatible with the runtime of the target system.");
                File.WriteAllText(Path.Combine(destDir, "warning.txt"), warning.ToString());
            }
        }

        static public string Isolate(string scriptFile, bool asScript, string targerRuntimeVersion, bool windowApp)
        {
            string dir = Path.Combine(Path.GetDirectoryName(scriptFile), Path.GetFileNameWithoutExtension(scriptFile));

            EnsureCleanDirectory(dir);

            bool net4 = (targerRuntimeVersion == "v4.0.30319");
            bool net2 = (targerRuntimeVersion == "v2.0.50727");

            string engineFile;
            if (net4)
                engineFile = cscs_exe;
            else if (net2)
                engineFile = cscs_v35_exe;
            else
                throw new Exception("The requested Target Runtime version (" + targerRuntimeVersion + ") is not supported.");

            Project proj = GenerateProjectFor(scriptFile);
            var assemblies = proj.Assemblies.Where(a => !a.Contains("GAC_MSIL")).ToArray();
            Action<string, string> copy = (file, directory) => File.Copy(file, Path.Combine(directory, Path.GetFileName(file)), true);

            if (asScript)
            {
                proj.SourceFiles.Concat(assemblies)
                                .Concat(engineFile)
                                .ForEach(file => copy(file, dir));

                string batchFile = Path.Combine(dir, "run.cmd");
                string engineName = Path.GetFileName(engineFile);

                if (scriptFile.IsVbFile())
                {
                    if (net4 && Config.Instance.VbCodeProvider.EndsWith("CSSCodeProvider.v4.0.dll"))
                    {
                        //single file code provider
                        var providerSrc = Path.Combine(Plugin.PluginDir, Config.Instance.VbCodeProvider);
                        var providerDest = providerSrc.PathChangeDir(dir);
                        File.Copy(providerSrc, providerDest);
                    }
                    else
                    {
                        //code provider potentially is a set of many files of a substantial size (e.g. Roslyn)
                        CreateProviderWarningFileIfNeeded(dir, scriptFile);
                    }

                    File.WriteAllText(batchFile, $"echo off\r\n{engineName} {GenerateNppExecutionArg(scriptFile)} \"{Path.GetFileName(scriptFile)}\"\r\npause");
                }
                else
                {
                    CreateProviderWarningFileIfNeeded(dir, scriptFile);
                    File.WriteAllText(batchFile, $"echo off\r\n{engineName} \"{Path.GetFileName(scriptFile)}\"\r\npause");
                }

                return dir;
            }
            else
            {
                string srcExe = Path.ChangeExtension(scriptFile, ".exe");
                string destExe = Path.Combine(dir, Path.GetFileName(srcExe));

                string script = "\"" + scriptFile + "\"";

                string cscs = engineFile;
                string args = string.Format("-e{2} {0} {1}", GenerateDefaultArgs(scriptFile), script, (windowApp ? "w" : ""));

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
                    args = string.Format("{0} -e  {1} {2}", cscs, GenerateDefaultArgs(scriptFile), script);
                    Process.Start(ConsoleHostPath, args);
                    return null;
                }
            }
        }

        static public void OpenAsVSProjectFor(string script)
        {
            var globalConfig = CSScriptIntellisense.CSScriptHelper.GetGlobalConfigItems();
            string[] defaultSearchDirs = globalConfig.Item1;
            string[] defaultRefAsms = globalConfig.Item2;
            string[] defaultNamespaces = globalConfig.Item3;

            var searchDirs = new List<string>();
            searchDirs.Add(Path.GetDirectoryName(script));

            searchDirs.AddRange(defaultSearchDirs);

            var parser = new ScriptParser(script, searchDirs.ToArray(), false);
            searchDirs.AddRange(parser.SearchDirs);        //search dirs could be also defined in the script

            if (NppScripts_ScriptsDir != null)
                searchDirs.Add(NppScripts_ScriptsDir);

            IList<string> sourceFiles = parser.SaveImportedScripts().ToList(); //this will also generate auto-scripts and save them
            sourceFiles.Add(script);
            sourceFiles = sourceFiles.Distinct().ToArray();

            //some assemblies are referenced from code and some will need to be resolved from the namespaces
            bool disableNamespaceResolving = (parser.IgnoreNamespaces.Count() == 1 && parser.IgnoreNamespaces[0] == "*");

            var refAsms = parser.ReferencedNamespaces
                                .Union(defaultNamespaces)
                                .Where(name => !disableNamespaceResolving && !parser.IgnoreNamespaces.Contains(name))
                                .SelectMany(name => AssemblyResolver.FindAssembly(name, searchDirs.ToArray()))
                                .Union(parser.ResolvePackages(suppressDownloading: true)) //it is not the first time we are loading the script so we already tried to download the packages
                                .Union(parser.ReferencedAssemblies
                                             .SelectMany(asm => AssemblyResolver.FindAssembly(asm.Replace("\"", ""), searchDirs.ToArray())))
                                .Union(defaultRefAsms)
                                .Distinct()
                                .ToArray();

            string dir = Path.Combine(VsDir, Process.GetCurrentProcess().Id.ToString()) + "-" + script.GetHashCode();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string projectName = Path.GetFileNameWithoutExtension(script);
            string projectFile = Path.Combine(dir, projectName + ".csproj");
            string solutionFile = Path.Combine(dir, projectName + ".sln");
            string scriptDir = Path.GetDirectoryName(script);

            string sourceFilesXml = string.Join(Environment.NewLine,
                                                sourceFiles.Select(file => "<Compile Include=\"" + file + "\" />").ToArray());

            string refAsmsXml = string.Join(Environment.NewLine,
                                            refAsms.Select(file => "<Reference Include=\"" + Path.GetFileNameWithoutExtension(file) + "\"><HintPath>" + file + "</HintPath></Reference>").ToArray());

            File.WriteAllText(projectFile,
                              File.ReadAllText(GetProjectTemplate())
                                  .Replace("<UseVSHostingProcess>false", "<UseVSHostingProcess>true") // to ensure *.vshost.exe is created
                                  .Replace("<OutputType>Library", "<OutputType>Exe")
                                  .Replace("{$SOURCES}", sourceFilesXml)
                                  .Replace("{$REFERENCES}", refAsmsXml));

            File.WriteAllText(projectFile + ".user",
                              Resources.Resources.VS2012UserTemplate
                                                 .Replace("<StartWorkingDirectory>",
                                                          "<StartWorkingDirectory>" + scriptDir));

            File.WriteAllText(solutionFile,
                              Encoding.UTF8.GetString(Resources.Resources.VS2012SolutionTemplate)
                                           .Replace("{$PROJECTNAME}", projectName));

            Process.Start(solutionFile);
        }

        internal static string GetProjectTemplate()
        {
            string projTemplateFile = Config.Instance.VSProjectTemplatePath;

            if (projTemplateFile.IsEmpty() || !File.Exists(projTemplateFile))

            {
                Config.Instance.VSProjectTemplatePath =
                projTemplateFile = Path.Combine(VsDir, "VisualStudio.csproj.template.txt");

                if (!File.Exists(projTemplateFile))
                    File.WriteAllText(projTemplateFile, Resources.Resources.VS2012ProjectTemplate);

                Config.Instance.Save();
            }

            return projTemplateFile;
        }

        static public string ScriptEngineVersion()
        {
            return FileVersionInfo.GetVersionInfo(cscs_exe).FileVersion.ToString();
        }

        static public bool Verify(string scriptFile)
        {
            var p = new Process();
            p.StartInfo.FileName = cscs_exe;
            //NOTE: it is important to always pass /d otherwise NPP will not be able to debug.
            //particularly important to do for //css_host scripts
            p.StartInfo.Arguments = "-l -d -ca " + GenerateDefaultArgs(scriptFile) + " \"" + scriptFile + "\"";
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

            string stdOutput = output.ToString().RemoveNonUserCompilingInfo();

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

            if (File.ReadAllText(destinationPath).Contains("Error 404"))
                throw new Exception($"Resource {url} cannot be downloaded.");
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

        internal static string GenerateDefaultArgs(string scriptFile)
        {
            return GenerateConfigFileExecutionArg() + GenerateProbingDirArg() + GenerateNppExecutionArg(scriptFile);
        }

        static string GenerateNppExecutionArg(string scriptFile)
        {
            string result = "";

            var language = Path.GetExtension(scriptFile).Replace(".", "").ToUpper();

            if (language == "VB" && CSScriptIntellisense.Config.Instance.VbSupportEnabled)
            {
                //only CSSCodeProvider.v4.0.dll can support VB
                var provider = Plugin.PluginDir.PathJoin(Config.Instance.VbCodeProvider);
                result = " \"-provider:" + provider + "\"";
            }
            else if (Config.Instance.UseRoslynProvider)
            {
                var provider = Plugin.PluginDir.PathJoin("CSSRoslynProvider.dll");
                result = " \"-provider:" + provider + "\"";
            }

            return result;
        }

        static string GenerateConfigFileExecutionArg()
        {
            string result = "";
            string globalConfig = CSScriptIntellisense.CSScriptHelper.GetGlobalCSSConfig();
            if (globalConfig != null)
                result = " \"-config:" + globalConfig + "\"";

            return result;
        }

        static string GenerateProbingDirArg()
        {
            string probingDirArg = "";

            if (NppScripts_ScriptsDir != null || !CSScriptIntellisense.Config.Instance.DefaultSearchDirs.IsEmpty())
                probingDirArg = (NppScripts_ScriptsDir + "," + CSScriptIntellisense.Config.Instance.DefaultSearchDirs).Trim('\'').Trim(',');

            if (!probingDirArg.IsEmpty())
                probingDirArg = " \"-dir:" + probingDirArg + "\"";

            if (!CSScriptIntellisense.Config.Instance.DefaultRefAsms.IsEmpty())
                try
                {
                    string[] asms = CSScriptIntellisense.Config.Instance
                                                               .DefaultRefAsms.Split(';', ',')
                                                               .Where(x => !string.IsNullOrWhiteSpace(x))
                                                               .Select(x => Environment.ExpandEnvironmentVariables(x.Trim()))
                                                               .ToArray();

                    probingDirArg += " \"/r:" + string.Join(",", asms) + "\"";
                }
                catch { }

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

    static class GenericExtensionsved
    {
        public static string RemoveNonUserCompilingInfo(this string compilerOutput)
        {
            //for a better appearance remove CS-Script related stuff
            return compilerOutput.Replace("csscript.CompilerException: ", "")
                                 .Replace("vbc : Command line (0,0): warning BC2007: unrecognized option 'warn:0'; ignored\r\n", "")
                                 .Replace("vbc : Command line (0,0): warning BC2007: unrecognized option 'warn:0'; ignored", "");
        }

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