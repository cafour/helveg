using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

/// <summary>
/// An enum describing the kind of a symbol definition or a reference.
/// </summary>
/// <remarks>
/// Used to differentiate between types during deserialization.
/// </remarks>
public enum SymbolKind
{
    Unknown = 0,
    Assembly,
    Module,
    Namespace,

    Type,
    TypeParameter,

    Field,
    Method,
    Property,
    Event,

    Parameter
}
