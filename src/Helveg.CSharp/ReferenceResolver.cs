using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public class ReferenceResolver
{
    private readonly HelSolutionCS solution;

    public ReferenceResolver(HelSolutionCS solution)
    {
        this.solution = solution;
    }
}
