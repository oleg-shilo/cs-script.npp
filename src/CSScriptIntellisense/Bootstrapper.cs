using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using UltraSharp.Cecil;

namespace CSScriptIntellisense
{

 
    class Bootstrapper
    {
        //'standalone' is the deployment model that includes CSSCriptIntellisense.dll plugin only
        public static bool Init(bool standalone)
        {
            ReflectorExtensions.IgnoreDocumentationExceptions = Config.Instance.IgnoreDocExceptions;

            if (standalone) //CSScriptIntellisense Plugin
            {
                bool inConflict = IsInConflictWithCSScriptNpp();
                if (!inConflict)
                {
                    Task.Factory.StartNew(ClearReflectionCache);
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                    InitLogging();
                }
                return inConflict;
            }

            return true;
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
        }

        static void ClearReflectionCache()
        {
            lock (typeof(Bootstrapper))
            {
                var anotherNppInstance = Process.GetProcessesByName("Notepad++").Where(p => p.Id != Process.GetCurrentProcess().Id).FirstOrDefault();
                if (anotherNppInstance == null && Directory.Exists(Reflector.DefaultTempDir))
                {
                    foreach (string file in Directory.GetFiles(Reflector.DefaultTempDir))
                        try
                        {
                            File.Delete(file);
                        }
                        catch { }
                }
            }
        }

        static public bool IsInConflictWithCSScriptNpp()
        {
            //CSScriptIntellisense Plugin - C:\Program Files (x86)\Notepad++\plugins\CSScriptIntellisense.dll 
            //CSScriptNpp Plugin - C:\Program Files (x86)\Notepad++\plugins\CSScriptNpp\CSScriptIntellisense.dll 

            //conflict criteria: this asm is part of CSScriptIntellisense plugin and CSScriptNpp plugin is installed
            string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return File.Exists(Path.Combine(rootDir, @"CSScriptNpp\CSScriptIntellisense.dll"));
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                if (args.Name.StartsWith("Mono.Cecil,"))
                    return Assembly.LoadFrom(Path.Combine(rootDir, @"CSharpIntellisense\Mono.Cecil.dll"));
                else if (args.Name.StartsWith("ICSharpCode.NRefactory.CSharp,"))
                    return Assembly.LoadFrom(Path.Combine(rootDir, @"CSharpIntellisense\ICSharpCode.NRefactory.CSharp.dll"));
                else if (args.Name.StartsWith("ICSharpCode.NRefactory,"))
                    return Assembly.LoadFrom(Path.Combine(rootDir, @"CSharpIntellisense\ICSharpCode.NRefactory.dll"));
                else if (args.Name.StartsWith("CSScriptLibrary,"))
                    return Assembly.LoadFrom(Path.Combine(rootDir, @"CSharpIntellisense\CSScriptLibrary.dll"));
            }
            catch { }
            return null;
        }
    }
}
