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

        public static string Format(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var formattedCode = msFormatter.Format(tree.GetRoot(), DummyWorkspace)
                                           .ToFullString();

            return formattedCode;
        }

        static void Main(string[] args)
        {
            //string file = @"E:\Dev\BackupDir.cs";
            var code = File.ReadAllText(args.First());

            string formattedCode = Format(code);

            Console.WriteLine(formattedCode);
        }
    }
}
