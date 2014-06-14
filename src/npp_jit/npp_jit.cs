using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

class Program
{
    static public void Main(string[] args)
    {
        //"C:\Program Files (x86)\Notepad++\plugins\CSScriptNpp\npp_jit.exe" /css.attach:9248
        string argsFile = Path.GetTempFileName() + "npp.args";
        try
        {
            File.WriteAllLines(argsFile, args);
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string npp = Path.Combine(dir, @"..\..\notepad++.exe");
            Process.Start(npp, "\"" + argsFile + "\"");
        }
        catch
        {

        }
    }
}

