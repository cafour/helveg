using System;
using System.Globalization;
using System.Linq;
using System.Text;
using MCA = Microsoft.CodeAnalysis;
using MSB = Microsoft.Build;

namespace Helveg.CSharp;

public sealed record AssemblyId
{
    public const string AssemblyFileVersionAttributeName = "AssemblyFileVersionAttribute";
    public const string AssemblyInformationalVersionAttributeName = "AssemblyInformationalVersionAttribute";
    public const string TargetFrameworkAttributeName = "TargetFrameworkAttribute";

    public static AssemblyId Invalid { get; } = new();

    public string Name { get; init; } = Const.Invalid;

    public string? Version { get; init; }

    public string? FileVersion { get; init; }

    public string? InformationalVersion { get; init; }

    public string? TargetFramework { get; init; }

    public string CultureName { get; init; } = CultureInfo.InvariantCulture.Name;

    public string? PublicKeyToken { get; init; }

    public string? Path { get; init; }

    public bool IsValid => Name != Const.Invalid;

    public static AssemblyId Create(
        MCA.IAssemblySymbol assembly,
        MCA.PortableExecutableReference? reference = null)
    {
        var attributes = assembly.GetAttributes();
        var fileVersion = attributes
            .FirstOrDefault(a => a.AttributeClass?.Name == AssemblyFileVersionAttributeName)
            ?.ConstructorArguments.FirstOrDefault().Value as string;
        var informationalVersion = attributes
            .FirstOrDefault(a => a.AttributeClass?.Name == AssemblyInformationalVersionAttributeName)
            ?.ConstructorArguments.FirstOrDefault().Value as string;
        var targetFramework = attributes
            .FirstOrDefault(a => a.AttributeClass?.Name == TargetFrameworkAttributeName)
            ?.ConstructorArguments.FirstOrDefault().Value as string;

        return new AssemblyId
        {
            Name = assembly.Identity.Name,
            Version = assembly.Identity.Version.ToString(),
            FileVersion = fileVersion,
            InformationalVersion = informationalVersion,
            TargetFramework = targetFramework,
            CultureName = assembly.Identity.CultureName,
            PublicKeyToken = string.Concat(assembly.Identity.PublicKeyToken.Select(b => b.ToString("x"))),
            Path = reference?.FilePath
        };
    }

    public static AssemblyId Create(MSB.Framework.ITaskItem item)
    {
        static string? NullIfEmpty(string? value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        return new AssemblyId
        {
            Name = NullIfEmpty(item.GetMetadata("Filename")) ?? Const.Invalid,
            Path = NullIfEmpty(item.GetMetadata("FullPath")),
            Version = NullIfEmpty(item.GetMetadata("Version")),
            FileVersion = NullIfEmpty(item.GetMetadata("FileVersion")),
            PublicKeyToken = NullIfEmpty(item.GetMetadata("PublicKeyToken"))
        };
    }

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

        if (!string.IsNullOrEmpty(FileVersion) && FileVersion != Version && isFull)
        {
            ss.Append(", FileVersion=");
            ss.Append(FileVersion);
        }

        if (!string.IsNullOrEmpty(InformationalVersion) && InformationalVersion != Version && isFull)
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

        if (!string.IsNullOrEmpty(Path) && isFull)
        {
            ss.Append(", Path=");
            ss.Append(Path);
        }

        return ss.ToString();
    }

    public override int GetHashCode()
    {
        // NB: uses only name since the rules fo AssemblyId equality are complicated
        return Name.GetHashCode();
    }

    public bool Equals(AssemblyId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(other.Path))
        {
            return Path == other.Path;
        }

        if (Name != other.Name)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(PublicKeyToken) && !string.IsNullOrEmpty(other.PublicKeyToken)
            && PublicKeyToken != other.PublicKeyToken)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(FileVersion) && !string.IsNullOrEmpty(other.FileVersion))
        {
            return FileVersion == other.FileVersion;
        }

        if (!string.IsNullOrEmpty(Version) && !string.IsNullOrEmpty(other.Version))
        {
            return Version == other.Version;
        }
        
        if (!string.IsNullOrEmpty(TargetFramework) && !string.IsNullOrEmpty(other.TargetFramework))
        {
            // this is to distinguish multi-targeting projects
            return TargetFramework == other.TargetFramework;
        }

        return true;
    }
}
