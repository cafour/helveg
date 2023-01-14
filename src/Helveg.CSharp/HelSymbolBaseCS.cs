namespace Helveg.CSharp;

public record HelSymbolBaseCS : IHelSymbolCS
{
    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public virtual HelSymbolKindCS Kind { get; init; }

    public HelVisibilityCS Visibility { get; init; }

    public bool IsStatic { get; init; }

    public HelNamespaceCS ContainingNamespace { get; init; } = HelNamespaceCS.Invalid;

    public HelTypeCS? ContainingType { get; init; }
}
