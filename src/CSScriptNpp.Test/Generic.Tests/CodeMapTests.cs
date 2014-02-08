using ICSharpCode.NRefactory.CSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UltraSharp.Cecil;
using Xunit;

namespace CSScriptIntellisense.Test
{

    public class CodeMapTests
    {
        [Fact]
        public void CompleteFile()
        {
            var dataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string file = Path.Combine(dataDir, @"..\Local\Temp\CSScriptNpp\ReflctedTypes\Npp.2092620872.cs");
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
        public void AutoClassClass()
        {
            string code = @"//css_args /ac
                            //css_inc test.cs
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
    }
}
