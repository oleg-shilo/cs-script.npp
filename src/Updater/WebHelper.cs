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
        static public string DownloadDistro(string distroUrl, Action<long, long> onProgress)
        {
            try
            {
                string downloadDir = KnownFolders.UserDownloads;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(distroUrl);
                string distroExtension = Path.GetExtension(distroUrl);

                string destFile = Path.Combine(downloadDir, fileNameWithoutExtension + distroExtension);

                int numOfAlreadyDownloaded = Directory.GetFiles(downloadDir, fileNameWithoutExtension + "*" + distroExtension).Count();
                if (numOfAlreadyDownloaded > 0)
                    destFile = Path.Combine(downloadDir, fileNameWithoutExtension + " (" + (numOfAlreadyDownloaded + 1) + ")" + distroExtension);

                DownloadBinary(distroUrl, destFile, onProgress);

                return destFile;
            }
            catch
            {
                return null;
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