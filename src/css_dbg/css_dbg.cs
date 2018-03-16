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
    static bool force_to_break = false;

    static public void Main(string[] args)
    {
        //Debug.Assert(false);
        //<script_engine_executable> <script_args> [-css_break]
        if (args.Count() >= 2)
        {
            string engine_name = args[0];
            try
            {
                var actual_args = args.Skip(1).ToArray();

                if (actual_args.LastOrDefault() == "-css_break") //for future use
                {
                    force_to_break = true;
                    actual_args = actual_args.Take(actual_args.Length - 1).ToArray();
                }

                ExecuteScript(engine_name, args.Skip(1).ToArray());
            }
            catch
            {
                // MessageBox.Show(e.Message, "css_dbg");
            }
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

        if (force_to_break)
        {
            if (System.Diagnostics.Debugger.IsAttached) // In case a debugger is already attached,
                System.Diagnostics.Debugger.Break();    // Break() execution to start debugging.
            else                                        // Otherwise, Launch() will break execution
                System.Diagnostics.Debugger.Launch();   // and let the OS pop up a message box to
                                                        // let you to lauch the desired debugger.
        }

        AppDomain.CurrentDomain.ExecuteAssembly(css_asm, args);
    }
}