using System;

namespace Helveg.CSharp;

public record EntityWorkspace : IInvalidable<EntityWorkspace>
{
    public static EntityWorkspace Invalid { get; } = new();

    public DateTimeOffset CreatedAt { get; init; }

    public string? Revision { get; set; }

    public SolutionDefinition Solution { get; init; } = SolutionDefinition.Invalid;

    public string Name { get; init; } = IEntityDefinition.InvalidName;

    public IEntityDefinition Resolve(IEntityReference reference)
    {
        throw new NotImplementedException();
    }
}
