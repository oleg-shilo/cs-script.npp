using CSScriptIntellisense;
using System.Linq;
using Xunit;

namespace Testing
{
    public class ReferenceResolving

    {
        public ReferenceResolving()
        {
            RoslynHost.Init();
        }

        private static string code =
            @"using System;
using System.Windows.Forms;

class Script
{
    [STAThread]
    static public void Main(string[] args)
    {
        MessageBox.Show(""Just a test!"");
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine(args[i]);
        }
        MessageBox.Show(""Done..."", ""Testing"");
        MessageBox.Show(""Done..."");
        //MessageBox.Show(""Done..."");
    }
}";

        [Fact(Skip = "waiting for Syntaxer migration")]
        public void GetSimpleReferences()
        {
            SimpleCodeCompletion.ResetProject();

            //MessageBox.Sh|ow
            var refs = SimpleCodeCompletion.FindReferences(code, 152, "test.cs");
            Assert.Equal(2, refs.Count());
            Assert.Equal(refs[0], @"test.cs(9,20): MessageBox.Show(""Just a test!"");");
            Assert.Equal(refs[1], @"test.cs(15,20): MessageBox.Show(""Done..."");");

            //Main(string[] ar|gs
            refs = SimpleCodeCompletion.FindReferences(code, 119, "test.cs");
            Assert.True(refs.Count() == 3);
            Assert.Equal(refs[0], "test.cs(7,38): static public void Main(string[] args)");
            Assert.Equal(refs[1], "test.cs(10,29): for (int i = 0; i < args.Length; i++)");
            Assert.Equal(refs[2], "test.cs(12,31): Console.WriteLine(args[i]);");

            //Main(strin|g[] args
            refs = SimpleCodeCompletion.FindReferences(code, 113, "test.cs");
            Assert.True(refs.Count() == 1);
            Assert.Equal(refs[0], "test.cs(7,29): static public void Main(string[] args)");

            //MessageBo|x.Show
            refs = SimpleCodeCompletion.FindReferences(code, 148, "test.cs");
            Assert.True(refs.Count() == 3);
            Assert.Equal(refs[0], @"test.cs(9,9): MessageBox.Show(""Just a test!"");");
            Assert.Equal(refs[1], @"test.cs(14,9): MessageBox.Show(""Done..."", ""Testing"");");
            Assert.Equal(refs[2], @"test.cs(15,9): MessageBox.Show(""Done..."");");

            //for (int |i
            refs = SimpleCodeCompletion.FindReferences(code, 190, "test.cs");
            Assert.True(refs.Count() == 4);
            Assert.Equal(refs[0], @"test.cs(10,18): for (int i = 0; i < args.Length; i++)");
            Assert.Equal(refs[1], @"test.cs(10,25): for (int i = 0; i < args.Length; i++)");
            Assert.Equal(refs[2], @"test.cs(10,42): for (int i = 0; i < args.Length; i++)");
            Assert.Equal(refs[3], @"test.cs(12,36): Console.WriteLine(args[i]);");
        }
    }
}