using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using CSScriptIntellisense;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace CSScriptNpp
{
    public static class Utils
    {
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

        public static void EmbeddShortcutIntoTooltip(this ToolStripButton button, string shortcut)
        {
            string[] tooltipLines = button.ToolTipText.Split('\n');
            button.ToolTipText = string.Join(Environment.NewLine, tooltipLines.TakeWhile(x => !x.StartsWith("Shortcut: ")).ToArray());
            button.ToolTipText += Environment.NewLine + "Shortcut: " + shortcut;
        }

        public static bool IsEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool StartsWithAny(this string text, params string[] patterns)
        {
            return patterns.Any(x => text.StartsWith(x));
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