using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Roslyn;

internal class RoslynLocationEqualityComparer : IEqualityComparer<Location?>
{
    public bool Equals(Location? lhs, Location? rhs)
    {
        if ((lhs is null && rhs is null) || ReferenceEquals(lhs, rhs))
        {
            return true;
        }

        if (lhs is null || rhs is null || lhs.Kind != rhs.Kind)
        {
            return false;
        }

        return lhs.Kind switch
        {
            LocationKind.None => throw new ArgumentException("Location is unspecified."),
            LocationKind.SourceFile when lhs.IsInSource && rhs.IsInSource
                => lhs.SourceTree.FilePath.Equals(rhs.SourceTree.FilePath)
                    && lhs.SourceSpan == rhs.SourceSpan,
            LocationKind.MetadataFile when lhs.MetadataModule is not null && rhs.MetadataModule is not null
                => lhs.MetadataModule.ContainingAssembly.Identity.Equals(lhs.MetadataModule.ContainingAssembly.Identity),
            LocationKind.XmlFile | LocationKind.ExternalFile => lhs.GetLineSpan().Equals(rhs.GetLineSpan()),
            _ => throw new NotSupportedException($"Location kind '{lhs.Kind}' is not supported.")
        };
    }

    public int GetHashCode([DisallowNull] Location obj)
    {
        throw new NotImplementedException();
    }
}
