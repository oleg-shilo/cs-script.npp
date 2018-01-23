using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Updater;

namespace CSScriptNpp.Deployment
{
    class Program
    {
        const string asynchUpdateArg = "/asynch_update";
        static Mutex appSingleInstanceMutex;

        static void Main(string[] args)
        {
            try
            {
                Debug.Assert(false);

                if (!args.Any())
                {
                    string distroFile = UserInputForm.GetDistro();
                    if (!string.IsNullOrEmpty(distroFile))
                    {
                        // Debug.Assert(false);
                        StopVBCSCompilers();
                        if (EnsureNppNotRunning(false) && EnsureVBCSCompilerNotLocked(false))
                        {
                            if (distroFile.StartsWith("http"))
                            {
                                var url = distroFile;
                                distroFile = Path.Combine(KnownFolders.UserDownloads, "CSScriptNpp.ManualUpdate", Path.GetFileName(distroFile));
                                WebHelper.DownloadBinary(url, distroFile);
                            }
                            DeployItselfAndRestart(distroFile, FindPluginDir(distroFile));
                        }
                    }
                    return;
                }

                bool createdNew;
                appSingleInstanceMutex = new Mutex(true, "Npp.CSScript.PluginUpdater", out createdNew);

                if (!createdNew)
                {
                    MessageBox.Show($"Another Notepad++ plugin update in progress. Either wait or stop {Path.GetFileName(Assembly.GetExecutingAssembly().Location)}", "CS-Script");
                    return;
                }

                // <zipFile> [<pluginDir>] [/asynchUpdateArg]
                string zipFile = args[0];
                string pluginDir = args.Length > 1 ? args[1] : FindPluginDir(zipFile);

                string updaterDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (updaterDir.StartsWith(pluginDir, StringComparison.OrdinalIgnoreCase))
                {
                    DeployItselfAndRestart(zipFile, pluginDir);
                    return;
                }
                else
                {
                    DeployDependencies();
                }

                if (IsAdmin())
                {
                    StopVBCSCompilers();

                    bool isAsynchUpdate = args.Contains(asynchUpdateArg) || args.First().IsUrl() || args.First().IsVersion();
                    args = args.Where(x => x != asynchUpdateArg).ToArray();

                    if (pluginDir == null)
                        throw new Exception($"Cannot find Notepad++ installation.");

                    Debug.Assert(false);

                    if (EnsureNppNotRunning(isAsynchUpdate) && EnsureVBCSCompilerNotLocked(isAsynchUpdate))
                    {
                        if (isAsynchUpdate)
                        {
                            WaitPrompt.Show();

                            string arg = args[0];
                            zipFile = WebHelper.DownloadDistro(args[0], WaitPrompt.OnProgress);
                        }

                        // pluginDir: C:\Program Files\Notepad++\plugins\CSScriptNpp
                        Updater.Deploy(zipFile, Path.GetDirectoryName(pluginDir));

                        WaitPrompt.Hide();

                        if (EnsureNppNotRunning(isAsynchUpdate) && EnsureVBCSCompilerNotLocked(isAsynchUpdate))
                        {
                            MessageBox.Show("The update process has been completed.", "CS-Script Update");
                        }
                    }
                }
                else
                {
                    throw new Exception("You need admin rights to start CS-Script updater.");
                }
            }
            catch (Exception e)
            {
                WaitPrompt.Hide();
                MessageBox.Show("Update has not succeeded.\nError: " + e.Message, "CS-Script Update");
            }
        }

