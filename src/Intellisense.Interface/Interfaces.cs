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
        void ResetProject(Tuple<string, string>[] sourceFiles = null, params string[] assemblies);
    }

    public interface ICompletionData
    {
        string OperationContext { get; set; }
        bool InvokeParametersSet { get; set; }
        CompletionCategory CompletionCategory { get; set; }
        string CompletionText { get; set; }
        string Description { get; set; }
        DisplayFlags DisplayFlags { get; set; }
        string DisplayText { get; set; }
        CompletionType CompletionType { get; }
        IEnumerable<string> InvokeParameters { get; }
        bool HasOverloads { get; }

        IEnumerable<ICompletionData> OverloadedData { get; }

        void AddOverload(ICompletionData data);
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
        _namespace,
        unresolved
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

    public struct DomRegion
    {
        public static readonly DomRegion Empty = new DomRegion { IsEmpty = true };

        public int BeginColumn { get; set; }
        public int BeginLine { get; set; }
        public int EndLine { get; set; }
        public string FileName { get; set; }
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
        public bool InvokeParametersSet { get; set; }
        public CompletionCategory CompletionCategory { get; set; }
        public string CompletionText { get; set; }
        public string Description { get; set; }
        public DisplayFlags DisplayFlags { get; set; }
        public string DisplayText { get; set; }
        public CompletionType CompletionType { get; set; }
        public bool HasOverloads { get; }
        public IEnumerable<ICompletionData> OverloadedData { get; }
        public IEnumerable<string> InvokeParameters { get; set; }

        public void AddOverload(ICompletionData data)
        {
            (OverloadedData as List<ICompletionData>).Add(data);
        }

        public object RawData { get; set; }
    }
}