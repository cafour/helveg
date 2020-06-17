using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Helveg.Analysis
{
    public static class Analyze
    {
        public const int BaseTypeWeight = 100;
        public const int NestedTypeWeight = 100;
        public const int InterfaceImplementationWeight = 50;
        public const int CompositionWeight = 10;
        public const int ReferenceWeight = 1;

        public static async Task<AnalyzedProject> AnalyzeProject(string csprojPath)
        {
            var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(csprojPath);
            if (workspace.Diagnostics.Count != 0)
            {
                var sb = new StringBuilder("The project failed to load with the following MSBuild diagnostics:");
                sb.AppendLine();

                for (int i = 0; i < workspace.Diagnostics.Count; ++i)
                {
                    sb.AppendLine($"[{i}]: {workspace.Diagnostics[i].Kind}:");
                    sb.AppendLine($"\t{workspace.Diagnostics[i].Message}");
                }
                throw new InvalidOperationException(sb.ToString());
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

        public static (string[] names, int[,] graph) ConstructGraph(string projectPath)
        {
            var workspace = MSBuildWorkspace.Create();
            var project = workspace.OpenProjectAsync(projectPath).GetAwaiter().GetResult();
            foreach (var diagnostic in workspace.Diagnostics)
            {
                Console.Write($"[{diagnostic.Kind}]: {diagnostic.Message}\n\n");
            }
            Console.WriteLine($"Total MSBuild Diagnostics: {workspace.Diagnostics.Count}");
            if (!workspace.Diagnostics.IsEmpty)
            {
                return (new string[0], new int[0, 0]);
            }

            var compilation = project.GetCompilationAsync().GetAwaiter().GetResult();
            if (compilation is null)
            {
                return (new string[0], new int[0, 0]);

            }

            var weights = new Dictionary<(string from, string to), int>();

            var stack = new Stack<INamespaceSymbol>();
            stack.Push(compilation.Assembly.GlobalNamespace);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                foreach (var type in current.GetTypeMembers())
                {
                    if (type.BaseType is object && SymbolEqualityComparer.Default.Equals(type.BaseType.ContainingAssembly, compilation.Assembly))
                    {
                        var key = (type.MetadataName, type.BaseType.MetadataName);
                        if (!weights.ContainsKey(key))
                        {
                            weights[key] = 0;
                        }
                        weights[(type.MetadataName, type.BaseType.MetadataName)] += BaseTypeWeight;
                    }

                    foreach (var member in type.GetMembers())
                    {
                        ISymbol? relevantSymbol = null;
                        switch (member)
                        {
                            case IPropertySymbol property:
                                relevantSymbol = property.Type;
                                break;
                            case IFieldSymbol field:
                                relevantSymbol = field.Type;
                                break;
                            case IMethodSymbol method:
                                relevantSymbol = method.ReturnType;
                                break;
                            case IEventSymbol eventSymbol:
                                relevantSymbol = eventSymbol.Type;
                                break;
                            case ITypeSymbol nestedType:
                                // TODO: Handle nested types properly. Their members are currently not processed.
                                relevantSymbol = nestedType;
                                break;
                        }
                        if (relevantSymbol is object && SymbolEqualityComparer.Default.Equals(relevantSymbol.ContainingAssembly, compilation.Assembly))
                        {
                            var key = (type.MetadataName, relevantSymbol.MetadataName);
                            if (!weights.ContainsKey(key))
                            {
                                weights[key] = 0;
                            }
                            weights[(type.MetadataName, relevantSymbol.MetadataName)] += CompositionWeight;
                        }
                    }
                }

                foreach (var subnamespace in current.GetNamespaceMembers())
                {
                    stack.Push(subnamespace);
                }
            }
            var names = weights.Keys.SelectMany(k => new[] { k.from, k.to }).Distinct().OrderBy(k => k).ToArray();
            var matrix = new int[names.Length, names.Length];
            for (int f = 0; f < names.Length; ++f)
            {
                for (int t = 0; t < names.Length; ++t)
                {
                    if (f == t)
                    {
                        continue;
                    }

                    if (weights.TryGetValue((names[f], names[t]), out var weight))
                    {
                        matrix[f, t] = weight;
                    }
                }
            }
            return (names, matrix);
        }
    }
}
