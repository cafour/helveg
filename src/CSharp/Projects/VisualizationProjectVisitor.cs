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

        builder.AddNode(solution.Id, solution.Name)
            .SetProperty(nameof(Solution.Path), solution.Path)
            .SetProperty("Kind", "csharp:Solution");
        builder.AddEdges(CSConst.DeclaresId, solution.Projects.Select(p => new Edge(solution.Id, p.Id)));
    }

    public override void VisitProject(Project project)
    {
        base.VisitProject(project);

        builder.AddNode(project.Id, project.Name)
            .SetProperty(nameof(Solution.Path), project.Path)
            .SetProperty("Kind", "csharp:Project");

        var assemblies = project.Extensions.OfType<AssemblyExtension>().ToArray();
        if (assemblies.Length > 0)
        {
            builder.AddEdges(CSConst.DeclaresId, assemblies.Select(a => new Edge(project.Id, a.Assembly.Token)));
        }
    }
}
