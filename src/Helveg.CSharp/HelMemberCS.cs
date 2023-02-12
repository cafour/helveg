using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public abstract record HelMemberReferenceCS : HelReferenceCS
{
    public HelTypeReferenceCS? ContainingType { get; init; }

    public HelNamespaceReferenceCS? ContainingNamespace { get; init; }
}

public abstract record HelMemberCS<TReference> : HelDefinitionCS<TReference>
    where TReference : HelMemberReferenceCS
{
    public HelAccessibilityCS Accessibility { get; init; }

    public bool IsSealed { get; init; }

    public bool IsStatic { get; init; }

    public bool IsAbstract { get; init; }

    public bool IsExtern { get; init; }

    public bool IsOverride { get; init; }

    public bool IsVirtual { get; init; }

    public bool IsImplicitlyDeclared { get; init; }

    public bool CanBeReferencedByName { get; init; }

    public HelTypeReferenceCS? ContainingType { get; init; }

    public HelNamespaceReferenceCS ContainingNamespace { get; init; } = HelNamespaceReferenceCS.Invalid;
}
