using System;
using System.Globalization;

namespace Helveg.CSharp.Symbols;

public record AssemblyId
{
    public static AssemblyId Invalid { get; } = new();

    public string Name { get; init; } = Const.Invalid;

    public Version Version { get; init; } = new();

    public string? FileVersion { get; init; }

    public string? InformationalVersion { get; init; }

    public string CultureName { get; init; } = CultureInfo.InvariantCulture.Name;

    public string? PublicKey { get; init; }

    public bool IsInvalid => Name == Const.Invalid;
}
