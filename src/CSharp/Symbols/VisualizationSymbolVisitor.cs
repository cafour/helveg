using Helveg.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public class VisualizationSymbolVisitor : SymbolVisitor
{
    private readonly MultigraphBuilder builder;

    public VisualizationSymbolVisitor(MultigraphBuilder builder)
    {
        this.builder = builder;
    }

    public override void DefaultVisit(ISymbolDefinition symbol)
    {
        var node = builder.AddNode(symbol.Token, symbol.Name)
            .SetProperty(nameof(SymbolKind), symbol.Token.Kind.ToString());

        if (symbol is IMemberDefinition member)
        {
            node
            .SetProperty(nameof(member.Accessibility), member.Accessibility.ToString())
            .SetProperty(nameof(member.IsSealed), member.IsSealed.ToString())
            .SetProperty(nameof(member.IsStatic), member.IsStatic.ToString())
            .SetProperty(nameof(member.IsAbstract), member.IsAbstract.ToString())
            .SetProperty(nameof(member.IsExtern), member.IsExtern.ToString())
            .SetProperty(nameof(member.IsOverride), member.IsOverride.ToString())
            .SetProperty(nameof(member.IsVirtual), member.IsVirtual.ToString())
            .SetProperty(nameof(member.IsImplicitlyDeclared), member.IsImplicitlyDeclared.ToString())
            .SetProperty(nameof(member.CanBeReferencedByName), member.CanBeReferencedByName.ToString());
        }
    }

    public override void VisitAssembly(AssemblyDefinition assembly)
    {
        base.VisitAssembly(assembly);
        builder.AddEdges(CSConst.DeclaredInId, assembly.Modules.Select(m => new Edge(assembly.Token, m.Token)));
    }

    public override void VisitModule(ModuleDefinition module)
    {
        base.VisitModule(module);
        builder.AddEdge(CSConst.DeclaredInId, new Edge(module.Token, module.GlobalNamespace.Token));
    }

    public override void VisitNamespace(NamespaceDefinition @namespace)
    {
        base.VisitNamespace(@namespace);
        builder.AddEdges(CSConst.DeclaredInId, @namespace.Namespaces.Select(n => new Edge(@namespace.Token, n.Token)));
        builder.AddEdges(CSConst.DeclaredInId, @namespace.Types.Select(t => new Edge(@namespace.Token, t.Token)));
    }

    public override void VisitType(TypeDefinition type)
    {
        base.VisitType(type);
        builder.AddEdges(CSConst.DeclaredInId, type.Fields.Select(f => new Edge(type.Token, f.Token)));
        builder.AddEdges(CSConst.DeclaredInId, type.Events.Select(e => new Edge(type.Token, e.Token)));
        builder.AddEdges(CSConst.DeclaredInId, type.Properties.Select(p => new Edge(type.Token, p.Token)));
        builder.AddEdges(CSConst.DeclaredInId, type.Methods.Select(m => new Edge(type.Token, m.Token)));
        builder.AddEdges(CSConst.DeclaredInId, type.TypeParameters.Select(t => new Edge(type.Token, t.Token)));
        builder.AddEdges(CSConst.DeclaredInId, type.NestedTypes.Select(t => new Edge(type.Token, t.Token)));

        builder.AddEdges(CSConst.InheritsFromId, type.Interfaces.Select(i => new Edge(type.Token, i.Token)));
        if (type.BaseType is not null)
        {
            builder.AddEdge(CSConst.InheritsFromId, new Edge(type.Token, type.BaseType.Token));
        }

        builder.AddEdges(CSConst.ComposedOfId, type.Fields.Select(f => new Edge(type.Token, f.Token)));
    }

    public override void VisitProperty(PropertyDefinition property)
    {
        base.VisitProperty(property);
        builder.AddEdges(CSConst.DeclaredInId, property.Parameters.Select(p => new Edge(property.Token, p.Token)));
    }

    public override void VisitMethod(MethodDefinition method)
    {
        base.VisitMethod(method);
        builder.AddEdges(CSConst.DeclaredInId, method.Parameters.Select(p => new Edge(method.Token, p.Token)));
        builder.AddEdges(CSConst.DeclaredInId, method.TypeParameters.Select(t => new Edge(method.Token, t.Token)));
    }
}
