using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Packages;

public abstract class PackageVisitor : EntityVisitor, IPackageVisitor
{
    public virtual void VisitPackage(Package package)
    {
        DefaultVisit(package);
    }

    public virtual void VisitPackageRepository(PackageRepository repository)
    {
        DefaultVisit(repository);
    }
}
