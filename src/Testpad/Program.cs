using System;
using System.IO;
using System.Linq;
using CSScriptNpp;
using CSScriptNpp.Dialogs;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.Editor;

namespace Testpad
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            FormattingTest();
            new FavoritesPanel().ShowDialog(); return;
            DebugExternal.ShowModal(); return;
            //new UpdateOptionsPanel("1.0.1.1").ShowDialog(); return;

            //var tt = typeof(List<int>).GetProperties();

            //var args = "dsfsd,df".Split(',');
            //var list = new List<string>(args);
            //var map = new Dictionary<int, int>()
            //{
            //    {1,3},
            //    {2,7}
            //};


            //new CSharpFormatter (FormattingOptionsFactory.CreateAllman ()).Format (code));

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);

            ////var panel = new AutoWatchPanel();
            ////panel.Test();

            ////Screen.FromPoint();

            //var dlg = new ShortcutBuilder();
            //dlg.Name = "test gtdrfgfds";
            //dlg.Shortcut = "Ctrl+Shift+Alt+F7";
            ////dlg.ShowDialog();
            ////return;

            //var panel = new DebugPanel();
            //panel.UpdateCallstack("+1|Script.cs.compiled!Script.Main(string[] args) Line 13|{$NL}+2|[External Code]|{$NL}");
            //panel.ShowDialog();

            //Application.Run(new Form1());
        }

        static void FormattingTest()
        {
            var code = @"using System;

class Test
{
    public static void Main(string[] args)
    {
        if (args != null ) {
        }
    }
}";
            var option = FormattingOptionsFactory.CreateAllman();
            //option.BlankLinesAfterUsings = 2;
            //BraceStyle.NextLine
            //option.SpaceWithinMethodCallParentheses = true;
            //option.BlankLinesBeforeFirstDeclaration = 0;
            //option.BlankLinesBetweenTypes = 1;
            //option.BlankLinesBetweenFields = 0;
            //option.BlankLinesBetweenEventFields = 0;
            //option.BlankLinesBetweenMembers = 1;
            //option.BlankLinesInsideRegion = 1;
            //option.InterfaceBraceStyle = BraceStyle.NextLineShifted;

            var syntaxTree = new CSharpParser().Parse(code, "test.cs");
            var newCode = syntaxTree.GetText(option);

            //var document = new StringBuilderDocument(code);
            //var formattingOptions = FormattingOptionsFactory.CreateAllman();
            //var options = new TextEditorOptions();
            //using (var script = new DocumentScript(document, formattingOptions, options))
            //{
            //    foreach (InvocationExpression expr in file.IndexOfInvocations)
            //    {
            //        var copy = (InvocationExpression)expr.Clone();
            //        copy.Arguments.Add(new IdentifierExpression("StringComparison").Member("Ordinal"));
            //        script.Replace(expr, copy);
            //    }
            //}

            //var newCode = document.Text; 
        }
    }
}
