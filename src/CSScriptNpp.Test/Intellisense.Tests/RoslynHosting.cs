using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CSScriptIntellisense;

namespace Testing
{
    public class RoslynHosting
    {
        static RoslynHosting()
        {
            Environment.SetEnvironmentVariable("suppress_roslyn_preloading", "true");
            Environment.SetEnvironmentVariable("roslynintellisense_path",
                                                Path.GetFullPath(@"..\..\..\..\..\CSScript.Npp\src\Roslyn.Intellisesne\Roslyn.Intellisense\bin\Debug\RoslynIntellisense.exe"));

            Config.Instance.RoslynIntellisense = true;

            Init();
        }

        static string roslynBinDir = null;

        static public void Init()
        {
            if (roslynBinDir == null)
            {
                roslynBinDir = Path.GetFullPath(@"..\..\..\..\..\CSScript.Npp\src\Roslyn.Intellisesne\Roslyn.Intellisense\Microsoft.CodeAnalysis.CSharp.1.1.0");

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var path = $"{roslynBinDir}\\{args.Name.Split(',').First()}";
            if (File.Exists(path + ".dll"))
                return Assembly.LoadFrom(path + ".dll");
            if (File.Exists(path + ".exe"))
                return Assembly.LoadFrom(path + ".exe");
            return null;
        }
    }

}