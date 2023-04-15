using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Packages;

public interface IPackageVisitor : IEntityVisitor
{
    void VisitPackageRepository(PackageRepository repository);
    void VisitPackage(Package package);
}
