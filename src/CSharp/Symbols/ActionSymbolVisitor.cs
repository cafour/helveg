using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

/// <summary>
/// A Roslyn <see cref="Microsoft.CodeAnalysis.SymbolVisitor"/> that invokes an <see cref="Action{ISymbol}"/> for
/// each symbol definitions it visits.
/// </summary>
/// <remarks>
/// The order of the visists is pre-order from assemblies as the roots to namespaces, types, methods, etc.
/// all the way to method parameters.
/// </remarks>
internal class ActionSymbolVisitor : Microsoft.CodeAnalysis.SymbolVisitor
{
    private readonly Action<ISymbol> action;

    public ActionSymbolVisitor(Action<ISymbol> action)
    {
        this.action = action;
    }

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        action(symbol);
        foreach (var module in symbol.Modules)
        {
            VisitModule(module);
        }
    }

    public override void VisitModule(IModuleSymbol symbol)
    {
        action(symbol);
        VisitNamespace(symbol.GlobalNamespace);
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        action(symbol);

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
        action(symbol);

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
        action(symbol);
    }

    public override void VisitEvent(IEventSymbol symbol)
    {
        action(symbol);
    }

    public override void VisitProperty(IPropertySymbol symbol)
    {
        action(symbol);

        foreach (var parameter in symbol.Parameters)
        {
            VisitParameter(parameter);
        }
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        action(symbol);

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
        action(symbol);
    }

    public override void VisitTypeParameter(ITypeParameterSymbol symbol)
    {
        action(symbol);
    }
}
