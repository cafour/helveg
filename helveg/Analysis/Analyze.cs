using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace Helveg.Analysis
{
    public static class Analyze
    {
        public const int BaseTypeWeight = 8;
        public const int NestedTypeWeight = 8;
        public const int InterfaceImplementationWeight = 4;
        public const int CompositionWeight = 2;
        public const int ReferenceWeight = 1;

        public static async Task<AnalyzedProject?> AnalyzeProject(
            FileInfo csprojFile,
            IDictionary<string, string> properties,
            ILogger? logger = null)
        {
            var workspace = MSBuildWorkspace.Create(properties);
            var project = await workspace.OpenProjectAsync(csprojFile.FullName);
            if (workspace.Diagnostics.Count != 0)
            {
                logger.LogDebug("MSBuildWorkspace reported the following diagnostics while loading the "
                    + $"'{csprojFile.Name}' project:");
                for (int i = 0; i < workspace.Diagnostics.Count; ++i)
                {
                    logger?.LogDebug(new EventId(0, "MSBuildWorkspace"), workspace.Diagnostics[i].Message);
                }
            }

            if (workspace.Diagnostics.Any(d => d.Kind == WorkspaceDiagnosticKind.Failure))
            {
                logger?.LogCritical($"Failed to load the '{csprojFile.Name}' project. " +
                    "Make sure it can be built with 'dotnet build'.");
                return null;
            }

            return await AnalyzeProject(project);
        }

        public static async Task<AnalyzedProject> AnalyzeProject(Project project)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation is null)
            {
                throw new InvalidOperationException(
                    $"A compilation could not be obtained for the '{project.Name}' project.");
            }
            var namespaceStack = new Stack<INamespaceSymbol>();
            namespaceStack.Push(compilation.Assembly.GlobalNamespace);
            var types = ImmutableDictionary.CreateBuilder<AnalyzedTypeId, AnalyzedType>();
            while (namespaceStack.Count > 0)
            {
                var currentNamespace = namespaceStack.Pop();
                foreach (var type in currentNamespace.GetTypeMembers())
                {
                    var typeStack = new Stack<INamedTypeSymbol>();
                    typeStack.Push(type);
                    while (typeStack.Count > 0)
                    {
                        var currentType = typeStack.Pop();
                        var analyzedType = AnalyzeType(currentType);
                        types.Add(analyzedType.Id, analyzedType);
                        foreach (var nestedType in currentType.GetTypeMembers())
                        {
                            typeStack.Push(nestedType);
                        }
                    }
                }
                foreach (var subnamespace in currentNamespace.GetNamespaceMembers())
                {
                    namespaceStack.Push(subnamespace);
                }
            }
            return new AnalyzedProject(project.Name, types.ToImmutable());
        }

        public static AnalyzedType AnalyzeType(INamedTypeSymbol type)
        {
            var relations = ImmutableDictionary.CreateBuilder<AnalyzedTypeId, int>();
            void IncrementRelation(ITypeSymbol current, int value)
            {
                if (!SymbolEqualityComparer.Default.Equals(current.ContainingAssembly, type.ContainingAssembly)
                    || current.Kind == SymbolKind.TypeParameter)
                {
                    return;
                }

                var id = current.GetAnalyzedId();
                if (!relations.ContainsKey(id))
                {
                    relations[id] = value;
                }
                else
                {
                    relations[id] += value;
                }
            }

            if (type.BaseType is object)
            {
                IncrementRelation(type.BaseType, BaseTypeWeight);
            }

            foreach(var @interface in type.Interfaces)
            {
                IncrementRelation(@interface, InterfaceImplementationWeight);
            }

            var members = ImmutableArray.CreateBuilder<AnalyzedMember>();
            foreach (var member in type.GetMembers())
            {
                members.Add(new AnalyzedMember(member.Name, Diagnosis.None));
                switch(member)
                {
                    case IPropertySymbol property:
                        IncrementRelation(property.Type, CompositionWeight);
                        break;
                    case IFieldSymbol field:
                        IncrementRelation(field.Type, CompositionWeight);
                        break;
                    case IMethodSymbol method:
                        IncrementRelation(method.ReturnType, ReferenceWeight);
                        foreach(var parameter in method.Parameters)
                        {
                            IncrementRelation(parameter.Type, ReferenceWeight);
                        }
                        break;
                    case IEventSymbol @event:
                        IncrementRelation(@event.Type, CompositionWeight);
                        break;
                    case ITypeSymbol nestedType:
                        IncrementRelation(nestedType, NestedTypeWeight);
                        break;
                }
            }
            return new AnalyzedType(
                id: type.GetAnalyzedId(),
                kind: type.GetAnalyzedKind(),
                health: Diagnosis.None,
                members: members.ToImmutable(),
                relations: relations.ToImmutable());
        }
    }
}
