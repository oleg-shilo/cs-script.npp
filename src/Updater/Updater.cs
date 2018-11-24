using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace CSScriptNpp.Deployment
{
    class Updater
    {
        public static void Deploy(string zipFile, string targetDir)
        {
            DeployByReplacingFromZip(zipFile, targetDir);
        }

        public static void DeployByReplacingFromZip(string zipFile, string targetDir)
        {
            try
            {
                string tempDir = Path.Combine(KnownFolders.UserDownloads, "CSScriptNpp.ManualUpdate", "temp");
                Exract(zipFile, tempDir);
                DeployByReplacingFromFolder(tempDir, targetDir);
                DeleteDir(tempDir);
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
            }
        }

        public static void DeployByReplacingFromFolder(string distroFolder, string targetDir)
        {
            // Debug.Assert(false);

            bool singleFolderDeployment = false;

            var pluginDir = Path.Combine(targetDir, "CSScriptNpp");
            var pluginBackupDir = Path.Combine(targetDir, "CSScriptNpp.bak");
            // var tempDirRoot = Path.Combine(distroFolder, "plugins");
            var tempDirRoot = distroFolder;

            var current_config = Path.Combine(pluginDir, "css_config.xml");

            //if (File.Exists(Path.Combine(distroFolder, "CSScriptNpp.dll")))
            //{
            //    tempDirRoot = distroFolder;
            //    singleFolderDeployment = true;
            //}

            try
            {
                byte[] current_config_data = File.Exists(current_config) ?
                                             File.ReadAllBytes(current_config) :
                                             null;

                // To my disbelieve Directory.Move is very unreliable as it complains constantly about files being locked. While
                // custom *Dir(,) methods with retry work quite well
                DeleteDir(pluginBackupDir, retryDelay: 1000);
                if (Directory.Exists(pluginDir))
                {
                    CopyDir(pluginDir, pluginBackupDir);
                    DeleteDir(pluginDir);
                }

                // delete old version of plugin host
                var host_old_dll = Directory.GetFiles(targetDir, "CSScriptNpp*.dll").FirstOrDefault();
                if (File.Exists(host_old_dll))
                    DeleteFile(host_old_dll);

                if (singleFolderDeployment)
                    CopyDir(tempDirRoot, Path.Combine(targetDir, "CSScriptNpp"));
                else
                    CopyDir(tempDirRoot, targetDir);

                if (current_config_data != null)
                    File.WriteAllBytes(current_config, current_config_data);
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);

                MessageBox.Show(new Form { TopMost = true }, "Cannot update Notepad++ plugin. Most likely some files are still locked by the active Notepad++ instance.\n\n" +
                    "If you are running Updater.exe manually from the Notepad++ location then copy it somewhere else as it can be locking the plugin dir.", "CS-Script");

                if (!Directory.Exists(pluginDir) && Directory.Exists(pluginBackupDir))
                {
                    try
                    {
                        Directory.Move(pluginBackupDir, pluginDir);
                    }
                    catch { }
                }
            }
        }

        public static void DeployByMerging(string zipFile, string targetDir)
        {
            string tempDir = Path.Combine(targetDir, "CSScriptNpp.Update");

            Exract(zipFile, tempDir);

            CopyDir(tempDir + @"\Plugins", targetDir);

            Directory.Delete(tempDir, true);
            //RestorePluginTree(targetDir);
        }

        static void CopyDir(string source, string destination)
        {
            string srcDir = Path.GetFullPath(source);
            string detsDir = Path.GetFullPath(destination);

            if (!Directory.Exists(srcDir))
                throw new System.Exception("Plugin binaries could not be downloaded.");

            if (!Directory.Exists(detsDir))
                Directory.CreateDirectory(detsDir);

            string[] srcFiles = Directory.GetFiles(srcDir, "*", SearchOption.AllDirectories);

            foreach (string srcFile in srcFiles)
            {
                string srcFileRelativePath = srcFile.Substring(srcDir.Length + 1);
                string destFile = Path.Combine(detsDir, srcFileRelativePath);

                CopyFile(srcFile, destFile);
            }
        }

        static void CopyFile(string srcFile, string destFile, int retryDelay = 100)
        {
            string dir = Path.GetDirectoryName(destFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            for (int i = 0; i < 3; i++)
                try
                {
                    File.Copy(srcFile, destFile, true);
                }
                catch
                {
                    Thread.Sleep(retryDelay);
                }
        }

        static void DeleteDir(string dir, int retryDelay = 100)
        {
            if (Directory.Exists(dir))
            {
                foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                    DeleteFile(file);

                for (int i = 0; i < 3 && Directory.Exists(dir); i++)
                    try { Directory.Delete(dir, true); }
                    catch { Thread.Sleep(retryDelay); }
            }
        }

        static void DeleteFile(string file, int retryDelay = 100)
        {
            for (int i = 0; i < 3; i++)
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    try { File.SetAttributes(file, FileAttributes.Normal); }
                    catch { }
                    Thread.Sleep(retryDelay);
                }
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
    }
}