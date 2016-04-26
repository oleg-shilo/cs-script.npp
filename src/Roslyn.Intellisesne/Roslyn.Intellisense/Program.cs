using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace RoslynIntellisense
{
    // Roslyn packages:
    // 1.1.0 - currently used
    // 1.2.1 - very slow (e.g. with GetPossibleNamespaces)
    // 2.0.0 - beta1 is OK but Beta
    // Microsoft.CodeAnalysis.CSharp
    // Microsoft.CodeAnalysis.CSharp.Workspaces

    class Program
    {
        static int Main(string[] args)
        {
            //Debug.Assert(false);

            //Formatting(args);
            //IntellisenseSimple();
            //Intellisense();


             
            if (args.Contains("/test"))
                return Test();
            else if (args.Contains("/detect"))
                return Detect();
            else
                return 0;
        }

        static int Test()
        {
            //var engine 
            //Autocompleter.FindMissingUsings();
            //Autocompleter.FindMissingUsings11();
            //Intellisense2();
            ResolveNamespaces();
            return 0;
        }

        static int ResolveNamespaces()
        {
            string code = @"class Test 
            {
                void Foo()
                {
                    Form
                }
            }";

            var sources = new Tuple<string, string>[0];
            var asms = new[] { typeof(object).Assembly.Location, typeof(Form).Assembly.Location };

            var engine = new Engine();
            engine.Preload();
            engine.ResetProject(sources, asms);

            var result = engine.GetPossibleNamespaces(code, "Form", "script.cs");
            result = engine.GetPossibleNamespaces(code, "Form", "script.cs");
            return 0;
        }

        static int Detect()
        {
            try
            {
                Console.WriteLine("success");
                return 0;
            }
            catch (Exception)
            {
                Console.WriteLine("failure");
                return 1;
            }
        }

        static void Formatting(string[] args)
        {
            string file = @"C:\Users\%USERNAME%\Documents\C# Scripts\New Script34.cs";
            file = Environment.ExpandEnvironmentVariables(file);
            args = new[] { file };
            var code = File.ReadAllText(args.First());

            string formattedCode = RoslynIntellisense.Formatter.FormatHybrid(code);

            Console.WriteLine(formattedCode);
        }

        static void Intellisense()
        {
            string script = @"E:\Galos\Projects\CS-Script.Npp\CSScript.Npp\bin\Plugins\CSScriptNpp\NLog.test.cs";
            var sources = new[] { new Tuple<string, string>(File.ReadAllText(script), script) };
            var asms = new[] { @"C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll",
                               @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.IO\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.IO.dll",
                               @"C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Reflection\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Reflection.dll" };

            var engine = new Engine();
            engine.Preload();
            engine.ResetProject(sources, asms);

            var code = File.ReadAllText(script);

            var result = engine.GetCompletionData(code, 598, script);
        }

        static int Intellisense2()
        {
            var asms = new[]
            {
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Text.RegularExpressions\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Text.RegularExpressions.dll",
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Linq\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Linq.dll",
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.IO\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.IO.dll",
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll",
            };

            var code = @"
using System;
class Script
{
    static public void Main(string[] args)
    {
        TestA.|Who();
    }
}";
            var includeCode = @"
using System;

public class TestA
{
    static public void Who()
    {
        Console.WriteLine(""TestA"");
    }
    }";
            var sources = new List<Tuple<string, string>> { new Tuple<string, string> (includeCode, "code.cs") };

            int carret = code.IndexOf("|");
            code = code.Replace("|", "");

            var result = Autocompleter.GetAutocompletionFor(code, carret, asms.ToArray(), sources);

            Console.WriteLine("----------------------------------");
            Console.ReadLine();
            return 0;
        }

        static void IntellisenseSimple()
        {
            new Engine().Preload();
            var code = @"class Script
{
    static void Main()
    {
        var test = ""ttt"";
        System.Console.WriteLine($""Hello World!{test.Ends";

            var ttt = Autocompleter.GetAutocompletionFor(code, 132);
            Console.WriteLine("----------------------------------");
            Console.ReadLine();

            var ttt3 = Autocompleter.GetAutocompletionFor(code, 132);
            Console.ReadLine();
        }
    }

}
