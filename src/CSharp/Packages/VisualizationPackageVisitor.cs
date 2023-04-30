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
    private readonly MultigraphBuilder builder;

    public VisualizationPackageVisitor(MultigraphBuilder builder)
    {
        this.builder = builder;
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

        builder.GetNode(repository.Token, repository.Name)
            .SetProperty(Const.StyleProperty, CSConst.NodeStyle)
            .SetProperty(CSProperties.Kind, CSConst.KindOf<PackageRepository>());

        builder.AddEdges(
            CSRelations.Declares,
            repository.Packages.Select(p => new Edge(repository.Token, p.Token)),
            CSConst.RelationStyle);
    }

    public override void VisitPackage(Package package)
    {
        base.VisitPackage(package);

        builder.GetNode(package.Token, package.Name)
            .SetProperty(Const.StyleProperty, CSConst.NodeStyle)
            .SetProperty(CSProperties.Kind, CSConst.KindOf<Package>())
            .SetProperty(nameof(package.Versions), package.Versions);

        builder.AddEdges(CSRelations.Declares, package.Extensions.OfType<LibraryExtension>()
            .Select(l => new Edge(package.Token, l.Library.Token)),
            CSConst.RelationStyle);
    }
}
