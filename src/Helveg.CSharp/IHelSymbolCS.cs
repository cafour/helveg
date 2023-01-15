using System.Text.Json.Serialization;

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

    [JsonIgnore]
    HelNamespaceCS ContainingNamespace { get; }

    [JsonIgnore]
    HelTypeCS? ContainingType { get; }
}
