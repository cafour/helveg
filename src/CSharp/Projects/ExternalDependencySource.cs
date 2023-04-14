using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record ExternalDependencySource : EntityBase, IDependencySource
{
    public string Name { get; init; } = Const.Invalid;

    [JsonIgnore]
    public int Index { get; init; }

    [JsonIgnore]
    public NumericToken Token
        => NumericToken.Create(CSConst.CSharpNamespace, (int)RootKind.ExternalDependencySource, Index);

    public ImmutableArray<AssemblyDependency> Assemblies { get; init; }
        = ImmutableArray<AssemblyDependency>.Empty;

    public override string Id
    {
        get => Token;
        init => Index = NumericToken.Parse(value, CSConst.CSharpNamespace, 3, (int)RootKind.ExternalDependencySource).Values[^1];
    }

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IProjectVisitor projectVisitor)
        {
            projectVisitor.VisitExternalDependencySource(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        foreach (var dependency in Assemblies)
        {
            dependency.Accept(visitor);
        }

        base.Accept(visitor);
    }

    public IDependencySource WithAssemblies(ImmutableArray<AssemblyDependency> assemblies)
    {
        return this with { Assemblies = assemblies };
    }
}
