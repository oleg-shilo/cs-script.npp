using System;
using System.IO;
using System.Linq;
using System.Reflection;

class Program
{
    static public void Main(string[] args)
    {
        if (args.Count() >= 2)
        {
            string engine_name = args.First();

            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string css_asm = Path.Combine(dir, engine_name);
            AppDomain.CurrentDomain.ExecuteAssembly(css_asm, args.Skip(1).ToArray());
        }
    }
}

