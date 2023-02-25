using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record ModuleReference : EntityReference, IInvalidable<ModuleReference>
{
    public static ModuleReference Invalid { get; } = new();
}

public record ModuleDefinition : EntityDefinition, IInvalidable<ModuleDefinition>
{
    public static ModuleDefinition Invalid { get; } = new();

    public NamespaceDefinition GlobalNamespace { get; init; } = NamespaceDefinition.Invalid;

    public ImmutableArray<AssemblyReference> ReferencedAssemblies { get; init; }
        = ImmutableArray<AssemblyReference>.Empty;

    public AssemblyReference ContainingAssembly { get; init; } = AssemblyReference.Invalid;

    public override ModuleReference GetReference()
    {
        return new() { Token = Token };
    }
}
