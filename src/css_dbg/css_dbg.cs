using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

class Program
{
    static public void Main(string[] args)
    {
        //Debug.Assert(false);
        //<script_engine_executable> <script_args>
        if (args.Count() >= 2)
        {
            string engine_name = args[0];
            ExecuteScript(engine_name, args.Skip(1).ToArray());
        }
    }

    static void ExecuteScript(string engine, string[] args)
    {
        string css_asm;

        if (Path.GetDirectoryName(engine) != "")
        {
            css_asm = Path.GetFullPath(engine);
        }
        else
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            css_asm = Path.Combine(dir, engine);
        }

        if (css_asm.EndsWith("csws.exe", StringComparison.OrdinalIgnoreCase))
                Environment.SetEnvironmentVariable("CSS_IsRuntimeErrorReportingSupressed", "true");
            AppDomain.CurrentDomain.ExecuteAssembly(css_asm, args);
    }
}

