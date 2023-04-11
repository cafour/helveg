using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record Dependency
{
    public string Name { get; init; } = Const.Invalid;

    public string? FullPath { get; init; }

    public string? FileVersion { get; init; }

    public string? PublicKeyToken { get; init; }

    public Version Version { get; init; } = new();

    public string? PackageId { get; init; }

    public string? FrameworkId { get; init; }
}
