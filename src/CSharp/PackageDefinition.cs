using System;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record PackageReference : EntityReference
{
    public static PackageReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Package) };
}

public record PackageDefinition : EntityDefinition
{
    public static PackageDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Package) };

    public string? Version { get; init; }

    public ImmutableArray<AssemblyDefinition> Assemblies { get; set; }

    public override PackageReference GetReference()
    {
        return new() { Token = Token, Hint = Name };
    }
}
