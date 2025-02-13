namespace Helveg.CSharp.Projects;

public record PackageDependency
{
    public string Name { get; init; } = Const.Invalid;

    public string? Version { get; init; }

    public NumericToken Token { get; init; } = NumericToken.CreateInvalid(CSConst.CSharpNamespace);
}
