using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg;

public abstract class EntityVisitor : IEntityVisitor
{
    public abstract void DefaultVisit(IEntity entity);

    public void Visit(IEntity? entity)
    {
        entity?.Accept(this);
    }
}
