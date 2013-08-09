using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Microsoft.Win32;

namespace CSScriptNpp
{
    public class Utils
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
