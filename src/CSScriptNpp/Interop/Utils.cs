using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSScriptIntellisense;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public static class Utils
    {
        public static bool IsVS2010PlusAvailable
        {
            get
            {
                using (var vs2010 = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.10.0", false))
                using (var vs2012 = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.11.0", false))
                using (var vs2013 = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.12.0", false))
                {
                    return (vs2010 != null || vs2012 != null || vs2013 != null);
                }
            }
        }

        static public string GetStatementAtCaret()
        {
            string expression = Npp.GetSelectedText();
            if (string.IsNullOrWhiteSpace(expression))
                expression = CSScriptIntellisense.Npp.GetStatementAtPosition();
            return expression;
        }

        public static void EmbeddShortcutIntoTooltip(this ToolStripButton button, string shortcut)
        {
            string[] tooltipLines = button.ToolTipText.Split('\n');
            button.ToolTipText = string.Join(Environment.NewLine, tooltipLines.TakeWhile(x => !x.StartsWith("Shortcut: ")).ToArray());
            button.ToolTipText += Environment.NewLine + "Shortcut: " + shortcut;
        }

        public static string ConvertToXPM(Bitmap bmp, string transparentColor)
        {
            StringBuilder sb = new StringBuilder();
            List<string> colors = new List<string>();
            List<char> chars = new List<char>();
            int width = bmp.Width;
            int height = bmp.Height;
            int index;
            sb.Append("/* XPM */static char * xmp_data[] = {\"").Append(width).Append(" ").Append(height).Append(" ? 1\"");
            int colorsIndex = sb.Length;
            string col;
            char c;
            for (int y = 0; y < height; y++)
            {
                sb.Append(",\"");
                for (int x = 0; x < width; x++)
                {
                    col = ColorTranslator.ToHtml(bmp.GetPixel(x, y));
                    index = colors.IndexOf(col);
                    if (index < 0)
                    {
                        index = colors.Count + 65;
                        colors.Add(col);
                        if (index > 90) index += 6;
                        c = Encoding.ASCII.GetChars(new byte[] { (byte)(index & 0xff) })[0];
                        chars.Add(c);
                        sb.Insert(colorsIndex, ",\"" + c + " c " + col + "\"");
                        colorsIndex += 14;
                    }
                    else c = (char)chars[index];
                    sb.Append(c);
                }
                sb.Append("\"");
            }
            sb.Append("};");
            string result = sb.ToString();
            int p = result.IndexOf("?");
            string finalColor = result.Substring(0, p) + colors.Count + result.Substring(p + 1).Replace(transparentColor.ToUpper(), "None");

            return finalColor;
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
       
        public static CSScriptNpp.FuncItem ToLocal(this CSScriptIntellisense.FuncItem item)
        {
            return new CSScriptNpp.FuncItem
                {
                    _cmdID = item._cmdID,
                    _init2Check = item._init2Check,
                    _itemName = item._itemName,
                    _pFunc = item._pFunc,
                    _pShKey = new ShortcutKey
                                  {
                                      _isCtrl = item._pShKey._isCtrl,
                                      _isAlt = item._pShKey._isAlt,
                                      _isShift = item._pShKey._isShift,
                                      _key = item._pShKey._key
                                  }
                };
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
            Win32.SendMenuCmd(Npp.NppHandle, NppMenuCmd.IDM_FILE_EXIT, 0);

            string file;
            Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, out file);

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
                    string restarter = Path.Combine(Plugin.PluginDir, "Updater.exe");

                    //the restarter will also wait for this process to exit
                    Process.Start(restarter, string.Format("/restart /asadmin {0} \"{1}\"", processIdToWaitForExit, appToStart));
                }
            }
        }
    }
}