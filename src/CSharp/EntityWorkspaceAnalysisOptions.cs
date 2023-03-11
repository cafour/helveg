using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record EntityWorkspaceAnalysisOptions
{
    public static EntityWorkspaceAnalysisOptions Default { get; } = new();

    public bool IncludeExternalDepedencies { get; init; } = false;

    public ImmutableDictionary<string, string> MSBuildProperties { get; init; }
        = ImmutableDictionary<string, string>.Empty;
}
