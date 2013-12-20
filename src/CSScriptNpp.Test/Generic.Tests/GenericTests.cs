using System;
using System.Linq;
using CSScriptNpp;
using Xunit;

namespace CSScriptIntellisense.Test
{
    public class GenericTests
    {
        [Fact]
        public void ParseAsExceptionFileReference()
        {
            string text = @"   at ScriptClass.main(String[] args) in c:\Users\test\AppData\Local\Temp\CSSCRIPT\Cache\-1529274573\dev.g.csx:line 12";
            string file;
            int line;
            int column;

            bool success = text.ParseAsExceptionFileReference(out file, out line, out column);

            Assert.True(success);
            Assert.Equal(@"c:\Users\test\AppData\Local\Temp\CSSCRIPT\Cache\-1529274573\dev.g.csx", file);
            Assert.Equal(12, line);
            Assert.Equal(1, column);
        }
    }
}
