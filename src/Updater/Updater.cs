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
        static public void Deploy(string zipFile, string targetDir)
        {
            DeployByReplacing(zipFile, targetDir);
        }

        static public void DeployByReplacing(string zipFile, string targetDir)
        {
            string tempDir = Path.Combine(targetDir, "CSScriptNpp.Update");

            try
            {
                var pluginDir = Path.Combine(targetDir, "CSScriptNpp");
                var pluginBackupDir = Path.Combine(targetDir, "CSScriptNpp.bak");
                var current_config = Path.Combine(pluginDir, "css_config.xml");

                var count = 0;
                while (Directory.Exists(pluginBackupDir) && count < 3)
                {
                    Directory.Delete(pluginBackupDir, true);

                    // Some deleted files are still considered by OS as existing if
                    // Directory.Move is called immediately after Directory.Delete
                    Thread.Sleep(1000);
                    count++;
                }

                try { Directory.Move(pluginDir, pluginBackupDir); }
                catch { }

                byte[] current_config_data = null;
                if (File.Exists(current_config))
                    current_config_data = File.ReadAllBytes(current_config);

                Exract(zipFile, tempDir);
                CopyDir(tempDir + @"\Plugins", targetDir);

                if (current_config_data != null)
                    File.WriteAllBytes(current_config, current_config_data);

                try { Directory.Delete(tempDir, true); }
                catch { }
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);

                MessageBox.Show("Cannot update Notepad++ plugin. Most likely some files are still locked by the active Notepad++ instance.\n\n" +
                    "If you are running Updater.exe manually from the Notepad++ location then copy it somewhere else as it can be locking the plugin dir.", "CS-Script");
            }
        }

        static public void DeployByMerging(string zipFile, string targetDir)
        {
            string tempDir = Path.Combine(targetDir, "CSScriptNpp.Update");

            Exract(zipFile, tempDir);

            CopyDir(tempDir + @"\Plugins", targetDir);

            Directory.Delete(tempDir, true);
            //RestorePluginTree(targetDir);
        }

        static bool ExistAndOlderThan(string file, string fileToCompareTo)
        {
            return File.Exists(file);
            //return File.Exists(file) && File.GetCreationTimeUtc(file) <= File.GetCreationTimeUtc(fileToCompareTo);
            //return File.Exists(file) && new Version(FileVersionInfo.GetVersionInfo(file).ProductVersion) >= new Version(FileVersionInfo.GetVersionInfo(fileToCompareTo).ProductVersion);
        }

        internal static void RestorePluginTree(string pluginDir)
        {
            MessageBox.Show(" RestorePluginTree(string pluginDir) - 333");
            try
            {
                var filesFromSubDirs = Directory.GetDirectories(pluginDir)
                                                .SelectMany(x => Directory.GetFiles(x))
                                                .ToArray();

                var files = filesFromSubDirs
                                     .Select(x => new { FileInSubDir = x, FileInRoot = Path.Combine(pluginDir, Path.GetFileName(x)) })
                                     .Where(x => ExistAndOlderThan(x.FileInRoot, x.FileInSubDir))
                                     .Select(x => x.FileInRoot)
                                     .ToArray();

                files = files.Concat(Directory.GetFiles(pluginDir, "*.exe.config"))
                             .Concat(Directory.GetFiles(pluginDir, "*.pdb"))
                             .Concat(Directory.GetFiles(pluginDir, "roslyn.redme.txt"))
                             .Concat(Directory.GetFiles(pluginDir, "*.vshost*"))
                             .ToArray();

                foreach (var item in files)
                    try
                    {
                        File.Delete(item);
                    }
                    catch { }
            }
            catch { }
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
    }
}