using Helveg.CSharp.Projects;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

/// <summary>
/// Transcribes metadata from Roslyn's <see cref="ISymbol"/>s of a given <see cref="Compilation"/> to
/// Helveg.CSharp data types.
/// </summary>
internal class RoslynSymbolTranscriber
{
    private readonly WeakReference<Compilation?> compilationRef = new(null);
    private readonly SymbolTokenMap tokenMap;

    public SymbolAnalysisScope Scope { get; }

    public RoslynSymbolTranscriber(SymbolTokenMap tokenMap, SymbolAnalysisScope scope)
    {
        this.tokenMap = tokenMap;
        Scope = scope;
    }

    public AssemblyDefinition Transcribe(AssemblyId assemblyId)
    {
        var compilation = tokenMap.GetCompilation(assemblyId);
        if (compilation is null)
        {
            return AssemblyDefinition.Invalid;
        }

        compilationRef.SetTarget(compilation);
        if (AssemblyId.Create(compilation.Assembly) == assemblyId)
        {
            return GetAssembly(compilation.Assembly);
        }

        var reference = compilation.GetReferencedAssembly(assemblyId);
        if (reference is null)
        {
            return AssemblyDefinition.Invalid;
        }

        return GetAssembly(reference);
    }

    private AssemblyDefinition GetAssembly(IAssemblySymbol assembly)
    {
        var helAssembly = new AssemblyDefinition
        {
            Token = tokenMap.GetOrAdd(assembly),
            Name = assembly.Name,
            Identity = AssemblyId.Create(assembly)
        };

        return helAssembly with
        {
            Modules = assembly.Modules
                .Select(m => GetModule(m, helAssembly.Reference))
                .ToImmutableArray()
        };
    }

    private ModuleDefinition GetModule(IModuleSymbol module, AssemblyReference containingAssembly)
    {
        var helModule = new ModuleDefinition
        {
            Token = tokenMap.GetOrAdd(module),
            Name = module.Name,
            ReferencedAssemblies = module.ReferencedAssemblySymbols
                .Select(GetAssemblyReference)
                .ToImmutableArray(),
            ContainingAssembly = containingAssembly
        };

        return helModule with
        {
            GlobalNamespace = GetNamespace(module.GlobalNamespace, helModule.Reference, null)
        };
    }

    private NamespaceDefinition GetNamespace(
        INamespaceSymbol @namespace,
        ModuleReference containingModule,
        NamespaceReference? containingNamespace)
    {
        var helNamespace = new NamespaceDefinition
        {
            Token = tokenMap.GetOrAdd(@namespace),
            Name = @namespace.Name,
            ContainingModule = containingModule,
            ContainingNamespace = containingNamespace
        };

        return helNamespace with
        {
            Types = @namespace.GetTypeMembers()
                .Where(t => t.IsInAnalysisScope(Scope))
                .Select(t => GetType(t, helNamespace.Reference))
                .ToImmutableArray(),
            Namespaces = @namespace.GetNamespaceMembers()
                .Select(n => GetNamespace(n, containingModule, helNamespace.Reference))
                .ToImmutableArray()
        };
    }

