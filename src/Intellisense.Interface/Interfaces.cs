using System;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Intellisense.Common
{
    public interface IEngine
    {
        void Preload();

        IEnumerable<Common.ICompletionData> GetCompletionData(string editorText, int offset, string fileName, bool isControlSpace = true);

        string[] FindReferences(string editorText, int offset, string fileName);

        string[] GetMemberInfo(string editorText, int offset, string fileName, bool collapseOverloads, out int methodStartPos);

        IEnumerable<TypeInfo> GetPossibleNamespaces(string editorText, string nameToResolve, string fileName);

        DomRegion ResolveCSharpMember(string editorText, int offset, string fileName);

        CodeMapItem[] GetMapOf(string code, bool decorated, string codeFile);

        void ResetProject(Tuple<string, string>[] sourceFiles = null, params string[] assemblies);

        void SetOption(string name, object value);

        /// <summary>
        /// "C#" and "VB"
        /// </summary>
        string Language { get; set; }
    }

    public class SyntaxErrorParsingException : ApplicationException
    {
        public SyntaxErrorParsingException(string message)
            : base(message)
        {
        }
    }

    public interface ICompletionData
    {
        string OperationContext { get; set; }
        CompletionCategory CompletionCategory { get; set; }
        string CompletionText { get; set; }
        string Description { get; set; }
        DisplayFlags DisplayFlags { get; set; }
        string DisplayText { get; set; }
        CompletionType CompletionType { get; }
        bool InvokeParametersSet { get; set; }
        IEnumerable<string> InvokeParameters { get; }
        string InvokeReturn { get; }
        bool HasOverloads { get; }
        object Tag { get; set; }

        IEnumerable<ICompletionData> OverloadedData { get; }

        void AddOverload(ICompletionData data);
    }

    public class CodeMapItem
    {
        public int Column;
        public int Line;
        public string DisplayName;
        public string ParentDisplayName;
        public string ParentDisplayType;
        public string MemberType;

        public override string ToString()
        {
            return ParentDisplayName + "." + DisplayName;
        }
    }

    public class TypeInfo
    {
        public string FullName = "";
        public string Namespace = "";

        public bool IsNested
        {
            //"System.IO" or "System.IO.File"
            get { return FullName.IndexOf('.', Namespace.Length + 1) != -1; }
        }
    }

    public enum DisplayFlags
    {
        None = 0,
        Hidden = 1,
        Obsolete = 2,
        DescriptionHasMarkup = 4
    }

    public abstract class CompletionCategory
    {
        public string DisplayText { get; set; }
        public string Icon { get; set; }
    }

    public interface IEntityCompletionData : ICompletionData
    {
        IEntity Entity { get; }
    }

    public enum EntityType : byte
    {
        None = 0,
        TypeDefinition = 1,
        Field = 2,
        Property = 3,
        Indexer = 4,
        Event = 5,
        Method = 6,
        Operator = 7,
        Constructor = 8,
        Destructor = 9,
        Accessor = 10
    }

    public enum CompletionType : byte
    {
        none,
        snippet,
        constructor,
        extension_method,
        method,
        _event,
        field,
        property,
        type,
        _namespace,
        unresolved,
        directive,
        file
    }

    public interface IEntity
    {
        //IType DeclaringType { get; }
        //ITypeDefinition DeclaringTypeDefinition { get; }
        //DocumentationComment Documentation { get; }
        EntityType EntityType { get; }

        bool IsStatic { get; }
    }

    public class EntityCompletionData : CompletionData, IEntityCompletionData
    {
        public IEntity Entity { get; set; }
    }

    public static class ReflectionExtensions
    {
        public static T CopyPropertiesFrom<T>(this T dest, T src)
        {
            var dummy = new object[0];

            foreach (PropertyInfo info in src.GetType().GetProperties())
                if (info.CanRead && info.CanWrite)
                    info.SetValue(dest, info.GetValue(src, dummy), dummy);

            return dest;
        }
    }

    public struct DomRegion
    {
        public static readonly DomRegion Empty = new DomRegion { IsEmpty = true };

        public int BeginColumn { get; set; }
        public int BeginLine { get; set; }
        public int EndLine { get; set; }
        public string FileName { get; set; }
        public string Hint { get; set; }
        public bool IsEmpty { get; set; }
    }

    public class CompletionData : ICompletionData
    {
        public CompletionData()
        {
            OverloadedData = new List<ICompletionData>();
            InvokeParameters = new List<string>();
        }

        public string OperationContext { get; set; }
        public CompletionCategory CompletionCategory { get; set; }
        public string CompletionText { get; set; }
        public string Description { get; set; }
        public DisplayFlags DisplayFlags { get; set; }
        public string DisplayText { get; set; }
        public CompletionType CompletionType { get; set; }

        public bool HasOverloads
        {
            get
            {
                return OverloadedData.Count() > 1;
            }
        }

        public IEnumerable<ICompletionData> OverloadedData { get; }
        public bool InvokeParametersSet { get; set; }
        public IEnumerable<string> InvokeParameters { get; set; }
        public string InvokeReturn { get; set; }
        public object Tag { get; set; }

        public void AddOverload(ICompletionData data)
        {
            (OverloadedData as List<ICompletionData>).Add(data);
        }

        public object RawData { get; set; }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}