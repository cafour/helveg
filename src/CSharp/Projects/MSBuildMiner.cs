using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MSB = Microsoft.Build;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;

namespace Helveg.CSharp.Projects;

public class MSBuildMiner : IMiner, IDisposable
{
    private readonly ILogger<MSBuildMiner> logger;
    private readonly MSB.Execution.BuildManager buildManager = new();

    private int counter = 0;
    private WeakReference<Workspace?> workspaceRef = new(null);
    private NumericToken edsToken
        = NumericToken.CreateNone(CSConst.CSharpNamespace, (int)RootKind.ExternalDependencySource);

    public MSBuildMinerOptions Options { get; }

    MinerOptions IMiner.Options => Options;

    public MSBuildMiner(MSBuildMinerOptions options, ILogger<MSBuildMiner>? logger = null)
    {
        Options = options;
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<MSBuildMiner>();
    }

    public async Task Mine(Workspace workspace, CancellationToken cancellationToken = default)
    {
        workspaceRef = new(workspace);

        if (Options.IncludeExternalDependencies)
        {
            var eds = new ExternalDependencySource
            {
                Index = Interlocked.Increment(ref counter),
                Name = "External Dependencies"
            };
            if (!workspace.TryAddRoot(eds))
            {
                logger.LogError("Failed to add an external dependency source root to the workspace.");
            }
            else
            {
                edsToken = eds.Token;
            }
        }

        // 1. Find a solution file and discover projects in it.
        var solution = GetSolution(workspace.Source.Path, cancellationToken);
        if (solution is null)
        {
            return;
        }

        // 1.2 Find the ComparedTo solution file and check if it has the same name.
        Solution? compareToSolution = null;
        if (workspace.Source.CompareTo is not null)
        {
            compareToSolution = GetSolution(workspace.Source.CompareTo, cancellationToken);
            if (compareToSolution is null)
            {
                logger.LogError("The CompareTo solution at '{}' could no be loaded.", workspace.Source.CompareTo);
            }
            else
            {
                logger.LogInformation(
                    "Comparing the '{}' solution with the '{}' solution.",
                    solution.Path,
                    compareToSolution.Path);

                if (compareToSolution.Name != solution.Name)
                {
                    solution = solution with { DiffStatus = DiffStatus.Added };
                    compareToSolution = compareToSolution with { DiffStatus = DiffStatus.Deleted };
                }
            }
        }

        if (!workspace.TryAddRoot(solution))
        {
            logger.LogError("Failed to add the '{}' root to the mining workspace.", solution.Id);
        }

        if (compareToSolution is not null && !workspace.TryAddRoot(compareToSolution))
        {
            logger.LogError("Failed to add the CompareTo solution '{}' to the workspace.", compareToSolution.Id);
        }

        // 2. Mine the projects in the solution.
        // NB: All projects in the solution need to exist so that they can be referenced by each other.
        await MineSolutionProjects(solution.Token, cancellationToken);

        if (compareToSolution is not null)
        {
            await MineSolutionProjects(compareToSolution.Token, cancellationToken);

            using var solutionHandle = await workspace.GetRootExclusively<Solution>(
                solution.Token,
                cancellationToken);
            solution = solutionHandle.Entity!;

            using var compareToSolutionHandle = await workspace.GetRootExclusively<Solution>(
                compareToSolution.Token,
                cancellationToken);
            compareToSolution = compareToSolutionHandle.Entity!;

            var addedProjects = solution.Projects.ExceptBy(compareToSolution.Projects.Select(p => p.Name), p => p.Name);
            foreach (var addedProject in addedProjects)
            {
                solution = solution with
                {
                    Projects = solution.Projects.Replace(
                        addedProject,
                        addedProject with { DiffStatus = DiffStatus.Added })
                };
            }

            var removedProjects = compareToSolution.Projects.ExceptBy(
                compareToSolution.Projects.Select(p => p.Name),
                p => p.Name);
            foreach (var removedProject in removedProjects)
            {
                solution = solution with
                {
                    Projects = solution.Projects.Add(removedProject with
                    {
                        DiffStatus = DiffStatus.Deleted,
                        ContainingSolution = solution.Token
                    })
                };
            }
            solutionHandle.Entity = solution;
            compareToSolutionHandle.Entity = null;
        }

        return;
    }

