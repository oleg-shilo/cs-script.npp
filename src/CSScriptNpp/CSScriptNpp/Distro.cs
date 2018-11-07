using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CSScriptNpp
{
    public class Distro
    {
        public string Version;
        public string URL_root;
        public string fullUrl;
        public string ReleaseNotesText;

        public static Distro FromFixedLocation(string path)
        {
            return new Distro
            {
                fullUrl = path,
            };
        }
        public static Distro FromVersionInfo(string info)
        {
            var lines = info.Trim().Replace("\r\n", "\n").Split(new[] { '\n' }, 3);

            return new Distro
            {
                Version = lines[0],
                URL_root = lines[1],
                ReleaseNotesText = lines.Skip(2).FirstOrDefault()
            };
        }

        public string MsiUrl
        {
            get { return URL_root + "/" + FileNameWithoutExtension + ".msi"; }
        }

        public string ZipUrl
        {
            get
            {
                return URL_root != null ?
                   URL_root + "/" + FileNameWithoutExtension + ".zip" :
                   fullUrl.ToUri();
            }
        }

        public string ReleasePageUrl
        {
            // https://github.com/oleg-shilo/cs-script.npp/releases/download/beta-v1.7.7.3/CSScriptNpp.1.7.7.3.x64.zip
            // https://github.com/oleg-shilo/cs-script.npp/releases/tag/beta-v1.7.7.3
            get { return URL_root.Replace("/download/", "/tag/"); }
        }

        string FileNameWithoutExtension
        {
            get
            {
                var distroCpu = ".x86";
                if (IntPtr.Size == 8)
                    distroCpu = ".x64";
                return "CSScriptNpp." + Version + distroCpu;
            }
        }
    }
}