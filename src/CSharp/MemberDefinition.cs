using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public interface IMemberDefinition
{
    MemberAccessibility Accessibility { get; }

    bool IsSealed { get; }

    bool IsStatic { get; }

    bool IsAbstract { get; }

    bool IsExtern { get; }

    bool IsOverride { get; }

    bool IsVirtual { get; }

    bool IsImplicitlyDeclared { get; }

    bool CanBeReferencedByName { get; }

    TypeReference? ContainingType { get; }

    NamespaceReference ContainingNamespace { get; }
}

/// <summary>
/// A class for sharing properties among all member definitions (fields, events, properties, methods).
/// </summary>
/// <typeparam name="TReference"></typeparam>
public abstract record MemberDefinition : EntityDefinition, IMemberDefinition
{
    public MemberAccessibility Accessibility { get; init; }

    public bool IsSealed { get; init; }

    public bool IsStatic { get; init; }

    public bool IsAbstract { get; init; }

    public bool IsExtern { get; init; }

    public bool IsOverride { get; init; }

    public bool IsVirtual { get; init; }

    public bool IsImplicitlyDeclared { get; init; }

    public bool CanBeReferencedByName { get; init; }

    public TypeReference? ContainingType { get; init; }

    public NamespaceReference ContainingNamespace { get; init; } = NamespaceReference.Invalid;
}
