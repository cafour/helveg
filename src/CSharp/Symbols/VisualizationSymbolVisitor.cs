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
        var node = builder.GetNode(symbol.Token, symbol.Name)
            .SetProperty(Const.KindProperty, CSConst.KindOf(symbol.GetType()))
            .SetProperty(Const.DiagnosticsProperty, symbol.Diagnostics);

        if (symbol is IMemberDefinition member)
        {
            node
            .SetProperty(nameof(member.Accessibility), member.Accessibility)
            .SetProperty(nameof(member.IsSealed), member.IsSealed)
            .SetProperty(nameof(member.IsStatic), member.IsStatic)
            .SetProperty(nameof(member.IsAbstract), member.IsAbstract)
            .SetProperty(nameof(member.IsExtern), member.IsExtern)
            .SetProperty(nameof(member.IsOverride), member.IsOverride)
            .SetProperty(nameof(member.IsVirtual), member.IsVirtual)
            .SetProperty(nameof(member.IsImplicitlyDeclared), member.IsImplicitlyDeclared)
            .SetProperty(nameof(member.CanBeReferencedByName), member.CanBeReferencedByName);
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
            builder.GetNode(@namespace.Token).Label = CSConst.GlobalNamespaceName;
        }

        builder.AddEdges(CSConst.DeclaresId, @namespace.Namespaces.Select(n => new Edge(@namespace.Token, n.Token)));
        builder.AddEdges(CSConst.DeclaresId, @namespace.Types.Select(t => new Edge(@namespace.Token, t.Token)));
    }

    public override void VisitType(TypeDefinition type)
    {
        base.VisitType(type);

        var node = builder.GetNode(type.Token)
            .SetProperty(nameof(TypeDefinition.TypeKind), type.TypeKind)
            .SetProperty(nameof(TypeDefinition.IsAnonymousType), type.IsAnonymousType)
            .SetProperty(nameof(TypeDefinition.IsTupleType), type.IsTupleType)
            .SetProperty(nameof(TypeDefinition.IsNativeIntegerType), type.IsNativeIntegerType)
            .SetProperty(nameof(TypeDefinition.IsUnmanagedType), type.IsUnmanagedType)
            .SetProperty(nameof(TypeDefinition.IsReadOnly), type.IsReadOnly)
            .SetProperty(nameof(TypeDefinition.IsRefLikeType), type.IsRefLikeType)
            .SetProperty(nameof(TypeDefinition.IsRecord), type.IsRecord);

        var (instanceCount, staticCount) = CountMembers(type);
        node.SetProperty("InstanceMemberCount", instanceCount)
            .SetProperty("StaticMemberCount", staticCount);

        builder.AddEdges(CSConst.DeclaresId, type.Fields.Select(f => new Edge(type.Token, f.Token)));
        builder.AddEdges(CSConst.DeclaresId, type.Events.Select(e => new Edge(type.Token, e.Token)));
        builder.AddEdges(CSConst.DeclaresId, type.Properties.Select(p => new Edge(type.Token, p.Token)));
        builder.AddEdges(CSConst.DeclaresId, type.Methods.Select(m => new Edge(type.Token, m.Token)));
        builder.AddEdges(CSConst.DeclaresId, type.TypeParameters.Select(t => new Edge(type.Token, t.Token)));
        builder.AddEdges(CSConst.DeclaresId, type.NestedTypes.Select(t => new Edge(type.Token, t.Token)));

        builder.AddEdges(CSConst.InheritsFromId, type.Interfaces.Select(i => new Edge(type.Token, i.Token)));
        if (type.BaseType?.Token.IsValid == true)
        {
            builder.AddEdge(CSConst.InheritsFromId, new Edge(type.Token, type.BaseType.Token));
        }
    }

    public override void VisitField(FieldDefinition field)
    {
        base.VisitField(field);

        builder.GetNode(field.Token)
            .SetProperty(nameof(FieldDefinition.IsReadOnly), field.IsReadOnly)
            .SetProperty(nameof(FieldDefinition.IsVolatile), field.IsVolatile)
            .SetProperty(nameof(FieldDefinition.IsConst), field.IsConst)
            .SetProperty(nameof(FieldDefinition.IsEnumItem), field.IsEnumItem)
            .SetProperty(nameof(FieldDefinition.FieldType), field.FieldType?.Hint ?? Const.Unknown);

        if (field.FieldType?.Token.IsValid == true)
        {
            builder.AddEdge(CSConst.TypeOfId, new Edge(field.Token, field.FieldType.Token));
        }
    }

    public override void VisitParameter(ParameterDefinition parameter)
    {
        base.VisitParameter(parameter);

        builder.GetNode(parameter.Token)
            .SetProperty(nameof(ParameterDefinition.IsParams), parameter.IsParams)
            .SetProperty(nameof(ParameterDefinition.IsOptional), parameter.IsOptional)
            .SetProperty(nameof(ParameterDefinition.ParameterType), parameter.ParameterType?.Hint ?? Const.Unknown);

        if (parameter.ParameterType?.Token.IsValid == true)
        {
            builder.AddEdge(CSConst.TypeOfId, new Edge(parameter.Token, parameter.ParameterType.Token));
        }
    }

    public override void VisitEvent(EventDefinition @event)
    {
        base.VisitEvent(@event);

        builder.GetNode(@event.Token)
            .SetProperty(nameof(EventDefinition.EventType), @event.EventType?.Hint ?? Const.Unknown);

        if (@event.EventType?.Token.IsValid == true)
        {
            builder.AddEdge(CSConst.TypeOfId, new Edge(@event.Token, @event.EventType.Token));
        }
    }

    public override void VisitProperty(PropertyDefinition property)
    {
        base.VisitProperty(property);

        builder.GetNode(property.Token)
            .SetProperty(nameof(PropertyDefinition.PropertyType), property.PropertyType?.Hint ?? Const.Unknown);

        builder.AddEdges(CSConst.DeclaresId, property.Parameters.Select(p => new Edge(property.Token, p.Token)));

        if (property.PropertyType?.Token.IsValid == true)
        {
            builder.AddEdge(CSConst.TypeOfId, new Edge(property.Token, property.PropertyType.Token));
        }

    }

    public override void VisitMethod(MethodDefinition method)
    {
        base.VisitMethod(method);

        builder.GetNode(method.Token)
            .SetProperty(nameof(MethodDefinition.MethodKind), method.MethodKind)
            .SetProperty(nameof(MethodDefinition.IsExtensionMethod), method.IsExtensionMethod)
            .SetProperty(nameof(MethodDefinition.IsAsync), method.IsAsync)
            .SetProperty(nameof(MethodDefinition.ReturnType), method.ReturnsVoid
                ? "void"
                : method.ReturnType?.Hint ?? Const.Unknown);

        builder.AddEdges(CSConst.DeclaresId, method.Parameters.Select(p => new Edge(method.Token, p.Token)));
        builder.AddEdges(CSConst.DeclaresId, method.TypeParameters.Select(t => new Edge(method.Token, t.Token)));

        if (!method.ReturnsVoid && method.ReturnType?.Token.IsValid == true)
        {
            builder.AddEdge(CSConst.ReturnsId, new Edge(method.Token, method.ReturnType.Token));
        }
    }

    public override void VisitTypeParameter(TypeParameterDefinition typeParameter)
    {
        base.VisitTypeParameter(typeParameter);

        builder.GetNode(typeParameter.Token)
            .SetProperty("DeclaringKind", typeParameter.DeclaringMethod is not null
                ? CSConst.KindOf<MethodDefinition>()
                : CSConst.KindOf<TypeDefinition>());
    }

    private static (int instanceCount, int staticCount) CountMembers(TypeDefinition type)
    {
        var members = Enumerable.Empty<MemberDefinition>()
            .Concat(type.Fields)
            .Concat(type.Events)
            .Concat(type.Properties)
            .Concat(type.Methods);
        return (members.Count(m => !m.IsStatic), members.Count(m => m.IsStatic));
    }
}
