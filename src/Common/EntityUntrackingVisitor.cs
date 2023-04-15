using System.Collections.Concurrent;

namespace Helveg;

internal class EntityUntrackingVisitor : EntityVisitor
{
    private readonly ConcurrentDictionary<string, IEntity> entities;

    public EntityUntrackingVisitor(ConcurrentDictionary<string, IEntity> entities)
    {
        this.entities = entities;
    }
    
    public override void DefaultVisit(IEntity entity)
    {
        entities.TryRemove(entity.Id, out _);
    }
}
