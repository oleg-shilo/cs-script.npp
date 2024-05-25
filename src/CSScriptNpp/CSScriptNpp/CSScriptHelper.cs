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
using System.Threading;

// using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSScriptIntellisense;
using UltraSharp.Cecil;

namespace CSScriptNpp
{
    public static class Runtime
    {
        public static string cscs_asm { get; set; }
        public static string syntaxer_asm { get; set; }
        public static int syntaxer_port { get; set; }

        public static string dependenciesDirRoot => Environment.SpecialFolder.CommonApplicationData
                                                    .PathJoin("CSScriptNpp", Assembly.GetExecutingAssembly().GetName().Version);

        public static void Init()
        {
            if (!Config.Instance.UseEmbeddedEngine && Config.Instance.CustomSyntaxerAsm.HasText())
            {
                syntaxer_asm = Config.Instance.CustomSyntaxerAsm;
                syntaxer_port = Config.Instance.CustomSyntaxerPort;
            }
            else
            {
                syntaxer_asm = dependenciesDirRoot.PathJoin("cs-syntaxer", "syntaxer.dll");
                syntaxer_port = 18001;

                if (!Directory.Exists(syntaxer_asm.GetDirName()))
                    DeployDir(Bootstrapper.pluginDir.PathJoin("cs-syntaxer"),
                        syntaxer_asm.GetDirName());
            }

            if (!Config.Instance.UseEmbeddedEngine && Config.Instance.CustomEngineAsm.HasText())
            {
                cscs_asm = Config.Instance.CustomEngineAsm;
            }
            else
            {
                cscs_asm = dependenciesDirRoot.PathJoin("cs-script", "cscs.dll");

                if (!Directory.Exists(cscs_asm.GetDirName()))
                    DeployDir(Bootstrapper.pluginDir.PathJoin("cs-script"),
                              cscs_asm.GetDirName());

                var oldServicesVersions = Directory.GetDirectories(Path.GetDirectoryName(dependenciesDirRoot))
                                                   .Where(x => x != dependenciesDirRoot);
                foreach (var dir in oldServicesVersions)
                    DeleteDir(dir);
            }

            Syntaxer.cscs_asm = () => Runtime.cscs_asm;
            Syntaxer.syntaxer_asm = () => Runtime.syntaxer_asm;
            Syntaxer.syntaxer_port = () => Runtime.syntaxer_port;

            Syntaxer.StartServer(onlyIfNotRunning: true);
        }

        static void DeleteDir(string dir)
        {
            foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                try
                {
                    File.Delete(file);
                }
                catch { }

            for (int i = 0; i < 3 && Directory.Exists(dir); i++)
                try
                {
                    Directory.Delete(dir, true);
                    continue;
                }
                catch { Thread.Sleep(200); }
        }

        static void DeployDir(string srcDir, string destDir)
        {
            var srcFiles = Directory.GetFiles(srcDir, "*", SearchOption.AllDirectories).OrderBy(x => x).ToArray();

            foreach (string srcFile in srcFiles)
                try
                {
                    var detFile = destDir.PathJoin(srcFile.Substring(srcDir.Length + 1));
                    detFile.GetDirName().EnsureDir();
                    File.Copy(srcFile, detFile, true);
                }
                catch { }
        }
    }

    public static class CSScriptHelper
    {
        static string vsDir;

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

        internal static string LocateChocolateySyntaxerAppDir()
        {
            // C:\ProgramData\chocolatey\lib\cs-syntaxer\tools\syntaxer.dll
            return LocateChocolateyAppDir("cs-syntaxer", "syntaxer.dll");
        }
        internal static string LocateChocolateyCSScriptsAppDir()
        {
            // C:\ProgramData\chocolatey\lib\cs-script\tools\cscs.dll
            return LocateChocolateyAppDir("cs-script", "cscs.dll");
        }

        internal static string LocateChocolateyAppDir(string app, string fileName)
        {
            // C:\ProgramData\chocolatey\lib\<app>\tools\<file>
            try
            {
                var packageDir = Environment.SpecialFolder.CommonApplicationData.GetPath()
                    .PathJoin($@"chocolatey\lib\{app}\tools");

                if (Directory.Exists(packageDir))
                {
                    if (File.Exists(packageDir.PathJoin(fileName)))
                        return packageDir;
                }
            }
            catch { }
            return null;
        }

