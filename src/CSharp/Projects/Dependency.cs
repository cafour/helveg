using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record Dependency
{
    public AssemblyId Identity { get; init; } = AssemblyId.Invalid;

    public NumericToken Token { get; init; } = NumericToken.CreateInvalid(CSConst.CSharpNamespace);
}
