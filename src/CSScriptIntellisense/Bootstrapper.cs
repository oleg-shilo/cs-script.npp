using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using UltraSharp.Cecil;

namespace CSScriptIntellisense
{
    class Bootstrapper
    {
        public static bool Init(bool standalone)
        {
            Task.Factory.StartNew(ClearReflectionCache);

            if (IsInConflictWithCSScriptNpp())
            {
                if (!Config.Instance.NewPluginConfictReported)
                    MessageBox.Show("Multiple instances of the 'C# Intellisense' plugin detected.\n" +
                                    "You can remove the standalone 'C# Intellisense' plugin as it will loaded anyway\n" +
                                    "as part of the 'CS-Script' plugin.", "Notepad++");

                Config.Instance.NewPluginConfictReported = true;
                Config.Instance.Save();
                return false;
            }
            else
            {
                Config.Instance.NewPluginConfictReported = true;
                Config.Instance.Save();

                ReflectorExtensions.IgnoreDocumentationExceptions = Config.Instance.IgnoreDocExceptions;

                if (standalone)
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                
                return true;
            }
        }

        static void ClearReflectionCache()
        {
            lock (typeof(Bootstrapper))
            {
                var anotherNppInstance = Process.GetProcessesByName("Notepad++").Where(p => p.Id != Process.GetCurrentProcess().Id).FirstOrDefault();
                if (anotherNppInstance == null && Directory.Exists(Reflector.DefaultTempDir))
                {
                    foreach(string file in Directory.GetFiles(Reflector.DefaultTempDir))
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
