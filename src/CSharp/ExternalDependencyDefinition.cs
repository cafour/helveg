using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record ExternalDependencyReference : EntityReference, IInvalidable<ExternalDependencyReference>
{
    public static ExternalDependencyReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.ExternalDependency) };
}

public record ExternalDependencyDefinition : EntityDefinition, IInvalidable<ExternalDependencyDefinition>
{
    public static ExternalDependencyDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.ExternalDependency) };

    public AssemblyDefinition Assembly { get; init; } = AssemblyDefinition.Invalid;

    public SolutionReference ContainingSolution { get; init; } = SolutionReference.Invalid;

    public override ExternalDependencyReference GetReference()
    {
        return new() { Token = Token, Hint = Name };
    }
}
