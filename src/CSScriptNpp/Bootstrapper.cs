using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace CSScriptNpp
{
    class Bootstrapper
    {
        public static void Init()
        {
            try
            {
                //Debug.Assert(false);
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                InitLogging();
                ConnectPlugins(); //must be a separate method to allow assembly probing
            }
            catch(Exception e)
            {
                var customLog = Path.Combine(Plugin.LogDir, "last_startup_error.txt");
                File.WriteAllText(customLog, e.ToString());
                throw;
            }
        }

        static void InitLogging()
        {
            if (LogManager.Configuration != null)
            {
                FileTarget target = LogManager.Configuration.FindTargetByName("logfile") as FileTarget;
                target.FileName = Path.Combine(Plugin.LogDir, "app-log.txt");
                target.ArchiveFileName = Path.Combine(Plugin.LogDir, "app-log.old.txt");
            }
            else
            {
                var config = new LoggingConfiguration();

                var target = new FileTarget
                {
                    FileName = Path.Combine(Plugin.LogDir, "app-log.txt"),
                    ArchiveFileName = Path.Combine(Plugin.LogDir, "app-log.old.txt"),
                    Layout = "${longdate} ${processid} ${logger} ${message}",
                    ArchiveEvery = FileArchivePeriod.Day,
                    ConcurrentWrites = true,
                    MaxArchiveFiles = 1,
                    KeepFileOpen = false,
                    Encoding = Encoding.Default,
                    ArchiveNumbering = ArchiveNumberingMode.Rolling
                };
                var dbRule = new LoggingRule("*", LogLevel.Debug, target);

                config.AddTarget("logfile", target);
                config.LoggingRules.Add(dbRule);

                LogManager.Configuration = config;
            }

            Logger.LogAsDebug("Logger has been initialized");
        }

        static void ConnectPlugins()
        {
            try
            {
                CSScriptHelper.NotifyClient = CSScriptIntellisense.Npp.SetStatusbarLabel;
                CSScriptIntellisense.CSScriptHelper.GetEngineExe = () => CSScriptHelper.cscs_exe;
                UltraSharp.Cecil.Reflector.GetCodeCompileOutput = CSScriptHelper.GetCodeCompileOutput;
            }
            catch(Exception e)
            {
                e.LogAsError();
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                if (args.Name.StartsWith("CSScriptIntellisense,"))
                    return Assembly.LoadFrom(Path.Combine(rootDir, @"CSScriptNpp\CSScriptIntellisense.dll"));
                else if (args.Name.StartsWith("CSScriptLibrary,"))
                    return Assembly.LoadFrom(Path.Combine(rootDir, @"CSScriptNpp\CSScriptLibrary.dll"));
                else if (args.Name.StartsWith("NLog,"))
                    return Assembly.LoadFrom(Path.Combine(rootDir, @"CSScriptNpp\NLog.dll"));
                else if (args.Name == Assembly.GetExecutingAssembly().FullName)
                    return Assembly.GetExecutingAssembly();
            }
            catch { }
            return null;
        }
    }
}