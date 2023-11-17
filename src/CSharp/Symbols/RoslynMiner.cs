using Helveg.CSharp.Packages;
using Helveg.CSharp.Projects;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
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
    private bool isComparing = false;

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
        using var msbuildWorkspace = await OpenMSBuildWorkspace(solution.Path
            ?? solution.Projects.SingleOrDefault()?.Path, cancellationToken);
        if (msbuildWorkspace is null)
        {
            return;
        }

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

        MSBuildWorkspace? compareToMSBuildWorkspace = null;
        if (workspace.Source.CompareTo is not null)
        {
            compareToMSBuildWorkspace = await OpenMSBuildWorkspace(
                CSConst.FindSource(workspace.Source.CompareTo),
                cancellationToken);
            if (compareToMSBuildWorkspace is null)
            {
                logger.LogError("Failed to open the comparison target as a MSBuildWorkspace. Ignoring.");
            }

            isComparing = compareToMSBuildWorkspace is not null;
        }

        // mine each of the project for each target framework separately and likely in parallel
        await Task.WhenAll(matchedProjects.Select(p =>
        {
            var candidateCompareToProjects = compareToMSBuildWorkspace?.CurrentSolution.Projects
                .Where(r => r.AssemblyName == p.roslynProject.AssemblyName)
                .ToArray();
            MCA.Project? compareToProject = null;

            if (candidateCompareToProjects?.Length > 1)
            {
                logger.LogWarning(
                    "More than one matching comparison project found for '{}'. Taking the first ({}).",
                    p.roslynProject.Name,
                    candidateCompareToProjects[0].Name);
                compareToProject = candidateCompareToProjects[0];
            }
            else if (candidateCompareToProjects?.Length == 1)
            {
                compareToProject = candidateCompareToProjects[0];
            }

            return MineProject(p.project, p.roslynProject, compareToProject, cancellationToken);
        }));

        if (Options.ExternalSymbolAnalysisScope >= SymbolAnalysisScope.PublicApi)
        {
            // transcribe assemblies in each framework and external source
            await Task.WhenAll(workspace.Roots.Values.OfType<ILibrarySource>()
                .Select(async f => await MineLibrarySource(f.Token, cancellationToken)));
            await Task.WhenAll(workspace.Roots.Values.OfType<PackageRepository>()
                .Select(async f => await MinePackageRepository(f.Token, cancellationToken)));
        }

        compareToMSBuildWorkspace?.Dispose();
    }

    private async Task<MSBuildWorkspace?> OpenMSBuildWorkspace(
        string? path,
        CancellationToken cancellationToken = default)
    {
        var msbuildWorkspace = MCA.MSBuild.MSBuildWorkspace.Create(Options.MSBuildProperties);

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var extension = Path.GetExtension(path);

        // open the solution or project
        try
        {
            if (extension == CSConst.SolutionFileExtension)
            {
                await msbuildWorkspace.OpenSolutionAsync(path, cancellationToken: cancellationToken);
            }
            else if (extension == CSConst.ProjectFileExtension)
            {
                await msbuildWorkspace.OpenProjectAsync(path, cancellationToken: cancellationToken);
            }
            else
            {
                logger.LogError("No projects can be loaded because no solution or project paths are available.");
                return null;
            }
        }
        catch (Exception e)
        {
            logger.LogError("Failed to load a solution or a project.");
            logger.LogDebug(e, "MSBuildWorkspace threw an exception.");
            return null;
        }

        // log msbuild diagnostics both to console and to the entity
        LogMSBuildDiagnostics(msbuildWorkspace);
        return msbuildWorkspace;
    }

    private async Task MineProject(
        Project project,
        MCA.Project roslynProject,
        MCA.Project? compareToProject,
        CancellationToken cancellationToken = default)
    {
        var assembly = await GetProjectAssembly(project, roslynProject, compareToProject, cancellationToken);

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

    private async Task MineLibrarySource(NumericToken edsToken, CancellationToken cancellationToken)
    {
        using var dsHandle = await GetWorkspace().GetRootExclusively<ILibrarySource>(edsToken, cancellationToken);
        if (dsHandle.Entity is null)
        {
            logger.LogError("ExternalDependencySource '{}' has been removed during symbol mining.", edsToken);
            return;
        }

        var transcriber = new RoslynSymbolTranscriber(tokenMap, Options.ExternalSymbolAnalysisScope, logger);

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

    private async Task MinePackageRepository(NumericToken repoToken, CancellationToken cancellationToken)
    {
        using var repoHandle = await GetWorkspace().GetRootExclusively<PackageRepository>(repoToken, cancellationToken);
        if (repoHandle.Entity is null)
        {
            logger.LogError("PackageRepository '{}' has been removed during symbol mining.", repoToken);
            return;
        }

        var transcriber = new RoslynSymbolTranscriber(tokenMap, Options.ExternalSymbolAnalysisScope, logger);

        repoHandle.Entity = repoHandle.Entity with
        {
            Packages = repoHandle.Entity.Packages.Select(p =>
            {
                if (!p.Extensions.OfType<LibraryExtension>().Any())
                {
                    return p;
                }

                return p with
                {
                    Extensions = p.Extensions.Select(e =>
                    {
                        if (e is not LibraryExtension le || le.Library.Extensions.OfType<AssemblyExtension>().Any())
                        {
                            return e;
                        }

                        logger.LogDebug("Transcribing a '{}' dependency.", le.Library.Identity.Name);
                        le = le with
                        {
                            Library = le.Library with
                            {
                                Extensions = le.Library.Extensions.Add(new AssemblyExtension
                                {
                                    Assembly = transcriber.Transcribe(le.Library.Identity)
                                })
                            }
                        };
                        return le;
                    })
                    .ToImmutableArray()
                };
            })
            .ToImmutableArray()
        };
    }

    private async Task<AssemblyDefinition> GetProjectAssembly(
        Project project,
        MCA.Project roslynProject,
        MCA.Project? compareToProject,
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

        MCA.Compilation? compareToCompilation = null;
        var isCompareToCompilationTracked = true;
        if (compareToProject is not null)
        {
            compareToCompilation = await compareToProject.GetCompilationAsync(cancellationToken);
            if (compareToCompilation is not null)
            {
                var id = AssemblyId.Create(compilation.Assembly);
                var comparedId = AssemblyId.Create(compareToCompilation.Assembly);
                if (id == comparedId)
                {
                    logger.LogError("Compared assembly '{}' has the same id as the '{}' assembly. Comparison aborted.",
                        comparedId.ToDisplayString(),
                        id.ToDisplayString());
                    isCompareToCompilationTracked = false;
                }
                else
                {
                    tokenMap.TrackAndVisit(
                        compareToCompilation.Assembly,
                        project.Token,
                        compareToCompilation);
                    isCompareToCompilationTracked = true;
                }

            }
        }
    
        var transcriber = new RoslynSymbolTranscriber(tokenMap, Options.ProjectSymbolAnalysisScope, logger);
        logger.LogDebug("Transcribing the '{}' assembly.", compilation.Assembly.Name);
        return transcriber.Transcribe(
            AssemblyId.Create(compilation.Assembly),
            compareToCompilation,
            isComparing && compareToCompilation is null && isCompareToCompilationTracked ? DiffStatus.Added : null);
    }

    private void TrackAndVisitProject(Project project, MCA.Project roslynProject)
    {
        if (!roslynProject.TryGetCompilation(out var compilation))
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
            logger.LogError($"Failed to load the project or solution. "
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
