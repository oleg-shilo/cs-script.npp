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
using System.Text.RegularExpressions;

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

        public static string Format(string code)
        {
            bool normaliseLines = true;

            var result = "";
            var tree = CSharpSyntaxTree.ParseText(code.Trim());
            var root = msFormatter.Format(tree.GetRoot(), DummyWorkspace);

            if (normaliseLines)
            {
                //injecting line-breaks to separate declarations
                root = root.ReplaceNodes(root.DescendantNodes()
                                             .Where(n => n.IsKind(SyntaxKind.MethodDeclaration) || n.IsKind(SyntaxKind.ClassDeclaration)),
                                              (_, node) =>
                                              {
                                                  var existingTrivia = node.GetLeadingTrivia().ToFullString();
                                                  if (existingTrivia.Contains(Environment.NewLine))
                                                      return node;
                                                  else
                                                      return node.WithLeadingTrivia(SyntaxFactory.Whitespace(Environment.NewLine + existingTrivia));
                                              });

                //Removing multiple line breaks.
                //doesn't visit all "\r\n\r\n" cases. No time for this right now.
                //Using primitive RemoveDoubleLineBreaks instead but may need to be solved in the future
                //root = root.ReplaceNodes(root.DescendantNodes()
                //                             .Where(n => !n.IsKind(SyntaxKind.StringLiteralExpression)),
                //                              (_, node) =>
                //                              {

                //                                  var existingLTrivia = node.GetLeadingTrivia().ToFullString();

                //                                  if (existingLTrivia.Contains(Environment.NewLine + Environment.NewLine))
                //                                      return node.WithLeadingTrivia(SyntaxFactory.Whitespace(existingLTrivia.RemoveDoubleLineBreaks()));
                //                                  else
                //                                      return node;
                //                              });

                result = root.ToFullString()
                             .RemoveDoubleLineBreaks(root)
                             ;
            }
            else
                result = root.ToFullString();

            return result;
        }


        static string RemoveDoubleLineBreaks(this string formattedCode, SyntaxNode root)
        {
            var strings = root.DescendantNodes()
                                  .Where(n => n.Kind() == SyntaxKind.StringLiteralExpression)
                                  .Select(x => new { Start = x.FullSpan.Start, End = x.FullSpan.End })
                                  .ToArray();

            var sb = new StringBuilder(formattedCode);

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

            var result = sb.ToString();
            return result;
        }

        static void Main(string[] args)
        {
            //string file = @"C:\Users\%USERNAME%\Documents\C# Scripts\New Script34.cs";
            //file = Environment.ExpandEnvironmentVariables(file);
            //args = new[] { file };
            var code = File.ReadAllText(args.First());

            string formattedCode = Format(code);

            Console.WriteLine(formattedCode);
        }
    }
}
