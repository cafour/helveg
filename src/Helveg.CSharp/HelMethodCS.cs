using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record HelMethodCS : HelSymbolBaseCS
{
    public const string ConstructorName = ".ctor";
    public const string StaticConstructorName = ".cctor";

    public static readonly HelMethodCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.Method;

    [JsonIgnore]
    public bool IsConstructor => Name == ConstructorName;

    [JsonIgnore]
    public bool IsStaticConstructor => Name == StaticConstructorName;
}
