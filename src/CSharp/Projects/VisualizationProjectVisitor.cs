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

        builder.AddNode(solution.Id, Path.GetFileNameWithoutExtension(solution.FullName))
            .SetProperty(nameof(Solution.FullName), solution.FullName);
        builder.AddEdges(CSConst.DeclaredInId, solution.Projects.Select(p => new Edge(p.Id, solution.Id)));
    }

    public override void VisitProject(Project project)
    {
        base.VisitProject(project);

        var assemblies = project.Extensions.OfType<AssemblyExtension>().ToArray();
        if (assemblies.Length > 0)
        {
            builder.AddEdges(CSConst.DeclaredInId, assemblies.Select(a => new Edge(a.Assembly.Token, project.Id)));
        }
    }
}
