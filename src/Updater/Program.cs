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
            if (!args.Any())
            {
                MessageBox.Show($"{Path.GetFileName(Assembly.GetExecutingAssembly().Location)} is expected to be started only by Notepad++. But not by user.", "CS-Script");
                return;
            }

            bool createdNew;
            appSingleInstanceMutex = new Mutex(true, "Npp.CSScript.PluginUpdater", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show($"Another Notepad++ plugin update in progress. Either wait or stop {Path.GetFileName(Assembly.GetExecutingAssembly().Location)}", "CS-Script");
                return;
            }

            try
            {
                if (IsAdmin())
                {
                    StopVBCSCompilers();

                    //Debug.Assert(false);

                    bool isAsynchUpdate = args.Contains(asynchUpdateArg);

                    // <zipFile> <pluginDir>
                    string zipFile = args[0];
                    string pluginDir = args[1];

                    if (EnsureNppNotRunning(isAsynchUpdate) && EnsureVBCSCompilerNotLocked(isAsynchUpdate))
                    {
                        if (isAsynchUpdate)
                        {
                            WaitPrompt.Show();

                            string version = args[0];
                            zipFile = WebHelper.DownloadDistro(version, WaitPrompt.OnProgress);
                        }

                        string nppExe = Path.Combine(pluginDir, @"..\\notepad++.exe");
                        Updater.Deploy(zipFile, pluginDir);

                        WaitPrompt.Hide();

                        if (EnsureNppNotRunning(isAsynchUpdate) && EnsureVBCSCompilerNotLocked(isAsynchUpdate))
                        {
                            if (File.Exists(nppExe))
                                Process.Start(nppExe);
                            else
                                MessageBox.Show("The update has been successfully installed.", "CS-Script Update");
                        }
                    }
                }
                else
                {
                    throw new Exception("You need admon rights to start CS-Script updater.");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Update has not succeeded.\nError: " + e.Message, "CS-Script Update");
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
            Thread.Sleep(2000);

            int count = 0;
            while (Process.GetProcessesByName("notepad++").Any())
            {
                if (backgroundWait)
                {
                    Thread.Sleep(5000);
                }
                else
                {
                    count++;

                    var buttons = MessageBoxButtons.OKCancel;
                    var prompt = "Please close any running instance of Notepad++ and press OK to proceed.";


                    if (count > 1)
                    {
                        prompt = "Please close any running instance of Notepad++ and try again.";
                        buttons = MessageBoxButtons.RetryCancel;
                    }

                    if (MessageBox.Show(prompt, "CS-Script Update", buttons) == DialogResult.Cancel)
                        return false;
                }
            }
            return true;
        }

    }
}