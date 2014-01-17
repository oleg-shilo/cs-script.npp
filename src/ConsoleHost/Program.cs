using System;
using System.Linq;
using System.Diagnostics;

class ConsoleHost
{
    [STAThread]
    static public void Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var newArgs = args.Skip(1).ToArray();
            AppDomain.CurrentDomain.ExecuteAssembly(args[0], newArgs);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }
}