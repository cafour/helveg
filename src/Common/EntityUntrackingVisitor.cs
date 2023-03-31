using System.Collections.Concurrent;

namespace Helveg;

internal class EntityUntrackingVisitor : IEntityVisitor
{
    private readonly ConcurrentDictionary<string, IEntity> entities;

    public EntityUntrackingVisitor(ConcurrentDictionary<string, IEntity> entities)
    {
        this.entities = entities;
    }
    
    public void DefaultVisit(IEntity entity)
    {
        entities.TryRemove(entity.Id, out _);
    }
}
