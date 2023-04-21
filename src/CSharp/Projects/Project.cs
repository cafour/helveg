using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Helveg.CSharp.Projects;

public record Project : EntityBase
{
    public static Project Invalid { get; } = new();

    public string Name { get; init; } = Const.Invalid;

    public string? Path { get; init; }

    [JsonIgnore]
    public bool IsValid => Name != Const.Invalid;

    [JsonIgnore]
    public NumericToken ContainingSolution { get; init; } = NumericToken.GlobalInvalid;

    [JsonIgnore]
    public int Index { get; init; } = -1;

    [JsonIgnore]
    public NumericToken Token => NumericToken.Create(CSConst.CSharpNamespace, ContainingSolution.Values.Add(Index));

    public override string Id
    {
        get => Token;
        init
        {
            if (!NumericToken.TryParse(value, out NumericToken token)
                || token.Values.Length < 3
                || token.Values[0] != (int)RootKind.Solution)
            {
                throw new ArgumentException(
                    $"Value is not a valid {nameof(Token)} for a {nameof(Project)}.",
                    nameof(value));
            }
            ContainingSolution = token.Parent;
            Index = token.Values[^1];
        }
    }

    public ImmutableDictionary<string, ImmutableArray<Dependency>> Dependencies { get; init; }
        = ImmutableDictionary<string, ImmutableArray<Dependency>>.Empty;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IProjectVisitor projectVisitor)
        {
            projectVisitor.VisitProject(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        base.Accept(visitor);
    }
}
