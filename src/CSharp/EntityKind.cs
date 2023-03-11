using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

/// <summary>
/// An enum describing the kind of an entity definition o reference. 
/// </summary>
/// 
/// <remarks>
/// Used to differentiate between types during deserialization.
/// </remarks>
public enum EntityKind
{
    Unknown = 0,
    Solution,
    Project,
    Package,
    ExternalDependency,
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
