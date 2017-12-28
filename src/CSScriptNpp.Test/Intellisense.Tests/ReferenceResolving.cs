using CSScriptIntellisense;
using System.Linq;
using Xunit;

namespace Testing
{
    public class ReferenceResolving

    {
        public ReferenceResolving()
        {
            // RoslynHost.Init();
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
            Assert.Equal(@"test.cs(9,20): MessageBox.Show(""Just a test!"");", refs[0]);
            Assert.Equal(@"test.cs(15,20): MessageBox.Show(""Done..."");", refs[1]);

            //Main(string[] ar|gs
            refs = SimpleCodeCompletion.FindReferences(code, 119, "test.cs");
            Assert.True(refs.Count() == 3);
            Assert.Equal("test.cs(7,38): static public void Main(string[] args)", refs[0]);
            Assert.Equal("test.cs(10,29): for (int i = 0; i < args.Length; i++)", refs[1]);
            Assert.Equal("test.cs(12,31): Console.WriteLine(args[i]);", refs[2]);

            //Main(strin|g[] args
            refs = SimpleCodeCompletion.FindReferences(code, 113, "test.cs");
            Assert.True(refs.Count() == 1);
            Assert.Equal("test.cs(7,29): static public void Main(string[] args)", refs[0]);

            //MessageBo|x.Show
            refs = SimpleCodeCompletion.FindReferences(code, 148, "test.cs");
            Assert.True(refs.Count() == 3);
            Assert.Equal(@"test.cs(9,9): MessageBox.Show(""Just a test!"");", refs[0]);
            Assert.Equal(@"test.cs(14,9): MessageBox.Show(""Done..."", ""Testing"");", refs[1]);
            Assert.Equal(@"test.cs(15,9): MessageBox.Show(""Done..."");", refs[2]);

            //for (int |i
            refs = SimpleCodeCompletion.FindReferences(code, 190, "test.cs");
            Assert.True(refs.Count() == 4);
            Assert.Equal(@"test.cs(10,18): for (int i = 0; i < args.Length; i++)", refs[0]);
            Assert.Equal(@"test.cs(10,25): for (int i = 0; i < args.Length; i++)", refs[1]);
            Assert.Equal(@"test.cs(10,42): for (int i = 0; i < args.Length; i++)", refs[2]);
            Assert.Equal(@"test.cs(12,36): Console.WriteLine(args[i]);", refs[3]);
        }
    }
}