using Helveg.Abstractions;
using System;

namespace Helveg.CSharp;

public record HelWorkspaceCS : IInvalidable<HelWorkspaceCS>
{
    public static HelWorkspaceCS Invalid { get; } = new();

    public DateTimeOffset CreatedAt { get; init; }

    public string? Revision { get; set; }

    public HelSolutionCS Solution { get; init; } = HelSolutionCS.Invalid;

    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public IHelEntityCS Resolve(IHelEntityCS reference)
    {
        throw new NotImplementedException();
    }
}
