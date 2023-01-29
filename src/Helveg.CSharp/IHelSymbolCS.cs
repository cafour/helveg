namespace Helveg.CSharp;

public interface IHelSymbolCS : IHelEntityCS
{
    HelSymbolKindCS Kind { get; }

    HelAccessibilityCS Accessibility { get; }

    bool IsSealed { get; }

    bool IsStatic { get; }

    bool IsAbstract { get; }

    bool IsExtern { get; }

    bool IsOverride { get; }

    bool IsVirtual { get; }

    bool IsImplicitlyDeclared { get; }

    bool CanBeReferencedByName { get; }

    IHelSymbolCS? ContainingSymbol { get; }

    HelAssemblyCS? ContainingAssembly { get; }

    HelModuleCS? ContainingModule { get; }

    HelTypeCS? ContainingType { get; }

    HelNamespaceCS? ContainingNamespace { get; }
}
