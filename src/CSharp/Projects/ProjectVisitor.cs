using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public abstract class ProjectVisitor : EntityVisitor
{
    public virtual void VisitProject(Project project)
    {
        DefaultVisit(project);
    }

    public virtual void VisitSolution(Solution solution)
    {
        DefaultVisit(solution);
    }
}
