using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp;

/// <summary>
/// Transcribes metadata from Roslyn's <see cref="ISymbol"/>s of a given <see cref="Project"/> to
/// Helveg.CSharp data types.
/// </summary>
internal class RoslynSymbolTranscriber
{
    private readonly Project project;
    private readonly EntityTokenGenerator tokenGenerator;
    private readonly EntityTokenSymbolVisitor visitor;
    private Compilation compilation = null!;

    public RoslynSymbolTranscriber(
        Project project,
        EntityTokenGenerator tokenGenerator,
        EntityTokenSymbolVisitor visitor)
    {
        this.project = project;
        this.tokenGenerator = tokenGenerator;
        this.visitor = visitor;
    }

    public async Task<HelProjectCS> Transcribe(CancellationToken cancellationToken = default)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            return HelProjectCS.Invalid;
        }

        this.compilation = compilation;

        var helProject = new HelProjectCS
        {
            Token = tokenGenerator.GetToken(HelEntityKindCS.Project),
            Name = project.Name,
            FullName = project.FilePath,
        };

        VisitAssembly(this.compilation.Assembly);

        helProject = helProject with
        {
            Assembly = GetAssembly(this.compilation.Assembly) with { ContainingProject = helProject.GetReference() }
        };

        this.compilation = null!;

        return helProject;

    }

    private void VisitAssembly(IAssemblySymbol symbol)
    {
        // NB: This is to prevent stack overflows on circular assembly dependencies.
        //     Yes, there can be circular assembly references. It worries me as well.
        //     Example: System -> System.Configuration -> System.Xml -> System.
        if (visitor.VisitedAssemblies.Contains(symbol.Identity))
        {
            return;
        }

        // NB: Visit the assembly itself first, so that its name gets into VisitedAssemblies, and the stack doesn't
        //     overflow.
        visitor.VisitAssembly(symbol);

        foreach (var module in symbol.Modules)
        {
            foreach (var depedency in module.ReferencedAssemblySymbols)
            {
                VisitAssembly(depedency);
            }
        }
    }

    private HelAssemblyCS GetAssembly(IAssemblySymbol assembly)
    {
        visitor.Visit(assembly);
        var helAssembly = new HelAssemblyCS
        {
            Token = RequireSymbolToken(assembly),
            Name = assembly.Name,
            Identity = assembly.Identity.ToHelvegAssemblyId()
        };

        return helAssembly with
        {
            Modules = assembly.Modules
                .Select(m => GetModule(m, helAssembly.GetReference()))
                .ToImmutableArray()
        };
    }

    private HelModuleCS GetModule(IModuleSymbol module, HelAssemblyReferenceCS containingAssembly)
    {
        var helModule = new HelModuleCS
        {
            Token = RequireSymbolToken(module),
            ReferencedAssemblies = module.ReferencedAssemblySymbols
                .Select(GetAssemblyReference)
                .ToImmutableArray(),
            ContainingAssembly = containingAssembly
        };

        return helModule with
        {
            GlobalNamespace = GetNamespace(module.GlobalNamespace, helModule.GetReference())
        };
    }

    private HelNamespaceCS GetNamespace(INamespaceSymbol @namespace, HelModuleReferenceCS containingModule)
    {
        var helNamespace = new HelNamespaceCS
        {
            Token = RequireSymbolToken(@namespace),
            Name = @namespace.Name,
            ContainingModule = containingModule
        };

        return helNamespace with
        {
            Types = @namespace.GetTypeMembers()
                .Select(t => GetType(t, helNamespace.GetReference()))
                .ToImmutableArray(),
            Namespaces = @namespace.GetNamespaceMembers()
                .Select(n => GetNamespace(n, containingModule))
                .ToImmutableArray()
        };
    }

    private HelTypeCS GetType(INamedTypeSymbol type,
        HelNamespaceReferenceCS containingNamespace,
        HelTypeReferenceCS? containingType = null)
    {
        if (!type.IsOriginalDefinition())
        {
            throw new ArgumentException("Only the original definition of a type symbol " +
                $"can be turned into a {nameof(HelTypeCS)}.");
        }

        var helType = new HelTypeCS
        {
            Token = RequireSymbolToken(type),
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

        var reference = helType.GetReference();

        return helType with
        {
            TypeParameters = type.TypeParameters
                .Select(p => GetTypeParameter(p, reference, null, reference, containingNamespace))
                .ToImmutableArray(),
            NestedTypes = type.GetTypeMembers()
                .Select(t => GetType(t, containingNamespace, reference))
                .ToImmutableArray(),
            // TODO: Does GetMembers() contain nested types as well?
            Fields = type.GetMembers()
                .Where(m => m.Kind == SymbolKind.Field)
                .Cast<IFieldSymbol>()
                .Select(f => GetField(f, reference, containingNamespace))
                .ToImmutableArray(),
            Events = type.GetMembers()
                .Where(m => m.Kind == SymbolKind.Event)
                .Cast<IEventSymbol>()
                .Select(e => GetEvent(e, reference, containingNamespace))
                .ToImmutableArray(),
            Properties = type.GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .Select(p => GetProperty(p, reference, containingNamespace))
                .ToImmutableArray(),
            Methods = type.GetMembers()
                .Where(m => m.Kind == SymbolKind.Method)
                .Cast<IMethodSymbol>()
                .Select(m => GetMethod(m, reference, containingNamespace))
                .ToImmutableArray(),

        };
    }

    private HelTypeParameterCS GetTypeParameter(
        ITypeParameterSymbol symbol,
        HelTypeReferenceCS? declaringType,
        HelMethodReferenceCS? declaringMethod,
        HelTypeReferenceCS containingType,
        HelNamespaceReferenceCS containingNamespace)
    {
        if (declaringType is null && declaringMethod is null)
        {
            throw new ArgumentException($"Either '{nameof(declaringType)}' or '{nameof(declaringMethod)}' " +
                "must not be null.");
        }

        var helTypeParameter = new HelTypeParameterCS
        {
            Token = RequireSymbolToken(symbol),
            Name = symbol.Name,
            DeclaringType = declaringType,
            DeclaringMethod = declaringMethod,
            ContainingNamespace = containingNamespace,
            // TODO: what does Roslyn return here? we should be consistent with them
            ContainingType = containingType,
        };

        return helTypeParameter;
    }

    private HelEventCS GetEvent(
        IEventSymbol symbol,
        HelTypeReferenceCS containingType,
        HelNamespaceReferenceCS containingNamespace)
    {
        var helEvent = new HelEventCS
        {
            Token = RequireSymbolToken(symbol),
            EventType = GetTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            AddMethod = symbol.AddMethod is null ? null : GetMethodReference(symbol.AddMethod),
            RemoveMethod = symbol.RemoveMethod is null ? null : GetMethodReference(symbol.RemoveMethod),
            RaiseMethod = symbol.RaiseMethod is null ? null : GetMethodReference(symbol.RaiseMethod)
        };
        
        return PopulateMember(symbol, helEvent);
    }

    private HelFieldCS GetField(
        IFieldSymbol symbol,
        HelTypeReferenceCS containingType,
        HelNamespaceReferenceCS containingNamespace)
    {
        var helField = new HelFieldCS
        {
            Token = RequireSymbolToken(symbol),
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

    private HelPropertyCS GetProperty(
        IPropertySymbol symbol,
        HelTypeReferenceCS containingType,
        HelNamespaceReferenceCS containingNamespace)
    {
        var helProperty = new HelPropertyCS
        {
            Token = RequireSymbolToken(symbol),
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
                .Select(p => GetParameter(p, null, helProperty.GetReference()))
                .ToImmutableArray()
        };
    }

    private HelMethodCS GetMethod(
        IMethodSymbol symbol,
        HelTypeReferenceCS containingType,
        HelNamespaceReferenceCS containingNamespace)
    {
        if (!symbol.IsOriginalDefinition())
        {
            throw new ArgumentException("Only the original definition of a method " +
                $"can be turned into a {nameof(HelMethodCS)}.");
        }

        var helMethod = new HelMethodCS
        {
            Token = RequireSymbolToken(symbol),
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
                .Select(p => GetTypeParameter(p, null, helMethod.GetReference(), containingType, containingNamespace))
                .ToImmutableArray(),
            Parameters = symbol.Parameters
                .Select(p => GetParameter(p, helMethod.GetReference(), null))
                .ToImmutableArray()
        };
    }

    private HelParameterCS GetParameter(
        IParameterSymbol symbol,
        HelMethodReferenceCS? declaringMethod,
        HelPropertyReferenceCS? declaringProperty)
    {
        var helParameter = new HelParameterCS
        {
            Token = RequireSymbolToken(symbol),
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
        where TMember : HelMemberCS
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

    private HelAssemblyReferenceCS GetAssemblyReference(IAssemblySymbol symbol)
    {
        var reference = new HelAssemblyReferenceCS
        {
            Token = GetSymbolToken(symbol),
            Hint = symbol.ToDisplayString()
        };
        return reference;
    }

    private HelModuleReferenceCS GetModuleReference(IModuleSymbol symbol)
    {
        var reference = new HelModuleReferenceCS
        {
            Token = GetSymbolToken(symbol),
            Hint = symbol.ToDisplayString()
        };
        return reference;
    }

    private HelNamespaceReferenceCS GetNamespaceReference(INamespaceSymbol symbol)
    {
        if (symbol.NamespaceKind != NamespaceKind.Module)
        {
            throw new ArgumentException($"Only 'module' namespaces can be turned into a {nameof(HelNamespaceReferenceCS)}.");
        }

        var reference = new HelNamespaceReferenceCS
        {
            Token = GetSymbolToken(symbol),
            Hint = symbol.ToDisplayString()
        };
        return reference;
    }

    private HelTypeReferenceCS GetTypeReference(ITypeSymbol symbol)
    {
        HelTypeReferenceCS? reference;
        // TODO: IDynamicTypeSymbol
        // TODO: IErrorTypeSymbol
        switch (symbol)
        {
            case IArrayTypeSymbol arrayType:
                var arrayToken = RequireSymbolToken(compilation.GetSpecialType(SpecialType.System_Array));
                reference = new HelArrayTypeCS
                {
                    Token = arrayToken,
                    ElementType = GetTypeReference(arrayType.ElementType),
                    TypeKind = HelTypeKindCS.Array,
                    Sizes = arrayType.Sizes,
                    Rank = arrayType.Rank,
                };
                break;
            case INamedTypeSymbol namedType:
                reference = new HelTypeReferenceCS
                {
                    Token = GetSymbolToken(symbol),
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
                var fnPtrToken = RequireSymbolToken(compilation.GetSpecialType(SpecialType.System_IntPtr));
                reference = new HelFunctionPointerTypeCS
                {
                    Token = fnPtrToken,
                    Signature = GetMethodReference(fpType.Signature)
                };
                break;
            case IPointerTypeSymbol pointerType:
                var intPtrToken = RequireSymbolToken(compilation.GetSpecialType(SpecialType.System_IntPtr));
                reference = new HelPointerTypeCS
                {
                    Token = intPtrToken,
                    PointedAtType = GetTypeReference(pointerType.PointedAtType)
                };
                break;
            case ITypeParameterSymbol typeParameter:
                reference = new HelTypeParameterReferenceCS
                {
                    Token = GetSymbolToken(typeParameter)
                };
                break;

            default:
                reference = HelTypeReferenceCS.Invalid;
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

    private HelMethodReferenceCS GetMethodReference(IMethodSymbol symbol)
    {
        var isOriginal = symbol.IsOriginalDefinition();

        var reference = new HelMethodReferenceCS
        {
            Token = GetSymbolToken(symbol),
            TypeArguments = isOriginal
                ? ImmutableArray<HelTypeReferenceCS>.Empty
                : symbol.TypeArguments
                    .Select(GetTypeReference)
                    .ToImmutableArray(),
        };

        return reference;
    }

    private HelEventReferenceCS GetEventReference(IEventSymbol symbol)
    {
        var reference = new HelEventReferenceCS
        {
            Token = GetSymbolToken(symbol)
        };
        return reference;
    }

    private HelPropertyReferenceCS GetPropertyReference(IPropertySymbol symbol)
    {
        var reference = new HelPropertyReferenceCS
        {
            Token = GetSymbolToken(symbol)
        };
        return reference;
    }

    private HelFieldReferenceCS GetFieldReference(IFieldSymbol symbol)
    {
        var reference = new HelFieldReferenceCS
        {
            Token = GetSymbolToken(symbol)
        };
        return reference;
    }

    private HelEntityTokenCS GetSymbolToken(ISymbol symbol)
    {
        return visitor.Tokens.TryGetValue(symbol, out var token)
            ? token
            : HelEntityTokenCS.CreateError(symbol.GetEntityKind());
    }

    private HelEntityTokenCS RequireSymbolToken(ISymbol symbol)
    {
        return visitor.Tokens.TryGetValue(symbol, out var token)
            ? token
            : throw new InvalidOperationException($"Symbol '{symbol}' does not have a token even though it is " +
                $"required. This could be a bug in {nameof(EntityTokenSymbolVisitor)}.");
    }
}
