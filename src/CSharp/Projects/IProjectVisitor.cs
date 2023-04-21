using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public interface IProjectVisitor : IEntityVisitor
{
    void VisitSolution(Solution solution);
    void VisitProject(Project project);
    void VisitFramework(Framework framework);
    void VisitExternalDependencySource(ExternalDependencySource externalDependencySource);
    void VisitLibrary(Library library);
}
