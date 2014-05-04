using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace CSScriptNpp.Deployment
{
    class Updater
    {
        static public void Deploy(string zipFile, string targetDir)
        {
            string tempDir = Path.Combine(targetDir, "CSScriptNpp.Update");

            Exract(zipFile, tempDir);

            CopyDir(tempDir + @"\Plugins", targetDir);

            Directory.Delete(tempDir, true);
        }

        static void CopyDir(string source, string destination)
        {
            string srcDir = Path.GetFullPath(source);
            string detsDir = Path.GetFullPath(destination);

            string[] srcFiles = Directory.GetFiles(srcDir, "*", SearchOption.AllDirectories);

            foreach (string srcFile in srcFiles)
            {
                string srcFileRelativePath = srcFile.Substring(srcDir.Length + 1);
                string destFile = Path.Combine(detsDir, srcFileRelativePath);

                CopyFile(srcFile, destFile);
            }
        }

        static void CopyFile(string srcFile, string destFile)
        {
            string dir = Path.GetDirectoryName(destFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.Copy(srcFile, destFile, true);
        }

        static void Exract(string zipFile, string targetDir)
        {
            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);

            string app = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "7z.exe");
            string args = string.Format("x -y \"-o{0}\" \"{1}\"", targetDir, zipFile);

            Run(app, args);
        }

        static void Run(string app, string args)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = app,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false
            }).WaitForExit();
        }

        static void Restart(string[] args)
        {
            //restartApp <prevInstanceProcId> <appPath>
            try
            {
                //Debug.Assert(false);
                Thread.Sleep(100);
                string appPath = args[1];
                int id = int.Parse(args[0]);

                var proc = Process.GetProcesses().Where(x => x.Id == id).FirstOrDefault();
                if (proc != null && !proc.HasExited)
                    proc.WaitForExit();

                Process.Start(appPath);
            }
            catch
            {
            }
        }
    }
}