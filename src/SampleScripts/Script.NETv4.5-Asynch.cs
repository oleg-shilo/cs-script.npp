//css_args /ac
using System.Threading;
using System.Threading.Tasks;
using System;

void main()
{
    Execute();
    Console.ReadLine();
}

async void Execute()
{
    await Task.Run(()=> 
    { 
        Thread.Sleep(1000);
        Console.WriteLine("Continue");
    });
    Console.WriteLine("Done");
}
