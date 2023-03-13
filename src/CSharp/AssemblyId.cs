using System;
using System.Globalization;

namespace Helveg.CSharp;

public record AssemblyId
{
    public static AssemblyId Invalid { get; } = new();

    public string Name { get; init; } = CSharpConstants.InvalidName;

    public Version Version { get; init; } = new();

    public string CultureName { get; init; } = CultureInfo.InvariantCulture.Name;

    public string? PublicKeyToken { get; init; }

    public bool IsInvalid => Name == CSharpConstants.InvalidName;
}
