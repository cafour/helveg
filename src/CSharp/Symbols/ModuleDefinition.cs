using System.Collections.Generic;
using System.Collections.Immutable;

namespace Helveg.CSharp.Symbols;

public record ModuleReference : SymbolReference
{
    public static ModuleReference Invalid { get; } = new();
}

public record ModuleDefinition : SymbolDefinition
{
    public static ModuleDefinition Invalid { get; } = new();

    public NamespaceDefinition GlobalNamespace { get; init; } = NamespaceDefinition.Invalid;

    public AssemblyReference ContainingAssembly { get; init; } = AssemblyReference.Invalid;

    public ImmutableArray<AssemblyReference> ReferencedAssemblies { get; init; }
        = ImmutableArray<AssemblyReference>.Empty;

    public ModuleReference Reference => new() { Token = Token, Hint = Name };

    public override ISymbolReference GetReference() => Reference;

    public IEnumerable<TypeDefinition> GetAllTypes()
    {
        return GlobalNamespace.GetAllTypes();
    }

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.VisitModule(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        GlobalNamespace.Accept(visitor);

        base.Accept(visitor);
    }
}
