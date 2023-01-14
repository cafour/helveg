using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record HelMethodCS : HelSymbolBaseCS
{
    public const string ConstructorName = ".ctor";
    public const string StaticConstructorName = ".cctor";

    public static readonly HelMethodCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.Method;

    public ImmutableArray<HelParameterCS> Parameters { get; init; } = ImmutableArray.Create<HelParameterCS>();

    public ImmutableArray<HelTypeParameterCS> TypeParameters { get; init; } = ImmutableArray<HelTypeParameterCS>.Empty;

    public HelTypeCS? ReturnType { get; init; }

    public bool IsExtensionMethod { get; init; }

    [JsonIgnore]
    public bool IsConstructor => Name == ConstructorName;

    [JsonIgnore]
    public bool IsStaticConstructor => Name == StaticConstructorName;
}
