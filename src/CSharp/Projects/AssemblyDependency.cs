using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

/// <summary>
/// Represents a dependency on an assembly. Corresponds with the `Reference` item in MSBuild, but is named this way to
/// avoid conflicts with the <see cref="Symbols.AssemblyReference"/> type.
/// </summary>
public record AssemblyDependency : EntityBase
{
    public AssemblyId Identity { get; init; } = AssemblyId.Invalid;

    public string? PackageId { get; init; }

    public string? PackageVersion { get; init; }

    [JsonIgnore]
    public NumericToken Token { get; init; } = CSConst.InvalidToken;

    public override string Id { get => Token; init => Token = NumericToken.Parse(value); }

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IProjectVisitor projectVisitor)
        {
            projectVisitor.VisitAssemblyDependency(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        base.Accept(visitor);
    }
}
