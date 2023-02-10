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

    private async Task<HelSolutionCS> GetSolution(Solution solution, CancellationToken cancellationToken = default)
    {
        var helSolution = new HelSolutionCS
        {
            Token = new HelEntityTokenCS(HelEntityKindCS.Solution, ++counter),
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
            Token = new HelEntityTokenCS(HelEntityKindCS.Project, ++counter),
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
            Token = new HelEntityTokenCS(HelEntityKindCS.Assembly, ++counter),
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
            Token = new HelEntityTokenCS(HelEntityKindCS.Module, ++counter),
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
            Token = new HelEntityTokenCS(HelEntityKindCS.Namespace, ++counter),
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
        var helType = new HelTypeCS
        {
            Token = new HelEntityTokenCS(HelEntityKindCS.Type, ++counter),
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

    private HelTypeParameterCS GetTypeParameter(
        ITypeParameterSymbol symbol,
        HelTypeReferenceCS? declaringType,
        HelMethodReferenceCS? declaringMethod)
    {
        if (declaringType is null && declaringMethod is null)
        {
            throw new ArgumentException($"Either '{nameof(declaringType)}' or '{nameof(declaringMethod)}' " +
                "must not be null.");
        }

        return new HelTypeParameterCS
        {
            Token = new HelEntityTokenCS(HelEntityKindCS.Type, ++counter),
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
            Token = new HelEntityTokenCS(HelEntityKindCS.Event, ++counter),
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
            Token = new HelEntityTokenCS(HelEntityKindCS.Field, ++counter),
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
            Token = new HelEntityTokenCS(HelEntityKindCS.Property, ++counter),
            PropertyType = GetUnresolvedTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace= containingType.ContainingNamespace,
            GetMethod = symbol.GetMethod is null ? null : GetUnresolvedMethodReference(symbol.GetMethod),
            SetMethod = symbol.SetMethod is null ? null : GetUnresolvedMethodReference(symbol.SetMethod),
            IsIndexer = symbol.IsIndexer,
            IsRequired= symbol.IsRequired,
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
        throw new NotImplementedException();
    }

    private HelParameterCS GetParameter(
        IParameterSymbol symbol,
        HelMethodReferenceCS? declaringMethod,
        HelPropertyReferenceCS? declaringProperty)
    {
        throw new NotImplementedException();
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
            Token = HelEntityTokenCS.GetUnresolved(HelEntityKindCS.Assembly),
            Identity = GetAssemblyId(symbol.Identity),
            Name = symbol.Name
        };
        unresolvedReferences.Push(reference);
        return reference;
    }

    private HelNamespaceReferenceCS GetUnresolvedNamespaceReference(INamespaceSymbol symbol)
    {
        throw new NotImplementedException();
    }

    private HelTypeReferenceCS GetUnresolvedTypeReference(ITypeSymbol symbol)
    {
        throw new NotImplementedException();
    }

    private HelMethodReferenceCS GetUnresolvedMethodReference(IMethodSymbol symbol)
    {
        throw new NotImplementedException();
    }

    private HelEventReferenceCS GetUnresolvedEventReference(IEventSymbol symbol)
    {
        throw new NotImplementedException();
    }

    private HelPropertyReferenceCS GetUnresolvedPropertyReference(IPropertySymbol symbol)
    {
        throw new NotImplementedException();
    }

    private HelPropertyReferenceCS GetUnresolvedFieldReference(IFieldSymbol symbol)
    {
        throw new NotImplementedException();
    }

    private HelAccessibilityCS GetAccessibility(Accessibility value)
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

    private HelTypeKindCS GetTypeKind(TypeKind value)
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

    private HelRefKindCS GetRefKind(RefKind value)
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
}
