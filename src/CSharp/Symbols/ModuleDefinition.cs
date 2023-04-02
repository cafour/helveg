using System.Collections.Generic;
using System.Collections.Immutable;

namespace Helveg.CSharp.Symbols;

public record ModuleReference : SymbolReference
{
    public static ModuleReference Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Module) };
}

public record ModuleDefinition : SymbolDefinition
{
    public static ModuleDefinition Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Module) };

    public NamespaceDefinition GlobalNamespace { get; init; } = NamespaceDefinition.Invalid;

    public ImmutableArray<AssemblyReference> ReferencedAssemblies { get; init; }
        = ImmutableArray<AssemblyReference>.Empty;

    public AssemblyReference ContainingAssembly { get; init; } = AssemblyReference.Invalid;

    public ModuleReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;

    public IEnumerable<TypeDefinition> GetAllTypes()
    {
        return GlobalNamespace.GetAllTypes();
    }
}
