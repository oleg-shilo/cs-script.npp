using System;
using System.IO;
using System.Linq;
using CSScriptIntellisense;
using RoslynIntellisense;
using Xunit;

namespace Testing
{
    public class RoslynIntellisense : RoslynHost
    {
        [Fact]
        public void DocProvider_CanAccess_LibXml()
        {
            var asm = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll";
            var provider = NppDocumentationProvider.NewFor(asm);

            var doc = provider.GetDocumentationFor("P:System.Console.Out");

            Assert.True(doc.StartsWith("<member name=\"P:System.Console.Out\">"));
            Assert.True(doc.Contains("that represents the standard output stream."));
        }

        [Fact]
        public void ComposeEventPropTooltip()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Collections.Generic;
using System.Linq;

static class Script
{
    static IEnumerable<List<int>> test = new List<List<int>>();
    IEnumerable<List<int>> Test {get { return test;} }
    event Action action;
    static public void Main(string[] args)
    {
        var t = Script.T|est;
        action = null;
    }
}";
            int pos = GetCaretPosition(ref code);

            var tooltip = SimpleCodeCompletion.GetMemberInfo(code, pos, "test.cs", true);
            Assert.StartsWith("Property: IEnumerable<List<int>> Script.Test { get; }", tooltip.First());

            //act|ion
            tooltip = SimpleCodeCompletion.GetMemberInfo(code, pos + 17, "test.cs", true);
            Assert.StartsWith("Event: Action Script.action", tooltip.First());
        }

        [Fact]
        public void ComposeMethodFieldTooltip()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Collections.Generic;
using System.Linq;

class Script
{
    static IEnumerable<List<int>> test = new List<List<int>>();
    static public void Main(string[] args)
    {
        test.Co|ncat(null);
        var t = """".Ca|st<char>();
        Console.WriteLine(1);
    }
}";

            int pos = GetCaretPosition(ref code);

            var tooltip = SimpleCodeCompletion.GetMemberInfo(code, pos, "test.cs", true);
            Assert.StartsWith("Method (extension): IEnumerable<List<int>> IEnumerable<List<int>>.Concat<List<int>>(IEnumerable<List<int>> second)", tooltip.First());

            //.Ca|st<char>();
            tooltip = SimpleCodeCompletion.GetMemberInfo(code, pos - 4, "test.cs", true);
            Assert.StartsWith("Field: IEnumerable<List<int>> Script.test", tooltip.First());

            //Console.Write|Line
            tooltip = SimpleCodeCompletion.GetMemberInfo(code, pos + 70, "test.cs", true);
            Assert.StartsWith("Method: void Console.WriteLine(int value) (+ 18 overloads)", tooltip.First());
        }

        [Fact]
        public void ProcessMethodOverloadsHint()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        Console.WriteLine(ar|gs.Length);
    }
}";

            var t = "".Cast<char>();
            int pos = GetCaretPosition(ref code);

            //Simulate invoking ShowMathodInfo
            //Console.WriteLine(|
            string[] signatures = SimpleCodeCompletion.GetMemberInfo(code, pos, "test.cs", false);

            Assert.Equal(19, signatures.Count()); // may need to be updated for the new .NET versions

            //Simulate typing...
            //Console.WriteLine("Time {0}", DateTime.|

            var popup = new MemberInfoPanel();

            popup.AddData(signatures);
            Assert.Equal(19, popup.items.Count);

            popup.ProcessMethodOverloadHint(new[] { "Time {0}" });  //'single and more' parameter methods
            Assert.Equal(18, popup.items.Count);

            popup.ProcessMethodOverloadHint(new[] { "\"Time {0}\"", "DateTime." });  //'two and more' parameter methods
            Assert.Equal(6, popup.items.Count);
        }

        [Fact]
        public void ResolveSymbolFromCode()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        Te|st = 5;
        Console.WriteLine(args.Length);
    }
    static int Test;
}";
            int pos = GetCaretPosition(ref code);

            var region = SimpleCodeCompletion.ResolveMember(code, pos, "test.cs");

            Assert.Equal(16, region.BeginColumn);
            Assert.Equal(11, region.BeginLine);
            Assert.Equal(11, region.EndLine);
            Assert.Equal("test.cs", region.FileName);
        }

        [Fact]
        public void ResolveSymbolFromCSharp_7_Code()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        Te|st = 5;
        Console.WriteLine(args.Length);
    }
    static int Test;
}";
            int pos = GetCaretPosition(ref code);

            var region = SimpleCodeCompletion.ResolveMember(code, pos, "test.cs");

            Assert.Equal(16, region.BeginColumn);
            Assert.Equal(11, region.BeginLine);
            Assert.Equal(11, region.EndLine);
            Assert.Equal("test.cs", region.FileName);
        }

        [Fact]
        public void ResolveSymbolFromAsm()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        var t2 = ""dsfas"".Repla|ce(""tttt"", "");
        var t = ""dsfas"".Leng|th;
        var err = Cons|ole.Error;
        //Console.Write|Line(""Test {0}"", args.Length);
    }
    static int Test;
}";
            int pos = GetCaretPosition(ref code);

            var region = SimpleCodeCompletion.ResolveMember(code, pos, "test.cs");

            //Assert.Equal(16, region.BeginColumn);
            //Assert.Equal(11, region.BeginLine);
            //Assert.Equal(11, region.EndLine);
            //Assert.Equal("test.cs", region.FileName);
        }

        [Fact]
        public void FindReferences()
        {
            SimpleCodeCompletion.ResetProject();

            var code =
    @"using System;
using System.Linq;

class Script
{
    static public void Main(string[] args)
    {
        var s = new Script();
        s.Test();
        s.Te|st();
        s.Test();
        s.Test();
    }

    static void Test(){};
}";
            int pos = GetCaretPosition(ref code);

            //test.cs(9,11): s.Test();
            var locations = SimpleCodeCompletion.FindReferences(code, pos, "test.cs");
        }

        int GetCaretPosition(ref string code)
        {
            int retval = code.IndexOf("|") + 1;
            code = code.Replace("|", "");
            return retval;
        }
    }
}