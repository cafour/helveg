using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Helveg
{
    public static class Analyze
    {
        public const int InheritanceWeight = 100;
        public const int CompositionWeight = 10;
        public const int ReferenceWeight = 1;

        public static (string[] names, float[,] graph) ConstructGraph(string projectPath)
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
                return (new string[0], new float[0, 0]);
            }

            var compilation = project.GetCompilationAsync().GetAwaiter().GetResult();
            if (compilation is null)
            {
                return (new string[0], new float[0, 0]);

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
                        weights[(type.MetadataName, type.BaseType.MetadataName)] += InheritanceWeight;
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
            var names = weights.Keys.SelectMany(k => new [] {k.from, k.to}).Distinct().OrderBy(k => k).ToArray();
            var matrix = new float[names.Length, names.Length];
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
