namespace Helveg.CSharp.Projects;

public record ProjectDependency
{
    public string Path { get; init; } = Const.Invalid;

    public NumericToken Token { get; init; } = NumericToken.CreateInvalid(CSConst.CSharpNamespace);
}
