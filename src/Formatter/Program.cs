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

        public static string Format(string code, ref int pos)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var formattedCode = msFormatter.Format(tree.GetRoot(), DummyWorkspace)
                                           .ToFullString();

            pos = MapAbsPosition(code, pos, formattedCode);

            return formattedCode;
        }

        public static int MapAbsPosition(string textA, int positionA, string textB)
        {
            bool atWhitespace = false;

            int syntaxPos = textA.GetSyntaxPos(positionA, ref atWhitespace);

            if (atWhitespace)
                return textB.GetAbsolutePos(syntaxPos);
            else
                return textB.GetAbsolutePos(syntaxPos) - 1;
        }

        internal static int GetSyntaxPos(this string text, int absolutePosition, ref bool atWhitespace)
        {
            int result = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (i == absolutePosition)
                    atWhitespace = (text[i] == ' ' || text[i] == '\t'); //printable whitespace

                if (!char.IsWhiteSpace(text[i]))
                {
                    if (absolutePosition < i)
                        break;
                    result++;
                }
            }
            return result;
        }

        internal static int GetAbsolutePos(this string text, int syntaxPosition)
        {
            int result = 0;
            int i = 0;
            for (; i < text.Length && syntaxPosition > 0; i++)
            {
                result++;
                if (!char.IsWhiteSpace(text[i]))
                {
                    syntaxPosition--;
                }
            }

            if (text[i] == '\n' && (i + 1) < text.Length)
                result++;

            return result;
        }

        static void Main(string[] args)
        {
            //string file = @"E:\Dev\BackupDir.cs";
            var code = File.ReadAllText(args.First());

            int pos = 0;
            string formattedCode = Format(code, ref pos);

            Console.WriteLine(formattedCode);
        }
    }
}
