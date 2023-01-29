using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelSolutionCS : IHelEntityCS
{
    public static readonly HelSolutionCS Invalid = new();
    
    public string Name { get; init; } = IHelEntityCS.InvalidName;
    
    public ImmutableArray<HelProjectCS> Projects { get; init; } = ImmutableArray<HelProjectCS>.Empty;
}
