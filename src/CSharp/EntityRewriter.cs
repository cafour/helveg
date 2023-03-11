using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

/// <summary>
/// A visitor of the <see cref="EntityDefinition"/> tree that can make changes to the entities.
/// </summary>
public abstract class EntityRewriter
{
    public virtual SolutionDefinition RewriteSolution(SolutionDefinition solution)
    {
        return solution with
        {
            Packages = solution.Packages.Select(RewritePackage).ToImmutableArray(),
            Projects = solution.Projects.Select(RewriteProject).ToImmutableArray()
        };
    }

    public virtual PackageDefinition RewritePackage(PackageDefinition package)
    {
        return package with
        {
            Assemblies = package.Assemblies.Select(RewriteAssembly).ToImmutableArray()
        };
    }

    public virtual ProjectDefinition RewriteProject(ProjectDefinition project)
    {
        return project with
        {
            Assembly = RewriteAssembly(project.Assembly)
        };
    }

    public virtual AssemblyDefinition RewriteAssembly(AssemblyDefinition assembly)
    {
        return assembly with
        {
            Modules = assembly.Modules.Select(RewriteModule).ToImmutableArray()
        };
    }

    public virtual ModuleDefinition RewriteModule(ModuleDefinition module)
    {
        return module with
        {
            GlobalNamespace = RewriteNamespace(module.GlobalNamespace)
        };
    }

    public virtual NamespaceDefinition RewriteNamespace(NamespaceDefinition @namespace)
    {
        return @namespace with
        {
            Namespaces = @namespace.Namespaces.Select(RewriteNamespace).ToImmutableArray(),
            Types = @namespace.Types.Select(RewriteType).ToImmutableArray()
        };
    }

    public virtual TypeDefinition RewriteType(TypeDefinition type)
    {
        return type with
        {
            Fields = type.Fields.Select(RewriteField).ToImmutableArray(),
            Events = type.Events.Select(RewriteEvent).ToImmutableArray(),
            Properties = type.Properties.Select(RewriteProperty).ToImmutableArray(),
            Methods = type.Methods.Select(RewriteMethod).ToImmutableArray(),
            TypeParameters = type.TypeParameters.Select(RewriteTypeParameter).ToImmutableArray(),
            NestedTypes = type.NestedTypes.Select(RewriteType).ToImmutableArray()
        };
    }

    public virtual FieldDefinition RewriteField(FieldDefinition field)
    {
        return field;
    }

    public virtual EventDefinition RewriteEvent(EventDefinition @event)
    {
        return @event;
    }

    public virtual PropertyDefinition RewriteProperty(PropertyDefinition property)
    {
        return property with
        {
            Parameters = property.Parameters.Select(RewriteParameter).ToImmutableArray()
        };
    }

    public virtual MethodDefinition RewriteMethod(MethodDefinition method)
    {
        return method with
        {
            Parameters = method.Parameters.Select(RewriteParameter).ToImmutableArray(),
            TypeParameters = method.TypeParameters.Select(RewriteTypeParameter).ToImmutableArray()
        };
    }

    public virtual ParameterDefinition RewriteParameter(ParameterDefinition parameter)
    {
        return parameter;
    }

    public virtual TypeParameterDefinition RewriteTypeParameter(TypeParameterDefinition typeParameter)
    {
        return typeParameter;
    }
}
