using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public class HelEntityRewriterCS
{
    public virtual HelSolutionCS RewriteSolution(HelSolutionCS solution)
    {
        return solution with
        {
            Packages = solution.Packages.Select(RewritePackage).ToImmutableArray(),
            Projects = solution.Projects.Select(RewriteProject).ToImmutableArray()
        };
    }

    public virtual HelPackageCS RewritePackage(HelPackageCS package)
    {
        return package with
        {
            Assemblies = package.Assemblies.Select(RewriteAssembly).ToImmutableArray()
        };
    }

    public virtual HelProjectCS RewriteProject(HelProjectCS project)
    {
        return project with
        {
            Assembly = RewriteAssembly(project.Assembly)
        };
    }

    public virtual HelAssemblyCS RewriteAssembly(HelAssemblyCS assembly)
    {
        return assembly with
        {
            Modules = assembly.Modules.Select(RewriteModule).ToImmutableArray()
        };
    }

    public virtual HelModuleCS RewriteModule(HelModuleCS module)
    {
        return module with
        {
            GlobalNamespace = RewriteNamespace(module.GlobalNamespace)
        };
    }

    public virtual HelNamespaceCS RewriteNamespace(HelNamespaceCS @namespace)
    {
        return @namespace with
        {
            Namespaces = @namespace.Namespaces.Select(RewriteNamespace).ToImmutableArray(),
            Types = @namespace.Types.Select(RewriteType).ToImmutableArray()
        };
    }

    public virtual HelTypeCS RewriteType(HelTypeCS type)
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

    public virtual HelFieldCS RewriteField(HelFieldCS field)
    {
        return field;
    }

    public virtual HelEventCS RewriteEvent(HelEventCS @event)
    {
        return @event;
    }

    public virtual HelPropertyCS RewriteProperty(HelPropertyCS property)
    {
        return property with
        {
            Parameters = property.Parameters.Select(RewriteParameter).ToImmutableArray()
        };
    }

    public virtual HelMethodCS RewriteMethod(HelMethodCS method)
    {
        return method with
        {
            Parameters = method.Parameters.Select(RewriteParameter).ToImmutableArray(),
            TypeParameters = method.TypeParameters.Select(RewriteTypeParameter).ToImmutableArray()
        };
    }

    public virtual HelParameterCS RewriteParameter(HelParameterCS parameter)
    {
        return parameter;
    }

    public virtual HelTypeParameterCS RewriteTypeParameter(HelTypeParameterCS typeParameter)
    {
        return typeParameter;
    }
}
