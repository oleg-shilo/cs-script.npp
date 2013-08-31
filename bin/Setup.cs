//css_ref ..\..\..\WixSharp\Output\WixSharp.dll;
using IO=System.IO;
using System;
using WixSharp;
class Script
{
	[STAThread]
	static public void Main(string[] args)
	{
        Compiler.WixLocation = @"..\..\..\WixSharp\Main\WixSharp.Samples\Wix_bin\bin";
        
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
                        new File(@"Plugins\CSScriptNpp\ICSharpCode.NRefactory.dll"))));

        project.Version = new Version("1.0.4.0");
        project.LicenceFile = "license.rtf";
        project.GUID = new Guid("6f930b47-2277-411d-9095-18614525889b");

        Compiler.BuildMsi(project, "CSScriptNpp.msi");
	}
}

