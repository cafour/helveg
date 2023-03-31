namespace Helveg;

public interface IEntityVisitor
{
    void DefaultVisit(IEntity entity);
}
