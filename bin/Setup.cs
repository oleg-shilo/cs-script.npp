//css_ref %WIXSHARP_DIR%\WixSharp.dll;
//css_dir %WIXSHARP_DIR%\\Wix_bin\SDK;

using Microsoft.Win32;
using IO = System.IO;
using System;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;

class Script
{
    [STAThread]
    static public void Main(string[] args)
    {
        string pluginFile = IO.Path.GetFullPath(@"Plugins\CSScriptNpp.dll");
        Version version = System.Reflection.Assembly.ReflectionOnlyLoadFrom(pluginFile).GetName().Version;

        var project =
            new Project("CS-Script for Notepad++",
                new Dir(@"%ProgramFiles%\Notepad++\Plugins",
                    new File(@"Plugins\CSScriptNpp.dll"),
                    new Dir("CSScriptNpp",
                        new DirFiles(@"Plugins\CSScriptNpp\*.*"),
                        new Dir("Mdbg",
                            new DirFiles(@"Plugins\CSScriptNpp\Mdbg\*.*")),
                        new Dir("Roslyn",
                            new DirFiles(@"Plugins\CSScriptNpp\Roslyn\*.*")))));

        project.ControlPanelInfo.UrlInfoAbout = "https://csscriptnpp.codeplex.com/";
        project.ControlPanelInfo.Contact = "Product owner";
        project.ControlPanelInfo.Manufacturer = "Oleg Shilo";

        project.Actions = new WixSharp.Action[]
        {
            new PathFileAction("%ProgramFiles%\\Notepad++\\notepad++.exe", "", "INSTALLDIR", Return.asyncNoWait, When.After, Step.InstallInitialize, Condition.NOT_Installed)
            {
                Name = "Action_StartNPP" //need to give custom name as "Action1_notepad++.exe" is illegal because of '++'
            },
            new ManagedAction(CustonActions.FindNpp, Return.check, When.Before, Step.LaunchConditions, Condition.NOT_Installed)
        };

        project.GUID = new Guid("6f930b47-2277-411d-9095-18614525889b");
        project.Version = version;
        project.MajorUpgradeStrategy = MajorUpgradeStrategy.Default;
        project.LicenceFile = "license.rtf";

        Compiler.ClientAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;

        //project.PreserveTempFiles = true;
        Compiler.BuildMsi(project, "CSScriptNpp." + version + ".msi");
    }
}

public class CustonActions
{
    [CustomAction]
    public static ActionResult FindNpp(Session session)
    {
        try
        {
            string installDir = GetNppDir();

            if (installDir == null)
            {
                MessageBox.Show("Cannot find Notepad++ installation.\n" +
                "If it is a portable Notepad++ installation then you need to install the plugin manually from the About Box 'Check for Updates...'",
                "CS-Script.Npp Update");
                return ActionResult.Failure;
            }
            else
                session["INSTALLDIR"] = System.IO.Path.Combine(installDir, "plugins");
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString(), "Error");
        }

        return ActionResult.Success;
    }

    public static string GetNppDir()
    {
        return TestPath(@"%ProgramFiles%\Notepad++\notepad++.exe") ??
               TestRegistry(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\notepad++.exe:") ??
               TestRegistry(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\App Paths\notepad++.exe:") ??
               TestRegistry(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Notepad++:DisplayIcon");
    }

    public static string TestPath(string nppFile)
    {
        string file = Environment.ExpandEnvironmentVariables(nppFile);

        if (IO.File.Exists(file))
            return IO.Path.GetDirectoryName(file);
        else
            return null;
    }

    public static string TestRegistry(string path)
    {
        var parts = path.Split(':');
        string keyName = parts[0];
        string valueName = parts[1];

        using (var regKey = Registry.LocalMachine.OpenSubKey(keyName))
        {
            if (regKey != null)
            {
                object val = regKey.GetValue(valueName);

                if (val is string)
                {
                    string nppFile = (string)val;

                    try
                    {
                        if (IO.File.Exists(nppFile) && nppFile.EndsWith("notepad++.exe", StringComparison.OrdinalIgnoreCase))
                            return IO.Path.GetDirectoryName(nppFile);
                    }
                    catch { }
                }
            }
        }

        return null;
    }
}