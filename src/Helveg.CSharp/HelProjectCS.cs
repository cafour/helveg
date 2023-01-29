using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelProjectCS : IHelEntityCS
{
    public static readonly HelProjectCS Invalid = new();

    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public ImmutableArray<HelProjectCS> ProjectDependencies { get; init; } = ImmutableArray.Create<HelProjectCS>();

    public ImmutableArray<HelPackageCS> PackageDependencies { get; init; } = ImmutableArray.Create<HelPackageCS>();

    public HelAssemblyCS Assembly { get; init; } = HelAssemblyCS.Invalid;
}
