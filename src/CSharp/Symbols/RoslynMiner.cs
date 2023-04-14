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
using MCA = Microsoft.CodeAnalysis;

namespace Helveg.CSharp.Symbols;

public class RoslynMiner : IMiner
{
    private readonly ILogger<RoslynMiner> logger;

    private readonly SymbolTokenMap tokenMap;
    private readonly SymbolTrackingVisitor symbolVisitor;
    private readonly WeakReference<Workspace?> workspaceRef = new(null);

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
        workspaceRef.SetTarget(workspace);

        var solutions = workspace.Roots.Values.OfType<Solution>().ToArray();
        if (solutions.Length == 0)
        {
            logger.LogError("The workspace contains no Solution therefore no symbols can be mined.");
            return;
        }
        else if (solutions.Length > 1)
        {
            logger.LogError("The workspace contains multiple solutions but RoslynMiner can only mine one.");
            return;
        }

        var solution = solutions[0];
        var msbuildWorkspace = MCA.MSBuild.MSBuildWorkspace.Create(Options.MSBuildProperties);

        // open the solution or project
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

        // log msbuild diagnostics both to console and to the entity
        LogMSBuildDiagnostics(msbuildWorkspace);
        using (var solutionHandle = await workspace.GetRootExclusively<Solution>(solution.Id, cancellationToken))
        {
            if (solutionHandle.Entity is null)
            {
                logger.LogError("The solution has been removed from the workspace while the miner was running.");
                return;
            }

            solutionHandle.Entity = solutionHandle.Entity with
            {
                Diagnostics = solutionHandle.Entity.Diagnostics.AddRange(
                    msbuildWorkspace.Diagnostics.Select(d => new Diagnostic(
                        Id: "MSBuildWorkspaceDiagnostic",
                        Message: d.Message,
                        Severity: d.Kind switch
                        {
                            MCA.WorkspaceDiagnosticKind.Failure => DiagnosticSeverity.Error,
                            MCA.WorkspaceDiagnosticKind.Warning => DiagnosticSeverity.Warning,
                            _ => DiagnosticSeverity.Info
                        })))
            };
        }

        // match helveg projects with roslyn projects
        var matchedProjects = solution.Projects.Join(msbuildWorkspace.CurrentSolution.Projects,
            p => p.Path ?? p.Name,
            p => p.FilePath ?? p.Name,
            (project, roslynProject) => (project, roslynProject));

        // mine each of the project for each target framework separately and likely in parallel
        await Task.WhenAll(matchedProjects.Select(p => MineProject(p.project, p.roslynProject, cancellationToken)));

