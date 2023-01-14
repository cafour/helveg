namespace Helveg.CSharp;

public record HelPackageCS : IHelEntityCS
{
    public static readonly HelPackageCS Invalid = new();
    
    public string Name { get; init; } = IHelEntityCS.InvalidName;
}
