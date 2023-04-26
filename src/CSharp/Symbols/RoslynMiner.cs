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
    private readonly WeakReference<Workspace?> workspaceRef = new(null);

    public RoslynMinerOptions Options { get; }

    MinerOptions IMiner.Options => Options;

    public RoslynMiner(RoslynMinerOptions options, ILogger<RoslynMiner>? logger = null)
    {
        Options = options;
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<RoslynMiner>();

        tokenMap = new(this.logger);
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
            await Task.WhenAll(workspace.Roots.Values.OfType<ILibrarySource>()
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
        using var dsHandle = await GetWorkspace().GetRootExclusively<ILibrarySource>(dsToken, cancellationToken);
        if (dsHandle.Entity is null)
        {
            logger.LogError("Dependency source '{}' has been removed during symbol mining.", dsToken);
            return;
        }

        var transcriber = new RoslynSymbolTranscriber(tokenMap, Options.ExternalSymbolAnalysisScope);

        var assemblies = dsHandle.Entity.Libraries.Select(l =>
            {
                if (l.Extensions.OfType<AssemblyExtension>().Any())
                {
                    return l;
                }

                logger.LogDebug("Transcribing a '{}' dependency.", l.Identity.Name);
                return l with
                {
                    Extensions = l.Extensions.Add(new AssemblyExtension
                    {
                        Assembly = transcriber.Transcribe(l.Identity)
                    })
                };
            })
            .ToImmutableArray();

        dsHandle.Entity = dsHandle.Entity.WithLibraries(assemblies);
    }

    private async Task<AssemblyDefinition> GetProjectAssembly(
        Project project,
        MCA.Project roslynProject,
        CancellationToken cancellationToken = default)
    {
        var compilation = await roslynProject.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            logger.LogError("Failed to compile the '{}' project.", roslynProject.Name);
            return AssemblyDefinition.Invalid;
        }

        logger.LogInformation("Found the '{}' assembly.", compilation.Assembly.Name);

        TrackAndVisitProject(project, roslynProject);

        var transcriber = new RoslynSymbolTranscriber(tokenMap, Options.ProjectSymbolAnalysisScope);
        logger.LogDebug("Transcribing the '{}' assembly.", compilation.Assembly.Name);
        return transcriber.Transcribe(AssemblyId.Create(compilation.Assembly));
    }

    private void TrackAndVisitProject(Project project, MCA.Project roslynProject)
    {
        if(!roslynProject.TryGetCompilation(out var compilation))
        {
            return;
        }

        // 1. Track the project's own assembly.
        tokenMap.TrackAndVisit(compilation.Assembly, project.Token, compilation);

        // 2. Figure out what set of dependencies to use.
        var targetFramework = ParseTargetFramework(roslynProject.Name);

        if (string.IsNullOrEmpty(targetFramework) && project.Dependencies.Count == 1)
        {
            // Since MSBuildWorkspace omits the " (<TargetFramework>)" project name suffix when there's just one
            // target framework, assign the only one directly.
            targetFramework = project.Dependencies.Keys.SingleOrDefault();
        }

        if (string.IsNullOrEmpty(targetFramework))
        {
            logger.LogError("Cannot track dependencies of the '{}' project " +
                "because its target framework could not be resolved.", roslynProject.Name);
            return;
        }

        if (!project.Dependencies.TryGetValue(targetFramework, out var dependencies))
        {
            logger.LogError("No dependencies for the '{}' target framework were mined for the '{}' project but " +
                "MSBuildWorkspace references them.", targetFramework, project.Name);
        }

        var workspace = GetWorkspace();

        var dependencyDict = dependencies.ToImmutableDictionary(d => d.Identity);

        // 3. Track the project's dependencies.
        foreach (var reference in compilation.References)
        {
            var symbol = compilation.GetAssemblyOrModuleSymbol(reference);
            if (symbol is not MCA.IAssemblySymbol assemblySymbol)
            {
                logger.LogDebug("Ignoring the '{}' reference of '{}' because it " +
                    "cannot be resolved to an IAssemblySymbol.",
                    reference.Display,
                    project.Name);
                continue;
            }
                
            var id = AssemblyId.Create(assemblySymbol, reference as MCA.PortableExecutableReference);

            if (!dependencyDict.TryGetValue(id, out var dependency))
            {
                logger.LogTrace("Cannot track Roslyn dependency of '{}' because it wasn't mined previously.",
                    id.Name);
                continue;
            }

            var dependencyContainer = workspace.Entities.GetValueOrDefault(dependency.Token);

            if (dependencyContainer is not Project
                && Options.ExternalSymbolAnalysisScope == SymbolAnalysisScope.None)
            {
                logger.LogTrace("Ignoring the '{}' reference of '{}' because analysis of external symbols " +
                    "is disabled.",
                    reference.Display,
                    project.Name);
                continue;
            }

            tokenMap.TrackAndVisit(assemblySymbol, dependency.Token, compilation);
        }
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
