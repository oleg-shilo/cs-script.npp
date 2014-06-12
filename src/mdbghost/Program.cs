using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, Unrestricted = true)]

class Program
{
    [MTAThread]
    static public void Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (args.First().StartsWith("/lp"))
            {
                bool isCurrentProcWin64 = Process.GetCurrentProcess().IsWin64();

                foreach (Process p in Process.GetProcesses())
                {
                    if (isCurrentProcWin64 != p.IsWin64())
                        continue;

                    try
                    {
                        string info = string.Format("{0}:{1}:{2}:{3}:{4}",
                                            p.ProcessName + ".exe",
                                            p.Id,
                                            isCurrentProcWin64 ? "x86" : "x64",
                                            p.IsManaged() ? "Managed" : "Native",
                                            p.MainWindowTitle);
                        Console.WriteLine(info);
                    }
                    catch { }
                }
            }
            else
            {
                string mdbg = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mdbg.exe");
                AppDomain.CurrentDomain.ExecuteAssembly(mdbg, args);
//                Bootstap.Main(args);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

static class ProcessExtensiuons
{
    public static bool IsWin64(this Process process)
    {
        if ((Environment.OSVersion.Version.Major > 5)
            || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
        {
            IntPtr processHandle;
            bool retVal;

            try
            {
                processHandle = Process.GetProcessById(process.Id).Handle;
            }
            catch
            {
                return false; // access is denied to the process
            }

            return IsWow64Process(processHandle, out retVal) && retVal;
        }

        return false; // not on 64-bit Windows
    }

    [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

    public static bool IsManaged(this Process proc)
    {
        try
        {
            return proc.Modules.Cast<ProcessModule>()
                               .Where(m => m.ModuleName.IsSameAs("mscorwks.dll", ignoreCase: true) ||      //OlderDesktopCLR
                                           m.ModuleName.IsSameAs("mscorlib.dll", ignoreCase: true) ||      //Mscorlib
                                           m.ModuleName.IsSameAs("mscorlib.ni.dll", ignoreCase: true) ||
                                           m.ModuleName.IsSameAs("mscoree.dll", ignoreCase: true) ||       //Desktop40CLR
                                           m.ModuleName.IsSameAs("mscoreei.ni.dll", ignoreCase: true))
                               .Any();
        }
        catch
        {
            return false;
        }
    }

    public static bool IsSameAs(this string text, string textToCompare, bool ignoreCase)
    {
        return string.Compare(text, textToCompare, ignoreCase) == 0;
    }
}