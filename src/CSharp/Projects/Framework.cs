using Helveg.CSharp.Packages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record Framework : EntityBase
{
    public string Name { get; init; } = Const.Invalid;

    public ImmutableArray<string> Versions { get; init; }
        = ImmutableArray<string>.Empty;

    public ImmutableArray<PackageReference> Packages { get; init; }
        = ImmutableArray<PackageReference>.Empty;
}

public record FrameworkReference
{
    public string Id { get; init; } = Const.Invalid;

    public string? Version { get; init; }
}
