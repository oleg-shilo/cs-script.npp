using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Intellisense.Common;
using System.Windows.Forms;
using CSScriptIntellisense;
using System.Reflection;

namespace Testing
{
    public static class StringExtensions
    {
        static public string RemoveOverloadsInfo(this string text)
        {
            return text.Split(new[] { "(+" }, StringSplitOptions.None)[0].TrimEnd();
        }
    }

    public class Misc
    {
        [Fact]
        public void ParseAsExceptionFileReference()
        {
            string text = @"   at ScriptClass.main(String[] args) in c:\Users\test\AppData\Local\Temp\CSSCRIPT\Cache\-1529274573\dev.g.csx:line 12";
            string file;
            int line;
            int column;

            bool success = text.ParseAsExceptionFileReference(out file, out line, out column);

            Assert.True(success);
            Assert.Equal(@"c:\Users\test\AppData\Local\Temp\CSSCRIPT\Cache\-1529274573\dev.g.csx", file);
            Assert.Equal(12, line);
            Assert.Equal(1, column);
        }

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

        int GetCaretPosition(ref string code)
        {
            int retval = code.IndexOf("|") + 1;
            code = code.Replace("|", "");
            return retval;
        }

        string GetCaretPosition(string code, out int pos)
        {
            pos = code.IndexOf("|") + 1;
            return code.Replace("|", "");
        }

        public Misc()
        {
            RoslynHost.Init();
        }

        [Fact]
        public void CompleteAtEmptySpace()
        {
            SimpleCodeCompletion.ResetProject();

            var data = SimpleCodeCompletion.GetCompletionData(code, 129, "test.cs");
            Assert.True(data.Count() > 0);
        }

        [Fact]
        public void GetPosition()
        {
            var file = Environment.ExpandEnvironmentVariables(@"C:\Users\%username%\Documents\C# Scripts\script.cs");

            if (File.Exists(file))
            {
                var pos = StringExtesnions.GetPosition(file, 7, 12);
                Assert.True(pos == 103);

                pos = StringExtesnions.GetPosition(file, 10, 22);
                Assert.True(pos == 146);
            }
        }

        [Fact]
        public void TypeNamespaceRemoved()
        {
            SimpleCodeCompletion.ResetProject();

            var data = SimpleCodeCompletion.GetCompletionData(code, 129, "test.cs");

            //no items in display text with full namespace present
            Assert.True(data.Where(x => x.DisplayText == "Environment").Count() > 0);
            Assert.True(data.Where(x => x.DisplayText == "System.Environment").Count() == 0);
        }

        [Fact]
        public void GetCssCompletion()
        {
            SimpleCodeCompletion.ResetProject();
            // "//css_|args "
            var data = SimpleCodeCompletion.GetCompletionData(@"//css_args /provider:E:\Galos\Pro...", 6, "test.cs");
        }

        [Fact]
        public void GetAddLocalEventHandlerCompletion()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Windows.Forms;

class Script
{
    static public void Main()
    {
        Script.Load +=|
    }
    static event EventHandler Load;
    static void OnLoad1(){}
}";
            int pos = GetCaretPosition(ref code);

            var data = SimpleCodeCompletion.GetCompletionData(code, pos, "test.cs");

            Assert.Equal(3, data.Count());
            Assert.Contains(data, x => x.DisplayText == "OnLoad - lambda");
            Assert.Contains(data, x => x.DisplayText == "OnLoad - delegate");
            Assert.Contains(data, x => x.DisplayText == "OnLoad - method");
            Assert.Equal("OnLoad2;", data.Last().CompletionText);
        }

        [Fact]
        public void GetAddExternalEventHandlerCompletion()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Windows.Forms;

class Script
{
    static public void Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += |
        //var form = new Form();
        //form.Load+= |
    }
}";
            int pos = GetCaretPosition(ref code);

            var data = SimpleCodeCompletion.GetCompletionData(code, pos, "test.cs");

            Assert.Equal(3, data.Count());
            Assert.Contains(data, x => x.DisplayText == "OnAssemblyResolve - lambda");
            Assert.Contains(data, x => x.DisplayText == "OnAssemblyResolve - delegate");
            Assert.Contains(data, x => x.DisplayText == "OnAssemblyResolve - method");
            //Assert.Equal("OnLoad;", data.Last().CompletionText);
        }

        [Fact]
        public void AddEventHandlerRespectsIncrement()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;

class Script
{
    static public void Main()
    {
        var text = ""test"";
        text +=|
    }
}";
            int pos = GetCaretPosition(ref code);

            var data = SimpleCodeCompletion.GetCompletionData(code, pos, "test.cs");

            Assert.Empty(data);
        }

        [Fact]
        public void GetCreateNewCompletion()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Windows.Forms;

class Script
{
    static public void Main()
    {
        Script.dialog =|
    }
    static Form dialog;
";
            int pos = GetCaretPosition(ref code);

