using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Helveg.CSharp.Projects;

public record Solution : EntityBase
{
    public static Solution Invalid { get; } = new();

    [JsonIgnore]
    public int Index { get; init; } = -1;

    public string Name { get; init; } = Const.Invalid;

    public string? Path { get; init; }

    [JsonIgnore]
    public NumericToken Token => NumericToken.Create(CSConst.CSharpNamespace, (int)RootKind.Solution, Index);

    public override string Id
    {
        get => Token;
        init
        {
            if (!NumericToken.TryParse(value, out NumericToken token)
                || token.Values.Length != 2
                || token.Values[0] != (int)RootKind.Solution)
            {
                throw new ArgumentException(
                    $"Value is not a valid {nameof(Token)} for a {nameof(Solution)}.",
                    nameof(value));
            }

            Index = token.Values[2];
        }
    }

    public ImmutableArray<Project> Projects { get; init; } = ImmutableArray<Project>.Empty;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IProjectVisitor projectVisitor)
        {
            projectVisitor.VisitSolution(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        foreach(var project in Projects)
        {
            project.Accept(visitor);
        }

        base.Accept(visitor);
    }
}
