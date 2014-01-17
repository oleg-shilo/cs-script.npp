using System;
using System.IO;
using System.Linq;
using DbMon.NET;

namespace DbMon
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            //RefreshListeners();
            try
            {
                DebugMonitor.OnOutputDebugString += DebugMonitor_OnOutputDebugString;
                DebugMonitor.Start();
                return 0;
            }
            catch (AlreadyRunningException)
            {
                return 3;
            }
            catch (Exception e)
            {
                Console.WriteLine("===== Error: " + e.Message + " =====");
                return 4;
            }
        }

        static void DebugMonitor_OnOutputDebugString(int pid, string text)
        {
            string msg = string.Format("{0}: {1}", pid, text);
            Console.Write(msg);

            //lock (listeners)
            //{
            //foreach (var item in listeners)
            //    Win32.SendWindowsStringMessage(item, 0, msg);
            //}
        }

        //static List<IntPtr> listeners = new List<IntPtr>();

        //static IEnumerable<IntPtr> GetListeners()
        //{
        //    var retval = new List<IntPtr>();

        //    foreach (IntPtr handle in Win32.GetDesktopWindows())
        //    {
        //        IntPtr listener = Win32.GetChild(handle, "CS-Script.Npp-Output");
        //        if (listener != IntPtr.Zero)
        //            retval.Add(listener);
        //    }

        //    return retval.Distinct();
        //}


        //static void RefreshListeners()
        //{
        //    lock (listeners)
        //    {
        //        listeners.Clear();
        //        listeners.AddRange(GetListeners());
        //    }
        //}

    }

    //too heavy for processing on the listener side
    //class Win32
    //{
    //    public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

    //    //[DllImport("user32.dll", CharSet = CharSet.Auto)]
    //    //public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, uint lParam);

    //    //For use with WM_COPYDATA and COPYDATASTRUCT
    //    [DllImport("User32.dll", EntryPoint = "PostMessage")]
    //    public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

    //    //For use with WM_COPYDATA and COPYDATASTRUCT
    //    [DllImport("User32.dll", EntryPoint = "SendMessage")]
    //    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

    //    [DllImport("user32")]
    //    [return: MarshalAs(UnmanagedType.Bool)]
    //    public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);

    //    [DllImport("user32.dll")]
    //    public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumWindowProc lpfn, IntPtr lParam);

    //    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    //    internal static extern int GetWindowTextLength(IntPtr hWnd);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    //    [DllImport("user32")]
    //    public static extern IntPtr GetDesktopWindow();

    //    public static IntPtr GetChild(IntPtr parent, string text)
    //    {
    //        IntPtr retval = IntPtr.Zero;

    //        Win32.EnumChildWindows(parent,
    //            (hWnd, param) =>
    //            {
    //                if (GetWindowText(hWnd) == text)
    //                {
    //                    retval = hWnd;
    //                    return false;
    //                }
    //                return true;
    //            },
    //            IntPtr.Zero);

    //        return retval;
    //    }

    //    public static string GetWindowText(IntPtr wnd)
    //    {
    //        int length = Win32.GetWindowTextLength(wnd);
    //        StringBuilder sb = new StringBuilder(length + 1);
    //        Win32.GetWindowText(wnd, sb, sb.Capacity);
    //        return sb.ToString();
    //    }

    //    public static IntPtr[] GetDesktopWindows()
    //    {
    //        var windows = new List<IntPtr>();
    //        IntPtr hDesktop = IntPtr.Zero; // current desktop
    //        bool success = Win32.EnumDesktopWindows(
    //            hDesktop,
    //            (hWnd, param) =>
    //            {
    //                windows.Add(hWnd);
    //                return true;
    //            },
    //            IntPtr.Zero);

    //        return windows.ToArray();
    //    }

    //    public const int WM_COPYDATA = 0x4A;

    //    public struct COPYDATASTRUCT
    //    {
    //        public IntPtr dwData;
    //        public int cbData;

    //        [MarshalAs(UnmanagedType.LPStr)]
    //        public string lpData;
    //    }

    //    static public int SendWindowsStringMessage(IntPtr hWnd, int wParam, string msg)
    //    {
    //        int result = 0;

    //        if ((int)hWnd > 0)
    //        {
    //            byte[] sarr = System.Text.Encoding.Default.GetBytes(msg);
    //            int len = sarr.Length;
    //            COPYDATASTRUCT cds;
    //            cds.dwData = (IntPtr)100;
    //            cds.lpData = msg;
    //            cds.cbData = len + 1;
    //            result = SendMessage(hWnd, WM_COPYDATA, wParam, ref cds);
    //        }

    //        return result;
    //    }
    //}
}