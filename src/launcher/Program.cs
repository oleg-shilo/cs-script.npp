using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace launcher
{
    static class Program
    {
        static void Main(string[] args)
        {
            // -restartApp <prevInstanceProcId> <appPath>
            // -stop_roslyn

            if (args.FirstOrDefault().IsAnyOf("/start", "-start"))
                Restart(args.Skip(1).ToArray());
            else if (args.FirstOrDefault().IsAnyOf("/stop_roslyn", "-stop_roslyn"))
                StopVBCSCompilers();
        }

        public static void StopVBCSCompilers()
        {
            foreach (var p in Process.GetProcessesByName("VBCSCompiler"))
                try { p.Kill(); }
                catch { } //cannot analyse main module as it may not be accessible for x86 vs. x64 reasons
        }

        static void Restart(string[] args)
        {
            try
            {
                //Debug.Assert(false);
                Thread.Sleep(100);
                string appPath = args[1];
                int id = int.Parse(args[0]);

                var proc = Process.GetProcesses().Where(x => x.Id == id).FirstOrDefault();
                if (proc.IsRunning())
                    proc.WaitForExit();

                Process.Start(appPath);
            }
            catch
            {
            }
        }

        static bool IsAnyOf(this string text, params string[] patterns)
        {
            if (text != null)
                foreach (var item in patterns)
                    if (text == item)
                        return true;

            return false;
        }

        static bool IsRunning(this Process p)
        {
            if (p == null)
                return false;

            try
            {
                return !p.HasExited;
            }
            catch { }
            return true;
        }
    }
}