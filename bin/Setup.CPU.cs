//css_ref %WIXSHARP_DIR%\WixSharp.dll;
//css_dir %WIXSHARP_DIR%\\Wix_bin\SDK;
using System.Reflection;
using IO = System.IO;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using WixSharp.CommonTasks;

class Script
{
    static void Main(string[] args)
    {
        BuidMSi(is64: true, guid: "6f930b47-2277-411d-9095-18614525889b");
        BuidMSi(is64: false, guid: "6f930b47-2277-411d-9095-186145258891");
    }

    static void BuidMSi(bool is64, string guid)
    {
        string pluginFile = IO.Path.GetFullPath(@"Plugins\CSScriptNpp\CSScriptNpp.dll");

        var cpu = is64 ? "x64" : "x86";

        Console.WriteLine(pluginFile);
        Version version = AssemblyName.GetAssemblyName(pluginFile).Version;

        // remove the revision value
        version = new Version(version.Major, version.Minor, version.Build, 0);

        Console.WriteLine($"Building CSScriptNpp.{version}.{cpu}.msi");

        var project =
            new Project($"CS-Script for Notepad++ ({cpu})",
                new Dir(@"%ProgramFiles%\Notepad++\Plugins",
                    new File($@"Plugins\CSScriptNpp.{cpu}.dll"),
                    new Dir("CSScriptNpp",
                        new DirFiles(@"Plugins\CSScriptNpp\*.*"),
                        new Dir("Mdbg",
                            new DirFiles(@"Plugins\CSScriptNpp\Mdbg\*.*")))),
                new CloseApplication("notepad++.exe", true, false) { Timeout = 5, Sequence = 1 },
                new CloseApplication("syntaxer.exe", true, false) { Timeout = 5, Sequence = 2 });

        project.ControlPanelInfo.UrlInfoAbout = "https://github.com/oleg-shilo/cs-script.npp/";
        project.ControlPanelInfo.Contact = "Product owner";
        project.ControlPanelInfo.Manufacturer = "Oleg Shilo";

        project.GUID = new Guid(guid);

        project.Version = version;
        project.Platform = is64 ? Platform.x64 : Platform.x86;
        project.MajorUpgradeStrategy = MajorUpgradeStrategy.Default;
        project.LicenceFile = "license.rtf";

        // needed to ensure the files that are possible replaced by manual updates can be overwritten
        project.WixSourceGenerated += document =>
                document.FindAll("File")
                        .ForEach(e =>
                        {
                            e.Parent("Component")
                             .AddElement("RemoveFile",
                                       $@"Id=Remove_{e.Attribute("Id").Value};
                                          Name={e.Attribute("Source").Value.PathGetFileName()};
                                          On=install");
                        });

        project.PreserveTempFiles = true;
        project.EmitConsistentPackageId = true;

        Compiler.BuildMsi(project, $"CSScriptNpp.{version}.{cpu}.msi");
    }
}