        if (Options.ExternalSymbolAnalysisScope >= SymbolAnalysisScope.PublicApi)
        {
            // transcribe assemblies in each framework and external source
            await Task.WhenAll(workspace.Roots.Values.OfType<IDependencySource>()
                .Select(async f => await MineDependencySource(f.Token, cancellationToken)));
        }
    }

    private async Task MineProject(
        Project project,
        MCA.Project roslynProject,
        CancellationToken cancellationToken = default)
    {
        var assembly = await GetProjectAssembly(project, roslynProject, cancellationToken);

        using var solutionHandle = await GetWorkspace().GetRootExclusively<Solution>(project.ContainingSolution, cancellationToken);

        if (solutionHandle.Entity is null)
        {
            logger.LogError("The solution has been removed from the workspace while the miner was running.");
            return;
        }

        var oldProject = solutionHandle.Entity.Projects.SingleOrDefault(p => p.Token == project.Token);
        if (oldProject is null)
        {
            logger.LogError("Project '{}' has been removed during symbol mining.", project.Name);
            return;
        }

        if (Options.ProjectSymbolAnalysisScope >= SymbolAnalysisScope.PublicApi)
        {
            solutionHandle.Entity = solutionHandle.Entity with
            {
                Projects = solutionHandle.Entity.Projects.Replace(oldProject, oldProject with
                {
                    Extensions = oldProject.Extensions.Add(new AssemblyExtension
                    {
                        Assembly = assembly with
                        {
                            ContainingEntity = oldProject.Token
                        }
                    })
                })
            };
        }
    }

    private async Task MineDependencySource(NumericToken dsToken, CancellationToken cancellationToken)
    {
        using var dsHandle = await GetWorkspace().GetRootExclusively<IDependencySource>(dsToken, cancellationToken);
        if (dsHandle.Entity is null)
        {
            logger.LogError("Dependency source '{}' has been removed during symbol mining.", dsToken);
            return;
        }

        var transcriber = new RoslynSymbolTranscriber(tokenMap, Options.ExternalSymbolAnalysisScope);

        var assemblies = dsHandle.Entity.Assemblies.Select(a =>
            {
                if (a.Extensions.OfType<AssemblyExtension>().Any())
                {
                    return a;
                }

                logger.LogDebug("Transcribing a '{}' dependency.", a.Identity.Name);
                return a with
                {
                    Extensions = a.Extensions.Add(new AssemblyExtension
                    {
                        Assembly = transcriber.Transcribe(a)
                    })
                };
            })
            .ToImmutableArray();

        dsHandle.Entity = dsHandle.Entity.WithAssemblies(assemblies);
    }

    private async Task<AssemblyDefinition> GetProjectAssembly(
        Project project,
        MCA.Project roslynProject,
        CancellationToken cancellationToken = default)
    {
        var workspace = GetWorkspace();

        var compilation = await roslynProject.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            logger.LogError("Failed to compile the '{}' project.", roslynProject.Name);
            return AssemblyDefinition.Invalid;
        }

        logger.LogInformation("Found the '{}' assembly.", compilation.Assembly.Name);

        tokenMap.Track(AssemblyId.Create(compilation.Assembly), project.Token, compilation);

        var targetFramework = ParseTargetFramework(roslynProject.Name) ?? project.Dependencies.Keys.FirstOrDefault();
        if (string.IsNullOrEmpty(targetFramework))
        {
            logger.LogError("The '{}' project does not have a target framework.", roslynProject.Name);
            return AssemblyDefinition.Invalid;
        }

        var diagnostics = new List<Diagnostic>();

        if (project.Dependencies.TryGetValue(targetFramework, out var dependencyTokens))
        {
            var dependencies = dependencyTokens
                .Select(t => workspace.Entities.GetValueOrDefault(t) as AssemblyDependency)
                .Where(d => d is not null)
                .Cast<AssemblyDependency>()
                .ToImmutableArray();

            var referenceIds = compilation.References.Select(r =>
                {
                    if (compilation.GetAssemblyOrModuleSymbol(r) is not MCA.IAssemblySymbol assembly)
                    {
                        return AssemblyId.Invalid;
                    }
                    return AssemblyId.Create(assembly, r as MCA.PortableExecutableReference);
                })
                .Where(a => a.IsValid)
                .ToImmutableArray();
            var dependencyReferenceGroups = dependencies.GroupJoin(
                referenceIds,
                d => d.Identity,
                r => r,
                (d, r) => (dependency: d, references: r.ToImmutableArray()))
                .ToImmutableArray();

            foreach (var (dependency, references) in dependencyReferenceGroups)
            {
                if (references.Length == 0 || references.Length > 1)
                {
                    diagnostics.Add(Diagnostic.Error(
                        "DependencyResolutionFailure",
                        $"Dependency '{dependency.Identity.Name}' could not be resolved."));
                    logger.LogError("Could not resolve dependency '{}' of project '{}' for target framework '{}'.",
                        dependency.Identity.Name, project.Name, targetFramework);
                    continue;
                }

                tokenMap.Track(references[0], dependency.Token, compilation);
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
        logger.LogDebug("Visiting the '{}' assembly.", compilation.Assembly.Name);
        symbolVisitor.VisitAssembly(compilation.Assembly);

        foreach (var reference in compilation.References)
        {
            var referenceSymbol = compilation.GetAssemblyOrModuleSymbol(reference);
            if (referenceSymbol is not MCA.IAssemblySymbol referencedAssembly)
            {
                var diagnostic = Diagnostic.Error(
                    "MissingAssemblySymbol",
                    $"Could not obtain an assembly symbol for a metadata reference of '{roslynProject.Name}'. " +
                    $"The reference is: '{reference}'.");
                diagnostics.Add(diagnostic);
                logger.LogWarning(diagnostic.Message);
                continue;
            }

            logger.LogDebug("Visiting a '{}' reference.", referencedAssembly.Name);
            symbolVisitor.VisitAssembly(referencedAssembly);
        }

        // Phase 2: Transcribe the assembly into Helveg's structures.
        logger.LogDebug("Transcribing the '{}' assembly.", compilation.Assembly.Name);
        var transcriber = new RoslynSymbolTranscriber(tokenMap, Options.ProjectSymbolAnalysisScope);
        return transcriber.Transcribe(AssemblyId.Create(compilation.Assembly));
    }

    private void LogMSBuildDiagnostics(MCA.MSBuild.MSBuildWorkspace workspace)
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

        if (workspace.Diagnostics.Any(d => d.Kind == MCA.WorkspaceDiagnosticKind.Failure))
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

    private Workspace GetWorkspace()
    {
        if (!workspaceRef.TryGetTarget(out var workspace) || workspace is null)
        {
            throw new InvalidOperationException("Cannot get a framework without a workspace. This is likely a bug.");
        }
        return workspace;
    }
}
