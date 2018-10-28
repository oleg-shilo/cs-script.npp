//css_args /ac
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System;

void main(string[] args)
{
    var version = AssemblyName.GetAssemblyName(@".\plugins\CSScriptNpp\CSScriptNpp.asm.dll").Version.ToString();
    // var version = Directory.GetFiles(".", "CSScriptNpp.*.msi").Select(x => Path.GetFileNameWithoutExtension(x)).First().Replace("CSScriptNpp.", "");

    Console.WriteLine("Injecting version into file names: " + version);

    //File.WriteAllText(@"E:\cs-script\cs-scriptWEB\npp\latest_version.txt", version);
    // https://github.com/oleg-shilo/cs-script.npp/releases/download/v1.7.10.0
    // File.WriteAllText(@".\latest_version.txt", version);
    // File.Copy(@".\latest_version.txt", @".\latest_version_dbg.txt", true);

    string releaseNotesFile = @"..\src\CSScriptNpp\Resources\WhatsNew.txt";
    File.Copy(releaseNotesFile, @".\CSScriptNpp." + version + ".ReleaseNotes.txt", true);
    File.Copy(releaseNotesFile, @".\CSScriptNpp." + version + ".ReleaseInfo.txt", true);

    string content = File.ReadAllText(releaseNotesFile).Replace("\n", "</br>");
    File.WriteAllText(@".\CSScriptNpp." + version + ".ReleaseNotes.html", content);

    rename_with_version(@".\CSScriptNpp.x86.zip", version);
    rename_with_version(@".\CSScriptNpp.x64.zip", version);
    rename_with_version(@".\CSScriptNpp.x86.7z", version);
    rename_with_version(@".\CSScriptNpp.x64.7z", version);
    rename_with_version(@".\PLuginAdmin.CSScriptNpp.x86.7z", version);
    rename_with_version(@".\PLuginAdmin.CSScriptNpp.x64.7z", version);
}

void rename_with_version(string fileName, string version)
{
    if (File.Exists(fileName))
    {
        var name_parts = Path.GetFileName(fileName).Split('.').ToList();
        name_parts.Insert(1, version);

        string distro = Path.Combine(Path.GetDirectoryName(fileName), string.Join(".", name_parts.ToArray()));

        if (File.Exists(distro))
            File.Delete(distro);
        File.Move(fileName, distro);

        Console.WriteLine(fileName + " -> " + distro);
    }
}