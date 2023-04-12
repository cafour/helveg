using Helveg.CSharp.Packages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record Framework : EntityBase
{
    public string Name { get; init; } = Const.Invalid;

    public string? Version { get; init; }

    [JsonIgnore]
    public int Index { get; init; }

    [JsonIgnore]
    public NumericToken Token => NumericToken.Create(CSConst.CSharpNamespace, (int)RootKind.Framework, Index);

    public override string Id
    {
        get => Token;
        init
        {
            if (!NumericToken.TryParse(value, out NumericToken token)
                || token.Values.Length != 2
                || token.Values[0] != (int)RootKind.Framework)
            {
                throw new ArgumentException(
                    $"Value is not a valid {nameof(Token)} for a {nameof(Framework)}.",
                    nameof(value));
            }
            Index = token.Values[1];
        }
    }

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IProjectVisitor projectVisitor)
        {
            projectVisitor.VisitFramework(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        base.Accept(visitor);
    }
}
