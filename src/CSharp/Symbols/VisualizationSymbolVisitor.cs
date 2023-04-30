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
            .SetProperty(CSProperties.Kind, CSConst.KindOf(symbol.GetType()))
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

        builder.GetNode(assembly.Name)
            .SetProperty(nameof(AssemblyId.Version), assembly.Identity.Version)
            .SetProperty(nameof(AssemblyId.FileVersion), assembly.Identity.FileVersion)
            .SetProperty(nameof(AssemblyId.CultureName), assembly.Identity.CultureName)
            .SetProperty(nameof(AssemblyId.PublicKeyToken), assembly.Identity.PublicKeyToken)
            .SetProperty(nameof(AssemblyId.TargetFramework), assembly.Identity.TargetFramework);

        builder.AddEdges(CSRelations.Declares, assembly.Modules.Select(m => new Edge(assembly.Token, m.Token)));
    }

    public override void VisitModule(ModuleDefinition module)
    {
        base.VisitModule(module);
        builder.AddEdge(CSRelations.Declares, new Edge(module.Token, module.GlobalNamespace.Token));
    }

    public override void VisitNamespace(NamespaceDefinition @namespace)
    {
        if (!@namespace.GetAllTypes().Any())
        {
            // ignore namespace without any types in it or any subnamespaces
            return;
        }

        base.VisitNamespace(@namespace);

        if (@namespace.IsGlobalNamespace)
        {
            builder.GetNode(@namespace.Token)
                .SetProperty(Const.LabelProperty, CSConst.GlobalNamespaceName);
        }

        builder.AddEdges(CSRelations.Declares, @namespace.Namespaces.Select(n => new Edge(@namespace.Token, n.Token)));
        builder.AddEdges(CSRelations.Declares, @namespace.Types.Select(t => new Edge(@namespace.Token, t.Token)));
    }

    public override void VisitType(TypeDefinition type)
    {
        base.VisitType(type);

        var node = builder.GetNode(type.Token)
            .SetProperty(nameof(TypeDefinition.IsNested), type.IsNested)
            .SetProperty(nameof(TypeDefinition.TypeKind), type.TypeKind)
            .SetProperty(nameof(TypeDefinition.IsAnonymousType), type.IsAnonymousType)
            .SetProperty(nameof(TypeDefinition.IsTupleType), type.IsTupleType)
            .SetProperty(nameof(TypeDefinition.IsNativeIntegerType), type.IsNativeIntegerType)
            .SetProperty(nameof(TypeDefinition.IsUnmanagedType), type.IsUnmanagedType)
            .SetProperty(nameof(TypeDefinition.IsReadOnly), type.IsReadOnly)
            .SetProperty(nameof(TypeDefinition.IsRefLikeType), type.IsRefLikeType)
            .SetProperty(nameof(TypeDefinition.IsRecord), type.IsRecord)
            .SetProperty(nameof(TypeDefinition.Arity), type.Arity)
            .SetProperty(nameof(TypeDefinition.IsImplicitClass), type.IsImplicitClass);

        var (instanceCount, staticCount) = CountMembers(type);
        node.SetProperty(CSProperties.InstanceMemberCount, instanceCount)
            .SetProperty(CSProperties.StaticMemberCount, staticCount);

        builder.AddEdges(CSRelations.Declares, type.Fields.Select(f => new Edge(type.Token, f.Token)));
        builder.AddEdges(CSRelations.Declares, type.Events.Select(e => new Edge(type.Token, e.Token)));
        builder.AddEdges(CSRelations.Declares, type.Properties.Select(p => new Edge(type.Token, p.Token)));
        builder.AddEdges(CSRelations.Declares, type.Methods.Select(m => new Edge(type.Token, m.Token)));
        builder.AddEdges(CSRelations.Declares, type.TypeParameters.Select(t => new Edge(type.Token, t.Token)));
        builder.AddEdges(CSRelations.Declares, type.NestedTypes.Select(t => new Edge(type.Token, t.Token)));

        builder.AddEdges(CSRelations.InheritsFrom, type.Interfaces.Select(i => new Edge(type.Token, i.Token)));
        if (type.BaseType?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.InheritsFrom, new Edge(type.Token, type.BaseType.Token));
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
            .SetProperty(nameof(FieldDefinition.RefKind), field.RefKind)
            .SetProperty(nameof(FieldDefinition.FieldType), field.FieldType?.Hint ?? Const.Unknown);

        if (field.FieldType?.Token.HasValue == true)
        {
            builder.AddEdges(CSRelations.TypeOf, GetReferencedTypes(field.FieldType)
                .Select(t => new Edge(field.Token, t)));
        }

        if (field.AssociatedEvent?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.AssociatedWith, new Edge(field.Token, field.AssociatedEvent.Token));
        }

        if (field.AssociatedProperty?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.AssociatedWith, new Edge(field.Token, field.AssociatedProperty.Token));
        }
    }

    public override void VisitParameter(ParameterDefinition parameter)
    {
        base.VisitParameter(parameter);

        builder.GetNode(parameter.Token)
            .SetProperty(nameof(ParameterDefinition.ParameterType), parameter.ParameterType?.Hint ?? Const.Unknown)
            .SetProperty(nameof(ParameterDefinition.IsDiscard), parameter.IsDiscard)
            .SetProperty(nameof(ParameterDefinition.RefKind), parameter.RefKind)
            .SetProperty(nameof(ParameterDefinition.Ordinal), parameter.Ordinal)
            .SetProperty(nameof(ParameterDefinition.IsParams), parameter.IsParams)
            .SetProperty(nameof(ParameterDefinition.IsOptional), parameter.IsOptional)
            .SetProperty(nameof(ParameterDefinition.IsThis), parameter.IsThis)
            .SetProperty(nameof(ParameterDefinition.HasExplicitDefaultValue), parameter.HasExplicitDefaultValue);

        if (parameter.ParameterType?.Token.HasValue == true)
        {
            builder.AddEdges(CSRelations.TypeOf, GetReferencedTypes(parameter.ParameterType)
                .Select(t => new Edge(parameter.Token, t)));
        }
    }

    public override void VisitEvent(EventDefinition @event)
    {
        base.VisitEvent(@event);

        builder.GetNode(@event.Token)
            .SetProperty(nameof(EventDefinition.EventType), @event.EventType?.Hint ?? Const.Unknown);

        if (@event.EventType?.Token.HasValue == true)
        {
            builder.AddEdges(CSRelations.TypeOf, GetReferencedTypes(@event.EventType)
                .Select(t => new Edge(@event.Token, t)));
        }

        if (@event.AddMethod?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.AssociatedWith, new Edge(@event.Token, @event.AddMethod.Token));
        }

        if (@event.RemoveMethod?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.AssociatedWith, new Edge(@event.Token, @event.RemoveMethod.Token));
        }

        if (@event.RaiseMethod?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.AssociatedWith, new Edge(@event.Token, @event.RaiseMethod.Token));
        }
    }

    public override void VisitProperty(PropertyDefinition property)
    {
        base.VisitProperty(property);

        builder.GetNode(property.Token)
            .SetProperty(nameof(PropertyDefinition.PropertyType), property.PropertyType?.Hint ?? Const.Unknown)
            .SetProperty(nameof(PropertyDefinition.IsIndexer), property.IsIndexer)
            .SetProperty(nameof(PropertyDefinition.IsReadOnly), property.IsReadOnly)
            .SetProperty(nameof(PropertyDefinition.IsWriteOnly), property.IsWriteOnly)
            .SetProperty(nameof(PropertyDefinition.IsReadOnly), property.IsRequired)
            .SetProperty(nameof(PropertyDefinition.RefKind), property.RefKind)
            .SetProperty(CSProperties.ParameterCount, property.Parameters.Length);

        builder.AddEdges(CSRelations.Declares, property.Parameters.Select(p => new Edge(property.Token, p.Token)));

        if (property.PropertyType?.Token.HasValue == true)
        {
            builder.AddEdges(CSRelations.TypeOf, GetReferencedTypes(property.PropertyType)
                .Select(t => new Edge(property.Token, t)));
        }

        if (property.OverriddenProperty?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.Overrides, new Edge(property.Token, property.OverriddenProperty.Token));
        }

        if (property.GetMethod?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.AssociatedWith, new Edge(property.Token, property.GetMethod.Token));
        }

        if (property.SetMethod?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.AssociatedWith, new Edge(property.Token, property.SetMethod.Token));
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
                : method.ReturnType?.Hint ?? Const.Unknown)
            .SetProperty(CSProperties.ParameterCount, method.Parameters.Length)
            .SetProperty(nameof(MethodDefinition.Arity), method.Arity)
            .SetProperty(nameof(MethodDefinition.IsReadOnly), method.IsReadOnly)
            .SetProperty(nameof(MethodDefinition.IsInitOnly), method.IsInitOnly);

        builder.AddEdges(CSRelations.Declares, method.Parameters.Select(p => new Edge(method.Token, p.Token)));
        builder.AddEdges(CSRelations.Declares, method.TypeParameters.Select(t => new Edge(method.Token, t.Token)));

        if (!method.ReturnsVoid && method.ReturnType?.Token.HasValue == true)
        {
            builder.AddEdges(CSRelations.Returns, GetReferencedTypes(method.ReturnType)
                .Select(t => new Edge(method.Token, t)));
        }

        if (method.OverridenMethod?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.Overrides, new Edge(method.Token, method.OverridenMethod.Token));
        }

        if (method.AssociatedEvent?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.AssociatedWith, new Edge(method.Token, method.AssociatedEvent.Token));
        }

        if (method.AssociatedProperty?.Token.HasValue == true)
        {
            builder.AddEdge(CSRelations.AssociatedWith, new Edge(method.Token, method.AssociatedProperty.Token));
        }
    }

    public override void VisitTypeParameter(TypeParameterDefinition typeParameter)
    {
        base.VisitTypeParameter(typeParameter);

        builder.GetNode(typeParameter.Token)
            .SetProperty(CSProperties.DeclaringKind, typeParameter.DeclaringMethod is not null
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

    private IEnumerable<NumericToken> GetReferencedTypes(TypeReference type)
    {
        yield return type.Token;
        foreach (var argument in type.TypeArguments)
        {
            foreach (var nested in GetReferencedTypes(argument))
            {
                // thankfully the reference is definitely finite because it couldn't be serialized otherwise
                yield return nested;
            }
        }
        switch (type)
        {
            case ArrayTypeReference arrayType:
                foreach (var token in GetReferencedTypes(arrayType.ElementType))
                {
                    yield return token;
                }
                break;
            case PointerTypeReference pointerType:
                foreach (var token in GetReferencedTypes(pointerType.PointedAtType))
                {
                    yield return token;
                }
                break;
            case FunctionPointerTypeReference fnPointerType:
                // TODO
                break;
        }
    }
}
