using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public class EntityLocator
{
    private readonly SolutionDefinition solution;

    public EntityLocator(SolutionDefinition solution)
    {
        this.solution = solution;
    }

    public ProjectDefinition? FindProject(string name)
    {
        return solution.Projects.SingleOrDefault(p => p.Name == name);
    }

    public PackageDefinition? FindPackage(string name)
    {
        return solution.Packages.SingleOrDefault(p => p.Name == name);
    }

    public AssemblyDefinition? FindAssembly(AssemblyId id)
    {
        return solution.Projects.Select(p => p.Assembly)
            .Concat(solution.Packages.SelectMany(p => p.Assemblies))
            .SingleOrDefault(a => a.Identity == id);
    }

    public ModuleDefinition? FindModule(string name)
    {
        return solution.Projects.Select(p => p.Assembly)
            .Concat(solution.Packages.SelectMany(p => p.Assemblies))
            .SelectMany(a => a.Modules)
            .SingleOrDefault(m => m.Name == name);
    }

    public NamespaceDefinition? FindNamespace(string moduleName, string? namespaceName)
    {
        var module = FindModule(moduleName);
        if (module is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(namespaceName))
        {
            return module.GlobalNamespace;
        }

        var segments = namespaceName?.Split('.') ?? Array.Empty<string>();
        var current = module.GlobalNamespace;
        foreach(var segment in segments)
        {
            current = current.Namespaces.SingleOrDefault(s => s.Name == segment);
            if (current is null)
            {
                return null;
            }
        }
        return current;
    }

    public TypeDefinition? FindType(string moduleName, string? namespaceName, string typeName, int arity)
    {
        var @namespace = FindNamespace(moduleName, namespaceName);
        if (@namespace is null)
        {
            return null;
        }

        return @namespace.Types.SingleOrDefault(t => t.Name == typeName && t.Arity == arity);
    }

}
