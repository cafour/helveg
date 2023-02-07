using System;
using System.Globalization;

namespace Helveg.CSharp;

public record HelAssemblyIdCS : IInvalidable<HelAssemblyIdCS>
{
    public static HelAssemblyIdCS Invalid { get; } = new();

    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public Version Version { get; init; } = new();

    public string CultureName { get; init; } = CultureInfo.InvariantCulture.Name;

    public string? PublicKeyToken { get; init; }

    public bool IsInvalid => Name == IHelEntityCS.InvalidName;
}
