using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Roslyn;

#pragma warning disable RS1024 // Symbols should be compared for equality
internal class RoslynCrossCompilationSymbolEqualityComparer : IEqualityComparer<ISymbol?>
{
    private readonly RoslynLocationEqualityComparer locationComparer = new();

    public bool Equals(ISymbol? lhs, ISymbol? rhs)
    {
        if ((lhs is null && rhs is null) || ReferenceEquals(lhs, rhs))
        {
            return true;
        }

        if (lhs is null || rhs is null || lhs.Kind != rhs.Kind)
        {
            return false;
        }

        if (lhs.Kind == SymbolKind.Assembly)
        {
            return AssemblyEquals((IAssemblySymbol)lhs, (IAssemblySymbol)rhs);
        }

        return lhs.MetadataName.Equals(rhs.MetadataName)
            && Equals(lhs.ContainingSymbol.OriginalDefinition, rhs.ContainingSymbol.OriginalDefinition)
            && Enumerable.SequenceEqual(lhs.Locations, rhs.Locations, locationComparer);

        //return lhs.Kind switch
        //{
        //    SymbolKind.Assembly => AssemblyEquals((IAssemblySymbol)lhs, (IAssemblySymbol)rhs),
        //    SymbolKind.NetModule => ModuleEquals((IModuleSymbol)lhs, (IModuleSymbol)rhs),
        //    SymbolKind.Namespace => NamespaceEquals((INamespaceSymbol)lhs, (INamespaceSymbol)rhs),
        //    SymbolKind.NamedType => NamedTypeEquals((INamedTypeSymbol)lhs, (INamedTypeSymbol)rhs),
        //    SymbolKind.TypeParameter => TypeParameterEquals((ITypeParameterSymbol)lhs, (ITypeParameterSymbol)rhs),
        //    SymbolKind.ArrayType => ArrayTypeEquals((IArrayTypeSymbol)lhs, (IArrayTypeSymbol)rhs),
        //    SymbolKind.PointerType => PointerTypeEquals((IPointerTypeSymbol)lhs, (IPointerTypeSymbol)rhs),
        //    SymbolKind.FunctionPointerType => FunctionPointerEquals(
        //        (IFunctionPointerTypeSymbol)lhs,
        //        (IFunctionPointerTypeSymbol)rhs),
        //    SymbolKind.Field => FieldEquals((IFieldSymbol)lhs, (IFieldSymbol)rhs),
        //    SymbolKind.Event => EventEquals((IEventSymbol)lhs, (IEventSymbol)rhs),
        //    SymbolKind.Property => PropertyEquals((IPropertySymbol)lhs, (IPropertySymbol)rhs),
        //    SymbolKind.Method => MethodEquals((IMethodSymbol)lhs, (IMethodSymbol)rhs),
        //    SymbolKind.Parameter => ParameterEquals((IParameterSymbol)lhs, (IParameterSymbol)rhs),
        //    _ => throw new NotSupportedException($"Symbol kind '{lhs.Kind}' is not supported.")
        //};
    }

    public int GetHashCode([DisallowNull] ISymbol? obj)
    {
        return obj.MetadataName.GetHashCode();
    }

    private bool AssemblyEquals(IAssemblySymbol lhs, IAssemblySymbol rhs)
    {
        return lhs.Identity.Equals(rhs.Identity);
    }

    private bool ModuleEquals(IModuleSymbol lhs, IModuleSymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && AssemblyEquals(lhs.ContainingAssembly, rhs.ContainingAssembly);
    }

    private bool NamespaceEquals(INamespaceSymbol lhs, INamespaceSymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && lhs.NamespaceKind.Equals(rhs.NamespaceKind)
            && AssemblyEquals(lhs.ContainingAssembly, rhs.ContainingAssembly);
    }

    private bool NamedTypeEquals(INamedTypeSymbol lhs, INamedTypeSymbol rhs)
    {
        var result = lhs.MetadataName.Equals(rhs.MetadataName)
            && lhs.Arity == rhs.Arity
            && AssemblyEquals(lhs.ContainingAssembly, rhs.ContainingAssembly);

        if (!lhs.IsOriginalDefinition() || !rhs.IsOriginalDefinition())
        {
            result = result && Enumerable.SequenceEqual(lhs.TypeArguments, rhs.TypeArguments, this);
        }

        return result;
    }

    private bool TypeParameterEquals(ITypeParameterSymbol lhs, ITypeParameterSymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && lhs.TypeParameterKind.Equals(rhs.TypeParameterKind)
            && Equals(lhs.DeclaringMethod?.OriginalDefinition, rhs.DeclaringMethod?.OriginalDefinition)
            && Equals(lhs.DeclaringType?.OriginalDefinition, rhs.DeclaringType?.OriginalDefinition);
    }

    private bool ArrayTypeEquals(IArrayTypeSymbol lhs, IArrayTypeSymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && lhs.Rank == rhs.Rank
            && Enumerable.SequenceEqual(lhs.Sizes, rhs.Sizes)
            && Equals(lhs.ElementType, rhs.ElementType);
    }

    private bool PointerTypeEquals(IPointerTypeSymbol lhs, IPointerTypeSymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && Equals(lhs.PointedAtType, rhs.PointedAtType);
    }

    private bool FunctionPointerEquals(IFunctionPointerTypeSymbol lhs, IFunctionPointerTypeSymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && MethodEquals(lhs.Signature, rhs.Signature);
    }

    private bool FieldEquals(IFieldSymbol lhs, IFieldSymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && NamedTypeEquals(lhs.ContainingType, rhs.ContainingType);
    }

    private bool EventEquals(IEventSymbol lhs, IEventSymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && NamedTypeEquals(lhs.ContainingType, rhs.ContainingType);
    }

    private bool PropertyEquals(IPropertySymbol lhs, IPropertySymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && Enumerable.SequenceEqual(lhs.Parameters, rhs.Parameters, this)
            && NamedTypeEquals(lhs.ContainingType, rhs.ContainingType);
    }

    private bool MethodEquals(IMethodSymbol lhs, IMethodSymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && lhs.Arity == rhs.Arity
            && NamedTypeEquals(lhs.ContainingType, rhs.ContainingType)
            && Enumerable.SequenceEqual(lhs.Locations, rhs.Locations);
    }

    private bool ParameterEquals(IParameterSymbol lhs, IParameterSymbol rhs)
    {
        return lhs.MetadataName.Equals(rhs.MetadataName)
            && lhs.Ordinal == rhs.Ordinal
            && Equals(lhs.ContainingSymbol, rhs.ContainingSymbol);
    }
}
#pragma warning restore RS1024 // Symbols should be compared for equality
