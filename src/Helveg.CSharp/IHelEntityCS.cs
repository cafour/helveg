namespace Helveg.CSharp;

public interface IHelEntityCS
{
    const string InvalidName = "Invalid";

    string Name { get; }

    bool IsInvalid => Name == InvalidName;

    bool IsDefinition { get; }

    HelEntityTokenCS DefinitionToken { get; }
}
