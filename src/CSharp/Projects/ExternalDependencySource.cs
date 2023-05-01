using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record ExternalDependencySource : EntityBase, ILibrarySource
{
    public string Name { get; init; } = Const.Invalid;

    [JsonIgnore]
    public int Index { get; init; }

    [JsonIgnore]
    public NumericToken Token
        => NumericToken.Create(CSConst.CSharpNamespace, (int)RootKind.ExternalDependencySource, Index);

    public ImmutableArray<Library> Libraries { get; init; }
        = ImmutableArray<Library>.Empty;

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

        foreach (var library in Libraries)
        {
            library.Accept(visitor);
        }

        base.Accept(visitor);
    }

    public ILibrarySource WithLibraries(ImmutableArray<Library> libraries)
    {
        return this with { Libraries = libraries };
    }
}
