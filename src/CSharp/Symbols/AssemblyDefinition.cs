using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Helveg.CSharp.Packages;

namespace Helveg.CSharp.Symbols;

public record AssemblyReference : SymbolReference
{
    public static AssemblyReference Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Assembly) };
}

public record AssemblyDefinition : SymbolDefinition
{
    public static AssemblyDefinition Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Assembly) };

    public AssemblyId Identity { get; init; } = AssemblyId.Invalid;

    public ImmutableArray<ModuleDefinition> Modules { get; init; } = ImmutableArray<ModuleDefinition>.Empty;

    public string? ContainingProject { get; init; }

    public string? ContainingPackage { get; init; }

    public AssemblyReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;

    public IEnumerable<TypeDefinition> GetAllTypes()
    {
        return Modules.SelectMany(m => m.GetAllTypes());
    }

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.VisitAssembly(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        foreach (var module in Modules)
        {
            module.Accept(visitor);
        }

        base.Accept(visitor);
    }
}