    private Solution? GetSolution(string path, CancellationToken cancellationToken = default)
    {
        var targetFile = CSConst.FindSource(path, logger);
        if (targetFile is null)
        {
            return null;
        }
        path = targetFile;

        var fileExtension = Path.GetExtension(path);

        var solution = new Solution
        {
            Index = Interlocked.Increment(ref counter),
            Path = fileExtension == CSConst.SolutionFileExtension ? path : null,
            Name = Path.GetFileNameWithoutExtension(path)
        };

        // the solution is synthetic and contains only one project
        if (fileExtension == CSConst.ProjectFileExtension)
        {
            return solution with
            {
                Projects = ImmutableArray.Create(
                    new Project
                    {
                        Index = Interlocked.Increment(ref counter),
                        ContainingSolution = solution.Token,
                        Path = path,
                        Name = Path.GetFileNameWithoutExtension(path)
                    })
            };
        }

        // the solution is real, so we have to parse the projects out of it

        var msbuildSolution = MSB.Construction.SolutionFile.Parse(path);
        logger.LogInformation("Found the '{}' solution.", solution.Name);

        solution = solution with
        {
            Projects = msbuildSolution.ProjectsInOrder
                .Where(p => p.ProjectType != MSB.Construction.SolutionProjectType.SolutionFolder)
                .Select(p => new Project
                {
                    Index = Interlocked.Increment(ref counter),
                    ContainingSolution = solution.Token,
                    Path = p.AbsolutePath,
                    Name = p.ProjectName
                })
                .ToImmutableArray()
        };

        return solution;
    }

    private async Task MineSolutionProjects(NumericToken solutionToken, CancellationToken cancellationToken = default)
    {
        var workspace = GetWorkspace();
        using var solutionHandle = await workspace.GetRootExclusively<Solution>(solutionToken, cancellationToken);
        if (solutionHandle.Entity is null)
        {
            logger.LogError("Cannot mine projects inside the '{}' solution because it has been deleted. " +
                "This is likely because of a race condition.", solutionToken);
            return;
        }


        try
        {
            var projectCollection = new MSB.Evaluation.ProjectCollection(Options.MSBuildProperties);
            var buildParams = new MSB.Execution.BuildParameters(projectCollection)
            {
                Loggers = new[]
                {
                    new MSBuildDiagnosticLogger(logger)
                    {
                        Verbosity = MSB.Framework.LoggerVerbosity.Normal
                    }
                }
            };
            buildManager.BeginBuild(buildParams);

            var projects = await Task.WhenAll(solutionHandle.Entity.Projects
                .Select(async p => await MineProject(p, projectCollection, cancellationToken)));
            solutionHandle.Entity = solutionHandle.Entity with
            {
                Projects = projects.ToImmutableArray()
            };
        }
        finally
        {
            buildManager.EndBuild();
        }

    }

    private async Task<Project> MineProject(
        Project project,
        MSB.Evaluation.ProjectCollection collection,
        CancellationToken cancellationToken = default)
    {
        MSB.Evaluation.Project? msbuildProject;
        try
        {
            msbuildProject = MSB.Evaluation.Project.FromFile(project.Path, new MSB.Definition.ProjectOptions
            {
                ProjectCollection = collection
            });
        }
        catch (MSB.Exceptions.InvalidProjectFileException e)
        {
            logger.LogError(
                exception: e,
                message: "Could not load project at '{}' because of an MSBuild error. Ignoring project.",
                project.Path);
            return project with
            {
                Diagnostics = project.Diagnostics.Add(Diagnostic.Error("InvalidProjectFile", e.Message))
            };
        }

        var projectName = msbuildProject.GetPropertyValue(CSConst.MSBuildProjectNameProperty);

        project = project with
        {
            Name = projectName
        };

        logger.LogInformation("Found the '{}' project.", project.Name);

        string[] targetFrameworks = Array.Empty<string>();

        var targetFrameworkProp = msbuildProject.GetPropertyValue(CSConst.TargetFrameworkProperty);
        if (!string.IsNullOrEmpty(targetFrameworkProp))
        {
            targetFrameworks = new string[] { targetFrameworkProp };
        }
        else
        {
            var targetFrameworksProp = msbuildProject.GetPropertyValue(CSConst.TargetFrameworksProperty);
            if (string.IsNullOrEmpty(targetFrameworksProp))
            {
                logger.LogDebug(
                    "The '{}' project has no target frameworks, '{}' will be used.",
                    projectName,
                    Const.Unknown);
            }
            else
            {
                targetFrameworks = targetFrameworksProp.Split(';');
            }

        }

        var dependenciesBuilder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<Dependency>>();
        if (targetFrameworks.Length == 0)
        {
            var dependencies = await ResolveDependencies(project, msbuildProject, null, cancellationToken);
            dependenciesBuilder.Add(Const.Unknown, dependencies);
        }
        else
        {
            foreach (var targetFramework in targetFrameworks)
            {
                var dependencies = await ResolveDependencies(project, msbuildProject, targetFramework, cancellationToken);
                dependenciesBuilder.Add(targetFramework, dependencies);
            }
        }

        return project with
        {
            Dependencies = dependenciesBuilder.ToImmutable()
        };
    }

