using CSScriptIntellisense;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xunit;

namespace Tests
{
   

    public class StringManipulation
    {
        [Fact]
        public void LeftAlign()
        {
            string[] lines = new[] {"Line1",
                                     "       Line2",
                                     "Line3", 
                                     "Line4",
                                     "           Line5"};

            string[] formatedLines = lines.LeftAlign();

            Assert.Equal("Line1", formatedLines[0]);
            Assert.Equal("Line2", formatedLines[1]);
            Assert.Equal("Line3", formatedLines[2]);
            Assert.Equal("Line4", formatedLines[3]);
            Assert.Equal("Line5", formatedLines[4]);
        }

        [Fact]
        public void StringLines()
        {
            var text = "Line1\nLine2\nLine3\r\nLine4\r\nLine5";
            string[] lines = text.NormalizeLines().GetLines();

            Assert.Equal(5, lines.Count());
        }

        [Fact]
        public void NormaliseClrName()
        {
            //Assert.Equal("System.Linq.EnumerableQuery", "System.Linq.EnumerableQuery".NormaliseClrName());
            //Assert.Equal("System.Linq.EnumerableQuery", "System.Linq.EnumerableQuery".NormaliseClrName("System"));
            //Assert.Equal("Activator", "System.Activator".NormaliseClrName("System"));
            //Assert.Equal("string", "System.String".NormaliseClrName("System"));
            //Assert.Equal("string", "System.String".NormaliseClrName());
            //Assert.Equal("TestClass", "CSScriptIntellisense.Test.TestClass".NormaliseClrName("System", "CSScriptIntellisense.Test"));
            //Assert.Equal("TestClass", "CSScriptIntellisense.Test.TestClass".NormaliseClrName("System", "CSScriptIntellisense.Test"));
            //Assert.Equal("CSScriptIntellisense.Test.TestClass", "CSScriptIntellisense.Test.TestClass".NormaliseClrName());
        }
    }
}