        static string FindPluginDir(string distroFile)
        {
            // CSScriptNpp.x86.zip
            var cpu = Path.GetExtension(Path.GetFileNameWithoutExtension(distroFile));
            if (cpu == null || cpu == ".x86")
                return findPluginDir(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            else if (cpu == ".x64")
                return findPluginDir(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            return null;
        }

        static string findPluginDir(string programFilesDir)
        {
            var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (pluginDir.EndsWith("CSScriptNpp") && pluginDir.StartsWith(programFilesDir, StringComparison.OrdinalIgnoreCase))
                return Path.GetDirectoryName(pluginDir);
            else
            {
                pluginDir = Path.Combine(programFilesDir, @"Notepad++\plugins");
                string nppExe = Path.Combine(pluginDir, @"..\notepad++.exe");

                if (Directory.Exists(pluginDir) && File.Exists(nppExe))
                    return pluginDir;
                else
                    return null;
            }
        }

        static bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Stop any running instances of the compiler server if any.
        /// <para>
        /// Stopping is needed in order to prevent any problems with copying/moving CS-Script binary files (e.g. Roslyn compilers).
        /// Servers restart automatically on any attempt to compile any C#/VB.NET code by any client (e.g. Visual Studio, MSBuild, CS-Script).
        /// </para>
        /// </summary>
        public static void StopVBCSCompilers()
        {
            foreach (var p in Process.GetProcessesByName("VBCSCompiler"))
                try { p.Kill(); }
                catch { } //cannot analyse main module as it may not be accessible for x86 vs. x64 reasons
        }

        public static bool IsVBCSCompilerLocked(string pluginDir = null)
        {
            if (pluginDir == null)
            {
                return Process.GetProcessesByName("VBCSCompiler").Any();
            }
            else
            {
                foreach (string file in Directory.GetFiles(pluginDir, "VBCSCompiler.exe", SearchOption.AllDirectories))
                    try
                    {
                        string temp = file + ".tmp";
                        if (File.Exists(temp))
                            File.Delete(temp);

                        File.Move(file, temp);
                        File.Move(temp, file);
                    }
                    catch
                    {
                        return true;
                    }

                return false;
            }
        }

        static void DeployItselfAndRestart(string zipFile, string pluginDir)
        {
            // string downloadDir = KnownFolders.UserDownloads;
            string destDir = Path.Combine(KnownFolders.UserDownloads, "CSScriptNpp.Updater");
            string destUpdater = Path.Combine(destDir, "updater.exe");

            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            File.Copy(Path.Combine(Assembly.GetExecutingAssembly().Location), destUpdater, true);
            DeployDependencies();

            Process.Start(destUpdater, $"\"{zipFile}\" \"{pluginDir}\"");
        }

        static void DeployDependencies()
        {
            string destDir = Path.Combine(KnownFolders.UserDownloads, "CSScriptNpp.Updater");
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            File.WriteAllBytes(Path.Combine(destDir, "7z.exe"), Resource1._7z_exe);
            File.WriteAllBytes(Path.Combine(destDir, "7z.dll"), Resource1._7z_dll);
        }

        static bool EnsureVBCSCompilerNotLocked(bool backgroundWait)
        {
            int count = 0;
            while (IsVBCSCompilerLocked())
            {
                if (backgroundWait)
                {
                    Thread.Sleep(5000);
                }
                else
                {
                    count++;

                    var buttons = MessageBoxButtons.OKCancel;
                    var prompt = "Updater detected running VBCSCompiler.exe, which may lock plugin files.\n" +
                        "Please close any running instance of VBCSCompiler.exe from Task Manager and press OK to proceed.";

                    if (count > 1)
                    {
                        prompt = "Please close any running instance of VBCSCompiler.exe from Task Manager and try again.";
                        buttons = MessageBoxButtons.RetryCancel;
                    }

                    if (MessageBox.Show(prompt, "CS-Script Update", buttons) == DialogResult.Cancel)
                        return false;
                }
            }
            return true;
        }

        static bool EnsureNppNotRunning(bool backgroundWait)
        {
            return EnsureAppNotRunning("Notepad++", backgroundWait);
        }

        static bool EnsureUpdaterNotRunning(bool backgroundWait)
        {
            return EnsureAppNotRunning("Updater", backgroundWait);
        }

        static bool EnsureAppNotRunning(string app, bool backgroundWait)
        {
            Thread.Sleep(2000);

            int count = 0;
            while (Process.GetProcessesByName(app).Any())
            {
                if (backgroundWait)
                {
                    Thread.Sleep(5000);
                }
                else
                {
                    count++;

                    var buttons = MessageBoxButtons.OKCancel;
                    var prompt = "Please close any running instance of " + app + ".exe and press OK to proceed.";

                    if (count > 1)
                    {
                        prompt = "Please close any running instance of " + app + ".exe and try again.";
                        buttons = MessageBoxButtons.RetryCancel;
                    }

                    if (MessageBox.Show(prompt, "CS-Script Update", buttons) == DialogResult.Cancel)
                        return false;
                }
            }
            return true;
        }
    }

    static class Extensions
    {
        public static bool IsVersion(this string text)
        {
            Version v;
            return Version.TryParse(text, out v);
        }

        public static bool IsUrl(this string text)
        {
            return text.Contains("http://");
        }
    }
}