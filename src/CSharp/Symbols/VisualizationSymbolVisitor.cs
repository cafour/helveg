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
            .SetProperty("Kind", $"csharp:{symbol.Token.Kind}");

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
        builder.AddEdges(CSConst.DeclaresId, assembly.Modules.Select(m => new Edge(assembly.Token, m.Token)));
    }

    public override void VisitModule(ModuleDefinition module)
    {
        base.VisitModule(module);
        builder.AddEdge(CSConst.DeclaresId, new Edge(module.Token, module.GlobalNamespace.Token));
    }

    public override void VisitNamespace(NamespaceDefinition @namespace)
    {
        base.VisitNamespace(@namespace);

        if (@namespace.IsGlobalNamespace)
        {
            builder.GetNode(@namespace.Id).Label = "global";
        }

        builder.AddEdges(CSConst.DeclaresId, @namespace.Namespaces.Select(n => new Edge(@namespace.Token, n.Token)));
        builder.AddEdges(CSConst.DeclaresId, @namespace.Types.Select(t => new Edge(@namespace.Token, t.Token)));
    }

    public override void VisitType(TypeDefinition type)
    {
        base.VisitType(type);
        builder.AddEdges(CSConst.DeclaresId, type.Fields.Select(f => new Edge(type.Token, f.Token)));
        builder.AddEdges(CSConst.DeclaresId, type.Events.Select(e => new Edge(type.Token, e.Token)));
        builder.AddEdges(CSConst.DeclaresId, type.Properties.Select(p => new Edge(type.Token, p.Token)));
        builder.AddEdges(CSConst.DeclaresId, type.Methods.Select(m => new Edge(type.Token, m.Token)));
        builder.AddEdges(CSConst.DeclaresId, type.TypeParameters.Select(t => new Edge(type.Token, t.Token)));
        builder.AddEdges(CSConst.DeclaresId, type.NestedTypes.Select(t => new Edge(type.Token, t.Token)));

        builder.AddEdges(CSConst.InheritsFromId, type.Interfaces.Select(i => new Edge(type.Token, i.Token)));
        if (type.BaseType is not null)
        {
            builder.AddEdge(CSConst.InheritsFromId, new Edge(type.Token, type.BaseType.Token));
        }
    }

    public override void VisitField(FieldDefinition field)
    {
        base.VisitField(field);

        builder.AddEdge(CSConst.TypeOfId, new Edge(field.Token, field.FieldType.Token));
    }

    public override void VisitParameter(ParameterDefinition parameter)
    {
        base.VisitParameter(parameter);

        builder.AddEdge(CSConst.TypeOfId, new Edge(parameter.Token, parameter.ParameterType.Token));
    }

    public override void VisitEvent(EventDefinition @event)
    {
        base.VisitEvent(@event);
        builder.AddEdge(CSConst.TypeOfId, new Edge(@event.Token, @event.EventType.Token));
    }

    public override void VisitProperty(PropertyDefinition property)
    {
        base.VisitProperty(property);

        builder.AddEdges(CSConst.DeclaresId, property.Parameters.Select(p => new Edge(property.Token, p.Token)));

        builder.AddEdge(CSConst.TypeOfId, new Edge(property.Token, property.PropertyType.Token));
    }

    public override void VisitMethod(MethodDefinition method)
    {
        base.VisitMethod(method);
        builder.AddEdges(CSConst.DeclaresId, method.Parameters.Select(p => new Edge(method.Token, p.Token)));
        builder.AddEdges(CSConst.DeclaresId, method.TypeParameters.Select(t => new Edge(method.Token, t.Token)));

        if (!method.ReturnsVoid && method.ReturnType is not null)
        {
            builder.AddEdge(CSConst.ReturnsId, new Edge(method.Token, method.ReturnType.Token));
        }
    }
}
