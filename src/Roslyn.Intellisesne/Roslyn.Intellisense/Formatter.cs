using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;
using msFormatter = Microsoft.CodeAnalysis.Formatting.Formatter;

namespace RoslynIntellisense
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

        public static string Format(string code, string file)
        {
            return Format(code, false, file);
        }

        public static string FormatHybrid(string code, string file)
        {
            return Format(code, true, file);
        }

        static string Format(string code, bool normaliseLines, string file)
        {
            var result = "";
            bool isVB = file.EndsWith(".vb", StringComparison.OrdinalIgnoreCase);

            SyntaxTree tree;
            if (isVB)
                tree = VB.VisualBasicSyntaxTree.ParseText(code.Trim());
            else
                tree = CSharpSyntaxTree.ParseText(code.Trim());

            SyntaxNode root = msFormatter.Format(tree.GetRoot(), DummyWorkspace);

            if (normaliseLines && !isVB) //VB doesn't do formatting nicely so limit it to default Roslyn handling
            {
                //injecting line-breaks to separate declarations
                if (!isVB)
                {
                    var declarations = root.DescendantNodes()
                                           .Where(n => n.IsKind(SyntaxKind.MethodDeclaration) ||
                                                       n.IsKind(SyntaxKind.ClassDeclaration) ||
                                                       n.IsKind(SyntaxKind.LocalDeclarationStatement) ||
                                                       n.IsNewBlockStatement() ||
                                                       n.IsNewDeclarationBlock())
                                           .ToList();

                    root = root.ReplaceNodes(declarations,
                                             (_, node) =>
                                             {
                                                 var isPrevLocalDeclaration = declarations.Prev(node).IsKind(SyntaxKind.LocalDeclarationStatement);
                                                 var isLocalDeclaration = node.IsKind(SyntaxKind.LocalDeclarationStatement);

                                                 var supressEmptyLineInsertion = isPrevLocalDeclaration && isLocalDeclaration;

                                                 var existingTrivia = node.GetLeadingTrivia().ToFullString();
                                                 if (existingTrivia.Contains(Environment.NewLine) || supressEmptyLineInsertion)
                                                     return node;
                                                 else
                                                     return node.WithLeadingTrivia(SyntaxFactory.Whitespace(Environment.NewLine + existingTrivia));
                                             });
                }
                else
                {
                    root = root.ReplaceNodes(root.DescendantNodes()
                                                 .Where(n => n.IsKind(VB.SyntaxKind.DeclareSubStatement /*MethodDeclaration*/) ||
                                                             n.IsKind(VB.SyntaxKind.EndClassStatement /*ClassDeclaration*/) ||
                                                             n.IsNewBlockStatement() ||
                                                             n.IsNewDeclarationBlock(isVB)),
                                             (_, node) =>
                                             {
                                                 var existingTrivia = node.GetLeadingTrivia().ToFullString();
                                                 if (existingTrivia.Contains(Environment.NewLine))
                                                     return node;
                                                 else
                                                     return node.WithLeadingTrivia(VB.SyntaxFactory.Whitespace(Environment.NewLine + existingTrivia));
                                             });
                }
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
                             .NormalizeLine(root, isVB);
            }
            else
                result = root.ToFullString();

            return result;
        }


        static SyntaxNode NodeAbove(this SyntaxNode node)
        {
            if (node.Parent != null)
            {
                SyntaxNode result = null;
                foreach (var item in node.Parent.ChildNodes())
                    if (item == node)
                        return result;
                    else
                        result = item;
            }
            return null;
        }

        static public string RemoveDoubleLineBreaks(this string formattedCode)
        {
            Func<string, string> clear = text => text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            string current = formattedCode;
            string next;

            while (current != (next = clear(current)))
                current = next;

            return next;
        }

        static string NormalizeLine(this string formattedCode, SyntaxNode root, bool isVB)
        {
            var strings = root.DescendantNodes()
                              .Where(n => n.IsKind(VB.SyntaxKind.StringLiteralExpression) || n.IsKind(SyntaxKind.StringLiteralExpression))
                              .Select(x => new { Start = x.FullSpan.Start, End = x.FullSpan.End })
                              .ToArray();

            var sb = new StringBuilder(formattedCode);

            for (int pos = formattedCode.Length; pos > 0;)
            {
                pos = formattedCode.LastIndexOf("\r\n", pos);
                if (pos != -1)
                {
                    if (strings.Any(x => x.Start <= pos && pos <= x.End))
                        continue;

                    string prevLine = sb.GetPrevLineFrom(pos);
                    string currLine = sb.GetLineFrom(pos);

                    if (currLine.EndsWith("}") && prevLine == "")
                    {
                        //remove extra line before 'end-of-block'
                        //     var t = "";
                        // -> remove
                        //}
                        int lineStartPos = sb.GetLineOffset(pos);
                        sb.Remove(lineStartPos, 2);
                        pos = lineStartPos;
                    }
                    else if (prevLine.EndsWith("{") && currLine == "")
                    {
                        //remove extra line after 'start-of-block'
                        //{
                        // -> remove
                        //    var t = "";
                        sb.Remove(pos, 2);
                    }
                    else
                    {
                        int doubleLineBreak = formattedCode.LastIndexOf("\r\n\r\n", pos);
                        if (doubleLineBreak != -1 && (pos - doubleLineBreak) == 4)
                        {
                            //remove double line-break
                            sb.Remove(pos, 2);
                        }
                    }
                }
            }

            var result = sb.ToString();
            return result;
        }

        static int GetLineOffset(this StringBuilder sb, int pos)
        {
            int startPos = pos;

            bool atLineEnd = sb[pos] == '\r';
            if (atLineEnd)
                startPos = pos - 1;

            for (int i = startPos; i >= 0; i--)
                if (sb[i] == '\r')
                    return i;
            return 0;
        }

        static string GetLineFrom(this StringBuilder sb, int pos)
        {
            var chars = new List<char>();
            bool atLineEnd = sb[pos] == '\r';
            int startPos = pos;

            if (atLineEnd)
                startPos = pos - 1;

            for (int i = startPos; i >= 0; i--)
            {
                if (sb[i] == '\n') // || sb[i-1] == '\r')
                    break;
                chars.Insert(0, sb[i]);
            }

            for (int i = startPos + 1; !atLineEnd && i < sb.Length; i++)
            {
                if (sb[i] == '\r') // || sb[i] == '\n')
                    break;
                chars.Add(sb[i]);
            }
            return new string(chars.ToArray()).Trim();
        }

        static string GetNextLineFrom(this StringBuilder sb, int pos)
        {
            var chars = new List<char>();
            bool atLineEnd = sb[pos] == '\r';
            int startPos = pos;

            if (atLineEnd)
                startPos = pos + 2; //advance forward

            for (int i = startPos; i < sb.Length; i++)
            {
                if (sb[i] == '\n') // || sb[i-1] == '\r')
                    break;
                chars.Insert(0, sb[i]);
            }
            return new string(chars.ToArray()).Trim();
        }

        static string GetPrevLineFrom(this StringBuilder sb, int pos)
        {
            var chars = new List<char>();
            bool atLineEnd = sb[pos] == '\r';
            int startPos = pos;

            if (atLineEnd)
                startPos--;

            for (int i = startPos; i >= 0; i--)
            {
                if (sb[i] == '\n') // || sb[i-1] == '\r')
                {
                    startPos = i - 1;
                    break;
                }
            }

            for (int i = startPos; i >= 0; i--)
            {
                if (sb[i] == '\n') // || sb[i-1] == '\r')
                    break;
                chars.Insert(0, sb[i]);
            }

            for (int i = startPos + 1; !atLineEnd && i < sb.Length; i++)
            {
                if (sb[i] == '\r') // || sb[i] == '\n')
                    break;
                chars.Add(sb[i]);
            }
            return new string(chars.ToArray()).Trim();
        }

        public static bool DoesntStartsWithAny(this string text, params string[] patterns)
        {
            foreach (string item in patterns)
                if (text.StartsWith(item))
                    return false;
            return true;
        }

        public static bool IsNewBlockStatement(this SyntaxNode node, bool isVB = false)
        {
            var prevNode = node.NodeAbove();
            if (isVB)
                return node.IsVbBlockStatement() && prevNode != null && prevNode.IsKind(VB.SyntaxKind.LocalDeclarationStatement);
            else
                return node.IsBlockStatement() && prevNode != null && prevNode.IsKind(SyntaxKind.LocalDeclarationStatement);
        }

        public static bool IsNewDeclarationBlock(this SyntaxNode node, bool isVB = false)
        {
            var prevNode = node.NodeAbove();
            if (isVB)
                return node.IsKind(VB.SyntaxKind.LocalDeclarationStatement) && prevNode != null && prevNode.IsKind(VB.SyntaxKind.LocalDeclarationStatement);
            else
                return node.IsKind(SyntaxKind.LocalDeclarationStatement) && prevNode != null && prevNode.IsKind(SyntaxKind.LocalDeclarationStatement);
        }

        public static bool IsBlockStatement(this SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.WhileStatement:
                case SyntaxKind.DoStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.ForEachStatement:
                case SyntaxKind.UsingStatement:
                case SyntaxKind.CheckedStatement:
                case SyntaxKind.UncheckedStatement:
                case SyntaxKind.UnsafeStatement:
                case SyntaxKind.LockStatement:
                case SyntaxKind.IfStatement:
                case SyntaxKind.SwitchStatement:
                case SyntaxKind.TryStatement:
                case SyntaxKind.Block:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsVbBlockStatement(this SyntaxNode node)
        {
            switch (VB.VisualBasicExtensions.Kind(node))
            {
                case VB.SyntaxKind.WhileStatement:
                case VB.SyntaxKind.DoUntilStatement:
                case VB.SyntaxKind.ForStatement:
                case VB.SyntaxKind.ForEachStatement:
                case VB.SyntaxKind.UsingStatement:
                case VB.SyntaxKind.SyncLockKeyword:
                case VB.SyntaxKind.IfStatement:
                case VB.SyntaxKind.TryStatement:
                    return true;
                default:
                    return false;
            }
        }
    }
}