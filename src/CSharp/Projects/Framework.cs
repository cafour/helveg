using Helveg.CSharp.Packages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record Framework : EntityBase
{
    public static Framework Invalid { get; } = new Framework
    {
        Index = NumericToken.InvalidValue
    };

    public string Name { get; init; } = Const.Invalid;

    public string? Version { get; init; }

    [JsonIgnore]
    public int Index { get; init; }

    [JsonIgnore]
    public NumericToken Token => NumericToken.Create(CSConst.CSharpNamespace, (int)RootKind.Framework, Index);

    public override string Id
    {
        get => Token;
        init => Index = NumericToken.Parse(value, CSConst.CSharpNamespace, 3, (int)RootKind.Framework).Values[^1];
    }

    public ImmutableArray<AssemblyDependency> Assemblies { get; init; }
        = ImmutableArray<AssemblyDependency>.Empty;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IProjectVisitor projectVisitor)
        {
            projectVisitor.VisitFramework(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        foreach (var assembly in Assemblies)
        {
            assembly.Accept(visitor);
        }

        base.Accept(visitor);
    }
}
