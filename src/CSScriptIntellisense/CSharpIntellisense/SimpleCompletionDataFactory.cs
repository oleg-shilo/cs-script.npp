using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public enum DeclarationType : byte
{
    None,
    Namespace,
    Type,
    Variable,
    Parameter,
    Event,
    Unresolved
}

public class TypeInfo
{
    public string FullName = "";
    public string Namespace = "";

    public bool IsNested
    {
        get
        {
            //"System.IO"
            //"System.IO.File"
            return FullName.IndexOf('.', Namespace.Length + 1) != -1;
        }
    }
}

public class CompletionData : ICompletionData
{
    public void AddOverload(ICompletionData data)
    {
        if (overloadedData.Count == 0)
            overloadedData.Add(this);
        overloadedData.Add(data);
    }

    public CompletionCategory CompletionCategory { get; set; }

    public string DisplayText { get; set; }

    public string Description { get; set; }

    public bool IsExtensionMethod { get; set; }

    public DeclarationType DeclarationType { get; set; }

    public string CompletionText { get; set; }

    public DisplayFlags DisplayFlags { get; set; }

    public bool HasOverloads
    {
        get { return overloadedData.Count > 0; }
    }

    List<ICompletionData> overloadedData = new List<ICompletionData>();

    public System.Collections.Generic.IEnumerable<ICompletionData> OverloadedData
    {
        get { return overloadedData; }
        set { throw new NotImplementedException(); }
    }

    public CompletionData(string text)
    {
        DisplayText =
        CompletionText =
        Description = text;
    }
}

public class OverrideCompletionData : CompletionData
{
    public int DeclarationBegin { get; set; }

    public OverrideCompletionData(string text, int declarationBegin)
        : base(text)
    {
        this.DeclarationBegin = declarationBegin;
    }
}

public class EntityCompletionData : CompletionData, IEntityCompletionData
{
    public IEntity Entity { get; private set; }

    public EntityCompletionData(IEntity entity)
        : this(entity, entity.Name)
    {
    }

    public EntityCompletionData(IEntity entity, string txt)
        : base(txt)
    {
        this.Entity = entity;

        var method = entity as DefaultResolvedMethod;

        if (method != null)
            base.IsExtensionMethod = method.IsExtensionMethod;
    }
}

public class ImportCompletionData : CompletionData
{
    public IType Type { get; private set; }

    public bool UseFullName { get; private set; }

    public ImportCompletionData(IType type, bool useFullName)
        : base(type.Name)
    {
        this.Type = type;
        this.UseFullName = useFullName;
    }
}
public class SimpleCompletionDataFactory : ICompletionDataFactory
{
    CSharpResolver state;
    TypeSystemAstBuilder builder;

    public SimpleCompletionDataFactory(CSharpResolver state)
    {
        this.state = state;
        builder = new TypeSystemAstBuilder(state);
    }



    public ICompletionData CreateEntityCompletionData(ICSharpCode.NRefactory.TypeSystem.IEntity entity)
    {
        ////[Method System.Object.Equals(objA:System.Object, objB:System.Object):System.Boolean]
        //[CSharpInvocationResolveResult [SpecializedMethod System.Linq.Enumerable.First[System.String](source:System.Collections.Generic.IEnumerable`1[[System.String]]):System.String]]
        //if (entity.Name.Contains("Length"))
        //    Debug.WriteLine("");
        //var tooltip = entity.ToTooltip();

        return new EntityCompletionData(entity);
    }

    public ICompletionData CreateEntityCompletionData(ICSharpCode.NRefactory.TypeSystem.IEntity entity, string text)
    {
        return new EntityCompletionData(entity, text);
    }

    public ICompletionData CreateEntityCompletionData(ICSharpCode.NRefactory.TypeSystem.IUnresolvedEntity entity)
    {
        return new CompletionData(entity.Name) { DeclarationType = DeclarationType.Unresolved };
    }

    public ICompletionData CreateMemberCompletionData(IType type, IEntity member)
    {
        string name = builder.ConvertType(type).ToString();
        return new EntityCompletionData(member, name + "." + member.Name);
    }

    public ICompletionData CreateLiteralCompletionData(string title, string description, string insertText)
    {
        return new CompletionData(title) { DeclarationType = DeclarationType.Unresolved };
    }

    public ICompletionData CreateNamespaceCompletionData(INamespace ns)
    {
        return new CompletionData(ns.Name) { DeclarationType = DeclarationType.Namespace };
    }

    public ICompletionData CreateVariableCompletionData(ICSharpCode.NRefactory.TypeSystem.IVariable variable)
    {
        return new CompletionData(variable.Name) { DeclarationType = DeclarationType.Variable };
    }

    public ICompletionData CreateVariableCompletionData(ICSharpCode.NRefactory.TypeSystem.ITypeParameter parameter)
    {
        return new CompletionData(parameter.Name) { DeclarationType = DeclarationType.Variable };
    }

    public ICompletionData CreateEventCreationCompletionData(string varName, ICSharpCode.NRefactory.TypeSystem.IType delegateType, ICSharpCode.NRefactory.TypeSystem.IEvent evt, string parameterDefinition, ICSharpCode.NRefactory.TypeSystem.IUnresolvedMember currentMember, ICSharpCode.NRefactory.TypeSystem.IUnresolvedTypeDefinition currentType)
    {
        return new CompletionData(varName) { DeclarationType = DeclarationType.Event };
    }

    public ICompletionData CreateNewOverrideCompletionData(int declarationBegin, ICSharpCode.NRefactory.TypeSystem.IUnresolvedTypeDefinition type, ICSharpCode.NRefactory.TypeSystem.IMember m)
    {
        return new OverrideCompletionData(m.Name, declarationBegin) { DeclarationType = DeclarationType.Unresolved };
    }

    public ICompletionData CreateNewPartialCompletionData(int declarationBegin, IUnresolvedTypeDefinition type, IUnresolvedMember m)
    {
        return new OverrideCompletionData(m.Name, declarationBegin) { DeclarationType = DeclarationType.Unresolved };
    }

    public ICompletionData CreateImportCompletionData(IType type, bool useFullName)
    {
        return new ImportCompletionData(type, useFullName) { DeclarationType = DeclarationType.Unresolved };
    }

    public System.Collections.Generic.IEnumerable<ICompletionData> CreateCodeTemplateCompletionData()
    {
        return Enumerable.Empty<ICompletionData>();
    }

    public IEnumerable<ICompletionData> CreatePreProcessorDefinesCompletionData()
    {
        yield return new CompletionData("DEBUG");
        //yield return new CompletionData("TEST");
    }

    public ICompletionData CreateNamespaceCompletionData(string name)
    {
        return new CompletionData(name) { DeclarationType = DeclarationType.Namespace };
    }

    public ICompletionData CreateTypeCompletionData(IType type, string shortType)
    {
        return new CompletionData(shortType) { DeclarationType = DeclarationType.Type };
    }
}

