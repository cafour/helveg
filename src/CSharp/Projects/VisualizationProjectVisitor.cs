using Helveg.CSharp.Symbols;
using Helveg.Visualization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Helveg.CSharp.Projects;

public class VisualizationProjectVisitor : ProjectVisitor
{
    private readonly Multigraph graph;

    public VisualizationProjectVisitor(Multigraph graph)
    {
        this.graph = graph;
    }

    public override void DefaultVisit(IEntity entity)
    {
    }

    public override void VisitSolution(Solution solution)
    {
        base.VisitSolution(solution);

        var node = graph.GetNode<CSharpNode>(solution.Token, solution.Name);
        node.Path = solution.Path;
        node.Kind = CSConst.KindOf<Solution>();
        graph.AddEdges(
            CSRelations.Declares,
            solution.Projects.Select(p => new MultigraphEdge(solution.Id, p.Id)), true);
    }

    public override void VisitProject(Project project)
    {
        base.VisitProject(project);

        var node = graph.GetNode<CSharpNode>(project.Token, project.Name);
        node.Path = project.Path;
        node.Kind = CSConst.KindOf<Project>();

        graph.AddEdges(CSRelations.DependsOn, project.Dependencies
            .SelectMany(d => d.Value)
            .Distinct()
            .Where(d => d.Token.HasValue)
            .Select(d => new MultigraphEdge(project.Id, d.Token)));

        var assemblies = project.Extensions.OfType<AssemblyExtension>().ToArray();
        if (assemblies.Length > 0)
        {
            graph.AddEdges(
                CSRelations.Declares,
                assemblies.Select(a => new MultigraphEdge(project.Id, a.Assembly.Token)),
                true);
        }
    }

    public override void VisitFramework(Framework framework)
    {
        base.VisitFramework(framework);

        var node = graph.GetNode<CSharpNode>(framework.Token, framework.Name);
        node.Version = framework.Version;
        node.Kind = CSConst.KindOf<Framework>();

        if (framework.Libraries.Length > 0)
        {
            graph.AddEdges(
                CSRelations.Declares,
                framework.Libraries.Select(d => new MultigraphEdge(framework.Token, d.Token)),
                true);
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

        var node = graph.GetNode<CSharpNode>(externalDependencySource.Token, externalDependencySource.Name);
        node.Kind = CSConst.KindOf<ExternalDependencySource>();
        if (externalDependencySource.Libraries.Length > 0)
        {
            graph.AddEdges(CSRelations.Declares, externalDependencySource.Libraries
                .Select(d => new MultigraphEdge(externalDependencySource.Token, d.Token)),
                true);
        }
    }

    public override void VisitLibrary(Library library)
    {
        base.VisitLibrary(library);

        var node = graph.GetNode<CSharpNode>(library.Token, library.Identity.Name);
        node.Kind = CSConst.KindOf<Library>();
        var assemblies = library.Extensions.OfType<AssemblyExtension>().ToArray();
        if (assemblies.Length > 0)
        {
            graph.AddEdges(
                CSRelations.Declares,
                assemblies.Select(a => new MultigraphEdge(library.Token, a.Assembly.Token)),
                true);
        }
    }
}
