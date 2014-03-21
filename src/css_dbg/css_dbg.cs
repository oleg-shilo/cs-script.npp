using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static public void Main(string[] args)
    {
        //<host_process_id> <script_engine_executable> <script_args>
        if (args.Count() >= 3)
        {
            string host_process = args[0];
            string engine_name = args[1];

            Task.Factory.StartNew(() => MonitorHost(host_process));
            ExecuteScript(engine_name, args.Skip(2).ToArray());
        }
    }

    static void ExecuteScript(string engine, string[] args)
    {
        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string css_asm = Path.Combine(dir, engine);
        AppDomain.CurrentDomain.ExecuteAssembly(css_asm, args);
    }

    static void MonitorHost(string host)
    {
        try
        {
            int procId = int.Parse(host);
            Process.GetProcessById(procId).WaitForExit();
            Process.GetCurrentProcess().Kill();
        }
        catch { }
    }
}

