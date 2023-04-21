using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public abstract class ProjectVisitor : EntityVisitor, IProjectVisitor
{
    public virtual void VisitProject(Project project)
    {
        DefaultVisit(project);
    }

    public virtual void VisitSolution(Solution solution)
    {
        DefaultVisit(solution);
    }

    public virtual void VisitFramework(Framework framework)
    {
        DefaultVisit(framework);
    }

    public virtual void VisitExternalDependencySource(ExternalDependencySource externalDependencySource)
    {
        DefaultVisit(externalDependencySource);
    }

    public virtual void VisitLibrary(Library library)
    {
        DefaultVisit(library);
    }
}
