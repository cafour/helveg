using Helveg.Abstractions;
using System;
using System.Globalization;

namespace Helveg.CSharp;

public record AssemblyId : IInvalidable<AssemblyId>
{
    public static AssemblyId Invalid { get; } = new();

    public string Name { get; init; } = IEntityDefinition.InvalidName;

    public Version Version { get; init; } = new();

    public string CultureName { get; init; } = CultureInfo.InvariantCulture.Name;

    public string? PublicKeyToken { get; init; }

    public bool IsInvalid => Name == IEntityDefinition.InvalidName;
}
