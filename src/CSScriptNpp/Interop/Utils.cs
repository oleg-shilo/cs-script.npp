using Microsoft.Win32;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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

        public static bool IsSameAs(this string text, string textToCompare, bool ignoreCase)
        {
            return string.Compare(text, textToCompare, ignoreCase) == 0;
        }

        public static bool ParseAsFileReference(this string text, out string file, out int line, out int column)
        {
            if (ParseAsErrorFileReference(text, out file, out line, out column))
                return true;
            else if (ParseAsExceptionFileReference(text, out file, out line, out column))
                return true;
            else
                return false;
        }

        public static bool ParseAsErrorFileReference(this string text, out string file, out int line, out int column)
        {
            line = -1;
            column = -1;
            file = "";
            //@"c:\Users\osh\AppData\Local\Temp\CSSCRIPT\Cache\-1529274573\New Script2.g.cs(11,1): error";
            var match = Regex.Match(text, @"\(\d+,\d+\):\s+");
            if (match.Success)
            {
                //"(11,1):"
                string[] parts = match.Value.Substring(1, match.Value.Length - 4).Split(',');
                if (!int.TryParse(parts[0], out line))
                    return false;
                else if (!int.TryParse(parts[1], out column))
                    return false;
                else
                    file = text.Substring(0, match.Index).Trim();
                return true;
            }
            return false;
        }

        public static bool ParseAsExceptionFileReference(this string text, out string file, out int line, out int column)
        {
            line = -1;
            column = 1;
            file = "";
            //@"   at ScriptClass.main(String[] args) in c:\Users\osh\AppData\Local\Temp\CSSCRIPT\Cache\-1529274573\dev.g.csx:line 12";
            var match = Regex.Match(text, @".*:line\s\d+\s?");
            if (match.Success)
            {
                //"...mp\CSSCRIPT\Cache\-1529274573\dev.g.csx:line 12"
                int pos = match.Value.LastIndexOf(":line");
                if (pos != -1)
                {
                    string lineRef = match.Value.Substring(pos + 5, match.Value.Length - (pos + 5));
                    if (!int.TryParse(lineRef, out line))
                        return false;

                    var fileRef = match.Value.Substring(0, pos);
                    pos = fileRef.LastIndexOf(":");
                    if (pos > 0)
                    {
                        file = fileRef.Substring(pos - 1);
                        return true;
                    }
                }
            }
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
    }
}