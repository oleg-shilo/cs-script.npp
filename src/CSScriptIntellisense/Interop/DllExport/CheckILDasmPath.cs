using System;
using System.IO;

//part of the custom MSBuild task 'DllExport' 
class Script
{
    static public void Main(string[] args)
    {
        string path = Environment.GetEnvironmentVariable("PATH") ?? "";

        foreach (string dir in path.Split(';'))
            if (File.Exists(Path.Combine(dir, "ildasm.exe")))
                return;

        Console.WriteLine("Error DLL_EXPORT: Cannot find ILDasm.exe. Make sure the path to ILDasm.exe is added to the environment variable PATH (e.g. 'c:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v8.0A\\bin\\NETFX 4.0 Tools')");
    }
}

