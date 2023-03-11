using Helveg.Abstractions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Helveg.CSharp;

public record AssemblyReference : EntityReference, IInvalidable<AssemblyReference>
{
    public static AssemblyReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Assembly) };
}

public record AssemblyDefinition : EntityDefinition, IInvalidable<AssemblyDefinition>
{
    public static AssemblyDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Assembly) };

    public AssemblyId Identity { get; init; } = AssemblyId.Invalid;

    public ImmutableArray<ModuleDefinition> Modules { get; init; } = ImmutableArray<ModuleDefinition>.Empty;

    public ProjectReference? ContainingProject { get; init; }

    public PackageReference? ContainingPackage { get; init; }

    public override AssemblyReference GetReference()
    {
        return new() { Token = Token, Hint = Name };
    }

    public IEnumerable<TypeDefinition> GetAllTypes()
    {
        return Modules.SelectMany(m => m.GetAllTypes());
    }
}
