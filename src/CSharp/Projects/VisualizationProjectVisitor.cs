using Helveg.CSharp.Symbols;
using Helveg.Visualization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Helveg.CSharp.Projects;

public class VisualizationProjectVisitor : ProjectVisitor
{
    private readonly MultigraphBuilder builder;

    public VisualizationProjectVisitor(MultigraphBuilder builder)
    {
        this.builder = builder;
    }

    public override void DefaultVisit(IEntity entity)
    {
    }

    public override void VisitSolution(Solution solution)
    {
        base.VisitSolution(solution);

        builder.GetNode(solution.Token, solution.Name)
            .SetProperty(nameof(Solution.Path), solution.Path)
            .SetProperty(CSProperties.Kind, CSConst.KindOf<Solution>());
        builder.AddEdges(CSRelations.Declares, solution.Projects.Select(p => new Edge(solution.Id, p.Id)));
    }

    public override void VisitProject(Project project)
    {
        base.VisitProject(project);

        builder.GetNode(project.Token, project.Name)
            .SetProperty(nameof(Solution.Path), project.Path)
            .SetProperty(CSProperties.Kind, CSConst.KindOf<Project>());

        builder.AddEdges(CSRelations.DependsOn, project.Dependencies
            .SelectMany(d => d.Value)
            .Where(d => d.Token.HasValue)
            .Select(d => new Edge(project.Id, d.Token)));

        var assemblies = project.Extensions.OfType<AssemblyExtension>().ToArray();
        if (assemblies.Length > 0)
        {
            builder.AddEdges(CSRelations.Declares, assemblies.Select(a => new Edge(project.Id, a.Assembly.Token)));
        }
    }

    public override void VisitFramework(Framework framework)
    {
        base.VisitFramework(framework);

        builder.GetNode(framework.Token, framework.Name)
            .SetProperty(nameof(Framework.Version), framework.Version)
            .SetProperty(CSProperties.Kind, CSConst.KindOf<Framework>());

        if (framework.Libraries.Length > 0)
        {
            builder.AddEdges(CSRelations.Declares, framework.Libraries.Select(d => new Edge(framework.Token, d.Token)));
        }
    }

    public override void VisitExternalDependencySource(ExternalDependencySource externalDependencySource)
    {
        base.VisitExternalDependencySource(externalDependencySource);

        if (externalDependencySource.Libraries.Length == 0)
        {
            // don't add the node, if it will have been empty
            return;
        }

        builder.GetNode(externalDependencySource.Token, externalDependencySource.Name)
            .SetProperty(CSProperties.Kind, CSConst.KindOf<ExternalDependencySource>());
        if (externalDependencySource.Libraries.Length > 0)
        {
            builder.AddEdges(CSRelations.Declares, externalDependencySource.Libraries
                .Select(d => new Edge(externalDependencySource.Token, d.Token)));
        }
    }

    public override void VisitLibrary(Library library)
    {
        base.VisitLibrary(library);

        builder.GetNode(library.Token, library.Identity.Name)
            .SetProperty(CSProperties.Kind, CSConst.KindOf<Library>());
        var assemblies = library.Extensions.OfType<AssemblyExtension>().ToArray();
        if (assemblies.Length > 0)
        {
            builder.AddEdges(CSRelations.Declares, assemblies.Select(a => new Edge(library.Token, a.Assembly.Token)));
        }
    }
}
