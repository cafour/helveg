using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record Library : EntityBase
{
    public static Library Invalid { get; } = new Library();

    public AssemblyId Identity { get; set; } = AssemblyId.Invalid;

    [JsonIgnore]
    public NumericToken Token { get; init; } = NumericToken.CreateInvalid(CSConst.CSharpNamespace);

    public override string Id
    {
        get => Token;
        init => Token = NumericToken.Parse(value);
    }

    public string? PackageId { get; init; }

    public string? PackageVersion { get; init; }

    public NumericToken ContainingEntity { get; init; } = NumericToken.CreateInvalid(CSConst.CSharpNamespace);

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IProjectVisitor projectVisitor)
        {
            projectVisitor.VisitLibrary(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        base.Accept(visitor);
    }
}
