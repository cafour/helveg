using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record EntityWorkspaceAnalysisOptions
{
    public static EntityWorkspaceAnalysisOptions Default { get; } = new();

    public bool IncludeExternalDepedencies { get; init; } = false;
}
