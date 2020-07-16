using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Helveg.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Helveg.Analysis
{
    public static class Analyze
    {
        public const int BaseTypeWeight = 8;
        public const int NestedTypeWeight = 8;
        public const int InterfaceImplementationWeight = 4;
        public const int CompositionWeight = 2;
        public const int ReferenceWeight = 1;

        public static async Task<AnalyzedSolution?> AnalyzeProjectOrSolution(
            FileInfo file,
            IDictionary<string, string> properties,
            ILogger? logger = null,
            SerializableSolution? cache = null)
        {
            logger ??= NullLogger.Instance;

            var workspace = MSBuildWorkspace.Create(properties);
            Solution? solution = null;
            Project? project = null;
            try
            {
                switch (file.Extension)
                {
                    case ".sln":
                        solution = await workspace.OpenSolutionAsync(file.FullName);
                        break;
                    case ".csproj":
                        project = await workspace.OpenProjectAsync(file.FullName);
                        break;
                    default:
                        logger.LogCritical($"File extension '{file.Extension}' is not supported.");
                        return null;
                }
            }
            catch(Exception e)
            {
                logger.LogCritical($"MSBuild failed to load the '{file}' project or solution. "
                    + "Run with '--verbose' for more information.");
                logger.LogDebug(e, "MSBuildWorkspace threw an exception.");
                return null;
            }
            LogMSBuildDiagnostics(workspace, logger);
            if (workspace.Diagnostics.Any(d => d.Kind == WorkspaceDiagnosticKind.Failure))
            {
                return null;
            }

            if (solution is object)
            {
                return await AnalyzeSolution(solution, logger, cache);
            }

            if (project is object)
            {
                SerializableProject? projectCache = null;
                if (cache is object)
                {
                    cache.Projects?.TryGetValue(project.Name, out projectCache);
                }
                var analyzedProject = await AnalyzeProject(project, logger, projectCache);
                if (analyzedProject is null)
                {
                    return null;
                }
                return new AnalyzedSolution(
                    path: string.Empty,
                    name: string.Empty,
                    projects: ImmutableDictionary.CreateRange(new[]
                    {
                        new KeyValuePair<string, AnalyzedProject>(project.Name, analyzedProject.Value)
                    }));
            }

            throw new NotImplementedException(); // wtf is happenning?
        }

        public static async Task<AnalyzedSolution?> AnalyzeSolution(
            Solution solution,
            ILogger? logger = null,
            SerializableSolution? cache = null)
        {
            logger ??= NullLogger.Instance;

            var name = Path.GetFileNameWithoutExtension(solution.FilePath) ?? string.Empty;
            if (cache is object && (solution.FilePath != cache.Path || name != cache.Name))
            {
                logger.LogDebug("Analysis cache is unrelated. Discarding.");
                cache = null;
            }

            var projectBuilder = ImmutableDictionary.CreateBuilder<string, AnalyzedProject>();
            foreach (var project in solution.Projects)
            {
                SerializableProject? projectCache = null;
                if (cache is object && !(cache!.Projects?.TryGetValue(project.Name, out projectCache) ?? false))
                {
                    logger.LogDebug($"Analysis cache doesn't contain '{project.Name}'.");
                }

                var analyzedProject = await AnalyzeProject(project, logger, projectCache);
                if (analyzedProject is object)
                {
                    projectBuilder.Add(analyzedProject.Value.Name, analyzedProject.Value);
                }
            }
            return new AnalyzedSolution(
                path: solution.FilePath ?? string.Empty,
                name: name,
                projects: projectBuilder.ToImmutable());
        }

        public static async Task<AnalyzedProject?> AnalyzeProject(
            Project project,
            ILogger? logger = null,
            SerializableProject? cache = null)
        {
            logger ??= NullLogger.Instance;

            var lwt = GetProjectLastWriteTime(project);
            if (cache is object)
            {
                if (cache.Name == project.Name
                    && cache.Path == project.FilePath
                    && lwt == cache.LastWriteTime)
                {
                    logger.LogInformation($"Using cached analysis for '{project.Name}'.");
                    return cache.ToAnalyzed();
                }
                logger.LogDebug($"Analysis of '{project.Name}' is outdated or irrelevant. Discarding.");
            }

            logger.LogInformation($"Analyzing '{project.Name}'.");
            var compilation = await project.GetCompilationAsync();
            if (compilation is null)
            {
                logger.LogError($"A Compilation could not be produced for the '{project.Name}' project.");
                return null;
            }

            var refCounter = new ReferenceCountingAnalyzer();
            var withAnalyzersOptions = new CompilationWithAnalyzersOptions(
                options: new AnalyzerOptions(ImmutableArray.Create<AdditionalText>()),
                onAnalyzerException: (e, a, d) =>
                {
                    logger.LogError($"The '{a.GetType().Name}' analyzer failed during "
                        + $"analysis of the '{project.Name}' project.");
                    logger.LogDebug(e, $"'{a.GetType().Name}' failed.");
                },
                concurrentAnalysis: true,
                logAnalyzerExecutionTime: false);
            var withAnalyzers = new CompilationWithAnalyzers(
                compilation,
                ImmutableArray.Create<DiagnosticAnalyzer>(refCounter),
                withAnalyzersOptions);
            var diagnostics = await withAnalyzers.GetAllDiagnosticsAsync();

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
                        var existingRelations = refCounter.ReferenceCounts
                            .GetValueOrDefault(currentType.GetAnalyzedId());
                        var analyzedType = AnalyzeType(currentType, existingRelations);
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

            foreach (var diagnostic in diagnostics)
            {
                if (diagnostic.Location.SourceTree is null)
                {
                    continue;
                }
                var semanticModel = compilation.GetSemanticModel(
                    diagnostic.Location.SourceTree,
                    ignoreAccessibility: true);
                var mid = (diagnostic.Location.SourceSpan.Start + diagnostic.Location.SourceSpan.End) / 2;
                var symbol = semanticModel.GetEnclosingSymbol(mid);
                var typeSymbol = symbol is INamedTypeSymbol t ? t : symbol.ContainingType;
                if (typeSymbol is object && types.TryGetValue(typeSymbol.GetAnalyzedId(), out var analyzedType))
                {
                    types[analyzedType.Id] = analyzedType.WithHealth(
                        analyzedType.Health | diagnostic.Severity.GetDiagnosis());
                }
            }

            var projectReferences = project.ProjectReferences.Select(r => project.Solution.GetProject(r.ProjectId))
                .Where(p => p is object)
                .Select(p => p!.Name)
                .ToImmutableHashSet();
            var packageReferences = project.MetadataReferences.Where(r => r.Display is object)
                .Select(r => r.Display!)
                .ToImmutableHashSet();
            return new AnalyzedProject(
                project.FilePath ?? string.Empty,
                project.Name,
                types.ToImmutable(),
                projectReferences,
                packageReferences,
                lastWriteTime: lwt);
        }

        public static AnalyzedType AnalyzeType(INamedTypeSymbol type, IDictionary<AnalyzedTypeId, int>? existing)
        {
            var relations = ImmutableDictionary.CreateBuilder<AnalyzedTypeId, int>();
            if (existing is object)
            {
                relations.AddRange(existing);
            }
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

            foreach (var @interface in type.Interfaces)
            {
                IncrementRelation(@interface, InterfaceImplementationWeight);
            }

            int memberCount = 0;
            foreach (var member in type.GetMembers())
            {
                if (member.IsImplicitlyDeclared)
                {
                    continue;
                }

                memberCount++;
                switch (member)
                {
                    case IPropertySymbol property:
                        IncrementRelation(property.Type, CompositionWeight);
                        break;
                    case IFieldSymbol field:
                        IncrementRelation(field.Type, CompositionWeight);
                        break;
                    case IMethodSymbol method:
                        IncrementRelation(method.ReturnType, ReferenceWeight);
                        foreach (var parameter in method.Parameters)
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
                memberCount: memberCount,
                relations: relations.ToImmutable());
        }

        public static AnalyzedTypeGraph GetTypeGraph(AnalyzedProject project)
        {
            var graph = new AnalyzedTypeGraph
            {
                Ids = project.Types.Keys.OrderBy(k => k.ToString()).ToArray(),
                Weights = new int[project.Types.Count, project.Types.Count],
                Sizes = new int[project.Types.Count],
                ProjectName = project.Name,
                Positions = new Vector2[project.Types.Count]
            };

            for (int i = 0; i < graph.Ids.Length; ++i)
            {
                var type = project.Types[graph.Ids[i]];
                graph.Sizes[i] = type.MemberCount;
                foreach (var (friend, weight) in type.Relations)
                {
                    // TODO: This shouldn't happen, but I don't have time to fix it right now.
                    var friendIndex = Array.IndexOf(graph.Ids, friend);
                    if (friendIndex != -1)
                    {
                        graph.Weights[i, friendIndex] += weight;
                    }
                }
            }

            return graph;
        }

        private static void LogMSBuildDiagnostics(MSBuildWorkspace workspace, ILogger logger)
        {
            if (workspace.Diagnostics.IsEmpty)
            {
                return;
            }

            logger.LogDebug("MSBuildWorkspace reported the following diagnostics.");
            foreach (var diagnostic in workspace.Diagnostics)
            {
                for (int i = 0; i < workspace.Diagnostics.Count; ++i)
                {
                    logger.LogDebug(new EventId(0, "MSBuildWorkspace"), workspace.Diagnostics[i].Message);
                }
            }

            if (workspace.Diagnostics.Any(d => d.Kind == WorkspaceDiagnosticKind.Failure))
            {
                logger.LogCritical($"Failed to load the project or solution. "
                    + "Make sure it can be built with 'dotnet build'.");
            }
        }

        private static DateTime GetProjectLastWriteTime(Project project)
        {
            var lastWriteTime = DateTime.MinValue;
            foreach (var document in project.Documents)
            {
                if (document.FilePath is object)
                {
                    var documentLwt = File.GetLastWriteTimeUtc(document.FilePath);
                    if (lastWriteTime < documentLwt)
                    {
                        lastWriteTime = documentLwt;
                    }
                }
            }
            return lastWriteTime;
        }
    }
}
