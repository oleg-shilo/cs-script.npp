using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

namespace CSScriptNpp.Deployment
{
    class Program
    {
        const string elevationIndicatorArg = "/elevated";

        static void Main(string[] args)
        {
            try
            {
                if (args[0] == "/restart") //restart
                {
                    // /restart [/asadmin] <prevInstanceProcId> <appPath>

                    int id;
                    string appPath;
                    bool asAdmin = false;

                    if (args[1] == "/asadmin")
                    {
                        asAdmin = true;
                        id = int.Parse(args[2]);
                        appPath = args[3];
                    }
                    else
                    {
                        id = int.Parse(args[1]);
                        appPath = args[2];
                    }

                    var proc = Process.GetProcesses().Where(x => x.Id == id).FirstOrDefault();
                    if (proc != null && !proc.HasExited)
                        proc.WaitForExit();

                    if (asAdmin)
                    {
                        var p = new Process();
                        p.StartInfo.FileName = appPath;
                        p.StartInfo.Verb = "runas";
                        p.Start();
                    }
                    else
                    {
                        Process.Start(appPath);
                    }
                }
                else  //update
                {
                    if (IsAdmin())
                    {
                        if (args.Contains(elevationIndicatorArg))
                            args = args.Where(a => a != elevationIndicatorArg).ToArray();

                        if (EnsureNppNotRunning())
                        {
                            // <zipFile> <pluginDir>
                            string zipFile = args[0];
                            string pluginDir = args[1];
                            string nppExe = Path.Combine(pluginDir, @"..\\notepad++.exe");
                            Updater.Deploy(zipFile, pluginDir);
                            if (File.Exists(nppExe))
                                Process.Start(nppExe);
                            else
                                MessageBox.Show("The update has been successfully installed.", "CS-Script Update");
                        }
                    }
                    else
                    {
                        if (!args.Contains(elevationIndicatorArg)) //has not been attempted to elevate yet
                            RestartElevated(args);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Update has not succeeded.\nError: " + e, "CS-Script Update");
            }
        }

        static bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static bool RestartElevated(string[] arguments)
        {
            string args = elevationIndicatorArg;
            for (int i = 0; i < arguments.Length; i++)
                args += " \"" + arguments[i] + "\"";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Assembly.GetExecutingAssembly().Location;
            startInfo.Arguments = args;

            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                startInfo.Verb = "runas";

            Process.Start(startInfo);
            return true;
        }

        static bool EnsureNppNotRunning()
        {
            int count = 0;
            while (Process.GetProcessesByName("notepad++").Any())
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
            return true;
        }
    }
}