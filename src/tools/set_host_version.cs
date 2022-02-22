using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using static dbg;

class script
{
    static string compiler = Path.GetFullPath("ResourceHacker.exe");

    static void Main()
    {
        var plugin_file = @"..\output\plugins\CSScriptNpp\CSScriptNpp.asm.dll";
        var host_file = @"..\NppPlugin.Host\output\NppPlugin.host.dll";

        var version = AssemblyName.GetAssemblyName(plugin_file).Version.ToString();
        var hostVersion = AssemblyName.GetAssemblyName(host_file).Version.ToString();

        // Console.WriteLine(version + " - "+ Path.GetFullPath(plugin_file));
        // Console.WriteLine(hostVersion + " - " + Path.GetFullPath(host_file));

        var info = FileVersionInfo.GetVersionInfo(plugin_file);

        var res = rc_template.Replace("{FILEVERSION}", version.Replace(".", ","))
                             .Replace("{FileVersion}", version)
                             .Replace("{ProductVersion}", version)
                             .Replace("{AssemblyVersion}", version)
                             .Replace("{ProductName}", info.ProductName)
                             .Replace("{OriginalFilename}", Path.GetFileName(host_file) + " (v" + hostVersion + ")")
                             .Replace("{CompanyName}", info.CompanyName)
                             .Replace("{LegalCopyright}", info.LegalCopyright);

        update_resources(@"..\output\plugins\CSScriptNpp.x86.dll", res);
        update_resources(@"..\output\plugins\CSScriptNpp.x64.dll", res);
    }

    static void update_resources(string file, string res)
    {
        var info = FileVersionInfo.GetVersionInfo(file);

        string src = file;
        string dest = file + ".new";

        patch(src, dest, res);

        File.Copy(src, Path.Combine(Path.GetDirectoryName(src), "original_" + Path.GetFileName(src)), true);
        File.Copy(dest, src, true);
        File.Delete(dest);
    }

    static void patch(string src, string dest, string resources)
    {
        // Debug.Assert(false);

        src = Path.GetFullPath(src);
        dest = Path.GetFullPath(dest);

        var rc = src + ".rc";
        var res = src + ".res";

        var info = FileVersionInfo.GetVersionInfo(src);

        var cpu = Path.GetExtension(Path.GetFileNameWithoutExtension(dest.Replace(".new", ""))).Replace(".", "");

        var description = info.FileDescription;
        if (!description.Contains(cpu))
            description += " (" + cpu + ")";

        var originalFileName = "info.OriginalFilename" + " (v" + "originalVersion" + ")";

        File.WriteAllText(rc, resources.Replace("{FILEVERSION}", info.FileVersion.Replace(".", ","))
                                       .Replace("{Comments}", info.Comments)
                                       .Replace("{CompanyName}", info.CompanyName)
                                       .Replace("{FileDescription}", description)
                                       .Replace("{FileVersion}", info.FileVersion)
                                       .Replace("{InternalName}", info.InternalName)
                                       .Replace("{LegalTrademarks}", info.LegalTrademarks)
                                       //.Replace("{OriginalFilename}", )
                                       .Replace("{ProductName}", info.ProductName)
                                       .Replace("{ProductVersion}", info.ProductVersion)
                                       .Replace("{AssemblyVersion}", info.FileVersion));

        Process.Start(compiler, $"-open \"{rc}\" -save \"{res}\" -action compile").WaitForExit();
        Process.Start(compiler, $"-open \"{src}\" -save \"{dest}\" -action addoverwrite -res \"{res}\"").WaitForExit();

        File.Delete(rc);
        File.Delete(res);

        print("Patched:", Path.GetFileName(src));
        // print("Patched:", src + " - " + originalFileName);
        print("Patched>:", dest);
    }

    static string rc_template = @"LANGUAGE LANG_NEUTRAL, SUBLANG_NEUTRAL
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
}