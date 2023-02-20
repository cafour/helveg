using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

internal static class RoslynExtensions
{
    public static bool IsOriginalDefinition(this ISymbol symbol)
    {
        return SymbolEqualityComparer.Default.Equals(symbol, symbol.OriginalDefinition);
    }

    public static HelAssemblyIdCS ToAssemblyId(this AssemblyIdentity identity)
    {
        return new HelAssemblyIdCS
        {
            Name = identity.Name,
            Version = identity.Version,
            CultureName = identity.CultureName,
            PublicKeyToken = string.Concat(identity.PublicKeyToken.Select(b => b.ToString("x")))
        };
    }
}
