using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

/// <summary>
/// A Roslyn <see cref="Microsoft.CodeAnalysis.SymbolVisitor"/> that assigns <see cref="SymbolToken"/>s to all symbol
/// definitions it visits.
/// </summary>
internal class SymbolTrackingVisitor : Microsoft.CodeAnalysis.SymbolVisitor
{
    private readonly SymbolTokenMap map;

    public SymbolTrackingVisitor(SymbolTokenMap map)
    {
        this.map = map;
    }

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        map.GetOrAdd(symbol);
        foreach (var module in symbol.Modules)
        {
            VisitModule(module);
        }
    }

    public override void VisitModule(IModuleSymbol symbol)
    {
        map.GetOrAdd(symbol);
        VisitNamespace(symbol.GlobalNamespace);
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        map.GetOrAdd(symbol);

        foreach (var ns in symbol.GetNamespaceMembers())
        {
            VisitNamespace(ns);
        }

        foreach (var type in symbol.GetTypeMembers())
        {
            VisitNamedType(type);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        map.GetOrAdd(symbol);

        foreach (var typeParameter in symbol.TypeParameters)
        {
            VisitTypeParameter(typeParameter);
        }

        foreach (var type in symbol.GetTypeMembers())
        {
            VisitNamedType(type);
        }

        foreach (var member in symbol.GetMembers())
        {
            Visit(member);
        }
    }

    public override void VisitField(IFieldSymbol symbol)
    {
        map.GetOrAdd(symbol);
    }

    public override void VisitEvent(IEventSymbol symbol)
    {
        map.GetOrAdd(symbol);
    }

    public override void VisitProperty(IPropertySymbol symbol)
    {
        map.GetOrAdd(symbol);

        foreach (var parameter in symbol.Parameters)
        {
            VisitParameter(parameter);
        }
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        map.GetOrAdd(symbol);

        foreach (var typeParameter in symbol.TypeParameters)
        {
            VisitTypeParameter(typeParameter);
        }

        foreach (var parameter in symbol.Parameters)
        {
            VisitParameter(parameter);
        }
    }

    public override void VisitParameter(IParameterSymbol symbol)
    {
        map.GetOrAdd(symbol);
    }

    public override void VisitTypeParameter(ITypeParameterSymbol symbol)
    {
        map.GetOrAdd(symbol);
    }
}
