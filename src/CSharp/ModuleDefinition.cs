using System.Collections.Generic;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record ModuleReference : EntityReference, IInvalidable<ModuleReference>
{
    public static ModuleReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Module) };
}

public record ModuleDefinition : EntityDefinition, IInvalidable<ModuleDefinition>
{
    public static ModuleDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Module) };

    public NamespaceDefinition GlobalNamespace { get; init; } = NamespaceDefinition.Invalid;

    public ImmutableArray<AssemblyReference> ReferencedAssemblies { get; init; }
        = ImmutableArray<AssemblyReference>.Empty;

    public AssemblyReference ContainingAssembly { get; init; } = AssemblyReference.Invalid;

    public override ModuleReference GetReference()
    {
        return new() { Token = Token, Hint = Name };
    }

    public IEnumerable<TypeDefinition> GetAllTypes()
    {
        return GlobalNamespace.GetAllTypes();
    }
}
