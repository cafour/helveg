using System;
using System.Collections.Immutable;

namespace Helveg.CSharp.Symbols;

public record PackageReference : SymbolReference
{
    public static PackageReference Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Package) };
}

public record PackageDefinition : SymbolDefinition
{
    public static PackageDefinition Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Package) };

    public string? Version { get; init; }

    public ImmutableArray<AssemblyDefinition> Assemblies { get; set; }

    public PackageReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;
}