            var data = SimpleCodeCompletion.GetCompletionData(code, pos, "test.cs");
        }

        [Fact]
        public void GetAssignmentCompletion()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Windows.Forms;

class Script
{
    static public void Main()
    {
        var form = new Form();
        form.DialogResult=|
    }
";
            int pos = GetCaretPosition(ref code);

            var data = SimpleCodeCompletion.GetCompletionData(code, pos, "test.cs");

            Assert.Equal(1, data.Count());
            Assert.Contains(data, x => x.DisplayText == "DialogResult - value");
            Assert.Equal("DialogResult.", data.Last().CompletionText);
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
        public void GenerateMemberMethodQuickInfo()
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
            Assert.Equal("Method: void Console.WriteLine(int value) (+ 18 overloads)", info.First().GetLine(0));
        }

        [Fact]
        public void GenerateMemberQuickInfo()
        {
            SimpleCodeCompletion.ResetProject();

            //132 - Console.Out|putEncoding;
            var code = @"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        var p = Console.O|ut;
    }
}";
            int pos = GetCaretPosition(ref code);

            string[] info = SimpleCodeCompletion.GetMemberInfo(code, pos, "test.cs", true);

            var p = Console.Out;

            Assert.Equal(1, info.Count());
            Assert.Equal("Property: TextWriter Console.Out { get; }", info.First().GetLine(0));
        }

        [Fact]
        public void GenerateTypeQuickInfo()
        {
            SimpleCodeCompletion.ResetProject();

            var code = @"using System;
using System.Linq;

/// <summary>
/// class decl
/// </summary>
class Script<T,T1,T2>
{
    static public void Main(string[] args)
    {
        var p = new Script<int, int, int>();
        var d = new DateT|ime(1);
    }
}";
            int pos = GetCaretPosition(ref code);

            string[] info = SimpleCodeCompletion.GetMemberInfo(code, pos, "test.cs", true);

            var p = Console.Out;

            Assert.Equal(1, info.Count());
            Assert.Equal("Constructor: DateTime(long ticks) (+ 11 overloads)", info.First().GetLine(0));
        }

        [Fact]
        public void GenerateConstructorQuickInfo()
        {
            SimpleCodeCompletion.ResetProject();

            //124 - new DateTim|e(
            string[] info = SimpleCodeCompletion.GetMemberInfo(@"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        new DateTime(1, 1, 1);
    }
}", 124, "test.cs", true);

            Assert.Equal(1, info.Count());
            var firstLine = info.First().GetLines(2).First();
            Assert.Equal("Constructor: DateTime(int year, int month, int day) (+ 11 overloads)", firstLine);
        }

        [Fact]
        public void GenerateConstructorQuickInfo1()
        {
            SimpleCodeCompletion.ResetProject();

            //121 - new Scrip|t()
            string[] info = SimpleCodeCompletion.GetMemberInfo(@"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        new Script();
    }
}", 121, "test.cs", true);

            Assert.Equal(1, info.Count());
            Assert.Equal("Constructor: Script()", info.First());
        }

        [Fact]
        public void GenerateTypeDeclarationQuickInfo()
        {
            SimpleCodeCompletion.ResetProject();

            //61 - Scr|ipt
            string[] info = SimpleCodeCompletion.GetMemberInfo(@"using System;
using System.Linq;

class Script
{
    Script script;

    static public void Main(string[] args)
    {
    }
}", 61, "test.cs", true);

            Assert.Equal(1, info.Count());
            Assert.Equal("Class: Script", info.First());
        }

        [Fact]
        public void GenerateConstructorFullInfo()
        {
            SimpleCodeCompletion.ResetProject();

            //126 - new DateTime(|
            string[] info = SimpleCodeCompletion.GetMemberInfo(@"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        new DateTime(1, 1, 1);new Script();
    }
}", 126, "test.cs", false);

            Assert.Equal(12, info.Count());
            Assert.Equal("Constructor: DateTime()", info.OrderBy(x => x).First().GetLine(0));
        }

        [Fact]
        public void GenerateMemeberFullInfo()
        {
            SimpleCodeCompletion.ResetProject();

            var code = @"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        Console.WriteLine(|args.Length);
    }
}";
            int pos = GetCaretPosition(ref code);
            //Console.WriteLine(|
            string[] info = SimpleCodeCompletion.GetMemberInfo(code, 131, "test.cs", false)
                                                .OrderBy(x => x)
                                                .ToArray();

            Assert.Equal(19, info.Count());
            Assert.Equal("Method: void Console.WriteLine()", info[0].GetLine(0).RemoveOverloadsInfo());
            Assert.Equal("Method: void Console.WriteLine(bool value)", info[1].GetLine(0).RemoveOverloadsInfo());
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
            Assert.Equal("test" + Environment.NewLine, builder.TrimEmptyEndLines(2).ToString());

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