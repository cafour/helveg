using Helveg.CSharp.Projects;
using Helveg.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Packages;

public class VisualizationPackageVisitor : PackageVisitor
{
    private readonly Multigraph graph;

    public VisualizationPackageVisitor(Multigraph graph)
    {
        this.graph = graph;
    }

    public override void DefaultVisit(IEntity entity)
    {
    }

    public override void VisitPackageRepository(PackageRepository repository)
    {
        base.VisitPackageRepository(repository);

        if (repository.Packages.Length == 0)
        {
            // don't emit a node for an empty package repository
            return;
        }

        var node = graph.GetNode<CSharpNode>(repository.Token, repository.Name);
        node.Kind = CSConst.KindOf<PackageRepository>();

        graph.AddEdges(
            CSRelations.Declares,
            repository.Packages.Select(p => new MultigraphEdge(repository.Token, p.Token)), true);
    }

    public override void VisitPackage(Package package)
    {
        base.VisitPackage(package);

        var node = graph.GetNode<CSharpNode>(package.Token, package.Name);
        node.Kind = CSConst.KindOf<Package>();
        node.Versions = package.Versions;

        graph.AddEdges(CSRelations.Declares, package.Extensions.OfType<LibraryExtension>()
            .Select(l => new MultigraphEdge(package.Token, l.Library.Token)), true);
    }
}
