using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

/// <summary>
/// A visitor of the <see cref="EntityDefinition"/> tree.
/// </summary>
public abstract class EntityVisitor
{
    public void Visit(EntityWorkspace workspace)
    {
        VisitWorkspace(workspace);
    }

    public void Visit(IEntityDefinition entity)
    {
        switch (entity)
        {
            case SolutionDefinition solution:
                VisitSolution(solution);
                break;
            case ProjectDefinition project:
                VisitProject(project);
                break;
            case PackageDefinition package:
                VisitPackage(package);
                break;
            case AssemblyDefinition assembly:
                VisitAssembly(assembly);
                break;
            case ModuleDefinition module:
                VisitModule(module);
                break;
            case NamespaceDefinition @namespace:
                VisitNamespace(@namespace);
                break;
            case TypeParameterDefinition typeParameter:
                VisitTypeParameter(typeParameter);
                break;
            case TypeDefinition type:
                VisitType(type);
                break;
            case FieldDefinition field:
                VisitField(field);
                break;
            case EventDefinition @event:
                VisitEvent(@event);
                break;
            case PropertyDefinition property:
                VisitProperty(property);
                break;
            case MethodDefinition method:
                VisitMethod(method);
                break;
            case ParameterDefinition parameter:
                VisitParameter(parameter);
                break;
            default:
                throw new NotSupportedException($"Entity type '{entity.GetType()}' is not supported.");
        }
    }

    protected virtual void DefaultVisit(IEntityDefinition entity)
    {
    }

    protected virtual void VisitWorkspace(EntityWorkspace workspace)
    {
        VisitSolution(workspace.Solution);
    }

    protected virtual void VisitSolution(SolutionDefinition solution)
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

    protected virtual void VisitPackage(PackageDefinition package)
    {
        DefaultVisit(package);

        foreach (var assembly in package.Assemblies)
        {
            VisitAssembly(assembly);
        }
    }

    protected virtual void VisitProject(ProjectDefinition project)
    {
        DefaultVisit(project);

        VisitAssembly(project.Assembly);
    }

    protected virtual void VisitAssembly(AssemblyDefinition assembly)
    {
        DefaultVisit(assembly);

        foreach (var module in assembly.Modules)
        {
            VisitModule(module);
        }
    }

    protected virtual void VisitModule(ModuleDefinition module)
    {
        DefaultVisit(module);

        VisitNamespace(module.GlobalNamespace);
    }

    protected virtual void VisitNamespace(NamespaceDefinition @namespace)
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

    protected virtual void VisitType(TypeDefinition type)
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

    protected virtual void VisitField(FieldDefinition field)
    {
        DefaultVisit(field);
    }

    protected virtual void VisitEvent(EventDefinition @event)
    {
        DefaultVisit(@event);
    }

    protected virtual void VisitProperty(PropertyDefinition property)
    {
        DefaultVisit(property);

        foreach(var parameter in property.Parameters)
        {
            VisitParameter(parameter);
        }
    }

    protected virtual void VisitMethod(MethodDefinition method)
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

    protected virtual void VisitParameter(ParameterDefinition parameter)
    {
        DefaultVisit(parameter);
    }

    protected virtual void VisitTypeParameter(TypeParameterDefinition typeParameter)
    {
        DefaultVisit(typeParameter);
    }
}
