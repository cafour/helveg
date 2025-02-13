namespace Helveg.CSharp.Projects;

public record AssemblyDependency
{
    public AssemblyId Identity { get; init; } = AssemblyId.Invalid;

    public NumericToken Token { get; init; } = NumericToken.CreateInvalid(CSConst.CSharpNamespace);
}
