using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CompatibilityTest
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Notepad++\plugins\CSScriptNpp.dll");

            if (args.Any())
                file = args.First();

            Console.WriteLine();

            if (!File.Exists(file) || !file.EndsWith("CSScriptNpp.dll", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Please pass a correct location of the CSScriptNpp.dll file.");
                return;
            }

            try
            {
                var init = Assembly.LoadFrom(file)
                                   .GetTypes()
                                   .First(t => t.FullName == "CSScriptNpp.Bootstrapper")
                                   .GetMethod("Init");

                init.Invoke(null, new object[0]);
                Console.WriteLine("Plugin loading test was successful.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Plugin loading test failed.\nError: " + e);
            }

            Console.WriteLine("\n\nPress 'Enter' to continue . . .");
            Console.ReadLine();
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("Trying to resolve " + args.Name);
            return null;
        }
    }
}