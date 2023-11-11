using System.Collections.Immutable;
using Helveg.CSharp.Symbols;
using Helveg.Visualization;

namespace Helveg.CSharp;

/// <summary>
/// The final resting place of the C# abstraction. A single class with every thinkable property.
/// </summary>
public class CSharpNode : MultigraphNode
{
    public ImmutableArray<string>? Versions { get; set; }
    public string? Path { get; set; }
    public string? Version { get; set; }
    public MemberAccessibility? Accessibility { get; set; }
    public bool? IsSealed { get; set; }
    public bool? IsStatic { get; set; }
    public bool? IsAbstract { get; set; }
    public bool? IsExtern { get; set; }
    public bool? IsOverride { get; set; }
    public bool? IsVirtual { get; set; }
    public bool? IsImplicitlyDeclared { get; set; }
    public bool? CanBeReferencedByName { get; set; }
    public string? FileVersion { get; set; }
    public string? CultureName { get; set; }
    public string? PublicKeyToken { get; set; }
    public string? TargetFramework { get; set; }
    public bool? IsNested { get; set; }
    public TypeKind? TypeKind { get; set; }
    public bool? IsAnonymousType { get; set; }
    public bool? IsTupleType { get; set; }
    public bool? IsNativeIntegerType { get; set; }
    public bool? IsUnmanagedType { get; set; }
    public bool? IsReadOnly { get; set; }
    public bool? IsRefLikeType { get; set; }
    public bool? IsRecord { get; set; }
    public int? Arity { get; set; }
    public bool? IsImplicitClass { get; set; }
    public int? InstanceMemberCount { get; set; }
    public int? StaticMemberCount { get; set; }
    public bool? IsVolatile { get; set; }
    public bool? IsConst { get; set; }
    public bool? IsEnumItem { get; set; }
    public RefKind? RefKind { get; set; }
    public string? FieldType { get; set; }
    public string? ParameterType { get; set; }
    public bool? IsDiscard { get; set; }
    public int? Ordinal { get; set; }
    public bool? IsParams { get; set; }
    public bool? IsOptional { get; set; }
    public bool? IsThis { get; set; }
    public bool? HasExplicitDefaultValue { get; set; }
    public string? EventType { get; set; }
    public string? PropertyType { get; set; }
    public bool? IsIndexer { get; set; }
    public bool? IsWriteOnly { get; set; }
    public int? ParameterCount { get; set; }
    public MethodKind? MethodKind { get; set; }
    public bool? IsExtensionMethod { get; set; }
    public bool? IsAsync { get; set; }
    public string? ReturnType { get; set; }
    public bool? IsInitOnly { get; set; }
    public string? DeclaringKind { get; set; }
}