    private TypeDefinition GetType(INamedTypeSymbol type,
        NamespaceReference containingNamespace,
        TypeReference? containingType = null)
    {
        if (!type.IsOriginalDefinition())
        {
            type = type.OriginalDefinition;
        }

        var helType = new TypeDefinition
        {
            Token = tokenMap.GetOrAdd(type),
            MetadataName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ContainingNamespace = containingNamespace,
            ContainingType = containingType,
            TypeKind = type.TypeKind.ToHelvegTypeKind(),
            IsReferenceType = type.IsReferenceType,
            IsValueType = type.IsValueType,
            IsAnonymousType = type.IsAnonymousType,
            IsTupleType = type.IsTupleType,
            IsNativeIntegerType = type.IsNativeIntegerType,
            IsRefLikeType = type.IsRefLikeType,
            IsUnmanagedType = type.IsUnmanagedType,
            IsReadOnly = type.IsReadOnly,
            IsRecord = type.IsRecord,
            IsImplicitClass = type.IsImplicitClass,
            BaseType = type.BaseType is null ? null : GetTypeReference(type.BaseType),
            Interfaces = type.Interfaces.Select(GetTypeReference).ToImmutableArray(),
        };

        helType = PopulateMember(type, helType);

        var reference = helType.Reference;

        var members = type.GetMembers()
            .Where(s => s.IsInAnalysisScope(Scope));

        return helType with
        {
            TypeParameters = type.TypeParameters
                .Select(p => GetTypeParameter(p, reference, null, containingNamespace))
                .ToImmutableArray(),
            NestedTypes = type.GetTypeMembers()
                .Where(t => t.IsInAnalysisScope(Scope))
                .Select(t => GetType(t, containingNamespace, reference))
                .ToImmutableArray(),
            // TODO: Does GetMembers() contain nested types as well?
            Fields = members
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Field)
                .Cast<IFieldSymbol>()
                .Select(f => GetField(f, reference, containingNamespace))
                .ToImmutableArray(),
            Events = members
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Event)
                .Cast<IEventSymbol>()
                .Select(e => GetEvent(e, reference, containingNamespace))
                .ToImmutableArray(),
            Properties = members
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .Select(p => GetProperty(p, reference, containingNamespace))
                .ToImmutableArray(),
            Methods = members
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Method)
                .Cast<IMethodSymbol>()
                .Select(m => GetMethod(m, reference, containingNamespace))
                .ToImmutableArray(),

        };
    }

    private TypeParameterDefinition GetTypeParameter(
        ITypeParameterSymbol symbol,
        TypeReference? declaringType,
        MethodReference? declaringMethod,
        NamespaceReference containingNamespace)
    {
        if (declaringType is null && declaringMethod is null)
        {
            throw new ArgumentException($"Either '{nameof(declaringType)}' or '{nameof(declaringMethod)}' " +
                "must not be null.");
        }

        var helTypeParameter = new TypeParameterDefinition
        {
            Token = tokenMap.GetOrAdd(symbol),
            Name = symbol.Name,
            DeclaringType = declaringType,
            DeclaringMethod = declaringMethod,
            ContainingNamespace = containingNamespace
        };

        return helTypeParameter;
    }

    private EventDefinition GetEvent(
        IEventSymbol symbol,
        TypeReference containingType,
        NamespaceReference containingNamespace)
    {
        var helEvent = new EventDefinition
        {
            Token = tokenMap.GetOrAdd(symbol),
            EventType = GetTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            AddMethod = symbol.AddMethod is null ? null : GetMethodReference(symbol.AddMethod),
            RemoveMethod = symbol.RemoveMethod is null ? null : GetMethodReference(symbol.RemoveMethod),
            RaiseMethod = symbol.RaiseMethod is null ? null : GetMethodReference(symbol.RaiseMethod)
        };

        return PopulateMember(symbol, helEvent);
    }

    private FieldDefinition GetField(
        IFieldSymbol symbol,
        TypeReference containingType,
        NamespaceReference containingNamespace)
    {
        var helField = new FieldDefinition
        {
            Token = tokenMap.GetOrAdd(symbol),
            FieldType = GetTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            AssociatedEvent = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IEventSymbol e
                ? GetEventReference(e)
                : null,
            AssociatedProperty = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IPropertySymbol p
                ? GetPropertyReference(p)
                : null,
            IsVolatile = symbol.IsVolatile,
            IsReadOnly = symbol.IsReadOnly,
            IsRequired = symbol.IsRequired,
            IsConst = symbol.IsConst,
            RefKind = symbol.RefKind.ToHelvegRefKind(),
            IsEnumItem = symbol.ContainingType.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum
        };

        return PopulateMember(symbol, helField);
    }

    private PropertyDefinition GetProperty(
        IPropertySymbol symbol,
        TypeReference containingType,
        NamespaceReference containingNamespace)
    {
        var helProperty = new PropertyDefinition
        {
            Token = tokenMap.GetOrAdd(symbol),
            PropertyType = GetTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            GetMethod = symbol.GetMethod is null ? null : GetMethodReference(symbol.GetMethod),
            SetMethod = symbol.SetMethod is null ? null : GetMethodReference(symbol.SetMethod),
            IsIndexer = symbol.IsIndexer,
            IsRequired = symbol.IsRequired,
            OverriddenProperty = symbol.OverriddenProperty is null
                ? null
                : GetPropertyReference(symbol.OverriddenProperty),
            RefKind = symbol.RefKind.ToHelvegRefKind()
        };

        helProperty = PopulateMember(symbol, helProperty);

        return helProperty with
        {
            Parameters = symbol.Parameters
                .Select(p => GetParameter(p, null, helProperty.Reference))
                .ToImmutableArray()
        };
    }

    private MethodDefinition GetMethod(
        IMethodSymbol symbol,
        TypeReference containingType,
        NamespaceReference containingNamespace)
    {
        if (!symbol.IsOriginalDefinition())
        {
            symbol = symbol.OriginalDefinition;
        }

        var helMethod = new MethodDefinition
        {
            Token = tokenMap.GetOrAdd(symbol),
            AssociatedEvent = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IEventSymbol e
                ? GetEventReference(e)
                : null,
            AssociatedProperty = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IPropertySymbol p
                ? GetPropertyReference(p)
                : null,
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            IsAsync = symbol.IsAsync,
            IsExtensionMethod = symbol.IsExtensionMethod,
            IsInitOnly = symbol.IsInitOnly,
            IsReadOnly = symbol.IsReadOnly,
            MethodKind = symbol.MethodKind.ToHelvegMethodKind(),
            OverridenMethod = symbol.OverriddenMethod is null ? null : GetMethodReference(symbol.OverriddenMethod),
            ReceiverType = symbol.ReceiverType is null ? null : GetTypeReference(symbol.ReceiverType),
            RefKind = symbol.RefKind.ToHelvegRefKind(),
            ReturnType = GetTypeReference(symbol.ReturnType)
        };

        helMethod = PopulateMember(symbol, helMethod);

        return helMethod with
        {
            ExplicitInterfaceImplementations = symbol.ExplicitInterfaceImplementations
                .Select(GetMethodReference)
                .ToImmutableArray(),
            TypeParameters = symbol.TypeParameters
                .Select(p => GetTypeParameter(p, null, helMethod.Reference, containingNamespace))
                .ToImmutableArray(),
            Parameters = symbol.Parameters
                .Select(p => GetParameter(p, helMethod.Reference, null))
                .ToImmutableArray()
        };
    }

    private ParameterDefinition GetParameter(
        IParameterSymbol symbol,
        MethodReference? declaringMethod,
        PropertyReference? declaringProperty)
    {
        var helParameter = new ParameterDefinition
        {
            Token = tokenMap.GetOrAdd(symbol),
            Name = symbol.Name,
            Ordinal = symbol.Ordinal,
            DeclaringMethod = declaringMethod,
            DeclaringProperty = declaringProperty,
            HasExplicitDefaultValue = symbol.HasExplicitDefaultValue,
            IsDiscard = symbol.IsDiscard,
            IsOptional = symbol.IsOptional,
            IsParams = symbol.IsParams,
            IsThis = symbol.IsThis,
            ParameterType = GetTypeReference(symbol.Type),
            RefKind = symbol.RefKind.ToHelvegRefKind()
        };

        return helParameter;
    }

    private TMember PopulateMember<TMember>(ISymbol symbol, TMember helMember)
        where TMember : MemberDefinition
    {
        return helMember with
        {
            Name = symbol.Name,
            Accessibility = symbol.DeclaredAccessibility.ToHelvegAccessibility(),
            CanBeReferencedByName = symbol.CanBeReferencedByName,
            IsAbstract = symbol.IsAbstract,
            IsExtern = symbol.IsExtern,
            IsImplicitlyDeclared = symbol.IsImplicitlyDeclared,
            IsOverride = symbol.IsOverride,
            IsSealed = symbol.IsSealed,
            IsStatic = symbol.IsStatic,
            IsVirtual = symbol.IsVirtual
        };
    }

    private AssemblyReference GetAssemblyReference(IAssemblySymbol symbol)
    {
        var reference = new AssemblyReference
        {
            Token = tokenMap.GetOrAdd(symbol),
            Hint = symbol.ToDisplayString()
        };
        return reference;
    }

    private ModuleReference GetModuleReference(IModuleSymbol symbol)
    {
        var reference = new ModuleReference
        {
            Token = tokenMap.GetOrAdd(symbol),
            Hint = symbol.ToDisplayString()
        };
        return reference;
    }

    private NamespaceReference GetNamespaceReference(INamespaceSymbol symbol)
    {

        var reference = new NamespaceReference
        {
            Token = tokenMap.GetOrAdd(symbol),
            Hint = symbol.ToDisplayString()
        };

        if (symbol.NamespaceKind != NamespaceKind.Module)
        {
            reference = reference with
            {
                Diagnostics = ImmutableArray.Create(Diagnostic.Warning(
                    "NonModuleNamespace",
                    $"Only 'module' namespaces should be turned into a {nameof(NamespaceReference)}."))
            };
        }

        return reference;
    }

    private TypeReference GetTypeReference(ITypeSymbol symbol)
    {
        if (!compilationRef.TryGetTarget(out var compilation) || compilation is null)
        {
            return TypeReference.Invalid;
        }

        TypeReference? reference;
        // TODO: IDynamicTypeSymbol
        // TODO: IErrorTypeSymbol
        switch (symbol)
        {
            case IArrayTypeSymbol arrayType:
                var arrayToken = tokenMap.GetOrAdd(compilation.GetSpecialType(SpecialType.System_Array));
                reference = new ArrayTypeReference
                {
                    Token = arrayToken,
                    ElementType = GetTypeReference(arrayType.ElementType),
                    TypeKind = TypeKind.Array,
                    Sizes = arrayType.Sizes,
                    Rank = arrayType.Rank,
                };
                break;
            case INamedTypeSymbol namedType:
                reference = new TypeReference
                {
                    Token = tokenMap.GetOrAdd(symbol.OriginalDefinition),
                };
                if (!namedType.IsOriginalDefinition())
                {
                    reference = reference with
                    {
                        TypeArguments = namedType.TypeArguments
                            .Select(GetTypeReference)
                            .ToImmutableArray()
                    };
                }
                break;
            case IFunctionPointerTypeSymbol fpType:
                var fnPtrToken = tokenMap.GetOrAdd(compilation.GetSpecialType(SpecialType.System_IntPtr));
                reference = new FunctionPointerTypeReference
                {
                    Token = fnPtrToken,
                    Signature = GetMethodReference(fpType.Signature)
                };
                break;
            case IPointerTypeSymbol pointerType:
                var intPtrToken = tokenMap.GetOrAdd(compilation.GetSpecialType(SpecialType.System_IntPtr));
                reference = new PointerTypeReference
                {
                    Token = intPtrToken,
                    PointedAtType = GetTypeReference(pointerType.PointedAtType)
                };
                break;
            case ITypeParameterSymbol typeParameter:
                reference = new TypeParameterReference
                {
                    Token = tokenMap.GetOrAdd(typeParameter)
                };
                break;

            default:
                reference = TypeReference.Invalid;
                break;
        }
        reference = reference with
        {
            Hint = symbol.ToDisplayString(),
            TypeKind = symbol.TypeKind.ToHelvegTypeKind(),
            Nullability = symbol.NullableAnnotation.ToHelvegNullability()
        };
        return reference;
    }

    private MethodReference GetMethodReference(IMethodSymbol symbol)
    {
        var isOriginal = symbol.IsOriginalDefinition();

        var reference = new MethodReference
        {
            Token = tokenMap.GetOrAdd(symbol),
            TypeArguments = isOriginal
                ? ImmutableArray<TypeReference>.Empty
                : symbol.TypeArguments
                    .Select(GetTypeReference)
                    .ToImmutableArray(),
        };

        return reference;
    }

    private EventReference GetEventReference(IEventSymbol symbol)
    {
        var reference = new EventReference
        {
            Token = tokenMap.GetOrAdd(symbol)
        };
        return reference;
    }

    private PropertyReference GetPropertyReference(IPropertySymbol symbol)
    {
        var reference = new PropertyReference
        {
            Token = tokenMap.GetOrAdd(symbol)
        };
        return reference;
    }

    private FieldReference GetFieldReference(IFieldSymbol symbol)
    {
        var reference = new FieldReference
        {
            Token = tokenMap.GetOrAdd(symbol)
        };
        return reference;
    }
}
