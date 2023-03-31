using System;
using System.Collections.Concurrent;

namespace Helveg;

internal class EntityTrackingVisitor : IEntityVisitor
{
    private readonly ConcurrentDictionary<string, IEntity> entities;

    public EntityTrackingVisitor(ConcurrentDictionary<string, IEntity> entities)
    {
        this.entities = entities;
    }
    
    public void DefaultVisit(IEntity entity)
    {
        if (!entities.TryAdd(entity.Id, entity))
        {
            throw new ArgumentException($"The environtment already tracks an entity with the '{entity.Id}' id. " +
                "The id of an entity must be unique.");
        }
    }
}
