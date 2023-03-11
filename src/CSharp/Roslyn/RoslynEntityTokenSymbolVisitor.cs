using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Roslyn;

/// <summary>
/// A Roslyn <see cref="SymbolVisitor"/> that assigns <see cref="EntityToken"/>s to all symbol definitions
/// it visits.
/// </summary>
internal class RoslynEntityTokenSymbolVisitor : SymbolVisitor
{
    private readonly RoslynEntityTokenSymbolCache tokens;

    public HashSet<AssemblyIdentity> VisitedAssemblies { get; }
        = new();

    private readonly object visitedAssembliesLock = new();

    public RoslynEntityTokenSymbolVisitor(
        RoslynEntityTokenSymbolCache tokens)
    {
        this.tokens = tokens;
    }

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        // NB: `VisitAssembly` can and will be called from multiple threads. This prevents multiple visits to the same
        //     assembly.
        lock (visitedAssembliesLock)
        {
            if (VisitedAssemblies.Contains(symbol.Identity))
            {
                return;
            }

            VisitedAssemblies.Add(symbol.Identity);
        }

        tokens.Add(symbol);

        foreach (var module in symbol.Modules)
        {
            VisitModule(module);
        }
    }

    public override void VisitModule(IModuleSymbol symbol)
    {
        tokens.Add(symbol);

        VisitNamespace(symbol.GlobalNamespace);
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        tokens.Add(symbol);

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
        tokens.Add(symbol);

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
        tokens.Add(symbol);
    }

    public override void VisitEvent(IEventSymbol symbol)
    {
        tokens.Add(symbol);
    }

    public override void VisitProperty(IPropertySymbol symbol)
    {
        tokens.Add(symbol);

        foreach (var parameter in symbol.Parameters)
        {
            VisitParameter(parameter);
        }
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        tokens.Add(symbol);

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
        tokens.Add(symbol);
    }

    public override void VisitTypeParameter(ITypeParameterSymbol symbol)
    {
        tokens.Add(symbol);
    }
}
