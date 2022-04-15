using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using CSScriptIntellisense;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace CSScriptNpp
{
    public static class Utils
    {
        public static string[] SplitCommandLine(this string commandLine)
        {
            bool inQuotes = false;
            bool isEscaping = false;

            return commandLine.Split(c =>
            {
                if (c == '\\' && !isEscaping) { isEscaping = true; return false; }

                if (c == '\"' && !isEscaping)
                    inQuotes = !inQuotes;

                isEscaping = false;

                return !inQuotes && Char.IsWhiteSpace(c)/*c == ' '*/;
            })
            .Select(arg => arg.Trim().TrimMatchingQuotes('\"').Replace("\\\"", "\""))
            .Where(arg => !string.IsNullOrEmpty(arg))
            .ToArray();
        }

        public static IEnumerable<string> Split(this string str, Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        public static string TrimMatchingQuotes(this string input, char quote)
        {
            if (input.Length >= 2)
            {
                //"-sconfig:My Script.cs.config"
                if (input.First() == quote && input.Last() == quote)
                {
                    return input.Substring(1, input.Length - 2);
                }
                //-sconfig:"My Script.cs.config"
                else if (input.Last() == quote)
                {
                    var firstQuote = input.IndexOf(quote);
                    if (firstQuote != input.Length - 1) //not the last one
                        return input.Substring(0, firstQuote) + input.Substring(firstQuote + 1, input.Length - 2 - firstQuote);
                }
            }
            return input;
        }

        public static bool IsVS2017PlusAvailable
        {
            get
            {
                if (Environment.GetEnvironmentVariable("CSSCRIPT_VSEXE") != null)
                    return true;

                using (var vs2017 = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.15.0", false))
                using (var vs2019 = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.16.0", false))
                using (var vs2022 = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.17.0", false))
                {
                    return (vs2022 != null || vs2019 != null || vs2017 != null);
                }
            }
        }

        static public string GetStatementAtCaret()
        {
            var document = Npp.GetCurrentDocument();

            string expression = document.GetSelectedText();
            if (string.IsNullOrWhiteSpace(expression))
                expression = document.GetStatementAtPosition();
            return expression;
        }

        public static void EmbeddShortcutIntoTooltip(this ToolStripButton button, string shortcut)
        {
            string[] tooltipLines = button.ToolTipText.Split('\n');
            button.ToolTipText = string.Join(Environment.NewLine, tooltipLines.TakeWhile(x => !x.StartsWith("Shortcut: ")).ToArray());
            button.ToolTipText += Environment.NewLine + "Shortcut: " + shortcut;
        }

        public static bool IsSameTimestamp(string file1, string file2)
        {
            return File.GetLastWriteTimeUtc(file1) == File.GetLastWriteTimeUtc(file2);
        }

        public static void SetSameTimestamp(string fileSrc, string fileDest)
        {
            File.SetLastWriteTimeUtc(fileDest, File.GetLastWriteTimeUtc(fileSrc));
        }

        //public static bool IsWin64(this Process process)
        //{
        //    if ((Environment.OSVersion.Version.Major > 5)
        //        || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
        //    {
        //        IntPtr processHandle;
        //        bool retVal;

        //        try
        //        {
        //            processHandle = Process.GetProcessById(process.Id).Handle;
        //        }
        //        catch
        //        {
        //            return false; // access is denied to the process
        //        }

        //        return IsWow64Process(processHandle, out retVal) && retVal;
        //    }

        //    return false; // not on 64-bit Windows
        //}

        //[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        //public static bool IsManaged(this Process proc)
        //{
        //    try
        //    {
        //        return proc.Modules.Cast<ProcessModule>()
        //                           .Where(m => m.ModuleName.IsSameAs("mscorwks.dll", ignoreCase: true) ||      //OlderDesktopCLR
        //                                       m.ModuleName.IsSameAs("mscorlib.dll", ignoreCase: true) ||      //Mscorlib
        //                                       m.ModuleName.IsSameAs("mscorlib.ni.dll", ignoreCase: true) ||
        //                                       m.ModuleName.IsSameAs("mscoree.dll", ignoreCase: true) ||       //Desktop40CLR
        //                                       m.ModuleName.IsSameAs("mscoreei.ni.dll", ignoreCase: true))
        //                           .Any();
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public static bool IsSameAs(this string text, string textToCompare, bool ignoreCase)
        {
            return string.Compare(text, textToCompare, ignoreCase) == 0;
        }

        public static bool IsEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool StartsWithAny(this string text, params string[] patterns)
        {
            return patterns.Any(x => text.StartsWith(x));
        }

        //type.member       - resolve
        //method("=(test)") - invoke
        //variable="(test)" - setter
        internal static bool IsInvokeExpression(this string text)
        {
            int bracketPos = text.IndexOf("(");
            int equalPos = text.IndexOf("=");
            if (bracketPos != -1 && (equalPos == -1 || bracketPos < equalPos))
                return true;
            return false;
        }

        internal static bool IsSetExpression(this string text)
        {
            int bracketPos = text.IndexOf("(");
            int equalPos = text.IndexOf("=");
            if (equalPos != -1 && (bracketPos == -1 || equalPos < bracketPos))
                return true;
            return false;
        }

        internal static bool IsResolveExpression(this string text)
        {
            int bracketPos = text.IndexOf("(");
            int equalPos = text.IndexOf("=");
            if (bracketPos == -1 && equalPos == -1)
                return true;
            return false;
        }

        internal static string NormalizeExpression(this string text)
        {
            if (text.IsSetExpression())
                return string.Join("=", text.Split(new[] { '=' }, 2).Select(x => x.Trim()).ToArray());
            else if (text.IsInvokeExpression())
                return string.Join("(", text.Split(new[] { '(' }, 2).Select(x => x.Trim()).ToArray());
            else
                return text;
        }

        public static string StripQuotation(this string text)
        {
            if (text.StartsWith("\"") && text.EndsWith("\""))
                return text.Substring(1, text.Length - 2);
            else
                return text;
        }

        public static ShortcutKey ParseAsShortcutKey(this string shortcutSpec, string displayName)
        {
            ShortcutKey retval;

            var parts = shortcutSpec.Split(':');

            string shortcutName = parts[0];
            string shortcutData = parts[1];

            try
            {
                var actualData = Config.Shortcuts.GetValue(shortcutName, shortcutData);
                retval = new ShortcutKey(actualData);
            }
            catch
            {
                Config.Shortcuts.SetValue(shortcutName, shortcutData);
                retval = new ShortcutKey(shortcutData);
            }

            Config.Shortcuts.MapDisplayName(shortcutName, displayName);

            return retval;
        }

        public static bool ParseAsFileReference(this string text, out string file, out int line, out int column)
        {
            if (text.ParseAsErrorFileReference(out file, out line, out column))
                return true;
            else if (text.ParseAsExceptionFileReference(out file, out line, out column))
                return true;
            else
                return false;
        }

        public static Icon NppBitmapToIcon(Bitmap bitmap)
        {
            using (Bitmap newBmp = new Bitmap(16, 16))
            {
                Graphics g = Graphics.FromImage(newBmp);
                ColorMap[] colorMap = new ColorMap[1];
                colorMap[0] = new ColorMap();
                colorMap[0].OldColor = Color.Fuchsia;
                colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                ImageAttributes attr = new ImageAttributes();
                attr.SetRemapTable(colorMap);
                //g.DrawImage(new Bitmap(@"E:\Dev\Notepad++.Plugins\NppScripts\css_logo_16x16.png"), new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                g.DrawImage(bitmap, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                return Icon.FromHandle(newBmp.GetHicon());
            }
        }

        static public void RestartNpp()
        {
            PluginBase.Editor.FileExit();

            string file = Npp.Editor.GetCurrentFilePath();

            if (string.IsNullOrEmpty(file)) //the Exit request has been processed and user did not cancel it
            {
                Application.DoEvents();

                int processIdToWaitForExit = Process.GetCurrentProcess().Id;
                string appToStart = Process.GetCurrentProcess().MainModule.FileName;

                bool lessReliableButLessIntrusive = true;

                if (lessReliableButLessIntrusive)
                {
                    var proc = new Process();
                    proc.StartInfo.FileName = appToStart;
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.Verb = "runas";
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                }
                else
                {
                    string restarter = Path.Combine(PluginEnv.PluginDir, "Updater.exe");

                    //the restarter will also wait for this process to exit
                    Process.Start(restarter, string.Format("/restart /asadmin {0} \"{1}\"", processIdToWaitForExit, appToStart));
                }
            }
        }
    }

    static class MouseControlledZoomingBehavier
    {
        static public void ChangeFontSize(this Control control, int sizeDelta)
        {
            control.Font = new Font(control.Font.FontFamily, control.Font.Size + sizeDelta);
        }

        static public void AttachMouseControlledZooming(this Control control, Action<Control, bool> customHandler = null)
        {
            AttachTo(control, customHandler);
        }

        static public void AttachTo(Control control, Action<Control, bool> customHandler = null)
        {
            control.MouseWheel += (s, e) => MapTxt_MouseWheel(s, e, customHandler);
        }

        static void MapTxt_MouseWheel(object sender, MouseEventArgs e, Action<Control, bool> customHandler)
        {
            if (KeyInterceptor.IsPressed(Keys.ControlKey))
            {
                var control = (Control)sender;
                if (customHandler != null)
                    customHandler(control, e.Delta > 0);
                else
                {
                    var fontSizeDelta = e.Delta > 0 ? 2 : -2;
                    control.ChangeFontSize(fontSizeDelta);
                }
            }
        }
    }

    class WaitableQueue<T> : Queue<T>
    {
        public string Name { get; set; }

        AutoResetEvent itemAvailable = new AutoResetEvent(true);

        public new void Enqueue(T item)
        {
            lock (this)
            {
                base.Enqueue(item);
                itemAvailable.Set();
            }
        }

        public T WaitItem()
        {
            while (true)
            {
                lock (this)
                {
                    if (base.Count > 0)
                    {
                        return base.Dequeue();
                    }
                }

                itemAvailable.WaitOne();
            }
        }
    }

    class MessageQueue
    {
        static WaitableQueue<string> commands = new WaitableQueue<string>();
        static WaitableQueue<string> notifications = new WaitableQueue<string>();
        static WaitableQueue<string> automationCommans = new WaitableQueue<string>();

        static public void AddCommand(string data)
        {
            if (data == "")
                Debug.Assert(false);

            if (data == "npp.exit")
                Debug.WriteLine("");
            commands.Enqueue(data);
        }

        static public void AddNotification(string data)
        {
            notifications.Enqueue(data);
        }

        static public void AddAutomationCommand(string data)
        {
            automationCommans.Enqueue(data);
        }

        static public void Clear()
        {
            notifications.Clear();
            commands.Clear();
        }

        static public string WaitForNotification()
        {
            return notifications.WaitItem();
        }

        static public string WaitForCommand()
        {
            return commands.WaitItem();
        }

        static public string WaitForAutomationCommand()
        {
            return automationCommans.WaitItem();
        }
    }

    class NppCommand
    {
        public static string Exit = "npp.exit";
    }
}