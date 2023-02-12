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
}
