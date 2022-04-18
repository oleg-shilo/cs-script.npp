using System.Collections.Generic;
using Intellisense.Common;

namespace CSScriptIntellisense
{
    public class RoslynCompletionEngine_old
    {
        public delegate void D2(string s);

        static public GetAutocompletionForDlgt GetAutocompletionFor;

        public delegate IEnumerable<ICompletionData> GetAutocompletionForDlgt(string code, int position, string[] references, string[] includes);
    }
}