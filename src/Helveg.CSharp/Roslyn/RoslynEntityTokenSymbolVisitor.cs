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
internal class RoslynEntityTokenSymbolVisitor : SymbolVisitor<Task>
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

    public override Task? DefaultVisit(ISymbol symbol)
    {
        return Task.CompletedTask;
    }

    public override async Task VisitAssembly(IAssemblySymbol symbol)
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

        await tokens.Add(symbol);

        foreach (var module in symbol.Modules)
        {
            await VisitModule(module);
        }
    }

    public override async Task VisitModule(IModuleSymbol symbol)
    {
        await tokens.Add(symbol);

        await VisitNamespace(symbol.GlobalNamespace);
    }

    public override async Task VisitNamespace(INamespaceSymbol symbol)
    {
        await tokens.Add(symbol);

        foreach (var ns in symbol.GetNamespaceMembers())
        {
            await VisitNamespace(ns);
        }

        foreach (var type in symbol.GetTypeMembers())
        {
            await VisitNamedType(type);
        }
    }

    public override async Task VisitNamedType(INamedTypeSymbol symbol)
    {
        await tokens.Add(symbol);

        foreach (var typeParameter in symbol.TypeParameters)
        {
            await VisitTypeParameter(typeParameter);
        }

        foreach (var type in symbol.GetTypeMembers())
        {
            await VisitNamedType(type);
        }

        foreach (var member in symbol.GetMembers())
        {
            await Visit(member)!;
        }
    }

    public override Task VisitField(IFieldSymbol symbol)
    {
        return tokens.Add(symbol);
    }

    public override Task VisitEvent(IEventSymbol symbol)
    {
        return tokens.Add(symbol);
    }

    public override async Task VisitProperty(IPropertySymbol symbol)
    {
        await tokens.Add(symbol);

        foreach (var parameter in symbol.Parameters)
        {
            await VisitParameter(parameter);
        }
    }

    public override async Task VisitMethod(IMethodSymbol symbol)
    {
        await tokens.Add(symbol);

        foreach (var typeParameter in symbol.TypeParameters)
        {
            await VisitTypeParameter(typeParameter);
        }

        foreach (var parameter in symbol.Parameters)
        {
            await VisitParameter(parameter);
        }
    }

    public override Task VisitParameter(IParameterSymbol symbol)
    {
        return tokens.Add(symbol);
    }

    public override Task VisitTypeParameter(ITypeParameterSymbol symbol)
    {
        return tokens.Add(symbol);
    }
}
