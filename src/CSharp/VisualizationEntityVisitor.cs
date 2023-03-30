using Helveg.Visualization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public class VisualizationEntityVisitor : EntityVisitor
{
    private string name = CSharpConstants.InvalidName;
    private readonly ImmutableArray<Node>.Builder nodes = ImmutableArray.CreateBuilder<Node>();
    private readonly ImmutableArray<Edge>.Builder implementsRelation = ImmutableArray.CreateBuilder<Edge>();
    private readonly ImmutableArray<Edge>.Builder isComposedOfRelation = ImmutableArray.CreateBuilder<Edge>();
    private readonly ImmutableArray<Edge>.Builder containsRelation = ImmutableArray.CreateBuilder<Edge>();

    public Multigraph Build()
    {
        return new Multigraph(
            Id: name,
            Label: name,
            Nodes: nodes.ToImmutable(),
            Relations: ImmutableArray.Create(
                new Relation("implements", "Implements", implementsRelation.ToImmutable()),
                new Relation("isComposedOf", "Is composed of", isComposedOfRelation.ToImmutable()),
                new Relation("contains", "Contains", containsRelation.ToImmutable())
            )
        );
    }

    protected override void DefaultVisit(IEntityDefinition entity)
    {
        var node = new Node(entity.Token, entity.Name, ImmutableDictionary.CreateRange(
            new KeyValuePair<string, string>[]
            {
                new("EntityKind", entity.Token.Kind.ToString())
            }));
        if (entity is IMemberDefinition member)
        {
            node = node with
            {
                Properties = node.Properties.AddRange(new KeyValuePair<string, string>[]
                {
                    new(nameof(member.Accessibility), member.Accessibility.ToString()),
                    new(nameof(member.IsSealed), member.IsSealed.ToString()),
                    new(nameof(member.IsStatic), member.IsStatic.ToString()),
                    new(nameof(member.IsAbstract), member.IsAbstract.ToString()),
                    new(nameof(member.IsExtern), member.IsExtern.ToString()),
                    new(nameof(member.IsOverride), member.IsOverride.ToString()),
                    new(nameof(member.IsVirtual), member.IsVirtual.ToString()),
                    new(nameof(member.IsImplicitlyDeclared), member.IsImplicitlyDeclared.ToString()),
                    new(nameof(member.CanBeReferencedByName), member.CanBeReferencedByName.ToString()),
                })
            };
        }

        nodes.Add(node);
    }

    protected override void VisitWorkspace(EntityWorkspace workspace)
    {
        name = workspace.Name;
        base.VisitWorkspace(workspace);
    }

    protected override void VisitSolution(SolutionDefinition solution)
    {
        containsRelation.AddRange(solution.Projects.Select(p => new Edge(solution.Token, p.Token)));

        base.VisitSolution(solution);
    }

    protected override void VisitProject(ProjectDefinition project)
    {
        containsRelation.Add(new Edge(project.Token, project.Assembly.Token));

        base.VisitProject(project);
    }

    protected override void VisitAssembly(AssemblyDefinition assembly)
    {
        containsRelation.AddRange(assembly.Modules.Select(m => new Edge(assembly.Token, m.Token)));

        base.VisitAssembly(assembly);
    }

    protected override void VisitModule(ModuleDefinition module)
    {
        containsRelation.Add(new Edge(module.Token, module.GlobalNamespace.Token));

        base.VisitModule(module);
    }

    protected override void VisitNamespace(NamespaceDefinition @namespace)
    {
        containsRelation.AddRange(@namespace.Namespaces.Select(n => new Edge(@namespace.Token, n.Token)));
        containsRelation.AddRange(@namespace.Types.Select(t => new Edge(@namespace.Token, t.Token)));

        base.VisitNamespace(@namespace);
    }

    protected override void VisitType(TypeDefinition type)
    {
        containsRelation.AddRange(type.Fields.Select(f => new Edge(type.Token, f.Token)));
        containsRelation.AddRange(type.Events.Select(e => new Edge(type.Token, e.Token)));
        containsRelation.AddRange(type.Properties.Select(p => new Edge(type.Token, p.Token)));
        containsRelation.AddRange(type.Methods.Select(m => new Edge(type.Token, m.Token)));
        containsRelation.AddRange(type.TypeParameters.Select(t => new Edge(type.Token, t.Token)));
        containsRelation.AddRange(type.NestedTypes.Select(t => new Edge(type.Token, t.Token)));

        implementsRelation.AddRange(type.Interfaces.Select(i => new Edge(type.Token, i.Token)));
        if (type.BaseType is not null)
        {
            implementsRelation.Add(new Edge(type.Token, type.BaseType.Token));
        }

        isComposedOfRelation.AddRange(type.Fields.Select(f => new Edge(type.Token, f.Token)));

        base.VisitType(type);
    }

    protected override void VisitProperty(PropertyDefinition property)
    {
        containsRelation.AddRange(property.Parameters.Select(p => new Edge(property.Token, p.Token)));

        base.VisitProperty(property);
    }

    protected override void VisitMethod(MethodDefinition method)
    {
        containsRelation.AddRange(method.Parameters.Select(p => new Edge(method.Token, p.Token)));
        containsRelation.AddRange(method.TypeParameters.Select(t => new Edge(method.Token, t.Token)));

        base.VisitMethod(method);
    }
}
