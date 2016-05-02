using CSScriptIntellisense;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xunit;

namespace Tests
{
    public class SnippetsSupport
    {
        [Fact]
        public void Parsing_1()
        {
            var template = "public $int$ $MyPorp$ { get; set; }$|$";
            var expected = "public int MyPorp { get; set; }|";

            var snippetParametersRegions = new List<Point>();
            var result = Snippets.PrepareForIncertion(template, 0);

            Assert.Equal(expected, result.ReplacementString);
        }

        [Fact]
        public void Parsing_2()
        {
            var template =
@"for (int $i$ = 0; $i$ < $length$; $i$++)
{
    $|$
}";
            var expected =
@"for (int i = 0; i < length; i++)
{
    |
}";
            var result = CSScriptIntellisense.Snippets.PrepareForIncertion(template, 0);

            Assert.Equal(expected, result.ReplacementString);
        }

        [Fact]
        public void Parsing_3()
        {
            var template =
@"for (int $i$ = 0; $i$ < $length$; $i$++)
{
    $|$
}";
            var expected =
@"for (int i = 0; i < length; i++)
    {
        |
    }";

            var result = CSScriptIntellisense.Snippets.PrepareForIncertion(template, 4);

            Assert.Equal(expected, result.ReplacementString);
        }
    }
}
