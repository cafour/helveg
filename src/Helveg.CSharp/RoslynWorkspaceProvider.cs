using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Helveg.CSharp;

public class RoslynWorkspaceProvider : IHelWorkspaceCSProvider
{
    private int counter = 0;

    private readonly ConditionalWeakTable<IHelEntityCS, ISymbol> symbolTable = new();

    // TODO: make this a HashSet to prevent multiple resolutions of the same reference
    private readonly Stack<HelReferenceCS> unresolvedReferences = new();

    public async Task<HelWorkspaceCS> GetWorkspace(string path, CancellationToken cancellationToken = default)
    {
        var workspace = MSBuildWorkspace.Create();
        await workspace.OpenSolutionAsync(path, cancellationToken: cancellationToken);

        return new HelWorkspaceCS
        {
            Solution = await GetSolution(workspace.CurrentSolution, cancellationToken)
        };
    }

    private HelEntityTokenCS GetToken(HelEntityKindCS kind)
    {
        return new HelEntityTokenCS(kind, ++counter);
    }

    private async Task<HelSolutionCS> GetSolution(Solution solution, CancellationToken cancellationToken = default)
    {
        var helSolution = new HelSolutionCS
        {
            DefinitionToken = GetToken(HelEntityKindCS.Solution),
            Name = solution.FilePath ?? IHelEntityCS.InvalidName,
            FullName = solution.FilePath
        };

        var projects = (await Task.WhenAll(solution.Projects
            .Select(p => GetProject(p, cancellationToken))))
            .Select(p => p with { ContainingSolution = helSolution.Reference })
            .ToImmutableArray();

        return helSolution with { Projects = projects };
    }

