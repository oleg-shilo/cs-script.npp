using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace CSScriptIntellisense.Test
{
    public class GenericTest
    {
        static string code =
            @"using System;
using System.Windows.Forms;

class Script
{
    [STAThread]
    static public void Main(string[] args)
    {
        MessageBox.Show(""Just a test!"");

        File.

        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine(args[i]);
        }
    }
}

namespace NSTest
{
    public class TopTest
    {
        public class SubTest
        {
        }
    }
}

";

        [Fact]
        public void CompleteEmptySpace()
        {
            SimpleCodeCompletion.ResetProject();

            var data = SimpleCodeCompletion.GetCompletionData(code, 120, "test.cs");
            Assert.True(data.Count() > 0);
        }

        [Fact]
        public void TypeNamespaceRemoved()
        {
            SimpleCodeCompletion.ResetProject();

            var data = SimpleCodeCompletion.GetCompletionData(code, 120, "test.cs");

            Assert.True(data.Where(x => x.DisplayText == "Environment").Count() > 0);
            Assert.True(data.Where(x => x.DisplayText == "System.Environment").Count() == 0);
        }

        [Fact]
        public void CompletePartialWord()
        {
            SimpleCodeCompletion.ResetProject();

            // Messa|geBox.Show("Just a test!");
            var data = SimpleCodeCompletion.GetCompletionData(code, 129, "test.cs");

            Assert.True(data.Where(x => x.DisplayText == "MessageBox").Any());
        }

        [Fact]
        public void CompleteMethodArguments()
        {
            SimpleCodeCompletion.ResetProject();

            //fileNam|
            var data = SimpleCodeCompletion.GetCompletionData(@"using System.IO;
using System;

class Script
{
    [STAThread]
    static public void Main(string[] args)
    {
        string dirName = ""111"";
        string fileName = ""222"";
        string statsFile = Path.Combine(dirName, fileNam
    }
}", 242, "test.cs", true);
            Assert.True(data.Where(x => x.DisplayText == "fileName").Any());

            //Note the test will fail if "dirName, fileNam" replaced with "dirName,fileNam"
            //It is a CSharpCompletionEngine flaw.
        }

        [Fact]
        public void SuggestMissingUsings()
        {
            SimpleCodeCompletion.ResetProject();

            // File|.
            var info = SimpleCodeCompletion.GetMissingUsings(code, 187, "test.cs").FirstOrDefault();

            Assert.Equal("System.IO", info.Namespace);
            Assert.Equal("System.IO.File", info.FullName);
        }

        [Fact]
        public void SuggestMissingUsingsForPartialWordAtCaret()
        {
            SimpleCodeCompletion.ResetProject();

            // F|ile.
            var info = SimpleCodeCompletion.GetMissingUsings(code, 184, "test.cs").FirstOrDefault();

            Assert.Equal("System.IO", info.Namespace);
            Assert.Equal("System.IO.File", info.FullName);
        }

        [Fact]
        public void SuggestMissingUsingsForTopLevelType()
        {
            SimpleCodeCompletion.ResetProject();

            var info = SimpleCodeCompletion.GetPossibleNamespaces(code, "File", "test.cs").ToArray();

            Assert.Equal("System.IO", info[0].Namespace);
            Assert.Equal("System.IO.File", info[0].FullName);

            Assert.Equal("System.Net", info[1].Namespace);
            Assert.Equal("System.Net.WebRequestMethods.File", info[1].FullName);

            Assert.False(info[0].IsNested);
            Assert.True(info[1].IsNested);
        }

        [Fact]
        public void SuggestMissingUsingsForNestedType()
        {
            SimpleCodeCompletion.ResetProject();

            var info = SimpleCodeCompletion.GetPossibleNamespaces(code, "SubTest", "test.cs").FirstOrDefault();

            Assert.Equal("NSTest", info.Namespace);
            Assert.Equal("NSTest.TopTest.SubTest", info.FullName);
        }

        [Fact]
        public void GetWordAt()
        {
            //Console.Wri|teLine
            string word = SimpleCodeCompletion.GetWordAt("Console.WriteLine;", 11);
            
            Assert.Equal("WriteLine", word);
        }

        [Fact]
        public void GenerateMemeberQuickInfo()
        {
            SimpleCodeCompletion.ResetProject();

            //Console.Wri|teLine
            string[] info = SimpleCodeCompletion.GetMemberInfo(@"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        Console.WriteLine(args.Length);
    }
}", 124, "test.cs", true);

            Assert.Equal(1, info.Count());
            Assert.Equal("Method: void Console.WriteLine() (+ 18 overload(s))", info.First().GetLines(2).First());
        }
        

        [Fact]
        public void GenerateMemeberFullInfo()
        {
            SimpleCodeCompletion.ResetProject();

            //Console.WriteLine(|
            string[] info = SimpleCodeCompletion.GetMemberInfo(@"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        Console.WriteLine(args.Length);
    }
}", 131, "test.cs", false);

            Assert.Equal(19, info.Count());
            Assert.Equal("Method: void Console.WriteLine()", info[0].GetLines(2).First());
            Assert.Equal("Method: void Console.WriteLine(bool value)", info[1].GetLines(2).First());
        }

        [Fact]
        public void ProcessGenericsInDisplayInfo()
        {
            string cleared = "Method: IQueryable`1[[``0]] Queryable.AsQueryable`1[[``0]](IEnumerable`1[[``0]] source, Action`1[[List`1[[``0]]]])".ProcessGenricNotations();
            Assert.Equal("Method: IQueryable<T> Queryable.AsQueryable<T>(IEnumerable<T> source, Action<List<T>>)", cleared);

            cleared = "Method: ``0[] Enumerable.ToArray`1[[``0]](IEnumerable`1[[``0]] source)".ProcessGenricNotations();
            Assert.Equal("Method: T[] Enumerable.ToArray<T>(IEnumerable<T> source)", cleared);

            cleared = "Method: IEnumerable`1[[``0]] Enumerable.Where`1[[``0]](IEnumerable`1[[``0]] source, Func`2[[``0],[bool]] predicate)".ProcessGenricNotations();
            Assert.Equal("Method: IEnumerable<T> Enumerable.Where<T>(IEnumerable<T> source, Func<T,bool> predicate)", cleared);

            cleared = "Method: Dictionary`2[[``1],[``0]] Enumerable.ToDictionary`2[[``0],[``1]](IEnumerable`1[[``0]] source, Func`2[[``0],[``1]] keySelector)".ProcessGenricNotations();
            Assert.Equal("Method: Dictionary<T1,T> Enumerable.ToDictionary<T,T1>(IEnumerable<T> source, Func<T,T1> keySelector)", cleared);
        }

        [Fact]
        public void StringBuilderExtensions1()
        {
            Assert.True("\r\ndo\r\n".IsToken("do", 3));
        }
       
        [Fact]
        public void StringBuilderExtensions3()
        {
            var builder = new StringBuilder();
            builder.Append("test\r\nTEST");
            Assert.Equal("test", builder.GetLineFrom(3));
            Assert.Equal("test", builder.GetLineFrom(2));
            Assert.Equal("test", builder.GetLineFrom(4));
            Assert.Equal("test", builder.GetLineFrom(5));
            Assert.Equal("TEST", builder.GetLineFrom(9));
            Assert.Equal(null, builder.GetLineFrom(29));
            Assert.Equal("TEST", builder.GetLastLine());

            builder.Clear();
            builder.Append("test\r\nTEST\r\n");
            Assert.Equal("", builder.GetLineFrom(11));
            Assert.Equal("TEST", builder.GetLineFrom(9));
            
            Assert.Equal("", builder.GetLastLine());
        }
        
        [Fact]
        public void StringBuilderExtensions2()
        {
            var builder = new StringBuilder();
            builder.AppendLine("");
            builder.AppendLine("");
            Assert.Equal("", builder.TrimEmptyEndLines().ToString());

            builder.Clear();
            builder.Append("test");
            Assert.Equal("test", builder.TrimEmptyEndLines().ToString());

            builder.Clear();
            builder.AppendLine("");
            Assert.Equal("", builder.TrimEmptyEndLines(2).ToString());

            builder.Clear();
            builder.AppendLine("test");
            Assert.Equal("test"+Environment.NewLine, builder.TrimEmptyEndLines(2).ToString());

            builder.Clear();
            builder.AppendLine("test");
            builder.AppendLine("");
            Assert.Equal("test" + Environment.NewLine + Environment.NewLine, builder.TrimEmptyEndLines(2).ToString());
            
            builder.Clear();
            builder.AppendLine("test");
            builder.AppendLine("");
            builder.AppendLine("");
            Assert.Equal("test" + Environment.NewLine + Environment.NewLine + Environment.NewLine, builder.TrimEmptyEndLines(2).ToString());

            builder.Clear();
            builder.AppendLine("test");
            builder.AppendLine("");
            builder.AppendLine("");
            builder.AppendLine("");
            builder.AppendLine("");
            builder.AppendLine("");
            builder.AppendLine("");
            Assert.Equal("test" + Environment.NewLine + Environment.NewLine + Environment.NewLine, builder.TrimEmptyEndLines(2).ToString());

            builder.Clear();
            builder.AppendLine("test");
            builder.AppendLine("");
            builder.AppendLine("");
            Assert.Equal("test" + Environment.NewLine, builder.TrimEmptyEndLines(0).ToString());
            
            builder.Clear();
            builder.Append(
@"{

");
            string test = builder.TrimEmptyEndLines(0).ToString();
            Assert.Equal("{" + Environment.NewLine, test);
        }
    }



}