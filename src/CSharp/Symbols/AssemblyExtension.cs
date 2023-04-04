using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public record AssemblyExtension : IVisitableEntityExtension
{
    public AssemblyDefinition Assembly { get; init; } = AssemblyDefinition.Invalid;

    public void Accept(IEntityVisitor visitor)
    {
        Assembly.Accept(visitor);
    }
}
