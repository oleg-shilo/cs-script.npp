//css_args /ac
using System.Reflection;
using System.Text;
using System.IO;
using System;
using System.Diagnostics;

string compiler = Path.GetFullPath("ResourceHacker.exe");

// cscs.exe set_version <product> <version>
void main(string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: cscs set_version <product> <version> <original_version>");
    }
    else
    {
        var product = args[0];
        var version = args[1];
        var originalVersion = args[2];

        patch("NppPlugin.x86.dll", product + ".x86.dll", version, originalVersion);
        patch("NppPlugin.x64.dll", product + ".x64.dll", version, originalVersion);
    }
}

void patch(string src, string dest, string version, string originalVersion)
{
    src = Path.GetFullPath(src);
    dest = Path.GetFullPath(dest);

    var rc = Path.GetTempFileName() + ".rc";
    var res = Path.GetTempFileName() + ".res";

    var info = FileVersionInfo.GetVersionInfo(src);

    var resources = rc_template
                        .Replace("{FILEVERSION}", version.Replace(".", ","))
                        .Replace("{Comments}", info.Comments)
                        .Replace("{CompanyName}", info.CompanyName)
                        .Replace("{FileDescription}", info.FileDescription)
                        .Replace("{FileVersion}", version)
                        .Replace("{InternalName}", info.InternalName)
                        .Replace("{LegalCopyright}", info.LegalCopyright)
                        .Replace("{LegalTrademarks}", info.LegalTrademarks)
                        // .Replace("{OriginalFilename}", info.OriginalFilename)
                        .Replace("{OriginalFilename}", Path.GetFileName(dest)+"-v"+originalVersion)
                        .Replace("{ProductName}", info.ProductName)
                        .Replace("{ProductVersion}", version)
                        .Replace("{AssemblyVersion}", version);

    File.WriteAllText(rc, resources);

    Process.Start(compiler, "-open \"" + rc + "\" -save \"" + res + "\" -action compile -log NUL").WaitForExit();
    Process.Start(compiler, "-open \"" + src + "\" -save \"" + dest + "\" -action addoverwrite -res \"" + res + "\" -log NUL").WaitForExit();

    File.Delete(rc);
    File.Delete(res);
    File.Delete("ResourceHacker.ini");

    Console.WriteLine("Patched: " + Path.GetFileName(dest));
}

string rc_template = @"LANGUAGE LANG_NEUTRAL, SUBLANG_NEUTRAL
1 VERSIONINFO
FILEVERSION {FILEVERSION}
PRODUCTVERSION 1,0,0,0
FILEOS 0x4
FILETYPE 0x2
{
    BLOCK ""StringFileInfo""
    {
        BLOCK ""000004B0""
        {
            VALUE ""Comments"", """"
            VALUE ""CompanyName"", ""{CompanyName}""
            VALUE ""FileDescription"", ""{FileDescription}""
            VALUE ""FileVersion"", ""{FileVersion}""
            VALUE ""InternalName"", ""{InternalName}""
            VALUE ""LegalCopyright"", ""{LegalCopyright}""
            VALUE ""LegalTrademarks"", ""{LegalTrademarks}""
            VALUE ""OriginalFilename"", ""{OriginalFilename}""
            VALUE ""ProductName"", ""{ProductName}""
            VALUE ""ProductVersion"", ""{ProductVersion}""
            VALUE ""Assembly Version"", ""{AssemblyVersion}""
        }
    }

    BLOCK ""VarFileInfo""
    {
        VALUE ""Translation"", 0x0000 0x04B0
    }
}
";