    private async Task<ImmutableArray<Dependency>> ResolveDependencies(
        Project project,
        MSB.Evaluation.Project msbuildProject,
        string? targetFramework,
        CancellationToken cancellationToken = default)
    {
        var projectName = msbuildProject.GetPropertyValue(CSConst.MSBuildProjectNameProperty);

        if (string.IsNullOrEmpty(msbuildProject.GetPropertyValue(CSConst.TargetFrameworkProperty))
            && !msbuildProject.GlobalProperties.ContainsKey(CSConst.TargetFrameworkProperty)
            && !string.IsNullOrEmpty(targetFramework))
        {
            msbuildProject.SetProperty(CSConst.TargetFrameworkProperty, targetFramework);
        }

        var instance = msbuildProject.CreateProjectInstance();

        var targets = new List<string>();
        if (Options.ShouldRestore)
        {
            targets.Add(CSConst.RestoreTarget);
        }
        targets.Add(CSConst.ResolveReferencesTarget);

        var buildRequest = new MSB.Execution.BuildRequestData(
            instance,
            targets.ToArray());
        logger.LogDebug("Resolving references of '{}' for the '{}' target framework using the '{}' targets.",
            projectName,
            targetFramework,
            string.Join(", ", targets));
        var buildResult = await BuildAsync(buildManager, buildRequest);
        if (buildResult.OverallResult == MSB.Execution.BuildResultCode.Failure)
        {
            return ImmutableArray<Dependency>.Empty;
        }

        if (!buildResult.ResultsByTarget.TryGetValue(CSConst.ResolveReferencesTarget, out var targetResult))
        {
            return ImmutableArray<Dependency>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<Dependency>();
        foreach (var item in targetResult.Items)
        {
            builder.Add(await ResolveDependency(project, item, cancellationToken));
        }
        return builder.ToImmutable();
    }

    private async Task<Dependency> ResolveDependency(
        Project project,
        MSB.Framework.ITaskItem item,
        CancellationToken cancellationToken = default)
    {
        var workspace = GetWorkspace();

        var dependency = new Dependency
        {
            Identity = AssemblyId.Create(item)
        };

        var packageId = NullIfEmpty(item.GetMetadata("NuGetPackageId"));
        var packageVersion = NullIfEmpty(item.GetMetadata("NuGetPackageVersion"));

        var projectReferencePath = NullIfEmpty(item.GetMetadata("OriginalProjectReferenceItemSpec"))
            ?? NullIfEmpty(item.GetMetadata("ProjectReferenceOriginalItemSpec"));

        // 1. See if it is a part of framework.
        var frameworkName = NullIfEmpty(item.GetMetadata("FrameworkReferenceName"));
        var frameworkVersion = NullIfEmpty(item.GetMetadata("FrameworkReferenceVersion"));

        if (Options.IncludeExternalDependencies && !string.IsNullOrEmpty(frameworkName))
        {
            using var frameworkHandle = await GetOrAddFramework(frameworkName, frameworkVersion, cancellationToken);
            if (frameworkHandle.Entity is not null)
            {
                var existing = frameworkHandle.Entity.Libraries.FirstOrDefault(l => l.Identity == dependency.Identity);
                if (existing is not null)
                {
                    return dependency with
                    {
                        Token = existing.Token,
                    };
                }

                var frameworkLibraryToken = frameworkHandle.Entity.Token.Derive(Interlocked.Increment(ref counter));

                frameworkHandle.Entity = frameworkHandle.Entity with
                {
                    Libraries = frameworkHandle.Entity.Libraries.Add(new Library
                    {
                        Token = frameworkLibraryToken,
                        Identity = dependency.Identity,
                        PackageId = packageId,
                        PackageVersion = packageVersion,
                        ContainingEntity = frameworkHandle.Entity.Token
                    })
                };

                return dependency with
                {
                    Token = frameworkLibraryToken
                };
            }
        }

        if (!string.IsNullOrEmpty(projectReferencePath))
        {
            projectReferencePath = new FileInfo(Path.Combine(Path.GetDirectoryName(project.Path) ?? "", projectReferencePath)).FullName;

            var solution = workspace.Roots.Values.OfType<Solution>().Single(r => r.Token == project.ContainingSolution);

            var referencedProject = solution?.Projects
                .Where(p => p.Path == projectReferencePath)
                .FirstOrDefault();
            if (referencedProject is null)
            {
                logger.LogError("Could not resolve a project reference to '{}'.", projectReferencePath);
                return dependency with
                {
                    Token = solution?.Token.Derive(NumericToken.NoneValue)
                        ?? CSConst.NoneToken
                };
            }

            return dependency with
            {
                Token = referencedProject.Token
            };
        }

        using var eds = await GetExternalDependencySource();
        if (Options.IncludeExternalDependencies && eds.Entity is not null)
        {

            var existingLibrary = eds.Entity.Libraries
                .FirstOrDefault(a => a.Identity == dependency.Identity);
            if (existingLibrary is not null)
            {
                return dependency with
                {
                    Token = existingLibrary.Token
                };
            }

            var libraryToken = edsToken.Derive(Interlocked.Increment(ref counter));

            eds.Entity = eds.Entity with
            {
                Libraries = eds.Entity.Libraries.Add(new Library
                {
                    Token = libraryToken,
                    ContainingEntity = eds.Entity.Token,
                    Identity = dependency.Identity,
                    PackageId = packageId,
                    PackageVersion = packageVersion
                })
            };

            return dependency with
            {
                Token = libraryToken
            };
        }

        return dependency with
        {
            Token = CSConst.NoneToken
        };
    }

    // NB: Based on Roslyn's ProjectBuildManager.
    private static Task<MSB.Execution.BuildResult> BuildAsync(
        MSB.Execution.BuildManager buildManager,
        MSB.Execution.BuildRequestData requestData)
    {
        var taskSource = new TaskCompletionSource<MSB.Execution.BuildResult>();

        // execute build async
        try
        {
            buildManager.PendBuildRequest(requestData).ExecuteAsync(sub =>
            {
                // when finished
                try
                {
                    var result = sub.BuildResult;
                    taskSource.TrySetResult(result);
                }
                catch (Exception e)
                {
                    taskSource.TrySetException(e);
                }
            }, null);
        }
        catch (Exception e)
        {
            taskSource.SetException(e);
        }

        return taskSource.Task;
    }

    private async Task<ExclusiveEntityHandle<Framework>> GetOrAddFramework(
        string name,
        string? version,
        CancellationToken cancellationToken = default)
    {
        var workspace = GetWorkspace();

        // TODO: Currently the method gets an exclusive handle even though sometimes no writing operation occurs.
        //       It simplifies error handling because it prevents race conditions but it also probably causes a
        //       performance hit. Investigate this further.

        var existing = workspace.Roots.Values
            .OfType<Framework>()
            .Where(f => f.Name == name && f.Version == version)
            .FirstOrDefault();

        if (existing is not null)
        {
            return await workspace.GetRootExclusively<Framework>(existing.Token, cancellationToken);
        }

        var framework = new Framework
        {
            Index = Interlocked.Increment(ref counter),
            Name = name,
            Version = version
        };

        var handle = await workspace.GetRootExclusively<Framework>(framework.Token, cancellationToken);
        handle.Entity = framework;
        return handle;
    }

    private Task<ExclusiveEntityHandle<ExternalDependencySource>> GetExternalDependencySource()
    {
        return GetWorkspace().GetRootExclusively<ExternalDependencySource>(edsToken);
    }

    private Workspace GetWorkspace()
    {
        if (!workspaceRef.TryGetTarget(out var workspace) || workspace is null)
        {
            throw new InvalidOperationException("Cannot get a framework without a workspace. This is likely a bug.");
        }
        return workspace;
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        buildManager.Dispose();
    }
}
