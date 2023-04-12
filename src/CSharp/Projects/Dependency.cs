using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record Dependency
{
    public string Name { get; init; } = Const.Invalid;

    public string? Path { get; init; }

    public string? FileVersion { get; init; }

    public string? PublicKeyToken { get; init; }

    public string? Version { get; init; }

    public string? PackageId { get; init; }

    public string? PackageVersion { get; init; }

    public NumericToken Framework { get; init; }
        = NumericToken.CreateNone(CSConst.CSharpNamespace, (int)RootKind.Framework);
}
