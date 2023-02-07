using System;

namespace Helveg.CSharp;

public record HelWorkspaceCS
{
    public static readonly HelWorkspaceCS Invalid = new();

    public DateTimeOffset CreatedAt { get; init; }

    public string? Revision { get; set; }

    public HelSolutionCS Solution { get; init; } = IInvalidable<HelSolutionCS>.Invalid;

    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public IHelEntityCS Resolve(IHelEntityCS reference)
    {
        throw new NotImplementedException();
    }
}
