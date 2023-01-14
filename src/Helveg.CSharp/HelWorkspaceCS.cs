using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelWorkspaceCS
{
    public ImmutableArray<HelProjectCS> Projects { get; init; } = ImmutableArray.Create<HelProjectCS>();
}
