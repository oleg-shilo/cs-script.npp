using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public static class Bootstrapper
    {
        public static string pluginDir = Assembly.GetExecutingAssembly().Location.GetDirName();

        public static void LoadRoslynResources()
        {
            Task.Factory.StartNew(() =>
            {
                //must be a separate method to allow assembly probing
                Runtime.Init();
                CSScriptHelper.InitRoslyn();
            });
        }

        public static void Init()
        {
            try
            {
                // Debug.Assert(false);
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                Environment.SetEnvironmentVariable("CSScriptNpp_dir", pluginDir);
                Environment.SetEnvironmentVariable("NPP_HOSTING", "true");
#if DEBUG
                //Environment.SetEnvironmentVariable("CSSCRIPT_NPP_REPO_URL", "http://csscript.net/npp/latest_version_dbg.txt");
#endif
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

        static void ConnectPlugins()
        {
            try
            {
                CSScriptHelper.NotifyClient = CSScriptIntellisense.npp.SetStatusbarLabel;
                CSScriptIntellisense.CSScriptHelper.GetEngineExe = () => Runtime.cscs_asm;
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
                // Logger.LogAsDebugAsync("Plugin has been loaded", 1000);
            });
        }
    }
}