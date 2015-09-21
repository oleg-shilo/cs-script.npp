using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using msFormatter = Microsoft.CodeAnalysis.Formatting.Formatter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using System.Diagnostics;

namespace CSScriptNpp.Roslyn
{
    public static class Formatter
    {
        static MSBuildWorkspace dummyWorkspace;
        static MSBuildWorkspace DummyWorkspace
        {
            get
            {
                if (dummyWorkspace == null)
                {
                    //https://github.com/dotnet/roslyn/issues/202
                    dummyWorkspace = MSBuildWorkspace.Create();
                    //var opt = workspace.Options;
                    //.WithChangedOption(CSharpFormattingOptions.SpaceBeforeDot, true)
                    //.WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, false)
                    //.WithChangedOption(CSharpFormattingOptions.OpenBracesInNewLineForMethods, false)
                    //.WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, 2);
                    //msFormatter.Format(tree.GetRoot(), DummyWorkspace, opt);
                }
                return dummyWorkspace;
            }
        }

        struct PosRange
        {
            public int Start;
            public int End;
        }

        public static string Format(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code.Trim());
            bool normaliseLines = false;

            var root = tree.GetRoot();
            //var testCode = root.ToFullString();

            //var node = root.DescendantNodes().First();
            //var newRoot = root.ReplaceNode(node, node.WithLeadingTrivia(SyntaxFactory.Whitespace("\r\n")));
            //testCode = newRoot.ToFullString();

            //if (normaliseLines)
            //{
            //    root = root.ReplaceNodes(root.DescendantNodes()
            //                                 .Where(n => n.Kind() == SyntaxKind.ClassDeclaration || n.Kind() == SyntaxKind.MethodDeclaration),
            //                             (_, n) => n.WithLeadingTrivia(SyntaxFactory.Whitespace("\r\n")));

            //    var formatted = root.ToFullString();
            //}

            var formattedTree = msFormatter.Format(root, DummyWorkspace);
            var formattedCode = formattedTree.ToFullString();

            if (normaliseLines)
            {
                var decls = formattedTree.DescendantNodes()
                                         .Where(n => n.Kind() == SyntaxKind.ClassDeclaration || n.Kind() == SyntaxKind.MethodDeclaration)
                                         .Select(x => x.FullSpan.Start)
                                         .Distinct()
                                         .OrderByDescending(x => x)
                                         .ToArray();

                //var comments = formattedTree.DescendantNodes()
                //                   .Where(n => n.Kind() == SyntaxKind.MultiLineCommentTrivia)
                //                   .Select(x => x.FullSpan.ToString())
                //                   .ToArray();

                var strings = formattedTree.DescendantNodes()
                                   .Where(n => n.Kind() == SyntaxKind.StringLiteralExpression)
                                   .Select(x => new PosRange { Start = x.FullSpan.Start, End = x.FullSpan.End })
                                   .ToArray();

                var sb = new StringBuilder(formattedCode);
                foreach (int pos in decls)
                {
                    sb.Insert(pos, "\r\n");
                    foreach (PosRange item in strings.ToArray())
                    {
                        if (item.Start >= pos)
                        {
                            var range = item;
                            range.Start += 2;
                            range.End += 2;
                        }
                    }
                }

                for (int i = formattedCode.Length - 0; i > 0;)
                {

                    i = formattedCode.LastIndexOf("\r\n", i);
                    if (i != -1)
                    {
                        if (strings.Any(x => x.Start <= i && i <= x.End))
                            continue;

                        var i2 = formattedCode.LastIndexOf("\r\n\r\n", i);
                        if (i2 != -1 && (i - i2) == 4)
                        {
                            sb.Remove(i, 2);
                        }
                    }
                }

                formattedCode = sb.ToString();
            }

            return formattedCode;

            //ff.DescendantNodes()
            //  .Where(n => n.Kind().ToString().EndsWith("Declaration"))
            //  .ToList()
            //  .ForEach(x => x.WithLeadingTrivia(SyntaxFactory.EndOfLine(" ")));

            //var ddd = ff.ToFullString();
            //var classDeclaratio = ttt.Last();
            //        .ReplaceNodes(
            //formattedUnit.DescendantNodes()
            //             .OfType<PropertyDeclarationSyntax>()
            //             .SelectMany(p => p.AttributeLists),
            //(_, node) => node.WithTrailingTrivia(Syntax.Whitespace("\n")));
            //classDeclaratio.InsertNodesBefore(Syntax.Whitespace("\n"));


        }

        static void Main(string[] args)
        {
            string file = @"C:\Users\%USERNAME%\Documents\C# Scripts\New Script34.cs";
            file = Environment.ExpandEnvironmentVariables(file);
            args = new[] { file };
            var code = File.ReadAllText(args.First());

            string formattedCode = Format(code);

            Console.WriteLine(formattedCode);
        }
    }
}
