using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CSScriptNpp.Deployment
{
    class WebHelper
    {
        static public string GetLatestAvailableDistro(string version, string distroExtension, Action<long, long> onProgress)
        {
            try
            {
                string downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

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
            return GetLatestAvailableDistro(version, ".zip", onProgress);
        }

        private static void DownloadBinary(string url, string destinationPath, Action<long, long> onProgress = null, string proxyUser = null, string proxyPw = null)
        {
            var sb = new StringBuilder();
            byte[] buf = new byte[1024 * 4];

            if (proxyUser != null)
                WebRequest.DefaultWebProxy.Credentials = new NetworkCredential(proxyUser, proxyPw);

            var request = WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

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