        internal static string LocateDotNetCSScriptsToolDir()
        {
            // C:\Users\user\.dotnet\tools
            // C:\Users\user\.dotnet\tools\.store\cs-script.cli\4.8.16\cs-script.cli\4.8.16\tools\net8.0\any\cscs.dll
            // dotnet tool install --global cs-script.cli
            try
            {
                var packageDir = Environment.SpecialFolder.UserProfile.GetPath()
                    .PathJoin(@".dotnet\tools\.store\cs-script.cli");

                if (Directory.Exists(packageDir))
                {
                    var installedVersionDir = Directory.GetDirectories(packageDir)
                        .OrderByDescending(x => x)
                        .FirstOrDefault();

                    if (installedVersionDir != null)
                    {
                        var appDir = Directory.GetDirectories(
                            installedVersionDir.PathJoin("cs-script.cli", Path.GetFileName(installedVersionDir), "tools"))
                            .OrderByDescending(x => x)
                            .FirstOrDefault()?
                            .PathJoin("any");

                        if (File.Exists(appDir.PathJoin("cscs.dll")))
                            return appDir;
                    }
                }
            }
            catch { }
            return null;
        }

        internal static string LocateWingetCSScriptAppDir()
        {
            // C:\Users\user\AppData\Local\Microsoft\WinGet\Packages
            // C:\Users\user\AppData\Local\Microsoft\WinGet\Packages\oleg-shilo.cs-script_Microsoft.Winget.Source_8wekyb3d8bbwe

            try
            {
                var uesrProfile = Environment.SpecialFolder.LocalApplicationData.GetPath();

                var appDir = Directory.GetDirectories(Path.Combine(uesrProfile, @"Microsoft\WinGet\Packages"), "oleg-shilo.cs-script*")
                .FirstOrDefault();

                if (File.Exists(appDir.PathJoin("cscs.dll")))
                    return appDir;
            }
            catch { }

            return null;
        }

        public static string SystemCSScriptDir =>
                Environment.GetEnvironmentVariable("CSSCRIPT_ROOT") ??
                LocateChocolateyCSScriptsAppDir() ??
                LocateWingetCSScriptAppDir() ??
                LocateDotNetCSScriptsToolDir();
        public static string SystemCSSyntaxerDir =>
            Environment.GetEnvironmentVariable("CSSYNTAXER_ROOT") ??
            LocateChocolateySyntaxerAppDir();
        public static bool IsCSSyntaxerInstalled => SystemCSSyntaxerDir.IsNotEmpty();
        public static bool IsCSScriptInstalled => SystemCSScriptDir.IsNotEmpty();
        public static bool IsCSScriptChocoInstalled => IsCSScriptInstalled && LocateChocolateyCSScriptsAppDir().IsNotEmpty();
        public static bool IsCSScriptWingetInstalled => IsCSScriptInstalled && LocateWingetCSScriptAppDir().IsNotEmpty();
        public static bool IsCSScriptDotnetInstalled => IsCSScriptInstalled && LocateDotNetCSScriptsToolDir().IsNotEmpty();

        public static string InstallCssCmd
        {
            get
            {
                if (IsCSScriptInstalled)
                {
                    if (IsCSScriptChocoInstalled)
                        return InstallCssChocoCmd;
                    else if (IsCSScriptWingetInstalled)
                        return InstallCssWingetCmd;
                    else if (IsCSScriptDotnetInstalled)
                        return InstallCssDotnetCmd;
                    return
                        null;
                }
                else
                    return InstallCssChocoCmd;
            }
        }
        public static string InstallCssChocoCmd => $"choco {(IsCSScriptInstalled ? "upgrade" : "install")} cs-script --y";
        public static string InstallCssWingetCmd => $"winget {(IsCSScriptInstalled ? "upgrade" : "install")} cs-script";
        public static string InstallCssDotnetCmd => $"dotnet tool {(IsCSScriptInstalled ? "update" : "install")} --global cs-script.cli";


        public static string InstallCsSyntaxerCmd = $"choco {(IsCSSyntaxerInstalled ? "upgrade" : "install")} cs-syntaxer --y";

