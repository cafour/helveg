namespace Helveg.CSharp;

public record HelSymbolBaseCS : IHelSymbolCS
{
    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public virtual HelSymbolKindCS Kind { get; init; }

    public HelAccessibilityCS Accessibility { get; init; }

    public bool IsSealed { get; init; }

    public bool IsStatic { get; init; }

    public bool IsAbstract { get; init; }

    public bool IsExtern { get; init; }

    public bool IsOverride { get; init; }

    public bool IsVirtual { get; init; }

    public HelNamespaceCS ContainingNamespace { get; init; } = HelNamespaceCS.Invalid;

    public HelTypeCS? ContainingType { get; init; }
}
