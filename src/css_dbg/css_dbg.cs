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
        //<script_engine_executable> <script_args>
        if (args.Count() >= 2)
        {
            string engine_name = args[0];
            ExecuteScript(engine_name, args.Skip(1).ToArray());
        }
    }

    static void ExecuteScript(string engine, string[] args)
    {
        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string css_asm = Path.Combine(dir, engine);
        AppDomain.CurrentDomain.ExecuteAssembly(css_asm, args);
    }
}

