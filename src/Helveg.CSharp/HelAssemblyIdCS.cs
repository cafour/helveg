using System;
using System.Globalization;

namespace Helveg.CSharp;

public record HelAssemblyIdCS(
    string Name,
    Version Version,
    string CultureName,
    string? PublicKeyToken)
{
    public static readonly HelAssemblyIdCS Invalid = new(
        Name: IHelEntityCS.InvalidName,
        Version: new(),
        CultureName: CultureInfo.InvariantCulture.Name,
        PublicKeyToken: null
    );
    
    public bool IsInvalid => Name == IHelEntityCS.InvalidName;
}
