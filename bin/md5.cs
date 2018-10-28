//css_args /ac
//css_inc cmd.cs
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System;
using System.Text;
using System.Diagnostics;

void main(string[] args)
{
    var md5_exe = @"..\src\tools\md5sum.exe";

    // raw:       "\7825e05d694088b227db29fb9425669b *plugins\\CSScriptNpp.x64.dll"
    // processed: "7825e05d694088b227db29fb9425669b": "CSScriptNpp.x64.dll",

    File.Copy(@"plugins\CSScriptNpp.x64.dll", @"plugins\CSScriptNpp\CSScriptNpp.dll", true);		
    var result = run(md5_exe, @"plugins\CSScriptNpp\*.dll") +
                 run(md5_exe, @"plugins\CSScriptNpp\*.pdb") +
                 run(md5_exe, @"plugins\CSScriptNpp\*.exe") +
                 run(md5_exe, @"plugins\CSScriptNpp\Mdbg\*.exe") +
                 run(md5_exe, @"plugins\CSScriptNpp\Mdbg\*.dll");

    result = "\"" + result.Substring(1)
                          .Replace(@" *plugins\\", "\": \"")
                          .Replace("\r\n\\", "\",\r\n\"")
                          .Trim() + "\"";

    Console.WriteLine(result);
}

string run(string app, string args)
{
    var sb = new StringBuilder();
    var myProcess = new Process();
    myProcess.StartInfo.FileName = app;
    myProcess.StartInfo.Arguments = args;
    myProcess.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
    myProcess.StartInfo.UseShellExecute = false;
    myProcess.StartInfo.RedirectStandardOutput = true;
    myProcess.StartInfo.CreateNoWindow = true;
    myProcess.Start();

    string line = null;

    while (null != (line = myProcess.StandardOutput.ReadLine()))
        sb.AppendLine(line);

    myProcess.WaitForExit();
    return sb.ToString();
}