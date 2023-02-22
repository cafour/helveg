using System;
using System.Collections.Concurrent;
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
    private int errorCounter = 0;

    private readonly ConcurrentDictionary<HelEntityTokenCS, ISymbol> underlyingSymbols = new();
    private readonly ConcurrentDictionary<HelEntityTokenCS, ISymbol> errorReferences = new();
    private readonly EntityTokenGenerator tokenGenerator = new();
    private readonly EntityTokenSymbolVisitor visitor;

    public RoslynWorkspaceProvider()
    {
        visitor = new(tokenGenerator);
    }

    public async Task<HelWorkspaceCS> GetWorkspace(string path, CancellationToken cancellationToken = default)
    {
        var workspace = MSBuildWorkspace.Create();
        await workspace.OpenSolutionAsync(path, cancellationToken: cancellationToken);

        return new HelWorkspaceCS
        {
            Solution = await GetSolution(workspace.CurrentSolution, cancellationToken)
        };
    }

    private HelEntityTokenCS GetResolvedToken(HelEntityKindCS kind)
    {
        return new HelEntityTokenCS(kind, Interlocked.Increment(ref counter));
    }

    private HelEntityTokenCS GetErrorToken(HelEntityKindCS kind)
    {
        return new HelEntityTokenCS(kind, Interlocked.Increment(ref errorCounter));
    }

    private async Task<HelSolutionCS> GetSolution(Solution solution, CancellationToken cancellationToken = default)
    {
        var helSolution = new HelSolutionCS
        {
            Token = tokenGenerator.GetToken(HelEntityKindCS.Solution),
            Name = solution.FilePath ?? IHelEntityCS.InvalidName,
            FullName = solution.FilePath
        };

        var projects = (await Task.WhenAll(solution.Projects
            .Select(p => GetProject(p, cancellationToken))))
            .Select(p => p with { ContainingSolution = helSolution.GetReference() })
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
            Token = tokenGenerator.GetToken(HelEntityKindCS.Project),
            Name = project.Name,
            FullName = project.FilePath,
        };

        return helProject with
        {
            Assembly = GetAssembly(compilation.Assembly) with { ContainingProject = helProject.GetReference() }
        };
    }

    private HelAssemblyCS GetAssembly(IAssemblySymbol assembly)
    {
        visitor.Visit(assembly);
        var helAssembly = new HelAssemblyCS
        {
            Token = GetResolvedToken(HelEntityKindCS.Assembly),
            Name = assembly.Name,
            Identity = assembly.Identity.ToHelvegAssemblyId()
        };
        underlyingSymbols.AddOrUpdate(helAssembly.Token, assembly, (_, _) => assembly);

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
            Token = GetResolvedToken(HelEntityKindCS.Module),
            ReferencedAssemblies = module.ReferencedAssemblySymbols
                .Select(GetErrorAssemblyReference)
                .ToImmutableArray(),
            ContainingAssembly = containingAssembly
        };
        underlyingSymbols.AddOrUpdate(helModule.Token, module, (_, _) => module);

        return helModule with
        {
            GlobalNamespace = GetNamespace(module.GlobalNamespace, helModule.GetReference())
        };
    }

    private HelNamespaceCS GetNamespace(INamespaceSymbol @namespace, HelModuleReferenceCS containingModule)
    {
        var helNamespace = new HelNamespaceCS
        {
            Token = GetResolvedToken(HelEntityKindCS.Namespace),
            Name = @namespace.Name,
            ContainingModule = containingModule
        };
        underlyingSymbols.AddOrUpdate(helNamespace.Token, @namespace, (_, _) => @namespace);

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
            Token = GetResolvedToken(HelEntityKindCS.Type),
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
            BaseType = type.BaseType is null ? null : GetErrorTypeReference(type.BaseType),
            Interfaces = type.Interfaces.Select(GetErrorTypeReference).ToImmutableArray(),
        };

        underlyingSymbols.AddOrUpdate(helType.Token, type, (_, _) => type);

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
            Token = GetResolvedToken(HelEntityKindCS.TypeParameter),
            Name = symbol.Name,
            DeclaringType = declaringType,
            DeclaringMethod = declaringMethod,
            ContainingNamespace = containingNamespace,
            // TODO: what does Roslyn return here? we should be consistent with them
            ContainingType = containingType,
        };
        underlyingSymbols.AddOrUpdate(helTypeParameter.Token, symbol, (_, _) => symbol);

        return helTypeParameter;
    }

    private HelEventCS GetEvent(
        IEventSymbol symbol,
        HelTypeReferenceCS containingType,
        HelNamespaceReferenceCS containingNamespace)
    {
        var helEvent = new HelEventCS
        {
            Token = GetResolvedToken(HelEntityKindCS.Event),
            EventType = GetErrorTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            AddMethod = symbol.AddMethod is null ? null : GetErrorMethodReference(symbol.AddMethod),
            RemoveMethod = symbol.RemoveMethod is null ? null : GetErrorMethodReference(symbol.RemoveMethod),
            RaiseMethod = symbol.RaiseMethod is null ? null : GetErrorMethodReference(symbol.RaiseMethod)
        };
        underlyingSymbols.AddOrUpdate(helEvent.Token, symbol, (_, _) => symbol);

        return PopulateMember(symbol, helEvent);
    }

    private HelFieldCS GetField(
        IFieldSymbol symbol,
        HelTypeReferenceCS containingType,
        HelNamespaceReferenceCS containingNamespace)
    {
        var helField = new HelFieldCS
        {
            Token = GetResolvedToken(HelEntityKindCS.Field),
            FieldType = GetErrorTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            AssociatedEvent = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IEventSymbol e
                ? GetErrorEventReference(e)
                : null,
            AssociatedProperty = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IPropertySymbol p
                ? GetErrorPropertyReference(p)
                : null,
            IsVolatile = symbol.IsVolatile,
            IsReadOnly = symbol.IsReadOnly,
            IsRequired = symbol.IsRequired,
            IsConst = symbol.IsConst,
            RefKind = symbol.RefKind.ToHelvegRefKind()
        };
        underlyingSymbols.AddOrUpdate(helField.Token, symbol, (_, _) => symbol);

        return PopulateMember(symbol, helField);
    }

    private HelPropertyCS GetProperty(
        IPropertySymbol symbol,
        HelTypeReferenceCS containingType,
        HelNamespaceReferenceCS containingNamespace)
    {
        var helProperty = new HelPropertyCS
        {
            Token = GetResolvedToken(HelEntityKindCS.Property),
            PropertyType = GetErrorTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            GetMethod = symbol.GetMethod is null ? null : GetErrorMethodReference(symbol.GetMethod),
            SetMethod = symbol.SetMethod is null ? null : GetErrorMethodReference(symbol.SetMethod),
            IsIndexer = symbol.IsIndexer,
            IsRequired = symbol.IsRequired,
            OverriddenProperty = symbol.OverriddenProperty is null
                ? null
                : GetErrorPropertyReference(symbol.OverriddenProperty),
            RefKind = symbol.RefKind.ToHelvegRefKind()
        };

        underlyingSymbols.AddOrUpdate(helProperty.Token, symbol, (_, _) => symbol);

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
            Token = GetResolvedToken(HelEntityKindCS.Method),
            AssociatedEvent = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IEventSymbol e
                ? GetErrorEventReference(e)
                : null,
            AssociatedProperty = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IPropertySymbol p
                ? GetErrorPropertyReference(p)
                : null,
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            IsAsync = symbol.IsAsync,
            IsExtensionMethod = symbol.IsExtensionMethod,
            IsInitOnly = symbol.IsInitOnly,
            IsReadOnly = symbol.IsReadOnly,
            MethodKind = symbol.MethodKind.ToHelvegMethodKind(),
            OverridenMethod = symbol.OverriddenMethod is null ? null : GetErrorMethodReference(symbol.OverriddenMethod),
            ReceiverType = symbol.ReceiverType is null ? null : GetErrorTypeReference(symbol.ReceiverType),
            RefKind = symbol.RefKind.ToHelvegRefKind(),
            ReturnType = GetErrorTypeReference(symbol.ReturnType)
        };
        underlyingSymbols.AddOrUpdate(helMethod.Token, symbol, (_, _) => symbol);

        helMethod = PopulateMember(symbol, helMethod);

        return helMethod with
        {
            ExplicitInterfaceImplementations = symbol.ExplicitInterfaceImplementations
                .Select(GetErrorMethodReference)
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
            Token = new HelEntityTokenCS(HelEntityKindCS.Parameter, ++counter),
            Name = symbol.Name,
            Ordinal = symbol.Ordinal,
            DeclaringMethod = declaringMethod,
            DeclaringProperty = declaringProperty,
            HasExplicitDefaultValue = symbol.HasExplicitDefaultValue,
            IsDiscard = symbol.IsDiscard,
            IsOptional = symbol.IsOptional,
            IsParams = symbol.IsParams,
            IsThis = symbol.IsThis,
            ParameterType = GetErrorTypeReference(symbol.Type),
            RefKind = symbol.RefKind.ToHelvegRefKind()
        };
        underlyingSymbols.AddOrUpdate(helParameter.Token, symbol, (_, _) => symbol);

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

    private HelAssemblyReferenceCS GetErrorAssemblyReference(IAssemblySymbol symbol)
    {
        var reference = new HelAssemblyReferenceCS
        {
            Token = GetErrorToken(HelEntityKindCS.Assembly),
            IsError = true
        };
        errorReferences.TryAdd(reference.Token, symbol);
        return reference;
    }

    private HelModuleReferenceCS GetErrorModuleReference(IModuleSymbol symbol)
    {
        var reference = new HelModuleReferenceCS
        {
            Token = GetErrorToken(HelEntityKindCS.Module),
            IsError = true
        };
        errorReferences.TryAdd(reference.Token, symbol);
        return reference;
    }

    private HelNamespaceReferenceCS GetErrorNamespaceReference(INamespaceSymbol symbol)
    {
        if (symbol.NamespaceKind != NamespaceKind.Module)
        {
            throw new ArgumentException($"Only 'module' namespaces can be turned into a {nameof(HelNamespaceReferenceCS)}.");
        }

        var reference = new HelNamespaceReferenceCS
        {
            Token = GetErrorToken(HelEntityKindCS.Namespace),
            IsError = true
        };
        errorReferences.TryAdd(reference.Token, symbol);
        return reference;
    }

    private HelTypeReferenceCS GetErrorTypeReference(ITypeSymbol symbol)
    {
        HelTypeReferenceCS? reference;
        // TODO: IDynamicTypeSymbol
        // TODO: IErrorTypeSymbol
        switch (symbol)
        {
            case IArrayTypeSymbol arrayType:
                reference = new HelArrayTypeCS
                {
                    ElementType = GetErrorTypeReference(arrayType.ElementType),
                    TypeKind = HelTypeKindCS.Array,
                    Sizes = arrayType.Sizes,
                    Rank = arrayType.Rank,
                };
                break;
            case INamedTypeSymbol namedType:
                reference = new HelTypeReferenceCS();
                if (!namedType.IsOriginalDefinition())
                {
                    reference = reference with
                    {
                        TypeArguments = namedType.TypeArguments
                            .Select(GetErrorTypeReference)
                            .ToImmutableArray()
                    };
                }
                break;
            case IFunctionPointerTypeSymbol fpType:
                reference = new HelFunctionPointerTypeCS
                {
                    Signature = GetErrorMethodReference(fpType.Signature)
                };
                break;
            case IPointerTypeSymbol pointerType:
                reference = new HelPointerTypeCS
                {
                    PointedAtType = GetErrorTypeReference(pointerType.PointedAtType)
                };
                break;
            case ITypeParameterSymbol typeParameter:
                reference = new HelTypeParameterReferenceCS();
                break;

            default:
                reference = HelTypeReferenceCS.Invalid;
                break;
        }
        reference = reference with
        {
            Token = GetErrorToken(HelEntityKindCS.Type),
            IsError = true,
            TypeKind = symbol.TypeKind.ToHelvegTypeKind(),
            Nullability = symbol.NullableAnnotation.ToHelvegNullability()
        };
        errorReferences.TryAdd(reference.Token, symbol);
        return reference;
    }

    private HelMethodReferenceCS GetErrorMethodReference(IMethodSymbol symbol)
    {
        var isOriginal = symbol.IsOriginalDefinition();

        var reference = new HelMethodReferenceCS
        {
            Token = GetErrorToken(HelEntityKindCS.Method),
            IsError = true,
            TypeArguments = isOriginal
                ? ImmutableArray<HelTypeReferenceCS>.Empty
                : symbol.TypeArguments
                    .Select(GetErrorTypeReference)
                    .ToImmutableArray(),
        };

        errorReferences.TryAdd(reference.Token, symbol);
        return reference;
    }

    private HelEventReferenceCS GetErrorEventReference(IEventSymbol symbol)
    {
        var reference = new HelEventReferenceCS
        {
            Token = GetErrorToken(HelEntityKindCS.Event),
            IsError = true
        };
        errorReferences.TryAdd(reference.Token, symbol);
        return reference;
    }

    private HelPropertyReferenceCS GetErrorPropertyReference(IPropertySymbol symbol)
    {
        var reference = new HelPropertyReferenceCS
        {
            Token = GetErrorToken(HelEntityKindCS.Property),
            IsError = true
        };
        errorReferences.TryAdd(reference.Token, symbol);
        return reference;
    }

    private HelFieldReferenceCS GetErrorFieldReference(IFieldSymbol symbol)
    {
        var reference = new HelFieldReferenceCS
        {
            Token = GetErrorToken(HelEntityKindCS.Field),
            IsError = true
        };
        errorReferences.TryAdd(reference.Token, symbol);
        return reference;
    }

    private class SymbolErrorReferenceRewriter : HelEntityRewriterCS
    {
        private HelEntityLocator locator = null!;
        private readonly ConcurrentDictionary<HelEntityTokenCS, ISymbol> underlyingSymbols;
        private readonly ConcurrentDictionary<HelEntityTokenCS, ISymbol> errorReferences;

        public SymbolErrorReferenceRewriter(
            ConcurrentDictionary<HelEntityTokenCS, ISymbol> underlyingSymbols,
            ConcurrentDictionary<HelEntityTokenCS, ISymbol> errorReferences)
        {
            this.underlyingSymbols = underlyingSymbols;
            this.errorReferences = errorReferences;
        }

        public override HelSolutionCS RewriteSolution(HelSolutionCS solution)
        {
            locator = new HelEntityLocator(solution);
            var newSolution = base.RewriteSolution(solution);
            locator = null!;
            return newSolution;
        }
    }
}
