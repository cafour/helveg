namespace Helveg;

public interface IEntityVisitor
{
    void Visit(IEntity? entity);

    void DefaultVisit(IEntity entity);
}
