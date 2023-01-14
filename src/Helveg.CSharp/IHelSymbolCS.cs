using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public interface IHelSymbolCS : IHelEntityCS
{
    HelSymbolKindCS Kind { get; }

    HelVisibilityCS Visibility { get; }

    bool IsStatic { get; }

    [JsonIgnore]
    HelNamespaceCS ContainingNamespace { get; }

    [JsonIgnore]
    HelTypeCS? ContainingType { get; }
}
