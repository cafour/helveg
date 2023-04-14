using Helveg.CSharp.Projects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public class RoslynMiner : IMiner
{
    private readonly ILogger<RoslynMiner> logger;

    private readonly SymbolTokenMap tokenMap;
    private readonly SymbolTrackingVisitor symbolVisitor;
    private readonly WeakReference<Workspace?> workspace = new(null);

    public RoslynMinerOptions Options { get; }

    MinerOptions IMiner.Options => Options;

    public RoslynMiner(RoslynMinerOptions options, ILogger<RoslynMiner>? logger = null)
    {
        Options = options;
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<RoslynMiner>();

        tokenMap = new();
        symbolVisitor = new(tokenMap);
    }

    public async Task Mine(Workspace workspace, CancellationToken cancellationToken = default)
    {
        this.workspace.SetTarget(workspace);

        var solution = workspace.Roots.Values.OfType<Solution>().Single();
        var msbuildWorkspace = Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace.Create(Options.MSBuildProperties);
        try
        {
            if (!string.IsNullOrEmpty(solution.Path))
            {
                await msbuildWorkspace.OpenSolutionAsync(solution.Path!, cancellationToken: cancellationToken);
            }
            else
            {
                var projectPath = solution.Projects.SingleOrDefault()?.Path;
                if (string.IsNullOrEmpty(projectPath))
                {
                    logger.LogCritical("No projects can be loaded because no solution or project paths are available.");
                    return;
                }

                await msbuildWorkspace.OpenProjectAsync(projectPath!, cancellationToken: cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogCritical("Failed to load a solution or a project.");
            logger.LogDebug(e, "MSBuildWorkspace threw an exception.");
            return;
        }
        LogMSBuildDiagnostics(msbuildWorkspace);

        var matchedProjects = solution.Projects.Join(msbuildWorkspace.CurrentSolution.Projects,
            p => p.Path ?? p.Name,
            p => p.FilePath ?? p.Name,
            (project, roslynProject) => (project, roslynProject));

        var projectAssemblies = (await Task.WhenAll(matchedProjects.Select(
            async p => (
                id: p.project.Id,
                assembly: await GetProjectAssembly(p.project, p.roslynProject, cancellationToken)))))
            .GroupBy(p => p.id)
            .ToDictionary(g => g.Key, g => g.Select(p => p.assembly));
        workspace.SetRoot(solution with
        {
            Diagnostics = solution.Diagnostics.AddRange(
                msbuildWorkspace.Diagnostics.Select(d => new Diagnostic(
                    Id: "MSBuildWorkspaceDiagnostic",
                    Message: d.Message,
                    Severity: d.Kind switch
                    {
                        Microsoft.CodeAnalysis.WorkspaceDiagnosticKind.Failure => DiagnosticSeverity.Error,
                        Microsoft.CodeAnalysis.WorkspaceDiagnosticKind.Warning => DiagnosticSeverity.Warning,
                        _ => DiagnosticSeverity.Info
                    }))
            ),
            Projects = solution.Projects
                .Select(p =>
                {
                    if (!projectAssemblies.TryGetValue(p.Id, out var assemblies))
                    {
                        logger.LogError("Could not map project '{}' onto a Roslyn project.", p.Name);
                        return p;
                    }

                    var extensions = p.Extensions.AddRange(assemblies.Select(a =>
                        new AssemblyExtension
                        {
                            Assembly = a with
                            {
                                ContainingProject = p.Id
                            }
                        }));
                    return p with
                    {
                        Extensions = p.Extensions.AddRange(extensions)
                    };
                })
                .ToImmutableArray()
        });

        var frameworkDeps = solution.Projects.SelectMany(p => p.Dependencies.SelectMany(d => d.Value))
            .Distinct()
            .GroupBy(d => d.Framework.ToString())
            .ToDictionary(g => g.Key, g => g);

        foreach (var f in workspace.Roots.Values.OfType<Framework>())
        {
            var framework = f;
            if (frameworkDeps.TryGetValue(framework.Token, out var deps))
            {
                var transcribedDeps = TranscribeDependencies(deps);
                foreach (var dep in transcribedDeps)
                {
                    if (framework.Extensions.OfType<AssemblyExtension>().Any(a => a.Assembly.Identity == dep.Identity))
                    {
                        continue;
                    }
                    framework = framework with
                    {
                        Extensions = framework.Extensions.Add(new AssemblyExtension
                        {
                            Assembly = dep
                        })
                    };
                }
                workspace.SetRoot(framework);
            }
        }
    }

    private async Task<AssemblyDefinition> GetProjectAssembly(
        Project project,
        Microsoft.CodeAnalysis.Project roslynProject,
        CancellationToken cancellationToken = default)
    {
        var compilation = await roslynProject.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            logger.LogError("Failed to compile the '{}' project.", roslynProject.Name);
            return AssemblyDefinition.Invalid;
        }

        logger.LogInformation("Found the '{}' assembly.", compilation.Assembly.GetHelvegAssemblyId().ToDisplayString());

        // TODO: Classify references into BCL, Packages, and Other.
        tokenMap.Track(compilation.Assembly.GetHelvegAssemblyId(), project.Token, compilation);

        var targetFramework = ParseTargetFramework(roslynProject.Name) ?? project.Dependencies.Keys.FirstOrDefault();

        var diagnostics = new List<Diagnostic>();

        if (project.Dependencies.TryGetValue(targetFramework, out var dependencies))
        {
            var referenceIds = compilation.References.Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<Microsoft.CodeAnalysis.IAssemblySymbol>()
                .Select(a => (assembly: a, id: a.GetHelvegAssemblyId()))
                .ToImmutableArray();
            var dependencyReferencePairs = dependencies.GroupJoin(
                referenceIds,
                d => (d.Name, d.FileVersion),
                r => (r.id.Name, r.id.FileVersion),
                (d, r) => (dependency: d, references: r.ToImmutableArray()))
                .ToImmutableArray();

            foreach (var (dependency, references) in dependencyReferencePairs)
            {
                if (references.Length == 0 || references.Length > 1)
                {
                    diagnostics.Add(Diagnostic.Error(
                        "DependencyResolutionFailure",
                        $"Dependency '{dependency.Name}' could not be resolved."));
                    logger.LogError("Could not resolve dependency '{}' of project '{}' for target framework '{}'.",
                        dependency.Name, project.Name, targetFramework);
                    continue;
                }

                var reference = references[0];

                if (dependency.Framework.HasValue)
                {
                    tokenMap.Track(reference.id, dependency.Framework, compilation);
                }
                else
                {
                    logger.LogDebug("Ignoring '{}' for now.", dependency);
                }
            }
        }
        else
        {
            diagnostics.Add(Diagnostic.Error(
                "TargetFrameworkResolutionFailure",
                $"Failed to resolve dependencies for target framework '{targetFramework}'."));
            logger.LogError(
                "Dependencies of '{}' for target framework '{}' could not be resolved.",
                project.Name,
                targetFramework);
        }


        // Phase 1: Discover all symbols within the assembly.
        logger.LogDebug("Visiting the '{}' assembly.", compilation.Assembly.GetHelvegAssemblyId().ToDisplayString());
        symbolVisitor.VisitAssembly(compilation.Assembly);

        foreach (var reference in compilation.References)
        {
            var referenceSymbol = compilation.GetAssemblyOrModuleSymbol(reference);
            if (referenceSymbol is not Microsoft.CodeAnalysis.IAssemblySymbol referencedAssembly)
            {
                var diagnostic = Diagnostic.Error(
                    "MissingAssemblySymbol",
                    $"Could not obtain an assembly symbol for a metadata reference of '{roslynProject.Name}'. " +
                    $"The reference is: '{reference}'.");
                diagnostics.Add(diagnostic);
                logger.LogWarning(diagnostic.Message);
                continue;
            }

            logger.LogDebug("Visiting a '{}' reference.", referencedAssembly.GetHelvegAssemblyId().ToDisplayString());
            symbolVisitor.VisitAssembly(referencedAssembly);
        }

        // Phase 2: Transcribe the assembly into Helveg's structures.
        logger.LogDebug("Transcribing the '{}' assembly.", compilation.Assembly.GetHelvegAssemblyId().ToDisplayString());
        var transcriber = new RoslynSymbolTranscriber(tokenMap);
        return transcriber.Transcribe(compilation.Assembly.GetHelvegAssemblyId());
    }

    private ImmutableArray<AssemblyDefinition> TranscribeDependencies(IEnumerable<AssemblyDependency> dependencies)
    {
        var transcriber = new RoslynSymbolTranscriber(tokenMap);
        return dependencies.Select(d =>
        {
            logger.LogDebug("Transcribing a '{}' dependency.", d.Name);
            return transcriber.Transcribe(d);
        }).ToImmutableArray();
    }

    private void LogMSBuildDiagnostics(Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace workspace)
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
                logger.LogDebug(workspace.Diagnostics[i].Message);
            }
        }

        if (workspace.Diagnostics.Any(d => d.Kind == Microsoft.CodeAnalysis.WorkspaceDiagnosticKind.Failure))
        {
            logger.LogCritical($"Failed to load the project or solution. "
                + "Make sure it can be built with 'dotnet build'.");
        }
    }

    private string? ParseTargetFramework(string projectName)
    {
        var match = Regex.Match(projectName, @"^.+\((.+)\)$");
        if (!match.Success)
        {
            return null;
        }

        return match.Groups[1].Value;
    }
}
