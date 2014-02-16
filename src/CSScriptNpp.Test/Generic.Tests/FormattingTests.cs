using ICSharpCode.NRefactory.CSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UltraSharp.Cecil;
using Xunit;

#pragma warning disable 1591

namespace CSScriptIntellisense.Test
{
    public class FormattingTests
    {
        [Fact]
        public void Lambdas_2()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";
            string code =
@"
using System;

    [STAThread]
    void main(string[] args)
    {          //comments A
    int test=0;

if()
{
}            
else
             {
}


//comments B
    InUIThread(() =>                       {
                           button1.Enabled = true;
                           textBox1.Enabled = false;
                       });

    }
void button1_Click(object sender,EventArg e)
        {
            try
            {
                proc.Kill();
            }
            catch { }
        }
";
            int pos = 0;
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);

            Assert.Equal(
@"using System;

[STAThread]
void main(string[] args)
{
    //comments A
    int test = 0;

    if()
    {
    }
    else
    {
    }

    //comments B
    InUIThread(() => {
        button1.Enabled = true;
        textBox1.Enabled = false;
    });
}

void button1_Click(object sender, EventArg e)
{
    try
    {
        proc.Kill();
    }
    catch { }
}", newCode);
        }

        [Fact]
        public void Lambdas_1()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";
            string code =
@"
using System;
    
void main()
{
    var lines = Npp.GetAllLines()
                   .Select(line =>
                           {
                                    if (false)
                               line.Split('\t'));
else 
                          line.Split('\n'));

                                    if (true)
{
line.Split('\t'));
}

                           });
}
";
            int pos = 0;
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);

            Assert.Equal(
@"using System;

void main()
{
    var lines = Npp.GetAllLines()
                   .Select(line =>
                           {
                               if (false)
                                   line.Split('\t'));
                               else
                                   line.Split('\n'));

                               if (true)
                               {
                                   line.Split('\t'));
                               }
                           });
}", newCode); ;
        }

        [Fact]
        public void NakedIf()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";
            string code =
@"using System;

void main()
{
    foreach(var lineItems in lines)
        if (lineItems.Length == 2)
            Npp.Output.WriteLine(""{0}: {1}"", lineItems.First(), lineItems.Last());
}";
            int pos = 0;
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);


            //File.WriteAllText(@"E:\Galos\Projects\CS-Script.Npp\CSScriptIntellisesnse\src\CSScriptIntellisense\bin\Debug\test.cs", newCode);

            Assert.Equal(code, newCode);  //the original code was already formatted
        }

        [Fact]
        public void FullSimpleIf()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";
            string code =
@"using System;
    
void main()
{
     if(true)
        Console.WriteLine(2);
    else if (false)
            print();
}";
            int pos = 0;
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);


            //File.WriteAllText(@"E:\Galos\Projects\CS-Script.Npp\CSScriptIntellisesnse\src\CSScriptIntellisense\bin\Debug\test.cs", newCode); return;

            Assert.Equal(
@"using System;

void main()
{
    if(true)
        Console.WriteLine(2);
    else if (false)
        print();
}", newCode);
        }

        [Fact]
        public void FullComplexIf()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";
            string code =
@"using System;
    
void main()
{

     if(true)
        Console.WriteLine(2);
    else if (false)
{            print();
}
}";
            int pos = 0;
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);

            Assert.Equal(
@"using System;

void main()
{
    if(true)
        Console.WriteLine(2);
    else if (false)
    {
        print();
    }
}", newCode);
        }

        [Fact]
        public void Comments()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";
            string code =
@"using System;
    
void main()
{
        Console.WriteLine(2); //test comment
}";
            int pos = 0;
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);

            Assert.Equal(
@"using System;

void main()
{
    Console.WriteLine(2); //test comment
}", newCode);
        }

        [Fact]
        public void Block_Indentation()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";
            string code =
@"
using System;
using System.Windows.Forms;

public class Test
{
    public static void Print(string text)
    {
        int t = """".Length;

        Console.WriteLine(text);
}

    public static void Print2(string text)
    {
        Print(text);
}
}";
            int pos = 0;
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);

            Assert.Equal(
@"using System;
using System.Windows.Forms;

public class Test
{
    public static void Print(string text)
    {
        int t = """".Length;

        Console.WriteLine(text);
    }

    public static void Print2(string text)
    {
        Print(text);
    }
}",
                    newCode);
        }


        [Fact]
        public void Class_Fluent_ControlLoops()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";
            string code =
