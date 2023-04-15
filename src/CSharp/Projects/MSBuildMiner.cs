using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MSB = Microsoft.Build;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Xml;
using NuGet.ProjectModel;
using System.Collections.Generic;
using NuGet.Frameworks;
using System.Collections.Concurrent;
using Helveg.CSharp.Packages;

namespace Helveg.CSharp.Projects;

public class MSBuildMiner : IMiner
{
    private readonly ILogger<MSBuildMiner> logger;
    private int counter = 0;
    private WeakReference<Workspace?> workspaceRef = new(null);
    private NumericToken edsToken = NumericToken.CreateNone(CSConst.CSharpNamespace, (int)RootKind.ExternalDependencySource);

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

        var solution = await GetSolution(workspace.Source.Path, cancellationToken);
        if (solution is null)
        {
            return;
        }

        if (!workspace.TryAddRoot(solution))
        {
            logger.LogError("Failed to add the '{}' root to the mining workspace.", solution.Id);
        }


        return;
    }

    private async Task<Solution?> GetSolution(string path, CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(path))
        {
            var solutionFiles = Directory.GetFiles(path, $"*{CSConst.SolutionFileExtension}");
            if (solutionFiles.Length > 1)
            {
                logger.LogCritical(
                    "The '{}' directory contains multiple solution files. Provide a path to one them.",
                    path);
                return null;
            }
            else if (solutionFiles.Length == 1)
            {
                path = solutionFiles[0];
            }
            else
            {
                var csprojFiles = Directory.GetFiles(path, $"*{CSConst.ProjectFileExtension}");
                if (csprojFiles.Length > 1)
                {
                    logger.LogCritical(
                        "The '{}' directory contains multiple C# project files. Provide a path to one them.",
                        path);
                    return null;
                }
                else if (csprojFiles.Length == 1)
                {
                    path = csprojFiles[0];
                }
                else
                {
                    logger.LogCritical(
                        "The '{}' directory contains no solution nor C# project files.",
                        path);
                    return null;
                }
            }
        }

        if (!File.Exists(path))
        {
            logger.LogCritical("The source file '{}' does not exist.", path);
            return null;
        }

        var fileExtension = Path.GetExtension(path);
        if (fileExtension != CSConst.SolutionFileExtension && fileExtension != CSConst.ProjectFileExtension)
        {
            logger.LogCritical("The source file '{}' is not a solution nor a C# project file.", path);
            return null;
        }

        var absolutePath = new FileInfo(path).FullName;
        var solution = new Solution
        {
            Index = Interlocked.Increment(ref counter),
            Path = fileExtension == CSConst.SolutionFileExtension ? path : null,
            Name = Path.GetFileNameWithoutExtension(path)
        };

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
            MSB.Execution.BuildManager.DefaultBuildManager.BeginBuild(buildParams);
            if (fileExtension == CSConst.ProjectFileExtension)
            {
                var project = await GetProject(path, projectCollection);
                if (!project.IsValid)
                {
                    return solution;
                }

                return solution with
                {
                    Projects = ImmutableArray.Create(project with
                    {
                        ContainingSolution = solution.Token
                    })
                };
            }

            var msbuildSolution = MSB.Construction.SolutionFile.Parse(path);
            logger.LogInformation("Found the '{}' solution.", solution.Name);
            var projectPaths = msbuildSolution.ProjectsInOrder
                .Where(p => p.ProjectType != MSB.Construction.SolutionProjectType.SolutionFolder)
                .Select(p => p.AbsolutePath);


            var projects = await Task.WhenAll(projectPaths
                .Select(async p => await GetProject(p, projectCollection)));
            return solution with
            {
                Projects = solution.Projects.AddRange(
                    projects
                    .Where(p => p.IsValid)
                    .Select(p => p with
                    {
                        ContainingSolution = solution.Token
                    })
                    .ToImmutableArray())
            };
        }
        finally
        {
            MSB.Execution.BuildManager.DefaultBuildManager.EndBuild();
        }
    }

    private async Task<Project> GetProject(
        string path,
        MSB.Evaluation.ProjectCollection collection)
    {
        MSB.Evaluation.Project? msbuildProject;
        try
        {
            msbuildProject = MSB.Evaluation.Project.FromFile(path, new MSB.Definition.ProjectOptions
            {
                ProjectCollection = collection
            });
        }
        catch (MSB.Exceptions.InvalidProjectFileException e)
        {
            logger.LogError(
                exception: e,
                message: "Could not load project at '{}' because of an MSBuild error. Ignoring project.",
                path);
            return Project.Invalid;
        }

        var projectName = msbuildProject.GetPropertyValue(CSConst.MSBuildProjectNameProperty);

        var project = new Project
        {
            Index = Interlocked.Increment(ref counter),
            Path = path,
            Name = projectName
        };

        logger.LogInformation("Found the '{}' project.", project.Name);

        string[]? targetFrameworks;

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
                logger.LogError("The '{}' project has no target frameworks.", projectName);
                return project with
                {
                    Diagnostics = project.Diagnostics.Add(
                        Diagnostic.Error("NoTargetFrameworks", "No target frameworks could be resolved."))
                };
            }

            targetFrameworks = targetFrameworksProp.Split(';');
        }

        var dependenciesBuilder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<NumericToken>>();
        foreach (var targetFramework in targetFrameworks)
        {
            var dependencies = await ResolveDependencies(msbuildProject, targetFramework);
            dependenciesBuilder.Add(targetFramework, dependencies);
        }

        return project with
        {
            Dependencies = dependenciesBuilder.ToImmutable()
        };
    }

    private async Task<ImmutableArray<NumericToken>> ResolveDependencies(
        MSB.Evaluation.Project msbuildProject,
        string targetFramework)
    {
        var projectName = msbuildProject.GetPropertyValue(CSConst.MSBuildProjectNameProperty);

        if (string.IsNullOrEmpty(msbuildProject.GetPropertyValue(CSConst.TargetFrameworkProperty))
            && !msbuildProject.GlobalProperties.ContainsKey(CSConst.TargetFrameworkProperty))
        {
            msbuildProject.SetProperty(CSConst.TargetFrameworkProperty, targetFramework);
        }

        var instance = msbuildProject.CreateProjectInstance();

        var buildRequest = new MSB.Execution.BuildRequestData(
            instance,
            new[] { CSConst.RestoreTarget, CSConst.ResolveReferencesTarget });
        logger.LogDebug("Resolving references of '{}' for the '{}' target framework.", projectName, targetFramework);
        var buildResult = await BuildAsync(MSB.Execution.BuildManager.DefaultBuildManager, buildRequest);
        if (buildResult.OverallResult == MSB.Execution.BuildResultCode.Failure)
        {
            return ImmutableArray<NumericToken>.Empty;
        }

        if (!buildResult.ResultsByTarget.TryGetValue(CSConst.ResolveReferencesTarget, out var targetResult))
        {
            return ImmutableArray<NumericToken>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<NumericToken>();
        foreach (var item in targetResult.Items)
        {
            var dependency = new AssemblyDependency
            {
                Identity = AssemblyId.Create(item),
                PackageId = NullIfEmpty(item.GetMetadata("NuGetPackageId")),
                PackageVersion = NullIfEmpty(item.GetMetadata("NuGetPackageVersion"))
            };

            var frameworkName = NullIfEmpty(item.GetMetadata("FrameworkReferenceName"));
            var frameworkVersion = NullIfEmpty(item.GetMetadata("FrameworkReferenceVersion"));
            using var framework = frameworkName is not null
                ? await GetFramework(frameworkName, frameworkVersion)
                : null;
            if (framework is not null && framework.Entity is not null)
            {
                var existing = framework.Entity.Assemblies
                    .FirstOrDefault(a => a.Identity == dependency.Identity);
                if (existing is not null)
                {
                    builder.Add(existing.Token);
                    continue;
                }

                // framework doesn't know about the assembly yet, so add it there
                dependency = dependency with
                {
                    Token = framework.Entity.Token.Derive(Interlocked.Increment(ref counter))
                };
                builder.Add(dependency.Token);
                framework.Entity = framework.Entity with
                {
                    Assemblies = framework.Entity.Assemblies.Add(dependency)
                };
            }
            else
            {
                using var eds = await GetExternalDependencySource();
                if (eds.Entity is null)
                {
                    logger.LogError("The global external dependency source doesn't exist. This is likely a bug.");
                    continue;
                }

                var existing = eds.Entity.Assemblies
                    .FirstOrDefault(a => a.Identity == dependency.Identity);
                if (existing is not null)
                {
                    builder.Add(existing.Token);
                    continue;
                }

                dependency = dependency with
                {
                    Token = edsToken.Derive(Interlocked.Increment(ref counter))
                };
                builder.Add(dependency.Token);
                eds.Entity = eds.Entity with
                {
                    Assemblies = eds.Entity.Assemblies.Add(dependency)
                };
            }
        }
        return builder.ToImmutable();
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

    private Task<ExclusiveEntityHandle<Framework>> GetFramework(string name, string? version)
    {
        var workspace = GetWorkspace();

        var existing = workspace.Roots.Values
            .OfType<Framework>()
            .Where(f => f.Name == name && f.Version == version)
            .FirstOrDefault();
        if (existing is not null)
        {
            return workspace.GetRootExclusively<Framework>(existing.Id);
        }

        var framework = new Framework
        {
            Index = Interlocked.Increment(ref counter),
            Name = name,
            Version = version
        };

        if (!workspace.TryAddRoot(framework))
        {
            // NB: This should never happen.
            throw new InvalidOperationException("An id collection occured. This is likely a bug.");
        }

        return workspace.GetRootExclusively<Framework>(framework.Id);
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
}
