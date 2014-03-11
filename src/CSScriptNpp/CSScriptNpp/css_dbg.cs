using System.IO;
using System.Reflection;
using System;

class Program
{
    static public void Main(string[] args)
    {
        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string css_asm = Path.Combine(dir, "cscs.exe");
		AppDomain.CurrentDomain.ExecuteAssembly(css_asm, args);
	}
}