@"
//css_args /ac
//css_inc test.cs
using System;

   [STAThread]
          void main(string[] args)


    {
    Console.WriteLine(             ""Hello     World!""   )  ;

var data = new []{1,2,3,4,5};
        var t =Win32.MF_UNCHECKED;
        var s = ""asdfgdsgfsd"".Replace(""\n"". ""#"")
                               .Replace(""\r"". """")
                               .Replace(""\t"". "" "") ;
Project project =
            new Project(""C# Intellisense for Notepad++"",
                new Dir(@""%ProgramFiles%\Notepad++\Plugins"",
                    new File(@""Plugins\CSScriptIntellisense.dll""),
                    new Dir(""CSharpIntellisense"",
                        new File(@""Plugins\CSharpIntellisense\CSScriptLibrary.dll""),
                        new File(@""Plugins\CSharpIntellisense\Mono.Cecil.dll""),
                        new File(@""Plugins\CSharpIntellisense\ICSharpCode.NRefactory.CSharp.dll""),
                        new File(@""Plugins\CSharpIntellisense\ICSharpCode.NRefactory.dll""))));


if(true)
{
Console.WriteLine( 1  )   ;
}
if(true)
    Console.WriteLine(2  )   ;
else if (false)
ttt();

if(true)     {}

        Test.Print(""Hello again..."");

     }
void Test(){
}void Test2(){
}

int Test3 { get { return 1; } }

public int MyProperty { get; set; }

        int testProQ;

        public int TestProp
        {
            get
            {
                return testProQ;
            }

            set
            {
                testProQ = value;
            }
        }

try
	{
		Console.WriteLine(7);
	}

	catch{}

while(false)
	{
		Console.WriteLine(7);
	}


do
	{
		Console.WriteLine(7);
	}while(false);

";
            int pos = 69; //void main(str|ing[] args)
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);

            //File.WriteAllText(@"E:\Galos\Projects\CS-Script.Npp\CSScriptIntellisesnse\src\CSScriptIntellisense\bin\Debug\test.cs", newCode);

            //return;
            Assert.Equal(
@"//css_args /ac
//css_inc test.cs
using System;

[STAThread]
void main(string[] args)
{
    Console.WriteLine(""Hello     World!"");

    var data = new [] { 1, 2, 3, 4, 5 };
    var t = Win32.MF_UNCHECKED;
    var s = ""asdfgdsgfsd"".Replace(""\n"". ""#"")
                               .Replace(""\r"". """")
                               .Replace(""\t"". "" "");
    Project project =
            new Project(""C# Intellisense for Notepad++"",
                new Dir(@""%ProgramFiles%\Notepad++\Plugins"",
                    new File(@""Plugins\CSScriptIntellisense.dll""),
                    new Dir(""CSharpIntellisense"",
                        new File(@""Plugins\CSharpIntellisense\CSScriptLibrary.dll""),
                        new File(@""Plugins\CSharpIntellisense\Mono.Cecil.dll""),
                        new File(@""Plugins\CSharpIntellisense\ICSharpCode.NRefactory.CSharp.dll""),
                        new File(@""Plugins\CSharpIntellisense\ICSharpCode.NRefactory.dll""))));

    if(true)
    {
        Console.WriteLine(1);
    }

    if(true)
        Console.WriteLine(2);
    else if (false)
        ttt();

    if(true) { }

    Test.Print(""Hello again..."");
}

void Test() {
}

void Test2() {
}

int Test3 { get { return 1; } }

public int MyProperty { get; set; }

int testProQ;

public int TestProp
{
    get
    {
        return testProQ;
    }

    set
    {
        testProQ = value;
    }
}

try
{
    Console.WriteLine(7);
}
catch { }

while(false)
{
    Console.WriteLine(7);
}

do
{
    Console.WriteLine(7);
}
while(false);", newCode);
        }

        [Fact]
        public void ShouldNotModifyStringsWithBrackets()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";

            string code = @"var t = ""    (\""{0}\"", \""{1}\"", \""{2}\"");"";";

            int pos = 0;
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);

            Assert.Equal(@"var t = ""    (\""{0}\"", \""{1}\"", \""{2}\"");"";", newCode);
        }

        [Fact]
        public void ShouldNotModifyStringsNorChars()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";

            string code = @"using System;
  
                            class Script
                            {  
                                void main2(int test,int test2) 
                                {
                                    var t = '=';
                                    t = ',';
                                    vat test=1;
                                }
                            }";

            int pos = 0;
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);

            Assert.Equal(
@"using System;

class Script
{
    void main2(int test, int test2)
    {
        var t = '=';
        t = ',';
        vat test = 1;
    }
}", newCode);

        }

        [Fact]
        public void ShouldNotBreakColloectionInits()
        {
            SourceCodeFormatter.UseTabs = false;
            SourceCodeFormatter.IndentText = "    ";

            string code = 
@"var tests = new []
{
    new Test
    {
        Name = ""Action_StartNPP""
    },
    new Test
    {
        Name = ""Action_StartNPP""
    }
};";

            int pos = 0;
            string newCode = SourceCodeFormatter.FormatCode(code, ref pos);

            Assert.Equal(code, newCode); //no changes

        }
    }
}
