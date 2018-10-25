//css_args /ac
using System.Reflection;
using System.Text;
using System.IO;
using System;
using System.Diagnostics;
using static dbg;

string compiler = Path.GetFullPath("ResourceHacker.exe");

void main()
{
    var plugin_file = @"..\output\plugins\CSScriptNpp\CSScriptNpp.asm.dll";

    var version = AssemblyName.GetAssemblyName(plugin_file).Version.ToString();
    var info = FileVersionInfo.GetVersionInfo(plugin_file);

    var res = rc_template.Replace("{FILEVERSION}", version.Replace(".", ","))
                         .Replace("{FileVersion}", version)
                         .Replace("{ProductVersion}", version)
                         .Replace("{AssemblyVersion}", version)
                         .Replace("{ProductName}", info.ProductName)
                         .Replace("{CompanyName}", info.CompanyName)
                         .Replace("{LegalCopyright}", info.LegalCopyright);

    update_resources(@"..\output\plugins\CSScriptNpp.x86.dll", res);
    update_resources(@"..\output\plugins\CSScriptNpp.x64.dll", res);
}

void update_resources(string file, string res)
{
    var info = FileVersionInfo.GetVersionInfo(file);

    string src = file;
    string dest = file + ".new";

    patch(src, dest, res);

    File.Copy(src, Path.Combine(Path.GetDirectoryName(src), "original_" + Path.GetFileName(src)), true);
    File.Copy(dest, src, true);
    File.Delete(dest);
}

void patch(string src, string dest, string resources)
{
    src = Path.GetFullPath(src);
    dest = Path.GetFullPath(dest);

    var rc = src + ".rc";
    var res = src + ".res";

    var info = FileVersionInfo.GetVersionInfo(src);

    var cpu = Path.GetExtension(Path.GetFileNameWithoutExtension(dest.Replace(".new", ""))).Replace(".", "");
    
    var description = info.FileDescription;
    if(!description.Contains(cpu))
        description += " ("+cpu+")";
    
    File.WriteAllText(rc, resources.Replace("{FILEVERSION}", info.FileVersion.Replace(".", ","))
                                   .Replace("{Comments}", info.Comments)
                                   .Replace("{CompanyName}", info.CompanyName)
                                   .Replace("{FileDescription}", description)
                                   .Replace("{FileVersion}", info.FileVersion)
                                   .Replace("{InternalName}", info.InternalName)
                                   .Replace("{LegalCopyright}", info.LegalCopyright)
                                   .Replace("{LegalTrademarks}", info.LegalTrademarks)
                                   .Replace("{OriginalFilename}", info.OriginalFilename)
                                   //.Replace("{OriginalFilename}", )
                                   .Replace("{ProductName}", info.ProductName  + "(ttt)")
                                   .Replace("{ProductVersion}", info.ProductVersion)
                                   .Replace("{AssemblyVersion}", info.FileVersion));

    Process.Start(compiler, $"-open \"{rc}\" -save \"{res}\" -action compile").WaitForExit();
    Process.Start(compiler, $"-open \"{src}\" -save \"{dest}\" -action addoverwrite -res \"{res}\"").WaitForExit();

    File.Delete(rc);
    File.Delete(res);

    print("Patched:", Path.GetFileName(src));
    print("Patched>:", dest);
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