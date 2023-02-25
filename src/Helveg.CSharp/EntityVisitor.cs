using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

/// <summary>
/// A visitor of the entity tree.
/// </summary>
public abstract class EntityVisitor
{
    public virtual void DefaultVisit(IEntityDefinition entity)
    {
    }

    public virtual void VisitSolution(SolutionDefinition solution)
    {
        DefaultVisit(solution);

        foreach (var package in solution.Packages)
        {
            VisitPackage(package);
        }

        foreach (var project in solution.Projects)
        {
            VisitProject(project);
        }
    }

    public virtual void VisitPackage(PackageDefinition package)
    {
        DefaultVisit(package);

        foreach (var assembly in package.Assemblies)
        {
            VisitAssembly(assembly);
        }
    }

    public virtual void VisitProject(ProjectDefinition project)
    {
        DefaultVisit(project);

        VisitAssembly(project.Assembly);
    }

    public virtual void VisitAssembly(AssemblyDefinition assembly)
    {
        DefaultVisit(assembly);

        foreach (var module in assembly.Modules)
        {
            VisitModule(module);
        }
    }

    public virtual void VisitModule(ModuleDefinition module)
    {
        DefaultVisit(module);

        VisitNamespace(module.GlobalNamespace);
    }

    public virtual void VisitNamespace(NamespaceDefinition @namespace)
    {
        DefaultVisit(@namespace);

        foreach (var subnamespace in @namespace.Namespaces)
        {
            VisitNamespace(subnamespace);
        }

        foreach (var type in @namespace.Types)
        {
            VisitType(type);
        }
    }

    public virtual void VisitType(TypeDefinition type)
    {
        DefaultVisit(type);

        foreach (var field in type.Fields)
        {
            VisitField(field);
        }

        foreach (var @event in type.Events)
        {
            VisitEvent(@event);
        }

        foreach (var property in type.Properties)
        {
            VisitProperty(property);
        }

        foreach (var method in type.Methods)
        {
            VisitMethod(method);
        }

        foreach(var typeParameter in type.TypeParameters)
        {
            VisitTypeParameter(typeParameter);
        }

        foreach(var nestedType in type.NestedTypes)
        {
            VisitType(nestedType);
        }
    }

    public virtual void VisitField(FieldDefinition field)
    {
        DefaultVisit(field);
    }

    public virtual void VisitEvent(EventDefinition @event)
    {
        DefaultVisit(@event);
    }

    public virtual void VisitProperty(PropertyDefinition property)
    {
        DefaultVisit(property);

        foreach(var parameter in property.Parameters)
        {
            VisitParameter(parameter);
        }
    }

    public virtual void VisitMethod(MethodDefinition method)
    {
        DefaultVisit(method);

        foreach(var parameter in method.Parameters)
        {
            VisitParameter(parameter);
        }

        foreach(var typeParameter in method.TypeParameters)
        {
            VisitTypeParameter(typeParameter);
        }
    }

    public virtual void VisitParameter(ParameterDefinition parameter)
    {
        DefaultVisit(parameter);
    }

    public virtual void VisitTypeParameter(TypeParameterDefinition typeParameter)
    {
        DefaultVisit(typeParameter);
    }
}
