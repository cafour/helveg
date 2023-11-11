using Helveg.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public class VisualizationSymbolVisitor : SymbolVisitor
{
    private readonly Multigraph graph;

    public VisualizationSymbolVisitor(Multigraph graph)
    {
        this.graph = graph;
    }

    public override void DefaultVisit(ISymbolDefinition symbol)
    {
        var node = graph.GetNode<CSharpNode>(symbol.Token, symbol.Name);
        node.Kind = CSConst.KindOf(symbol.GetType());
        node.Diagnostics = symbol.Diagnostics.Select(d => d.ToMultigraphDiagnostic()).ToList();

        if (symbol is IMemberDefinition member)
        {
            node.Accessibility = member.Accessibility;
            node.IsSealed = member.IsSealed;
            node.IsStatic = member.IsStatic;
            node.IsAbstract = member.IsAbstract;
            node.IsExtern = member.IsExtern;
            node.IsOverride = member.IsOverride;
            node.IsVirtual = member.IsVirtual;
            node.IsImplicitlyDeclared = member.IsImplicitlyDeclared;
            node.CanBeReferencedByName = member.CanBeReferencedByName;
        }
    }

    public override void VisitAssembly(AssemblyDefinition assembly)
    {
        base.VisitAssembly(assembly);

        var node = graph.GetNode<CSharpNode>(assembly.Name);
        node.Version = assembly.Identity.Version;
        node.FileVersion = assembly.Identity.FileVersion;
        node.CultureName = assembly.Identity.CultureName;
        node.PublicKeyToken = assembly.Identity.PublicKeyToken;
        node.TargetFramework = assembly.Identity.TargetFramework;

        graph.AddEdges(
            CSRelations.Declares,
            assembly.Modules.Select(m => new MultigraphEdge(assembly.Token, m.Token)),
            true);
    }

    public override void VisitModule(ModuleDefinition module)
    {
        base.VisitModule(module);
        graph.AddEdge(
            CSRelations.Declares,
            new MultigraphEdge(module.Token, module.GlobalNamespace.Token),
            true);

        graph.AddEdges(
            CSRelations.References,
            module.ReferencedAssemblies
                .Where(a => a.Token.HasValue)
                .Select(a => new MultigraphEdge(module.Token, a.Token)));
    }

    public override void VisitNamespace(NamespaceDefinition @namespace)
    {
        if (!@namespace.GetAllTypes().Any())
        {
            // ignore namespace without any types in it or any subnamespaces
            return;
        }

        if (@namespace.IsGlobalNamespace)
        {
            graph.GetNode<CSharpNode>(@namespace.Token, CSConst.GlobalNamespaceName);
        }

        base.VisitNamespace(@namespace);


        graph.AddEdges(
            CSRelations.Declares,
            @namespace.Namespaces.Select(n => new MultigraphEdge(@namespace.Token, n.Token)),
            true);
        graph.AddEdges(
            CSRelations.Declares,
            @namespace.Types.Select(t => new MultigraphEdge(@namespace.Token, t.Token)),
            true);
    }

    public override void VisitType(TypeDefinition type)
    {
        base.VisitType(type);

        var node = graph.GetNode<CSharpNode>(type.Token);
        node.IsNested = type.IsNested;
        node.TypeKind = type.TypeKind;
        node.IsAnonymousType = type.IsAnonymousType;
        node.IsTupleType = type.IsTupleType;
        node.IsNativeIntegerType = type.IsNativeIntegerType;
        node.IsUnmanagedType = type.IsUnmanagedType;
        node.IsReadOnly = type.IsReadOnly;
        node.IsRefLikeType = type.IsRefLikeType;
        node.IsRecord = type.IsRecord;
        node.Arity = type.Arity;
        node.IsImplicitClass = type.IsImplicitClass;

        var (instanceCount, staticCount) = CountMembers(type);
        node.InstanceMemberCount = instanceCount;
        node.StaticMemberCount = staticCount;

        graph.AddEdges(
            CSRelations.Declares,
            type.Fields.Select(f => new MultigraphEdge(type.Token, f.Token)),
            true);
        graph.AddEdges(
            CSRelations.Declares,
            type.Events.Select(e => new MultigraphEdge(type.Token, e.Token)),
            true);
        graph.AddEdges(
            CSRelations.Declares,
            type.Properties.Select(p => new MultigraphEdge(type.Token, p.Token)),
            true);
        graph.AddEdges(
            CSRelations.Declares,
            type.Methods.Select(m => new MultigraphEdge(type.Token, m.Token)),
            true);
        graph.AddEdges(
            CSRelations.Declares,
            type.TypeParameters.Select(t => new MultigraphEdge(type.Token, t.Token)),
            true);
        graph.AddEdges(
            CSRelations.Declares,
            type.NestedTypes.Select(t => new MultigraphEdge(type.Token, t.Token)),
            true);

        graph.AddEdges(
            CSRelations.InheritsFrom,
            type.Interfaces
                .Where(i => i.Token.HasValue)
                .Select(i => new MultigraphEdge(type.Token, i.Token)));

        if (type.BaseType?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.InheritsFrom,
                new MultigraphEdge(type.Token, type.BaseType.Token));
        }
    }

    public override void VisitField(FieldDefinition field)
    {
        base.VisitField(field);

        var node = graph.GetNode<CSharpNode>(field.Token);
        node.IsReadOnly = field.IsReadOnly;
        node.IsVolatile = field.IsVolatile;
        node.IsConst = field.IsConst;
        node.IsEnumItem = field.IsEnumItem;
        node.RefKind = field.RefKind;
        node.FieldType = field.FieldType?.Hint ?? Const.Unknown;

        if (field.FieldType?.Token.HasValue == true)
        {
            graph.AddEdges(CSRelations.TypeOf, GetReferencedTypes(field.FieldType)
                .Select(t => new MultigraphEdge(field.Token, t)));
        }

        if (field.AssociatedEvent?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.AssociatedWith,
                new MultigraphEdge(field.Token, field.AssociatedEvent.Token));
        }

        if (field.AssociatedProperty?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.AssociatedWith,
                new MultigraphEdge(field.Token, field.AssociatedProperty.Token));
        }
    }

    public override void VisitParameter(ParameterDefinition parameter)
    {
        base.VisitParameter(parameter);

        var node = graph.GetNode<CSharpNode>(parameter.Token);
        node.ParameterType = parameter.ParameterType?.Hint ?? Const.Unknown;
        node.IsDiscard = parameter.IsDiscard;
        node.RefKind = parameter.RefKind;
        node.Ordinal = parameter.Ordinal;
        node.IsParams = parameter.IsParams;
        node.IsOptional = parameter.IsOptional;
        node.IsThis = parameter.IsThis;
        node.HasExplicitDefaultValue = parameter.HasExplicitDefaultValue;

        if (parameter.ParameterType?.Token.HasValue == true)
        {
            graph.AddEdges(CSRelations.TypeOf, GetReferencedTypes(parameter.ParameterType)
                .Select(t => new MultigraphEdge(parameter.Token, t)));
        }
    }

    public override void VisitEvent(EventDefinition @event)
    {
        base.VisitEvent(@event);

        var node = graph.GetNode<CSharpNode>(@event.Token);
        node.EventType = @event.EventType?.Hint ?? Const.Unknown;

        if (@event.EventType?.Token.HasValue == true)
        {
            graph.AddEdges(CSRelations.TypeOf, GetReferencedTypes(@event.EventType)
                .Select(t => new MultigraphEdge(@event.Token, t)));
        }

        if (@event.AddMethod?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.AssociatedWith,
                new MultigraphEdge(@event.Token, @event.AddMethod.Token));
        }

        if (@event.RemoveMethod?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.AssociatedWith,
                 new MultigraphEdge(@event.Token, @event.RemoveMethod.Token));
        }

        if (@event.RaiseMethod?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.AssociatedWith,
                new MultigraphEdge(@event.Token, @event.RaiseMethod.Token));
        }
    }

    public override void VisitProperty(PropertyDefinition property)
    {
        base.VisitProperty(property);

        var node = graph.GetNode<CSharpNode>(property.Token);
        node.PropertyType = property.PropertyType?.Hint ?? Const.Unknown;
        node.IsIndexer = property.IsIndexer;
        node.IsReadOnly = property.IsReadOnly;
        node.IsWriteOnly = property.IsWriteOnly;
        node.IsReadOnly = property.IsRequired;
        node.RefKind = property.RefKind;
        node.ParameterCount = property.Parameters.Length;

        graph.AddEdges(
            CSRelations.Declares,
            property.Parameters.Select(p => new MultigraphEdge(property.Token, p.Token)),
            true);

        if (property.PropertyType?.Token.HasValue == true)
        {
            graph.AddEdges(CSRelations.TypeOf, GetReferencedTypes(property.PropertyType)
                .Select(t => new MultigraphEdge(property.Token, t)));
        }

        if (property.OverriddenProperty?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.Overrides,
                new MultigraphEdge(property.Token, property.OverriddenProperty.Token));
        }

        if (property.GetMethod?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.AssociatedWith,
                new MultigraphEdge(property.Token, property.GetMethod.Token));
        }

        if (property.SetMethod?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.AssociatedWith,
                new MultigraphEdge(property.Token, property.SetMethod.Token));
        }
    }

    public override void VisitMethod(MethodDefinition method)
    {
        base.VisitMethod(method);

        var node = graph.GetNode<CSharpNode>(method.Token);
        node.MethodKind = method.MethodKind;
        node.IsExtensionMethod = method.IsExtensionMethod;
        node.IsAsync = method.IsAsync;
        node.ReturnType = method.ReturnsVoid
                ? "void"
                : method.ReturnType?.Hint ?? Const.Unknown;
        node.ParameterCount = method.Parameters.Length;
        node.Arity = method.Arity;
        node.IsReadOnly = method.IsReadOnly;
        node.IsInitOnly = method.IsInitOnly;

        graph.AddEdges(
            CSRelations.Declares,
            method.Parameters.Select(p => new MultigraphEdge(method.Token, p.Token)),
            true);
        graph.AddEdges(
            CSRelations.Declares,
            method.TypeParameters.Select(t => new MultigraphEdge(method.Token, t.Token)),
            true);

        if (!method.ReturnsVoid && method.ReturnType?.Token.HasValue == true)
        {
            graph.AddEdges(CSRelations.Returns, GetReferencedTypes(method.ReturnType)
                .Select(t => new MultigraphEdge(method.Token, t)),
                true);
        }

        if (method.OverridenMethod?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.Overrides,
                new MultigraphEdge(method.Token, method.OverridenMethod.Token),
                true);
        }

        if (method.AssociatedEvent?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.AssociatedWith,
                new MultigraphEdge(method.Token, method.AssociatedEvent.Token),
                true);
        }

        if (method.AssociatedProperty?.Token.HasValue == true)
        {
            graph.AddEdge(
                CSRelations.AssociatedWith,
                new MultigraphEdge(method.Token, method.AssociatedProperty.Token),
                true);
        }
    }

    public override void VisitTypeParameter(TypeParameterDefinition typeParameter)
    {
        base.VisitTypeParameter(typeParameter);

        var node = graph.GetNode<CSharpNode>(typeParameter.Token);
        node.DeclaringKind = typeParameter.DeclaringMethod is not null
                ? CSConst.KindOf<MethodDefinition>()
                : CSConst.KindOf<TypeDefinition>();
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
