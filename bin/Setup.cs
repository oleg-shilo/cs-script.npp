//css_ref %WIXSHARP_DIR%\WixSharp.dll;
using IO = System.IO;
using System;
using WixSharp;

class Script
{
    [STAThread]
    static public void Main(string[] args)
    {
        string pluginFile = IO.Path.GetFullPath(@"Plugins\CSScriptNpp.dll");
        Version version = System.Reflection.Assembly.ReflectionOnlyLoadFrom(pluginFile).GetName().Version;

        Project project =
            new Project("CS-Script for Notepad++",
                new Dir(@"%ProgramFiles%\Notepad++\Plugins",
                    new File(@"Plugins\CSScriptNpp.dll"),
                    new Dir("CSScriptNpp",
                        new File(@"Plugins\CSScriptNpp\cscs.exe"),
                        new File(@"Plugins\CSScriptNpp\CSScriptIntellisense.dll"),
                        new File(@"Plugins\CSScriptNpp\CSScriptLibrary.dll"),
                        new File(@"Plugins\CSScriptNpp\Mono.Cecil.dll"),
                        new File(@"Plugins\CSScriptNpp\ICSharpCode.NRefactory.CSharp.dll"),
                        new File(@"Plugins\CSScriptNpp\ICSharpCode.NRefactory.dll")))
                    );

        project.Actions = new WixSharp.Action[]
        {
            new PathFileAction("%ProgramFiles%\\Notepad++\\notepad++.exe", "", "INSTALLDIR", Return.asyncNoWait, When.After, Step.InstallInitialize, Condition.NOT_Installed)
            {
                Name = "Action_StartNPP" //need to give custom name as "Action1_notepad++.exe" is illegal because of '++'
            }
        };
        
        project.GUID = new Guid("6f930b47-2277-411d-9095-18614525889b");
        project.Version = version;
        project.MajorUpgradeStrategy = MajorUpgradeStrategy.Default;
        project.LicenceFile = "license.rtf";

        Compiler.ClientAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;

        Compiler.BuildMsi(project, "CSScriptNpp."+version+".msi");
    }
}
