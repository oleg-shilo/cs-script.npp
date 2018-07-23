using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using CSScriptIntellisense;

namespace CSScriptNpp
{
    public static class Bootstrapper
    {
        public static string pluginDir;

        public static string dependenciesDir;
        public static string dependenciesDirRoot;

        public static string DependenciesDir
        {
            get
            {
                if (dependenciesDir == null)
                {
                    dependenciesDirRoot = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                                                      .PathJoin("CS-Script", "CSScriptNpp");

                    dependenciesDir = dependenciesDirRoot.PathJoin(Assembly.GetExecutingAssembly().GetName().Version.ToString());
                }

                return dependenciesDir;
            }
        }

        public static string syntaxerDir;
        public static string cscsDir;

        public static void InitSyntaxer(string sourceDir)
        {
            // needs to be a separate routine to avoid premature assembly loading
            DeploySyntaxer(sourceDir);
            CSScriptIntellisense.Syntaxer.StartServer(onlyIfNotRunning: true);
        }

        public static void DeploySyntaxer(string sourceDir)
        {
            syntaxerDir =
            CSScriptIntellisense.Syntaxer.syntaxerDir = DependenciesDir.PathJoin("Roslyn");
            CSScriptIntellisense.Syntaxer.cscsFile = pluginDir.PathJoin("cscs.exe");

            //#if !DEBUG
            if (!Directory.Exists(syntaxerDir) || !File.Exists(syntaxerDir.PathJoin("syntaxer.exe")))
            //#endif
            {
                Directory.CreateDirectory(syntaxerDir);

                SafeCopy("CSSRoslynProvider.dll", sourceDir, DependenciesDir);

                CSScriptIntellisense.Syntaxer.Exit();
                SafeCopy("syntaxer.exe", sourceDir, syntaxerDir);

                var oldSyntaxerVersions = Directory.GetDirectories(Path.GetDirectoryName(DependenciesDir)).Where(x => x != DependenciesDir);
                foreach (var dir in oldSyntaxerVersions)
                    DeleteDir(dir);
            }
        }

        static void DeleteDir(string dir)
        {
            foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                try
                { File.Delete(file); }
                catch { }

            for (int i = 0; i < 3 && Directory.Exists(dir); i++)
                try { Directory.Delete(dir, true); }
                catch { Thread.Sleep(200); }
        }

        static void SafeCopy(string file, string srcDir, string destDir)
        {
            try
            {
                File.Copy(srcDir.PathJoin(file),
                          destDir.PathJoin(file),
                          true);
            }
            catch { }
        }

        public static void LoadRoslynResources()
        {
            Task.Factory.StartNew(() =>
            {
                //must be a separate method to allow assembly probing
                InitSyntaxer(pluginDir);
                CSScriptHelper.InitRoslyn();
            });
        }

        public static void Init()
        {
            try
            {
                // Debug.Assert(false);
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                pluginDir = Assembly.GetExecutingAssembly().Location.GetDirName();

                Environment.SetEnvironmentVariable("CSScriptNpp_dir", pluginDir);
                Environment.SetEnvironmentVariable("NPP_HOSTING", "true");
                ConnectPlugins();
            }
            catch (Exception e)
            {
                var customLog = Path.Combine(PluginEnv.LogDir, "last_startup_error.txt");
                File.WriteAllText(customLog, e.ToString());
                throw;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var customLog = Path.Combine(PluginEnv.LogDir, "last_unhandled_error.txt");
            File.WriteAllText(customLog, e.ToString());
        }

        static bool ExistAndNotOlderThan(string file, string fileToCompareTo)
        {
            return File.Exists(file) && new Version(FileVersionInfo.GetVersionInfo(file).ProductVersion) >= new Version(FileVersionInfo.GetVersionInfo(fileToCompareTo).ProductVersion);
        }

        static void notinuse_RestorePluginTree(string pluginDir)
        {
            MessageBox.Show(" RestorePluginTree(string pluginDir)");
            try
            {
                var files = Directory.GetDirectories(pluginDir)
                                     .SelectMany(x => Directory.GetFiles(x))
                                     .Select(x => new { FileInSubDir = x, FileInRoot = Path.Combine(pluginDir, Path.GetFileName(x)) })
                                     .Where(x => ExistAndNotOlderThan(x.FileInRoot, x.FileInSubDir))
                                     .Select(x => x.FileInRoot)
                                     .ToArray();

                foreach (var item in files)
                    try
                    {
                        File.Delete(item);
                    }
                    catch { }
            }
            catch { }
        }

        static void ConnectPlugins()
        {
            try
            {
                CSScriptHelper.NotifyClient = CSScriptIntellisense.npp.SetStatusbarLabel;
                CSScriptIntellisense.CSScriptHelper.GetEngineExe = () => CSScriptHelper.cscs_exe;
                UltraSharp.Cecil.Reflector.GetCodeCompileOutput = CSScriptHelper.GetCodeCompileOutput;
            }
            catch (Exception e)
            {
                Logger.LogAsDebugAsync(e, 500);
            }

            CSScriptIntellisense.Logger.Error = PluginLogger.Error;
            CSScriptIntellisense.Logger.Debug = PluginLogger.Debug;
            Task.Factory.StartNew(() =>
            {
                //Logger.LogAsDebugAsync("Plugin has been loaded", 1000);
            });

            // MessageBox.Show("rrr");
        }
    }
}