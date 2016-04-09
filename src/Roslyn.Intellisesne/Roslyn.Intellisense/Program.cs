using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RoslynIntellisense
{
    public delegate void D2(string s);
    ///public delegate IEnumerable<ICompletionData> GetAutocompletionFor(string code, int position, string[] references, string[] includes);

    class Program
    {
        static void Main(string[] args)
        {
            //Debug.Assert(false);

            //Formatting(args);
            //IntellisenseSimple();
            Intellisense();
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

        static void Intellisense2()
        {
            var asms = new[]
            {
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Text.RegularExpressions\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Text.RegularExpressions.dll",
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Linq\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Linq.dll",
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.IO\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.IO.dll",
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll",
            };

            //var code = File.ReadAllText(@"E:\Galos\Projects\GitHub\cp2gp.Wiki.cs");
            var code = File.ReadAllText(@"C:\Users\osh\Documents\C# Scripts\New Script34.cs");

            //var result = Autocompleter.GetAutocompletionFor(code, 195, asms.ToArray());
            var result = Autocompleter.GetAutocompletionFor(code, 633, asms.ToArray());

            Console.WriteLine("----------------------------------");
            Console.ReadLine();
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
