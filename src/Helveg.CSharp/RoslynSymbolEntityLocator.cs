using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public class RoslynSymbolEntityLocator
{
    public ConcurrentDictionary<HelEntityTokenCS, ISymbol> UnderlyingSymbols { get; } = new();

    public ConcurrentDictionary<HelEntityTokenCS, ISymbol> ErrorReferenceSymbol { get; } = new();

    public HelSolutionCS? CurrentSolution { get; set; }

    public HelAssemblyCS? FindAssembly(IAssemblySymbol symbol)
    {
        if (CurrentSolution is null)
        {
            throw new InvalidOperationException("Search can only be conducted on a non-null solution.");
        }

        var id = symbol.Identity.ToHelvegAssemblyId();
        return CurrentSolution
            .Projects.Select(p => p.Assembly)
            .Concat(CurrentSolution.Packages.SelectMany(p => p.Assemblies))
            .SingleOrDefault(a => a.Identity == id);
    }

    private HelModuleCS? FindModule(IModuleSymbol symbol)
    {
        var assembly = FindAssembly(symbol.ContainingAssembly);
        if (assembly is null)
        {
            return null;
        }

        return assembly.Modules.SingleOrDefault(m => m.Name == symbol.Name);
    }

    private HelNamespaceCS? FindNamespace(INamespaceSymbol symbol)
    {
        if (symbol.NamespaceKind != NamespaceKind.Module)
        {
            throw new ArgumentException("Only module namespace are supported.");
        }

        if (symbol.IsGlobalNamespace)
        {
            var module = FindModule(symbol.ContainingModule);
            if (module is null)
            {
                return null;
            }

            return module.GlobalNamespace;
        }

        var parentNamespace = FindNamespace(symbol.ContainingNamespace);
        return parentNamespace?.Namespaces.SingleOrDefault(n => n.Name == symbol.Name);
    }

    private HelTypeCS? FindType(INamedTypeSymbol symbol)
    {
        if (symbol.ContainingType is not null)
        {
            var parentType = FindType(symbol.ContainingType);
            return parentType?.NestedTypes.SingleOrDefault(t => t.Name == symbol.Name && t.Arity == symbol.Arity);
        }

        var @namespace = FindNamespace(symbol.ContainingNamespace);
        if (@namespace is null)
        {
            return null;
        }

        return @namespace.Types.SingleOrDefault(t => t.Name == symbol.Name && t.Arity == symbol.Arity);
    }

    private HelFieldCS? FindField(IFieldSymbol symbol)
    {
        var type = FindType(symbol.ContainingType);
        return type?.Fields.SingleOrDefault(f => f.Name == symbol.Name);
    }

    private HelEventCS? FindEvent(IEventSymbol symbol)
    {
        var type = FindType(symbol.ContainingType);
        return type?.Events.SingleOrDefault(e => e.Name == symbol.Name);
    }

    private HelPropertyCS? FindProperty(IPropertySymbol symbol)
    {
        var type = FindType(symbol.ContainingType);
        // NB: Properties can be indexers, indexers have parameters, and parameter types can be erroneous thus we
        //     compare the underlying symbols instead.
        return type?.Properties.SingleOrDefault(p =>
            SymbolEqualityComparer.Default.Equals(UnderlyingSymbols.GetValueOrDefault(p.Token), symbol));
    }

    private HelMethodCS? FindMethod(IMethodSymbol symbol)
    {
        var type = FindType(symbol.ContainingType);
        return type?.Methods.SingleOrDefault(m =>
            SymbolEqualityComparer.Default.Equals(UnderlyingSymbols.GetValueOrDefault(m.Token), symbol));
    }

    private HelTypeParameterCS? FindTypeParameter(ITypeParameterSymbol symbol)
    {
        if (symbol.TypeParameterKind == TypeParameterKind.Type)
        {
            var type = FindType(symbol.DeclaringType!);
            return type?.TypeParameters.SingleOrDefault(p => p.Name == symbol.Name);
        }
        else if (symbol.TypeParameterKind == TypeParameterKind.Method)
        {
            var method = FindMethod(symbol.DeclaringMethod!);
            return method?.TypeParameters.SingleOrDefault(p => p.Name == symbol.Name);
        }
        else
        {
            throw new ArgumentException($"Type parameters of kind '{symbol.TypeParameterKind}' are not supported.");
        }
    }

    private HelParameterCS? FindParameter(IParameterSymbol symbol)
    {
        var test = new ConcurrentDictionary<ISymbol, HelEntityTokenCS>(SymbolEqualityComparer.Default);

        if (symbol.ContainingSymbol is IMethodSymbol methodSymbol)
        {
            var method = FindMethod(methodSymbol);

        }

        throw new NotImplementedException();
    }
}
