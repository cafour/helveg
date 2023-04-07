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
    private readonly Compilation compilation;
    private readonly SymbolTokenMap tokens;

    public RoslynSymbolTranscriber(
        Compilation compilation,
        SymbolTokenMap tokens)
    {
        this.compilation = compilation;
        this.tokens = tokens;
    }

    public AssemblyDefinition Transcribe()
    {
        //VisitAssembly(compilation.Assembly);

        return GetAssembly(compilation.Assembly);
    }

    public AssemblyDefinition? TranscribeReference(MetadataReference reference)
    {
        var symbol = compilation.GetAssemblyOrModuleSymbol(reference);
        if (symbol is null || symbol is not IAssemblySymbol assemblySymbol)
        {
            return null;
        }

        //VisitAssembly(assemblySymbol);

        return GetAssembly(assemblySymbol);
    }

    //private void VisitAssembly(IAssemblySymbol symbol)
    //{
    //    // NB: This is to prevent stack overflows on circular assembly dependencies.
    //    //     Yes, there can be circular assembly references. It worries me as well.
    //    //     Example: System -> System.Configuration -> System.Xml -> System.
    //    if (visitor.VisitedAssemblies.Contains(symbol.Identity))
    //    {
    //        return;
    //    }

    //    // NB: Visit the assembly itself first, so that its name gets into VisitedAssemblies, and the stack doesn't
    //    //     overflow.
    //    visitor.VisitAssembly(symbol);

    //    foreach (var module in symbol.Modules)
    //    {
    //        foreach (var depedency in module.ReferencedAssemblySymbols)
    //        {
    //            VisitAssembly(depedency);
    //        }
    //    }
    //}

    private AssemblyDefinition GetAssembly(IAssemblySymbol assembly)
    {
        var helAssembly = new AssemblyDefinition
        {
            Token = tokens.Get(assembly),
            Name = assembly.Name,
            Identity = assembly.GetHelvegAssemblyId()
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
            Token = tokens.Get(module),
            Name = module.Name,
            ReferencedAssemblies = module.ReferencedAssemblySymbols
                .Select(GetAssemblyReference)
                .ToImmutableArray(),
            ContainingAssembly = containingAssembly
        };

        return helModule with
        {
            GlobalNamespace = GetNamespace(module.GlobalNamespace, helModule.Reference)
        };
    }

    private NamespaceDefinition GetNamespace(INamespaceSymbol @namespace, ModuleReference containingModule)
    {
        var helNamespace = new NamespaceDefinition
        {
            Token = tokens.Get(@namespace),
            Name = @namespace.Name,
            ContainingModule = containingModule
        };

        return helNamespace with
        {
            Types = @namespace.GetTypeMembers()
                .Select(t => GetType(t, helNamespace.Reference))
                .ToImmutableArray(),
            Namespaces = @namespace.GetNamespaceMembers()
                .Select(n => GetNamespace(n, containingModule))
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
            Token = tokens.Get(type),
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

        return helType with
        {
            TypeParameters = type.TypeParameters
                .Select(p => GetTypeParameter(p, reference, null, containingNamespace))
                .ToImmutableArray(),
            NestedTypes = type.GetTypeMembers()
                .Select(t => GetType(t, containingNamespace, reference))
                .ToImmutableArray(),
            // TODO: Does GetMembers() contain nested types as well?
            Fields = type.GetMembers()
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Field)
                .Cast<IFieldSymbol>()
                .Select(f => GetField(f, reference, containingNamespace))
                .ToImmutableArray(),
            Events = type.GetMembers()
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Event)
                .Cast<IEventSymbol>()
                .Select(e => GetEvent(e, reference, containingNamespace))
                .ToImmutableArray(),
            Properties = type.GetMembers()
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .Select(p => GetProperty(p, reference, containingNamespace))
                .ToImmutableArray(),
            Methods = type.GetMembers()
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
            Token = tokens.Get(symbol),
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
            Token = tokens.Get(symbol),
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
            Token = tokens.Get(symbol),
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
            RefKind = symbol.RefKind.ToHelvegRefKind()
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
            Token = tokens.Get(symbol),
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
            Token = tokens.Get(symbol),
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
            Token = tokens.Get(symbol),
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
            Token = tokens.Get(symbol),
            Hint = symbol.ToDisplayString()
        };
        return reference;
    }

    private ModuleReference GetModuleReference(IModuleSymbol symbol)
    {
        var reference = new ModuleReference
        {
            Token = tokens.Get(symbol),
            Hint = symbol.ToDisplayString()
        };
        return reference;
    }

    private NamespaceReference GetNamespaceReference(INamespaceSymbol symbol)
    {

        var reference = new NamespaceReference
        {
            Token = tokens.Get(symbol),
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
        TypeReference? reference;
        // TODO: IDynamicTypeSymbol
        // TODO: IErrorTypeSymbol
        switch (symbol)
        {
            case IArrayTypeSymbol arrayType:
                var arrayToken = tokens.Get(compilation.GetSpecialType(SpecialType.System_Array));
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
                    Token = tokens.Get(symbol.OriginalDefinition),
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
                var fnPtrToken = tokens.Get(compilation.GetSpecialType(SpecialType.System_IntPtr));
                reference = new FunctionPointerTypeReference
                {
                    Token = fnPtrToken,
                    Signature = GetMethodReference(fpType.Signature)
                };
                break;
            case IPointerTypeSymbol pointerType:
                var intPtrToken = tokens.Get(compilation.GetSpecialType(SpecialType.System_IntPtr));
                reference = new PointerTypeReference
                {
                    Token = intPtrToken,
                    PointedAtType = GetTypeReference(pointerType.PointedAtType)
                };
                break;
            case ITypeParameterSymbol typeParameter:
                reference = new TypeParameterReference
                {
                    Token = tokens.Get(typeParameter)
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
            Token = tokens.Get(symbol),
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
            Token = tokens.Get(symbol)
        };
        return reference;
    }

    private PropertyReference GetPropertyReference(IPropertySymbol symbol)
    {
        var reference = new PropertyReference
        {
            Token = tokens.Get(symbol)
        };
        return reference;
    }

    private FieldReference GetFieldReference(IFieldSymbol symbol)
    {
        var reference = new FieldReference
        {
            Token = tokens.Get(symbol)
        };
        return reference;
    }
}
