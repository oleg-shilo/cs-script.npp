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

        //public static string Format(string code, ref int pos)
        //{
        //    var tree = CSharpSyntaxTree.ParseText(code);

        //    var formattedCode = msFormatter.Format(tree.GetRoot(), DummyWorkspace)
        //                                   .ToFullString();

        //    pos = MapAbsPosition(code, pos, formattedCode);

        //    return formattedCode;
        //}

        public static string Format(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var formattedCode = msFormatter.Format(tree.GetRoot(), DummyWorkspace)
                                           .ToFullString();

            return formattedCode;
        }

        internal class PosInfo
        {
            public int Row = -1;
            public int Column = -1;
            public int SyntaxLength = -1;
            public int AbsolutePos = -1;

            public override string ToString()
            {
                return string.Format("R={0}, C={1}, S={2}, A={3}",
                    Row, Column, SyntaxLength, AbsolutePos);
            }
        }

        public static int MapAbsPosition(string textA, int positionA, string textB)
        {
            //bool endOfLine = (textA[positionA] == '\r');
            //if (endOfLine)
              //  positionA--;

            int rightOffset = textA.OffsetToNextToken(positionA);

            int syntaxLength = textA.PosToSyntaxLength(positionA + rightOffset);
            int positionB = textB.SyntaxLengthToPos(syntaxLength);

            string s1 = textA.Substring(0, positionA);
            string s2 = textA.Substring(0, positionA + rightOffset);
            string s3 = textB.Substring(0, positionB);

            //if (endOfLine)
            //    positionB++;

            return positionB;
        }

        internal static int OffsetToNextToken(this string text, int pos)
        {
            int offset = 0;
            for (int i = pos; i < text.Length; i++)
            {
                if (IsMeaningfull(text[i]))
                    break;
                offset++;
            }
            return offset;
        }

        static bool IsMeaningfull(char c)
        {
            return (c == '\r' || c == '\n' || !char.IsWhiteSpace(c));
        }

        internal static int PosToSyntaxLength(this string text, int pos)
        {
            var syntaxBuf = new StringBuilder();
            var textBuf = new StringBuilder();

            int syntaxLength = 0;

            for (int i = 0; i < text.Length; i++)
            {
                textBuf.Append(text[i]);
                if (IsMeaningfull(text[i]))
                {
                    syntaxLength++;
                    syntaxBuf.Append(text[i]);
                }

                if (i == pos)
                    break;

            }

            var tempS = syntaxBuf.ToString();
            return syntaxLength;
        }

        internal static int SyntaxLengthToPos(this string text, int syntaxLength)
        {
            var syntaxBuf = new StringBuilder();
            var textBuf = new StringBuilder();

            int absolutePos = 0;
            int currentSyntaxLength = 0;

            for (int i = 0; i < text.Length; i++)
            {
                textBuf.Append(text[i]);

                if (IsMeaningfull(text[i]))
                {
                    currentSyntaxLength++;
                    syntaxBuf.Append(text[i]);
                    if (currentSyntaxLength == syntaxLength)
                        break;
                }
            }

            absolutePos = textBuf.Length - 1;

            var tempA = syntaxBuf.ToString();

            return absolutePos;
        }

        static void Main(string[] args)
        {
            //string file = @"E:\Dev\BackupDir.cs";
            //var code = File.ReadAllText(args.First());

            //int pos = 0;
            //string formattedCode = Format(code, ref pos);

            //Console.WriteLine(formattedCode);
        }
    }
}
