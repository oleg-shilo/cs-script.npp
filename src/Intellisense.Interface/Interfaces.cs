using System.Collections.Generic;

namespace Intellisense.Common
{
    public interface IEngine
    {
        IEnumerable<ICompletionData> GetAutocompletionFor(string code, int position, string[] references, string[] includes);
    }

    public interface ICompletionData
    {
        CompletionCategory CompletionCategory { get; set; }
        string CompletionText { get; set; }
        string Description { get; set; }
        DisplayFlags DisplayFlags { get; set; }
        string DisplayText { get; set; }
        bool HasOverloads { get; }

        IconType Icon {get;}

        IEnumerable<ICompletionData> OverloadedData { get; }

        void AddOverload(ICompletionData data);
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

    public enum IconType : byte
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
        public CompletionCategory CompletionCategory { get; set; }
        public string CompletionText { get; set; }
        public string Description { get; set; }
        public DisplayFlags DisplayFlags { get; set; }
        public string DisplayText { get; set; }
        public IconType Icon { get; set; }
        public bool HasOverloads { get; }
        public IEnumerable<ICompletionData> OverloadedData { get; }

        public void AddOverload(ICompletionData data)
        {
        }

        public object RawData { get; set; }
    }
}