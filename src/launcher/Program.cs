using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            // -kill_process

            var pk_name = args.FirstOrDefault(x => x.StartsWith("/kill_process:") || x.StartsWith("-kill_process:"));

            if (args.FirstOrDefault().IsAnyOf("/start", "-start"))
                Restart(args.Skip(1).ToArray());
            else if (args.FirstOrDefault().IsAnyOf("/stop_roslyn", "-stop_roslyn"))
                StopVBCSCompilers();
            else if (pk_name != null) // -kill_process.
            {
                // It's important to use launcher.exe for killing VBCSCompiler.exe.
                // launcher.exe is "AnyCPU" soe it will be able to lookup VBCSCompiler.exe,
                // which ios also  "AnyCPU"
                KillProcessByName(pk_name.Substring("/kill_process:".Length));
            }
        }

        static void KillProcessByName(string name)
        {
            var is64 = IntPtr.Size == 8;

            if (name != Path.GetFileName(name)) // file path
            {
                Regex wildcard = null;

                if (name.Contains("*") || name.Contains("?"))
                {
                    // sample
                    // -kpname:C:\ProgramData\CS-Script\CSScriptNpp\*\VBCSCompiler.exe
                    var pattern = name.ConvertSimpleExpToRegExp();
                    wildcard = new Regex(pattern, RegexOptions.IgnoreCase);
                }

                Process.GetProcesses()
                       .Where(x =>
                       {
                           try
                           {
                               string file = x.FileName();
                               if (wildcard != null)
                               {
                                   return wildcard.IsMatch(file);
                               }
                               else
                               {
                                   return string.Compare(file, name, true) == 0;
                               }
                           }
                           catch
                           {
                               return false;
                           }
                       })
                       .ToList()
                       .ForEach(x =>
                       {
                           try
                           {
                               Console.WriteLine("Terminating: " + x.MainModule?.FileName);
                               x.Kill();
                           }
                           catch { }
                       });
            }
            else
            {
                foreach (var p in Process.GetProcessesByName(name))
                    try
                    {
                        Console.WriteLine("Terminating: " + p.MainModule?.FileName);
                        p.Kill();
                    }
                    catch { }
            }
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

public static class Utils
{
    public static string FileName(this Process proc)
    {
        return proc.MainModule?.FileName;
    }

    //Credit to MDbg team: https://github.com/SymbolSource/Microsoft.Samples.Debugging/blob/master/src/debugger/mdbg/mdbgCommands.cs
    public static string ConvertSimpleExpToRegExp(this string simpleExp)
    {
        var sb = new StringBuilder();
        sb.Append("^");
        foreach (char c in simpleExp)
        {
            switch (c)
            {
                case '\\':
                case '{':
                case '|':
                case '+':
                case '[':
                case '(':
                case ')':
                case '^':
                case '$':
                case '.':
                case '#':
                case ' ':
                    sb.Append('\\').Append(c);
                    break;

                case '*':
                    sb.Append(".*");
                    break;

                case '?':
                    sb.Append(".");
                    break;

                default:
                    sb.Append(c);
                    break;
            }
        }

        sb.Append("$");
        return sb.ToString();
    }
}