using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Helveg.CSharp;

public class RoslynWorkspaceProvider : IHelWorkspaceCSProvider
{
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
        var projects = (await Task.WhenAll(solution.Projects.Select(p => GetProject(p, cancellationToken))))
            .ToImmutableArray();

        return new HelSolutionCS
        {
            Name = solution.FilePath ?? IHelEntityCS.InvalidName,
            Projects = projects
        };
    }

    private async Task<HelProjectCS> GetProject(Project project, CancellationToken cancellationToken = default)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            return HelProjectCS.Invalid;
        }

        return new HelProjectCS
        {
            Name = project.Name,
            Assembly = GetAssembly(compilation.Assembly)
        };
    }

    private HelAssemblyCS GetAssembly(IAssemblySymbol assembly)
    {
        var helAssembly = new HelAssemblyCS
        {
            Identity = GetAssemblyId(assembly.Identity),
            Modules = assembly.Modules.Select(GetModule).ToImmutableArray()
        };

        // TODO: Merge module-scoped global namespaces

        return PopulateSymbol(assembly, helAssembly);
    }

    private HelAssemblyIdCS GetAssemblyId(AssemblyIdentity assemblyIdentity)
    {
        return new HelAssemblyIdCS(
            Name: assemblyIdentity.Name,
            Version: assemblyIdentity.Version,
            CultureName: assemblyIdentity.CultureName,
            PublicKeyToken: string.Concat(assemblyIdentity.PublicKeyToken.Select(b => b.ToString("x"))));
    }

    private HelModuleCS GetModule(IModuleSymbol module)
    {
        var helModule = new HelModuleCS
        {
            GlobalNamespace = GetNamespace(module.GlobalNamespace),
            ReferencedAssemblies = module.ReferencedAssemblies.Select(GetAssemblyId)
                .ToImmutableArray()
        };
        return PopulateSymbol(module, helModule);
    }

    private HelNamespaceCS GetNamespace(INamespaceSymbol @namespace)
    {
        var helNamespace = new HelNamespaceCS
        {
            TypeMembers = @namespace.GetTypeMembers().Select(GetType).ToImmutableArray(),
            NamespaceMembers = @namespace.GetNamespaceMembers().Select(GetNamespace).ToImmutableArray()
        };
        return PopulateSymbol(@namespace, helNamespace);
    }

    private HelTypeCS GetType(ITypeSymbol type)
    {
        var helType = new HelTypeCS
        {
            TypeKind = GetTypeKind(type.TypeKind),
            IsReferenceType = type.IsReferenceType,
            IsValueType = type.IsValueType,
            IsAnonymousType = type.IsAnonymousType,
            IsTupleType = type.IsTupleType,
            IsNativeIntegerType = type.IsNativeIntegerType,
            IsRefLikeType = type.IsRefLikeType,
            IsUnmanagedType = type.IsUnmanagedType,
            IsReadOnly = type.IsReadOnly,
            IsRecord = type.IsRecord
        };

        helType = helType with
        {
            NestedTypes = type.GetTypeMembers().Select(GetType).ToImmutableArray(),
            // TODO: Does GetMembers() contain nested types as well?
            Members = type.GetMembers().Select(GetTypeMember).ToImmutableArray()
        };

        // TODO: BaseType, Interfaces
        return PopulateSymbol(type, helType);
    }

    private IHelSymbolCS GetTypeMember(ISymbol member)
    {
        throw new NotImplementedException();
    }

    private THelSymbol PopulateSymbol<THelSymbol>(ISymbol symbol, THelSymbol helSymbol)
        where THelSymbol : HelSymbolBaseCS
    {
        return helSymbol with
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
            IsVirtual = symbol.IsVirtual,
            Kind = GetSymbolKind(symbol.Kind)
        };
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

    private HelSymbolKindCS GetSymbolKind(SymbolKind value)
    {
        return value switch
        {
            SymbolKind.Assembly => HelSymbolKindCS.Assembly,
            SymbolKind.Event => HelSymbolKindCS.Event,
            SymbolKind.Field => HelSymbolKindCS.Field,
            SymbolKind.Local => HelSymbolKindCS.Local,
            SymbolKind.Method => HelSymbolKindCS.Method,
            SymbolKind.NetModule => HelSymbolKindCS.Module,
            SymbolKind.NamedType => HelSymbolKindCS.NamedType,
            SymbolKind.Namespace => HelSymbolKindCS.Namespace,
            SymbolKind.Parameter => HelSymbolKindCS.Parameter,
            SymbolKind.PointerType => HelSymbolKindCS.PointerType,
            SymbolKind.Property => HelSymbolKindCS.Property,
            SymbolKind.TypeParameter => HelSymbolKindCS.TypeParameter,
            _ => HelSymbolKindCS.Invalid
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
}
