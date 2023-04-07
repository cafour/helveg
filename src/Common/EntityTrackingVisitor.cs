using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;

namespace Helveg;

internal class EntityTrackingVisitor : EntityVisitor
{
    private readonly ConcurrentDictionary<string, IEntity> entities;
    private readonly ILogger logger;

    public EntityTrackingVisitor(
        ConcurrentDictionary<string, IEntity> entities,
        ILogger? logger = null)
    {
        this.entities = entities;
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<EntityTrackingVisitor>();
    }

    public override void DefaultVisit(IEntity entity)
    {
        if (!entities.TryAdd(entity.Id, entity))
        {
            logger.LogError("The environment already tracks an entity with the '{}' id. " +
                "The id of an entity must be unique.", entity.Id);
        }
    }
}
