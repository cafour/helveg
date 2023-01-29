namespace Helveg.CSharp;

// based on Microsoft.CodeAnalysis.MethodKind
public enum HelMethodKindCS
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