    private async Task<HelProjectCS> GetProject(Project project, CancellationToken cancellationToken = default)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            return HelProjectCS.Invalid;
        }

        var helProject = new HelProjectCS
        {
            DefinitionToken = GetToken(HelEntityKindCS.Project),
            Name = project.Name,
            FullName = project.FilePath,
        };

        return helProject with
        {
            Assembly = GetAssembly(compilation.Assembly) with { ContainingProject = helProject.Reference }
        };
    }

    private HelAssemblyCS GetAssembly(IAssemblySymbol assembly)
    {
        var helAssembly = new HelAssemblyCS
        {
            DefinitionToken = GetToken(HelEntityKindCS.Assembly),
            Name = assembly.Name,
            Identity = GetAssemblyId(assembly.Identity)
        };

        return helAssembly with
        {
            Modules = assembly.Modules
                .Select(m => GetModule(m, helAssembly.Reference))
                .ToImmutableArray()
        };
    }

    private HelAssemblyIdCS GetAssemblyId(AssemblyIdentity assemblyIdentity)
    {
        return new HelAssemblyIdCS
        {
            Name = assemblyIdentity.Name,
            Version = assemblyIdentity.Version,
            CultureName = assemblyIdentity.CultureName,
            PublicKeyToken = string.Concat(assemblyIdentity.PublicKeyToken.Select(b => b.ToString("x")))
        };
    }

    private HelModuleCS GetModule(IModuleSymbol module, HelAssemblyReferenceCS containingAssembly)
    {
        var helModule = new HelModuleCS
        {
            DefinitionToken = GetToken(HelEntityKindCS.Module),
            ReferencedAssemblies = module.ReferencedAssemblySymbols
                .Select(GetUnresolvedAssemblyReference)
                .ToImmutableArray(),
            ContainingAssembly = containingAssembly
        };
        return helModule with
        {
            GlobalNamespace = GetNamespace(module.GlobalNamespace, helModule.Reference)
        };
    }

    private HelNamespaceCS GetNamespace(INamespaceSymbol @namespace, HelModuleReferenceCS containingModule)
    {
        var helNamespace = new HelNamespaceCS
        {
            DefinitionToken = GetToken(HelEntityKindCS.Namespace),
            Name = @namespace.Name,
            ContainingModule = containingModule
        };

        return helNamespace with
        {
            Types = @namespace.GetTypeMembers()
                .Select(t => GetType(t, helNamespace))
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
            DefinitionToken = GetToken(HelEntityKindCS.Type),
            ContainingNamespace = containingNamespace,
            ContainingType = containingType,
            TypeKind = GetTypeKind(type.TypeKind),
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
            BaseType = type.BaseType is null ? null : GetUnresolvedTypeReference(type.BaseType),
            Interfaces = type.Interfaces.Select(GetUnresolvedTypeReference).ToImmutableArray(),
        };

        helType = PopulateMember<HelTypeCS, HelTypeReferenceCS>(type, helType);

        return helType with
        {
            TypeParameters = type.TypeParameters
                .Select(p => GetTypeParameter(p, helType, null))
                .ToImmutableArray(),
            NestedTypes = type.GetTypeMembers()
                .Select(t => GetType(t, containingNamespace))
                .ToImmutableArray(),
            // TODO: Does GetMembers() contain nested types as well?
            Fields = type.GetMembers()
                .Where(m => m.Kind == SymbolKind.Field)
                .Cast<IFieldSymbol>()
                .Select(f => GetField(f, helType.Reference))
                .ToImmutableArray(),
            Events = type.GetMembers()
                .Where(m => m.Kind == SymbolKind.Event)
                .Cast<IEventSymbol>()
                .Select(e => GetEvent(e, helType.Reference))
                .ToImmutableArray(),
            Properties = type.GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .Select(p => GetProperty(p, helType.Reference))
                .ToImmutableArray(),
            Methods = type.GetMembers()
                .Where(m => m.Kind == SymbolKind.Method)
                .Cast<IMethodSymbol>()
                .Select(m => GetMethod(m, helType.Reference))
                .ToImmutableArray(),

        };
    }

    private HelTypeParameterReferenceCS GetTypeParameter(
        ITypeParameterSymbol symbol,
        HelTypeReferenceCS? declaringType,
        HelMethodReferenceCS? declaringMethod)
    {
        if (declaringType is null && declaringMethod is null)
        {
            throw new ArgumentException($"Either '{nameof(declaringType)}' or '{nameof(declaringMethod)}' " +
                "must not be null.");
        }

        return new HelTypeParameterReferenceCS
        {
            DefinitionToken = GetToken(HelEntityKindCS.Type),
            Name = symbol.Name,
            Ordinal = symbol.Ordinal,
            DeclaringType = declaringType,
            DeclaringMethod = declaringMethod,
            ContainingNamespace = (declaringType?.ContainingNamespace ?? declaringMethod?.ContainingNamespace)!,
            // TODO: what does Roslyn return here? we should be consistent with them
            ContainingType = (declaringType ?? declaringMethod?.ContainingType)!,
        };
    }

    private HelEventCS GetEvent(
        IEventSymbol symbol,
        HelTypeReferenceCS containingType)
    {
        var helEvent = new HelEventCS
        {
            DefinitionToken = GetToken(HelEntityKindCS.Event),
            EventType = GetUnresolvedTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingType.ContainingNamespace,
            AddMethod = symbol.AddMethod is null ? null : GetUnresolvedMethodReference(symbol.AddMethod),
            RemoveMethod = symbol.RemoveMethod is null ? null : GetUnresolvedMethodReference(symbol.RemoveMethod),
            RaiseMethod = symbol.RaiseMethod is null ? null : GetUnresolvedMethodReference(symbol.RaiseMethod)
        };

        return PopulateMember<HelEventCS, HelEventReferenceCS>(symbol, helEvent);
    }

    private HelFieldCS GetField(
        IFieldSymbol symbol,
        HelTypeReferenceCS containingType)
    {
        var helField = new HelFieldCS
        {
            DefinitionToken = GetToken(HelEntityKindCS.Field),
            FieldType = GetUnresolvedTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingType.ContainingNamespace,
            AssociatedEvent = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IEventSymbol e
                ? GetUnresolvedEventReference(e)
                : null,
            AssociatedProperty = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IPropertySymbol p
                ? GetUnresolvedPropertyReference(p)
                : null,
            IsVolatile = symbol.IsVolatile,
            IsReadOnly = symbol.IsReadOnly,
            IsRequired = symbol.IsRequired,
            IsConst = symbol.IsConst,
            RefKind = GetRefKind(symbol.RefKind)
        };

        return PopulateMember<HelFieldCS, HelFieldReferenceCS>(symbol, helField);
    }

    private HelPropertyCS GetProperty(
        IPropertySymbol symbol,
        HelTypeReferenceCS containingType)
    {
        var helProperty = new HelPropertyCS
        {
            DefinitionToken = GetToken(HelEntityKindCS.Property),
            PropertyType = GetUnresolvedTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingType.ContainingNamespace,
            GetMethod = symbol.GetMethod is null ? null : GetUnresolvedMethodReference(symbol.GetMethod),
            SetMethod = symbol.SetMethod is null ? null : GetUnresolvedMethodReference(symbol.SetMethod),
            IsIndexer = symbol.IsIndexer,
            IsRequired = symbol.IsRequired,
            OverriddenProperty = symbol.OverriddenProperty is null
                ? null
                : GetUnresolvedPropertyReference(symbol.OverriddenProperty),
            RefKind = GetRefKind(symbol.RefKind)
        };

        helProperty = PopulateMember<HelPropertyCS, HelPropertyReferenceCS>(symbol, helProperty);

        return helProperty with
        {
            Parameters = symbol.Parameters
                .Select(p => GetParameter(p, null, helProperty.Reference))
                .ToImmutableArray()
        };
    }

    private HelMethodCS GetMethod(
        IMethodSymbol symbol,
        HelTypeReferenceCS containingType)
    {
        if (!symbol.IsOriginalDefinition())
        {
            throw new ArgumentException("Only the original definition of a method " +
                $"can be turned into a {nameof(HelMethodCS)}.");
        }

        var helMethod = new HelMethodCS
        {
            DefinitionToken = GetToken(HelEntityKindCS.Method),
            AssociatedEvent = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IEventSymbol e
                ? GetUnresolvedEventReference(e)
                : null,
            AssociatedProperty = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IPropertySymbol p
                ? GetUnresolvedPropertyReference(p)
                : null,
            ContainingType = containingType,
            ContainingNamespace = containingType.ContainingNamespace,
            IsAsync = symbol.IsAsync,
            IsExtensionMethod = symbol.IsExtensionMethod,
            IsInitOnly = symbol.IsInitOnly,
            IsReadOnly = symbol.IsReadOnly,
            MethodKind = GetMethodKind(symbol.MethodKind),
            OverridenMethod = symbol.OverriddenMethod is null ? null : GetUnresolvedMethodReference(symbol.OverriddenMethod),
            ReceiverType = symbol.ReceiverType is null ? null : GetUnresolvedTypeReference(symbol.ReceiverType),
            RefKind = GetRefKind(symbol.RefKind),
            ReturnType = GetUnresolvedTypeReference(symbol.ReturnType)
        };

        helMethod = PopulateMember<HelMethodCS, HelMethodReferenceCS>(symbol, helMethod);

        return helMethod with
        {
            ExplicitInterfaceImplementations = symbol.ExplicitInterfaceImplementations
                .Select(GetUnresolvedMethodReference)
                .ToImmutableArray(),
            TypeParameters = symbol.TypeParameters
                .Select(p => GetTypeParameter(p, null, helMethod.Reference))
                .ToImmutableArray(),
            Parameters = symbol.Parameters
                .Select(p => GetParameter(p, helMethod.Reference, null))
                .ToImmutableArray()
        };
    }

    private HelParameterCS GetParameter(
        IParameterSymbol symbol,
        HelMethodReferenceCS? declaringMethod,
        HelPropertyReferenceCS? declaringProperty)
    {
        return new HelParameterCS
        {
            DefinitionToken = new HelEntityTokenCS(HelEntityKindCS.Parameter, ++counter),
            Name = symbol.Name,
            Ordinal = symbol.Ordinal,
            DeclaringMethod = declaringMethod,
            DeclaringProperty = declaringProperty,
            HasExplicitDefaultValue = symbol.HasExplicitDefaultValue,
            IsDiscard = symbol.IsDiscard,
            IsOptional = symbol.IsOptional,
            IsParams = symbol.IsParams,
            IsThis = symbol.IsThis,
            ParameterType = GetUnresolvedTypeReference(symbol.Type),
            RefKind = GetRefKind(symbol.RefKind)
        };
    }

    private TMember PopulateMember<TMember, TMemberReference>(ISymbol symbol, TMember helMember)
        where TMemberReference : HelMemberReferenceCS
        where TMember : HelMemberCS<TMemberReference>
    {
        return helMember with
        {
            Name = symbol.Name,
            Accessibility = GetAccessibility(symbol.DeclaredAccessibility),
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

    private HelAssemblyReferenceCS GetUnresolvedAssemblyReference(IAssemblySymbol symbol)
    {
        var reference = new HelAssemblyReferenceCS
        {
            DefinitionToken = HelEntityTokenCS.GetUnresolved(HelEntityKindCS.Assembly),
            Identity = GetAssemblyId(symbol.Identity),
            Name = symbol.Name
        };
        unresolvedReferences.Push(reference);
        return reference;
    }

    private HelModuleReferenceCS GetUnresolvedModuleReference(IModuleSymbol symbol)
    {
        var reference = new HelModuleReferenceCS
        {
            DefinitionToken = HelEntityTokenCS.GetUnresolved(HelEntityKindCS.Module),
            Name = symbol.Name,
            ContainingAssembly = GetUnresolvedAssemblyReference(symbol.ContainingAssembly)
        };
        unresolvedReferences.Push(reference);
        return reference;
    }

    private HelNamespaceReferenceCS GetUnresolvedNamespaceReference(INamespaceSymbol symbol)
    {
        if (symbol.NamespaceKind != NamespaceKind.Module)
        {
            throw new ArgumentException($"Only 'module' namespaces can be turned into a {nameof(HelNamespaceReferenceCS)}.");
        }

        var reference = new HelNamespaceReferenceCS
        {
            DefinitionToken = HelEntityTokenCS.GetUnresolved(HelEntityKindCS.Namespace),
            Name = symbol.Name,
            ContainingModule = GetUnresolvedModuleReference(symbol.ContainingModule)
        };
        unresolvedReferences.Push(reference);
        return reference;
    }

    private HelTypeReferenceCS GetUnresolvedTypeReference(ITypeSymbol symbol)
    {
        HelTypeReferenceCS? reference;
        // TODO: IDynamicTypeSymbol
        // TODO: IErrorTypeSymbol
        switch (symbol)
        {
            case IArrayTypeSymbol arrayType:
                reference = new HelArrayTypeCS
                {
                    ElementType = GetUnresolvedTypeReference(arrayType.ElementType),
                    TypeKind = HelTypeKindCS.Array,
                    Sizes = arrayType.Sizes,
                    Rank = arrayType.Rank,
                };
                break;
            case INamedTypeSymbol namedType:
                if (namedType.IsOriginalDefinition())
                {
                    reference = new HelTypeReferenceCS();
                }
                else
                {
                    reference = new HelConstructedTypeCS
                    {
                        ConstructedFrom = GetUnresolvedTypeReference(namedType.ConstructedFrom),
                        TypeArguments = namedType.TypeArguments
                            .Select(GetUnresolvedTypeReference)
                            .ToImmutableArray()
                    };
                }
                reference = reference with
                {
                    Arity = namedType.TypeParameters.Length
                };
                break;
            case IFunctionPointerTypeSymbol fpType:
                reference = new HelFunctionPointerTypeCS
                {
                    Signature = GetUnresolvedMethodReference(fpType.Signature)
                };
                break;
            case IPointerTypeSymbol pointerType:
                reference = new HelPointerTypeCS
                {
                    PointedAtType = GetUnresolvedTypeReference(pointerType.PointedAtType)
                };
                break;
            case ITypeParameterSymbol typeParameter:
                reference = new HelTypeParameterReferenceCS
                {
                    Ordinal = typeParameter.Ordinal,
                    // TODO: Fix the reference cycle of Method->Parameter (of type TypeParameter)->DeclaringMethod
                    //DeclaringMethod = typeParameter.DeclaringMethod is null
                    //    ? null
                    //    : GetUnresolvedMethodReference(typeParameter.DeclaringMethod),
                    DeclaringType = typeParameter.DeclaringType is null
                        ? null
                        : GetUnresolvedTypeReference(typeParameter.DeclaringType),
                };
                break;

            default:
                reference = HelTypeReferenceCS.Invalid;
                break;
        }
        reference = reference with
        {
            DefinitionToken = HelEntityTokenCS.GetUnresolved(HelEntityKindCS.Type),
            Name = symbol.Name,
            TypeKind = GetTypeKind(symbol.TypeKind),
            ContainingType = symbol.ContainingType is null
                ? null
                : GetUnresolvedTypeReference(symbol.ContainingType),
            ContainingNamespace = symbol.ContainingNamespace is null
                ? null
                : GetUnresolvedNamespaceReference(symbol.ContainingNamespace),
            Nullability = GetNullability(symbol.NullableAnnotation)
        };
        unresolvedReferences.Push(reference);
        return reference;
    }

    private HelMethodReferenceCS GetUnresolvedMethodReference(IMethodSymbol symbol)
    {
        var isOriginal = symbol.IsOriginalDefinition();

        var reference = new HelMethodReferenceCS
        {
            DefinitionToken = HelEntityTokenCS.GetUnresolved(HelEntityKindCS.Method),
            Name = symbol.Name,
            Arity = symbol.TypeParameters.Length,
            TypeArguments = isOriginal
                ? ImmutableArray<HelTypeReferenceCS>.Empty
                : symbol.TypeArguments
                    .Select(GetUnresolvedTypeReference)
                    .ToImmutableArray(),
            ConstructedFrom = isOriginal
                ? null
                : GetUnresolvedMethodReference(symbol.OriginalDefinition),
            ContainingNamespace = GetUnresolvedNamespaceReference(symbol.ContainingNamespace),
            ContainingType = GetUnresolvedTypeReference(symbol.ContainingType),
            ParameterTypes = symbol.Parameters
                .Select(p => GetUnresolvedTypeReference(p.Type))
                .ToImmutableArray()
        };
        unresolvedReferences.Push(reference);
        return reference;
    }

    private HelEventReferenceCS GetUnresolvedEventReference(IEventSymbol symbol)
    {
        var reference = new HelEventReferenceCS
        {
            DefinitionToken = HelEntityTokenCS.GetUnresolved(HelEntityKindCS.Event),
            Name = symbol.Name,
            ContainingNamespace = GetUnresolvedNamespaceReference(symbol.ContainingNamespace),
            ContainingType = GetUnresolvedTypeReference(symbol.ContainingType)
        };
        unresolvedReferences.Push(reference);
        return reference;
    }

    private HelPropertyReferenceCS GetUnresolvedPropertyReference(IPropertySymbol symbol)
    {
        var reference = new HelPropertyReferenceCS
        {
            DefinitionToken = HelEntityTokenCS.GetUnresolved(HelEntityKindCS.Property),
            Name = symbol.Name,
            ContainingNamespace = GetUnresolvedNamespaceReference(symbol.ContainingNamespace),
            ContainingType = GetUnresolvedTypeReference(symbol.ContainingType)
        };
        unresolvedReferences.Push(reference);
        return reference;
    }

    private HelFieldReferenceCS GetUnresolvedFieldReference(IFieldSymbol symbol)
    {
        var reference = new HelFieldReferenceCS
        {
            DefinitionToken = HelEntityTokenCS.GetUnresolved(HelEntityKindCS.Field),
            Name = symbol.Name,
            ContainingNamespace = GetUnresolvedNamespaceReference(symbol.ContainingNamespace),
            ContainingType = GetUnresolvedTypeReference(symbol.ContainingType)
        };
        unresolvedReferences.Push(reference);
        return reference;
    }

    private static HelAccessibilityCS GetAccessibility(Accessibility value)
    {
        return value switch
        {
            Accessibility.Private => HelAccessibilityCS.Private,
            Accessibility.ProtectedAndInternal => HelAccessibilityCS.ProtectedAndInternal,
            Accessibility.Protected => HelAccessibilityCS.Protected,
            Accessibility.Internal => HelAccessibilityCS.Internal,
            Accessibility.ProtectedOrInternal => HelAccessibilityCS.ProtectedOrInternal,
            Accessibility.Public => HelAccessibilityCS.Public,
            _ => HelAccessibilityCS.Invalid
        };
    }

    private static HelTypeKindCS GetTypeKind(TypeKind value)
    {
        return value switch
        {
            TypeKind.Unknown => HelTypeKindCS.Unknown,
            TypeKind.Array => HelTypeKindCS.Array,
            TypeKind.Class => HelTypeKindCS.Class,
            TypeKind.Delegate => HelTypeKindCS.Delegate,
            TypeKind.Dynamic => HelTypeKindCS.Dynamic,
            TypeKind.Enum => HelTypeKindCS.Enum,
            TypeKind.Error => HelTypeKindCS.Error,
            TypeKind.Interface => HelTypeKindCS.Interface,
            TypeKind.Module => HelTypeKindCS.Module,
            TypeKind.Pointer => HelTypeKindCS.Pointer,
            TypeKind.Struct => HelTypeKindCS.Struct,
            TypeKind.TypeParameter => HelTypeKindCS.TypeParameter,
            TypeKind.Submission => HelTypeKindCS.Submission,
            TypeKind.FunctionPointer => HelTypeKindCS.FunctionPointer,
            _ => HelTypeKindCS.Unknown
        };
    }

    private static HelRefKindCS GetRefKind(RefKind value)
    {
        return value switch
        {
            RefKind.None => HelRefKindCS.None,
            RefKind.Ref => HelRefKindCS.Ref,
            RefKind.Out => HelRefKindCS.Out,
            RefKind.In => HelRefKindCS.In,
            _ => HelRefKindCS.None
        };
    }

    private static HelMethodKindCS GetMethodKind(MethodKind value)
    {
        return value switch
        {
            MethodKind.AnonymousFunction => HelMethodKindCS.AnonymousFunction,
            MethodKind.Constructor => HelMethodKindCS.Constructor,
            MethodKind.Conversion => HelMethodKindCS.Conversion,
            MethodKind.DelegateInvoke => HelMethodKindCS.DelegateInvoke,
            MethodKind.Destructor => HelMethodKindCS.Destructor,
            MethodKind.EventAdd => HelMethodKindCS.EventAdd,
            MethodKind.EventRaise => HelMethodKindCS.EventRaise,
            MethodKind.EventRemove => HelMethodKindCS.EventRemove,
            MethodKind.ExplicitInterfaceImplementation => HelMethodKindCS.ExplicitInterfaceImplementation,
            MethodKind.UserDefinedOperator => HelMethodKindCS.UserDefinedOperator,
            MethodKind.Ordinary => HelMethodKindCS.Ordinary,
            MethodKind.PropertyGet => HelMethodKindCS.PropertyGet,
            MethodKind.PropertySet => HelMethodKindCS.PropertySet,
            MethodKind.ReducedExtension => HelMethodKindCS.ReducedExtension,
            MethodKind.StaticConstructor => HelMethodKindCS.StaticConstructor,
            MethodKind.BuiltinOperator => HelMethodKindCS.BuiltinOperator,
            MethodKind.DeclareMethod => HelMethodKindCS.DeclareMethod,
            MethodKind.LocalFunction => HelMethodKindCS.LocalFunction,
            MethodKind.FunctionPointerSignature => HelMethodKindCS.FunctionPointerSignature,
            _ => HelMethodKindCS.Invalid
        };
    }

    private static HelNullabilityCS GetNullability(NullableAnnotation value)
    {
        return value switch
        {
            NullableAnnotation.None => HelNullabilityCS.None,
            NullableAnnotation.NotAnnotated => HelNullabilityCS.NotAnnotated,
            NullableAnnotation.Annotated => HelNullabilityCS.Annotated,
            _ => HelNullabilityCS.None
        };
    }
}
