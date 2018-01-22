using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace CSScriptNpp.Deployment
{
    class WebHelper
    {
        static public string GetLatestAvailableDistro(string version, string distroExtension, Action<long, long> onProgress)
        {
            try
            {
                string downloadDir = KnownFolders.UserDownloads;

                string destFile = Path.Combine(downloadDir, "CSScriptNpp." + version + distroExtension);

                int numOfAlreadyDownloaded = Directory.GetFiles(downloadDir, "CSScriptNpp." + version + "*" + distroExtension).Count();
                if (numOfAlreadyDownloaded > 0)
                    destFile = Path.Combine(downloadDir, "CSScriptNpp." + version + " (" + (numOfAlreadyDownloaded + 1) + ")" + distroExtension);

                DownloadBinary("http://csscript.net/npp/CSScriptNpp." + version + distroExtension, destFile, onProgress);

                return destFile;
            }
            catch
            {
                return null;
            }
        }

        public static string DownloadDistro(string version, Action<long, long> onProgress)
        {
            if (version.IsUrl())
            {
                var url = version;
                return DownloadDistroFrom(url, onProgress);
            }
            else
            {
                return DownloadDistroOf(version, onProgress);
            }
        }

        public static string DownloadDistroOf(string version, Action<long, long> onProgress)
        {
            return GetLatestAvailableDistro(version, ".zip", onProgress);
        }

        static public string DownloadDistroFrom(string url, Action<long, long> onProgress)
        {
            try
            {
                var file = Path.GetFileName(url);
                string destFile = Path.Combine(KnownFolders.UserDownloads, "CSScriptNpp.ManualUpdate", file);

                if (File.Exists(destFile))
                    File.Delete(destFile);

                DownloadBinary(url, destFile, onProgress);

                if (File.ReadAllText(destFile).Contains("Error 404"))
                    throw new Exception($"Resource {url} cannot be downloaded.");

                return destFile;
            }
            catch
            {
                throw;
            }
        }

        internal static void DownloadBinary(string url, string destinationPath, Action<long, long> onProgress = null, string proxyUser = null, string proxyPw = null)
        {
            var sb = new StringBuilder();
            byte[] buf = new byte[1024 * 4];

            if (proxyUser != null)
                WebRequest.DefaultWebProxy.Credentials = new NetworkCredential(proxyUser, proxyPw);

            var request = WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

            using (var destStream = new FileStream(destinationPath, FileMode.CreateNew))
            using (var resStream = response.GetResponseStream())
            {
                int totalCount = 0;
                int count = 0;

                while (0 < (count = resStream.Read(buf, 0, buf.Length)))
                {
                    destStream.Write(buf, 0, count);

                    totalCount += count;
                    if (onProgress != null)
                        onProgress(totalCount, response.ContentLength);
                }
            }
        }
    }
}