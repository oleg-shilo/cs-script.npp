using System;
using System.Linq;

namespace CSScriptNpp
{
    public class Distro
    {
        public string Version;
        public string URL_root;
        public string fullUrl;
        public string ReleaseNotesText;
        public string ReleaseNotesUrl;

        public static Distro FromFixedLocation(string path)
        {
            // URL:  https://github.com/oleg-shilo/cs-script.npp/releases/download/v2.0.0.0/CSScriptNpp.2.0.0.0.x64.zip
            // URL:  https://github.com/oleg-shilo/cs-script.npp/releases/download/v2.0.0.0/CSScriptNpp.2.0.0.0.ReleaseInfo.txt

            var baseUrl = "https://github.com/oleg-shilo/cs-script.npp/releases/download/";

            if (path.StartsWith(baseUrl))
            {
                var tokens = path.Substring(baseUrl.Length).Split('/');

                return new Distro
                {
                    Version = tokens.FirstOrDefault()?.Replace("v", ""), // v1.1.1 vs 1.1.1
                    URL_root = baseUrl + tokens.FirstOrDefault(),
                    ReleaseNotesUrl = path.Replace(".x64.zip", ".ReleaseInfo.txt").Replace(".x86.zip", ".ReleaseInfo.txt")
                };
            }
            else
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
                Version = lines[0].Replace("v", ""), // v1.1.1 vs 1.1.1
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