using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public class EntityTokenSymbolVisitor : SymbolVisitor
{
    private readonly EntityTokenGenerator gen;

    public ConcurrentDictionary<ISymbol, HelEntityTokenCS> Tokens { get; }
        = new(SymbolEqualityComparer.Default);

    public EntityTokenSymbolVisitor(EntityTokenGenerator gen)
    {
        this.gen = gen;
    }

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(HelEntityKindCS.Assembly), (_, e) => e);

        foreach (var module in symbol.Modules)
        {
            VisitModule(module);
        }
    }

    public override void VisitModule(IModuleSymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(HelEntityKindCS.Module), (_, e) => e);

        VisitNamespace(symbol.GlobalNamespace);
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(HelEntityKindCS.Namespace), (_, e) => e);

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
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(HelEntityKindCS.Type), (_, e) => e);

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
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(HelEntityKindCS.Field), (_, e) => e);
    }

    public override void VisitEvent(IEventSymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(HelEntityKindCS.Event), (_, e) => e);
    }

    public override void VisitProperty(IPropertySymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(HelEntityKindCS.Property), (_, e) => e);

        foreach (var parameter in symbol.Parameters)
        {
            VisitParameter(parameter);
        }
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(HelEntityKindCS.Method), (_, e) => e);

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
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(HelEntityKindCS.Parameter), (_, e) => e);
    }

    public override void VisitTypeParameter(ITypeParameterSymbol symbol)
    {
        Tokens.AddOrUpdate(symbol, _ => gen.GetToken(HelEntityKindCS.TypeParameter), (_, e) => e);
    }
}
