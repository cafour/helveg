namespace Helveg;

public interface IEntityVisitor
{
    void Visit(IEntity? entity)
    {
        entity?.Accept(this);
    }

    void DefaultVisit(IEntity entity);
}
