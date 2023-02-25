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
    private readonly EntityTokenGenerator gen;

    public ConcurrentDictionary<ISymbol, EntityToken> Tokens { get; }
        = new(SymbolEqualityComparer.Default);

    public HashSet<AssemblyIdentity> VisitedAssemblies { get; }
        = new();

    private readonly object visitedAssembliesLock = new();

    public RoslynEntityTokenSymbolVisitor(EntityTokenGenerator gen)
    {
        this.gen = gen;
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

        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(EntityKind.Assembly), (_, e) => e);

        foreach (var module in symbol.Modules)
        {
            VisitModule(module);
        }
    }

    public override void VisitModule(IModuleSymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(EntityKind.Module), (_, e) => e);

        VisitNamespace(symbol.GlobalNamespace);
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(EntityKind.Namespace), (_, e) => e);

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
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(EntityKind.Type), (_, e) => e);

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
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(EntityKind.Field), (_, e) => e);
    }

    public override void VisitEvent(IEventSymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(EntityKind.Event), (_, e) => e);
    }

    public override void VisitProperty(IPropertySymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(EntityKind.Property), (_, e) => e);

        foreach (var parameter in symbol.Parameters)
        {
            VisitParameter(parameter);
        }
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(EntityKind.Method), (_, e) => e);

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
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(EntityKind.Parameter), (_, e) => e);
    }

    public override void VisitTypeParameter(ITypeParameterSymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(EntityKind.TypeParameter), (_, e) => e);
    }

    public EntityToken GetSymbolToken(ISymbol symbol)
    {
        return Tokens.TryGetValue(symbol, out var token)
            ? token
            : EntityToken.CreateError(symbol.GetEntityKind());
    }

    public EntityToken RequireSymbolToken(ISymbol symbol)
    {
        return Tokens.TryGetValue(symbol, out var token)
            ? token
            : throw new InvalidOperationException($"Symbol '{symbol}' does not have a token even though it is " +
                $"required. This could be a bug in {nameof(RoslynEntityTokenSymbolVisitor)}.");
    }
}
