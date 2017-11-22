using ICSharpCode.NRefactory.CSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Testing;
using UltraSharp.Cecil;
using Xunit;

namespace Tests
{
    public class CodeMap
    {
        [Fact]
        public void CompleteFile()
        {
            var dataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string file = Path.Combine(dataDir, @"..\Local\Temp\CSScriptNpp\ReflctedTypes\Npp.2092620872.cs");
            //string file = Path.Combine(dataDir, @"..\Local\Temp\CSScriptNpp\ReflctedTypes\Int32.-1466498946.cs");
            //string file = Path.Combine(dataDir, @"E:\cs-script\lib\debugVS12.0.cs");
            //file = Path.Combine(dataDir, @"E:\cs-script\inc\WebApi.cs");
            if (File.Exists(file)) //test during dev only
            {
                string code = File.ReadAllText(file);
                var map = Reflector.GetMapOf(code);
            }
        }

        [Fact]
        public void CompleteClass()
        {
            string code = @"using System;

                            class Script
                            {
                                int Count;
                                int fieldI;
                                int prop {get;set;}
                                void main0() {}
                                void main1(int test) {}
                                void main2(int test, int test2) {}
                            }";

            var map = Reflector.GetMapOf(code);

            Assert.Equal(4, map.Count());

            Assert.Equal(7, map[0].Line);
            Assert.Equal("prop", map[0].DisplayName);
            Assert.Equal("Script", map[0].ParentDisplayName);

            Assert.Equal(8, map[1].Line);
            Assert.Equal("main0()", map[1].DisplayName);

            Assert.Equal(9, map[2].Line);
            Assert.Equal("main1()", map[2].DisplayName);

            Assert.Equal(10, map[3].Line);
            Assert.Equal("main2(,)", map[3].DisplayName);
        }

        [Fact]
        public void DetectGetUsings()
        {
            string code = @"using System;
                            using System.IO;

                            class Script
                            {
                                int Count;
                                int fieldI;
                                int prop {get;set;}
                                void main0() {}
                                void main1(int test) {}
                                void main2(int test, int test2) {}
                            }";

            string[] items = Reflector.GetCodeUsings(code);
            Assert.Equal(2, items.Length);
            Assert.Equal("System", items[0]);
            Assert.Equal("System.IO", items[1]);
        }

        [Fact]
        public void AutoClassClass()
        {
            string code =
@"//css_inc test.cs
//css_args /ac
using System;
using System.Linq;
using System.Data;

void main (string[] args)
{
    Console.WriteLine(""Hello World!"");
}
int Count;
int TestProp { get; set; }
void TestMethod() {}";

            var map = Reflector.GetMapOf(code);

            Assert.Equal(3, map.Count());

            Assert.Equal(7, map[0].Line);
            Assert.Equal("main()", map[0].DisplayName);

            Assert.Equal(12, map[1].Line);
            Assert.Equal("TestProp", map[1].DisplayName);

            Assert.Equal(13, map[2].Line);
            Assert.Equal("TestMethod()", map[2].DisplayName);
        }

        [Fact]
        public void NestedClasses()
        {
            string code = @"using System;

//test
[Description(""Test"")]
class ScriptA
{
    int Count;
    int fieldI;
    int prop {get;set;}
    void main0() {}
    void main1(int test) {}
    void main2(int test, int test2) {}

    class Printer
    {
        void Print(int test) {}
        string Name {get;set;}

        class Settings
        {
            void Print(int test) {}
            string Name {get;set;}
        }
    }
}

class ScriptB
{
    int CountB;
    int fieldIB;
    int propB {get;set;}
    void main0() {}
    void main1B(int test) {}
    void main2B(int test, int test2) {}
}";

            var map = Reflector.GetMapOf(code).OrderBy(x => x.ParentDisplayName)
                               .Select(x => string.Format("{0}.{1}: Line {2}", x.ParentDisplayName, x.DisplayName, x.Line))
                               .ToArray();

            string mapDisplay = string.Join(Environment.NewLine, map);
            TextAssert.Equal(mapDisplay,
@"ScriptA.prop: Line 10
ScriptA.main0(): Line 11
ScriptA.main1(): Line 12
ScriptA.main2(,): Line 13
ScriptA.Printer.Print(): Line 17
ScriptA.Printer.Name: Line 18
ScriptA.Printer.Settings.Print(): Line 22
ScriptA.Printer.Settings.Name: Line 23
ScriptB.propB: Line 33
ScriptB.main0(): Line 34
ScriptB.main1B(): Line 35
ScriptB.main2B(,): Line 36");
        }
    }
}