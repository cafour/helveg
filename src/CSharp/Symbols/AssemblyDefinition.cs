using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

    public ProjectReference? ContainingProject { get; init; }

    public PackageReference? ContainingPackage { get; init; }

    public AssemblyReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;

    public IEnumerable<TypeDefinition> GetAllTypes()
    {
        return Modules.SelectMany(m => m.GetAllTypes());
    }
}
