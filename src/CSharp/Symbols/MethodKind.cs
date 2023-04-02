namespace Helveg.CSharp.Symbols;

/// <summary>
/// The kind of a method.
/// </summary>
///
/// <remarks>
/// Based on Microsoft.CodeAnalysis.MethodKind.
/// </remarks>
public enum MethodKind
{
    Invalid,
    AnonymousFunction,
    Constructor,
    Conversion,
    DelegateInvoke,
    Destructor,
    EventAdd,
    EventRaise,
    EventRemove,
    ExplicitInterfaceImplementation,
    UserDefinedOperator,
    Ordinary,
    PropertyGet,
    PropertySet,
    ReducedExtension,
    StaticConstructor,
    BuiltinOperator,
    DeclareMethod,
    LocalFunction,
    FunctionPointerSignature
}