        public static bool IsChocoInstalled
        {
            get
            {
                try
                {
                    Run("choco", "");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static string VsDir
        {
            get
            {
                if (vsDir == null)
                    vsDir = Path.Combine(GetScriptTempDir(), "NppScripts");
                return vsDir;
            }
        }

        public static string CSScriptTempDir
        {
            get
            {
                if (vsDir == null)
                    vsDir = Path.Combine(GetScriptTempDir(), "CSScriptNpp");
                return vsDir;
            }
        }

        static string tempDir = null;

        static public string GetScriptTempDir()
        {
            if (tempDir == null)
            {
                tempDir = Environment.GetEnvironmentVariable("CSS_CUSTOM_TEMPDIR");
                if (tempDir == null)
                {
                    tempDir = Path.Combine(Path.GetTempPath(), "CSSCRIPT");
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                }
            }
            return tempDir;
        }

        static internal bool RunningAsAdmin
        {
            get
            {
                var p = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                return p.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        static public string Build(string scriptFile, Action<string> onCompilerOutput)
        {
            string compilerOutput;
            bool success = Build(scriptFile, out compilerOutput, onCompilerOutput);
            if (!success)
                throw new ApplicationException(compilerOutput.RemoveNonUserCompilingInfo());

            return compilerOutput;
        }

        static public string[] GetCodeCompileOutput(string scriptFile)
        {
            string compilerOutput;
            bool success = Build(scriptFile, out compilerOutput, null);
            if (!success)
                return compilerOutput.RemoveNonUserCompilingInfo().Split('\n');
            else
                return new string[0];
        }

        static string Run(string file, string args)
        {
            var p = new Process();
            p.StartInfo.FileName = file;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            p.Start();

            var output = new StringBuilder();

            string line = null;
            while (null != (line = p.StandardOutput.ReadLine()))
                output.AppendLine(line);

            Task.Run(() =>
            {
                try
                {
                    string error = null;
                    while (null != (error = p.StandardError.ReadLine()))
                        output.AppendLine(error);
                }
                catch
                {
                }
            });

            p.WaitForExit();
            return output.ToString();
        }

        static bool Build(string scriptFile, out string compilerOutput, Action<string> onCompilerOutput)
        {
            string oldNotificationMessage = null;
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                var output = new StringBuilder();

                bool error = false;
                bool printOutput = false;

                run("dotnet", $"\"{Runtime.cscs_asm}\" -check " + GenerateDefaultArgs(scriptFile) + " \"" + scriptFile + "\"",
                    line =>
                    {
                        printOutput = printOutput || line.Contains("NuGet");

                        if (printOutput && onCompilerOutput != null)
                            onCompilerOutput(line);

                        if (line.Contains("NuGet") && NotifyClient != null && !oldNotificationMessage.HasText())
                        {
                            oldNotificationMessage = NotifyClient("Processing NuGet packages...");
                        }

                        if (line.ToString().StartsWith("Error: Specified file could not be compiled.")
                            || Regex.IsMatch(line, @"Compile: \d error")) // Compile: 1 error(s)
                        {
                            error = true;
                        }
                        else
                            if (!string.IsNullOrEmpty(line) && !line.Contains("at csscript.CSExecutor."))
                            output.AppendLine(line);
                    }
                   );

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

        static public void run(this string exe, string args, Action<string> onStdOut = null)
        {
            string outputFile = null;
            try
            {
                var p = new Process();
                p.StartInfo.FileName = exe;
                p.StartInfo.Arguments = args;

                if (onStdOut != null)
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                }

                p.Start();

                var output = new StringBuilder();

                if (onStdOut != null)
                {
                    string line;
                    while (null != (line = p.StandardOutput.ReadLine()))
                    {
                        output.AppendLine(line);
                        onStdOut(line);
                    }
                }
                p.WaitForExit();
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

        static public void ExecuteScript(string scriptFile, Action<Process> onStart = null, Action<object> onStdOut = null)
        {
            bool needsElevation = !RunningAsAdmin && IsAsAdminScriptFile(scriptFile);
            Execute(
                "dotnet",
                $"\"{Runtime.cscs_asm}\" -l -d " + GenerateDefaultArgs(scriptFile) + " \"" + scriptFile + "\"",
                needsElevation,
                onStart, onStdOut);
        }

        static public void Execute(string exe, string args, bool needsElevation, Action<Process> onStart = null, Action<object> onStdOut = null)
        {
            string outputFile = null;
            try
            {
                var p = new Process();
                p.StartInfo.FileName = exe;
                p.StartInfo.Arguments = args;

                bool useFileRedirection = false;

                if (needsElevation)
                {
                    p.StartInfo.Verb = "runas";
                    useFileRedirection = (onStdOut != null);

                    if (useFileRedirection)
                        onStdOut("WARNING: StdOut interception is impossible for elevated process. The whole output will be displayed at the end of the execution instead. Alternatively you can elevate Notepad++ process.".ToCharArray());
                }

                if (onStdOut != null)
                {
                    if (useFileRedirection)
                    {
                        // interferes with dbmon redirection as the PID of the actual script process becomes unknown
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
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    }
                }

                p.Start();

                var cancel = new CancellationTokenSource();

                p.Exited += (s, e) =>
                {
                    // process can exit but WaitForExit may not return until blocking IO reads are in progress so need
                    // to cancel Tasks via the cancellation token
                    cancel.Cancel();
                };

                if (onStart != null)
                    onStart(p);

                var output = new StringBuilder();

                if (onStdOut != null && !useFileRedirection)
                {
                    if (Config.Instance.InterceptConsoleByCharacter)
                    {
                        char[] buf = new char[1];
                        while (0 != p.StandardOutput.Read(buf, 0, 1))
                        {
                            output.Append(buf[0]);
                            onStdOut(buf);
                        }
                    }
                    else
                    {
                        Task.Run(() =>
                        {
                            string line;
                            while (null != (line = p.StandardOutput.ReadLine()))
                            {
                                System.Diagnostics.Debug.WriteLine("Listening OUT!!!");
                                output.AppendLine(line);
                                onStdOut(line);
                            }
                        }, cancel.Token);

                        Task.Run(() =>
                        {
                            try
                            {
                                System.Diagnostics.Debug.WriteLine("Listening Err!!!");
                                string errorLine;
                                while (null != (errorLine = p.StandardError.ReadLine()))
                                {
                                    output.AppendLine(errorLine);
                                    onStdOut(errorLine);
                                }
                            }
                            catch { }
                        }, cancel.Token);
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
                keepRoslynLoadedTimer = new System.Windows.Forms.Timer();

                // LoadRoslyn();
                Task.Factory.StartNew(LoadRoslyn);

                keepRoslynLoadedTimer.Interval = 1000 * 60 * 9; //9 min
                keepRoslynLoadedTimer.Tick += (s, e) =>
                                               {
                                                   Task.Factory.StartNew(LoadRoslyn);
                                               };
                keepRoslynLoadedTimer.Enabled = true;
                keepRoslynLoadedTimer.Start();
            }
        }

        static public void LoadRoslyn()
        {
            try
            {
                var p = new Process();
                p.StartInfo.FileName = "dotnet";
                p.StartInfo.Arguments = $"\"{Runtime.cscs_asm}\" -speed";

                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();
            }
            catch
            {
            }
        }

        static public void ExecuteAsynch(string scriptFile)
        {
            var args = string.Format($"\"{Runtime.cscs_asm}\" -wait /nl -d /l {GenerateDefaultArgs(scriptFile)} \"{scriptFile}\"");

            if (!RunningAsAdmin)
                ProcessStart("dotnet", args, IsAsAdminScriptFile(scriptFile));
            else
                ProcessStart("dotnet", args);
        }

        static public void Debug(string scriptFile)
        {
            var args = string.Format($"\"{Runtime.cscs_asm}\" -wait /nl /l {GenerateDefaultArgs(scriptFile)} \"{scriptFile}\" //x");
            ProcessStart("dotnet", args);
        }

        static public Func<string, string> NotifyClient;

        static public Project GenerateProjectFor(string script)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var project = new Project { PrimaryScript = script, Assemblies = new string[0], SourceFiles = new string[0] };

                NotifyClient("Processing NuGet packages...");
                Application.DoEvents();

                "dotnet".run($"\"{Runtime.cscs_asm}\" -proj {GenerateDefaultArgs(script)} \"{script}\"",
                    onStdOut: line =>
                    {
                        if (line.StartsWith("file:"))
                            project.SourceFiles = project.SourceFiles.Concat(line.Substring("file:".Length)).ToArray();
                        if (line.StartsWith("ref:"))
                            project.Assemblies = project.SourceFiles.Concat(line.Substring("ref:".Length)).ToArray();
                    });
                return project;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                NotifyClient("");
            }
        }

        static public string DownloadDistro(string distroUrl, Action<long, long> onProgress)
        {
            try
            {
                string downloadDir = KnownFolders.UserDownloads;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(distroUrl);
                string distroExtension = Path.GetExtension(distroUrl);

                string destFile = Path.Combine(downloadDir, fileNameWithoutExtension + distroExtension);

                int numOfAlreadyDownloaded = Directory.GetFiles(downloadDir, fileNameWithoutExtension + "*" + distroExtension).Count();
                if (numOfAlreadyDownloaded > 0)
                    destFile = Path.Combine(downloadDir, fileNameWithoutExtension + " (" + (numOfAlreadyDownloaded + 1) + ")" + distroExtension);

                DownloadBinary(distroUrl, destFile, onProgress);

                return destFile;
            }
            catch
            {
                return null;
            }
        }

        static public Distro GetLatestAvailableVersion()
        {
            var github_distro_latest_version = "https://raw.githubusercontent.com/oleg-shilo/cs-script.npp/master/bin/latest_version.txt";
            // github_repo_latest_version = "http://csscript.net/npp/latest_version.txt";

            string url = Environment.GetEnvironmentVariable("CSSCRIPT_NPP_REPO_URL") ?? github_distro_latest_version;

            Distro stableVersion = GetLatestAvailableDistro(url);

            if (Config.Instance.CheckPrereleaseUpdates)
            {
                Distro prereleaseVersion = GetLatestAvailableDistro(url.Replace(".txt", ".pre.txt"));
                if (stableVersion != null)
                {
                    return prereleaseVersion;
                }
                if (prereleaseVersion == null)
                {
                    return stableVersion;
                }
                else
                {
                    try
                    {
                        if (Version.Parse(prereleaseVersion.Version) > Version.Parse(stableVersion.Version))
                            return prereleaseVersion;
                        else
                            return stableVersion;
                    }
                    catch { }
                }
            }

            return stableVersion;
        }

        static public Distro GetLatestAvailableDistro(string url)
        {
            try
            {
                return Distro.FromVersionInfo(DownloadText(url));
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

        static bool IsAsAdminScriptDirective(string text)
        {
            string[] words = text.Split(lineWordDelimiters, StringSplitOptions.RemoveEmptyEntries);
            return words.Any() && words.First() == "//css_npp" && (words.Contains("asadmin") || words.Contains("asadmin,") || words.Contains("asadmin;"));
        }

        static public string Isolate(string scriptFile, bool asScript, bool windowApp, bool asDll)
        {
            Cursor.Current = Cursors.WaitCursor;

            var done = false;
            Task.Run(() =>
            {
                var tick = 1;
                while (!done)
                {
                    NotifyClient("Building deployment package" + new string('.', tick++));
                    Thread.Sleep(200);
                    if (tick > 5)
                        tick = 1;
                }
                NotifyClient("");
            });

            try
            {
                string dir = Path.Combine(Path.GetDirectoryName(scriptFile), Path.GetFileNameWithoutExtension(scriptFile));

                EnsureCleanDirectory(dir);

                var engineFile = Runtime.cscs_asm;

                Project proj = GenerateProjectFor(scriptFile);
                var assemblies = proj.Assemblies.Where(a => !a.Contains("GAC_MSIL")).ToArray();
                Action<string, string> copy = (file, directory) => File.Copy(file, Path.Combine(directory, Path.GetFileName(file)), true);

                if (asScript)
                {
                    string batchFile = Path.Combine(dir, "run.cmd");

                    var files = proj.SourceFiles.Concat(assemblies);

                    files.ForEach(file => copy(file, dir));
                    copy(engineFile, dir);

                    foreach (var file in Directory.GetFiles(engineFile.GetDirName(), "Microsoft.CodeAnalysis.*.dll"))
                        copy(file, dir);

                    foreach (var file in Directory.GetFiles(engineFile.GetDirName(), "cscs.*.json"))
                        copy(file, dir);

                    File.WriteAllText(batchFile, $"echo off\r\ndotnet .\\cscs.dll \"{Path.GetFileName(scriptFile)}\"\r\npause");

                    return dir;
                }
                else
                {
                    string srcBinary = Path.ChangeExtension(scriptFile, asDll ? ".dll" : ".exe");
                    string destBinary = Path.Combine(dir, Path.GetFileName(srcBinary));

                    string build_arg = asDll ? "-cd" : "-e" + (windowApp ? "w" : "");
                    string args = $"\"{(windowApp ? engineFile : engineFile.Replace("cscs.dll", "csws.dll"))}\" {GenerateDefaultArgs(scriptFile)} {build_arg} \"{scriptFile}\"";

                    var p = new Process();
                    p.StartInfo.FileName = "dotnet";
                    p.StartInfo.Arguments = args;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    p.WaitForExit();

                    if (File.Exists(srcBinary))
                    {
                        assemblies.Concat(srcBinary)
                                  .Concat(Directory.GetFiles(Path.GetDirectoryName(scriptFile), $"{Path.GetFileNameWithoutExtension(scriptFile)}.*"))
                                  .Where(x => x != scriptFile)
                                  .ForEach(file => copy(file, dir));

                        copy(engineFile, dir);

                        File.Delete(srcBinary);
                        return dir;
                    }
                    else
                    {
                        //just show why building has failed
                        args = $"\"{engineFile}\" {GenerateDefaultArgs(scriptFile)} {build_arg} -wait \"{scriptFile}\"";
                        Process.Start("dotnet", args);
                        return null;
                    }
                }
            }
            finally
            {
                done = true;
                Cursor.Current = Cursors.Default;
            }
        }

        static public void OpenAsVSProjectFor(string script)
        {
            if (Environment.GetEnvironmentVariable("CSSCRIPT_VSEXE") == null)
            {
                var vsDirs = Directory.GetDirectories(Environment.SpecialFolder.ProgramFiles.GetPath(), "Microsoft Visual Studio*").Concat(
                             Directory.GetDirectories(Environment.SpecialFolder.ProgramFilesX86.GetPath(), "Microsoft Visual Studio*"));

                var ide = vsDirs.SelectMany(d => Directory.GetFiles(d, "devenv.exe", SearchOption.AllDirectories))
                                .OrderByDescending(x => x)
                                .FirstOrDefault();
                if (ide == null)
                {
                    MessageBox.Show("Could not detect any installation of Visual Studio.", "CS-Script");
                    return;
                }

                Environment.SetEnvironmentVariable("CSSCRIPT_VSEXE", ide);
            }

            var p = new Process();
            p.StartInfo.FileName = "dotnet";
            p.StartInfo.Arguments = $"\"{Runtime.cscs_asm}\" -vs \"{script}\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
        }

        static public string ScriptEngineVersion()
        {
            return FileVersionInfo.GetVersionInfo(Runtime.cscs_asm).FileVersion.ToString();
        }

        static void DownloadBinary(string url, string destinationPath, Action<long, long> onProgress = null, string proxyUser = null, string proxyPw = null)
        {
            byte[] buf = new byte[1024 * 4];

            // GitHub does no longer accept SSL3. It's a common trend as SSL3 is 21 years old is no longer secure enough.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (proxyUser != null)
                WebRequest.DefaultWebProxy.Credentials = new NetworkCredential(proxyUser, proxyPw);

            var request = WebRequest.Create(url);
            var response = (WebResponse)request.GetResponse();

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

        public static string DownloadText(this string url, string proxyUser = null, string proxyPw = null)
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
            return GenerateConfigFileExecutionArg() + GenerateProbingDirArg();
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
                                                               .DefaultRefAsms.Split('|')
                                                               .Where(x => !string.IsNullOrWhiteSpace(x))
                                                               .Select(x => Environment.ExpandEnvironmentVariables(x.Trim()))
                                                               .ToArray();

                    probingDirArg += " \"/r:" + string.Join(",", asms) + "\"";
                }
                catch { }

            return probingDirArg;
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