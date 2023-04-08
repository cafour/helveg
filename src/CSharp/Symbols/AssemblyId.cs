using System;
using System.Globalization;
using System.Text;

namespace Helveg.CSharp.Symbols;

public record AssemblyId
{
    public static AssemblyId Invalid { get; } = new();

    public string Name { get; init; } = Const.Invalid;

    public Version Version { get; init; } = new();

    public string? FileVersion { get; init; }

    public string? InformationalVersion { get; init; }

    public string? TargetFramework { get; init; }

    public string CultureName { get; init; } = CultureInfo.InvariantCulture.Name;

    public string? PublicKeyToken { get; init; }

    public bool IsInvalid => Name == Const.Invalid;

    public string ToDisplayString(bool isFull = false)
    {
        var ss = new StringBuilder(Name);
        ss.Append(", ");
        ss.Append(Version);

        if (!string.IsNullOrEmpty(TargetFramework))
        {
            ss.Append(", TargetFramework=");
            ss.Append(TargetFramework);
        }

        if (!string.IsNullOrEmpty(FileVersion) && FileVersion != Version.ToString() && isFull)
        {
            ss.Append(", FileVersion=");
            ss.Append(FileVersion);
        }

        if (!string.IsNullOrEmpty(InformationalVersion) && InformationalVersion != Version.ToString() && isFull)
        {
            ss.Append(", InformationalVersion=");
            ss.Append(InformationalVersion);
        }

        if (!string.IsNullOrEmpty(CultureName) && isFull)
        {
            ss.Append(", Culture=");
            ss.Append(CultureName);
        }

        if (!string.IsNullOrEmpty(PublicKeyToken) && isFull)
        {
            ss.Append(", PublicKeyToken=");
            ss.Append(PublicKeyToken);
        }

        return ss.ToString();
    }
}
