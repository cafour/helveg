using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record LibraryExtension : IVisitableEntityExtension
{
    public Library Library { get; init; } = Library.Invalid;

    public void Accept(IEntityVisitor visitor)
    {
        Library.Accept(visitor);
    }
}
