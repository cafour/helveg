using System.Globalization;

namespace Helveg.CSharp;

public record HelAssemblyIdCS(
    string Name,
    Version Version,
    string CultureName,
    string PublicKey)
{
    public static readonly HelAssemblyIdCS Invalid = new(
        Name: IHelEntityCS.InvalidName,
        Version: new(),
        CultureName: CultureInfo.InvariantCulture.Name,
        PublicKey: string.Empty
    );
    
    public bool IsInvalid => Name == IHelEntityCS.InvalidName;
}
