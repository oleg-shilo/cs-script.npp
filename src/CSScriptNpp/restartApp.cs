using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;

class Script
{
    static public void Main(string[] args)
    {
        //restartApp <prevInstanceProcId> <appPath> 
        try
        {
            //Debug.Assert(false);
            Thread.Sleep(100);
            string appPath = args[1];
            int id = int.Parse(args[0]);

            var proc = Process.GetProcesses().Where(x => x.Id == id).FirstOrDefault();
            if(proc != null && !proc.HasExited)
                 proc.WaitForExit();

            Process.Start(appPath);
        }
        catch
        {
        }
    }
}
