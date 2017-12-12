using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UltraSharp.Cecil;

namespace CSScriptIntellisense
{
    public class Logger
    {
        public static Action<object> Error;
        public static Action<object> Debug;

        static Logger()
        {
            Error = msg => { };
            Debug = msg => { };
        }
    }

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
                }
                return inConflict;
            }

            return true;
        }

        static void ClearReflectionCache()
        {
            lock (typeof(Bootstrapper))
            {
                var anotherNppInstance = Process.GetProcessesByName("Notepad++").FirstOrDefault(p => p.Id != Process.GetCurrentProcess().Id);
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

        public static bool IsInConflictWithCSScriptNpp()
        {
            //CSScriptIntellisense Plugin - C:\Program Files (x86)\Notepad++\plugins\CSScriptIntellisense.dll
            //CSScriptNpp Plugin - C:\Program Files (x86)\Notepad++\plugins\CSScriptNpp\CSScriptIntellisense.dll

            //conflict criteria: this asm is part of CSScriptIntellisense plugin and CSScriptNpp plugin is installed
            string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // checking rootDir is not null
            return rootDir != null && File.Exists(Path.Combine(rootDir, @"CSScriptNpp\CSScriptIntellisense.dll"));
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (rootDir != null)
            {
                try
                {
                    if (args.Name.StartsWith("Mono.Cecil,"))
                        return Assembly.LoadFrom(Path.Combine(rootDir, @"CSharpIntellisense\Mono.Cecil.dll"));
                    if (args.Name.StartsWith("ICSharpCode.NRefactory.CSharp,"))
                        return Assembly.LoadFrom(Path.Combine(rootDir, @"CSharpIntellisense\ICSharpCode.NRefactory.CSharp.dll"));
                    if (args.Name.StartsWith("ICSharpCode.NRefactory,"))
                        return Assembly.LoadFrom(Path.Combine(rootDir, @"CSharpIntellisense\ICSharpCode.NRefactory.dll"));
                    if (args.Name.StartsWith("CSScriptLibrary,"))
                        return Assembly.LoadFrom(Path.Combine(rootDir, @"CSharpIntellisense\CSScriptLibrary.dll"));
                }
                catch { }
            }
            
            return null;
        }
    }
}