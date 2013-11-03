//css_args /ac
using System;

void main()
{
    Console.WriteLine("CLR: " + Environment.Version);
    if(IsNet45OrNewer())
        Console.WriteLine(".NET: v4.5");
}

bool IsNet45OrNewer()
{
   return Type.GetType("System.Reflection.ReflectionContext", false) != null; //ReflectionContext exists in .NET 4.5+
}
