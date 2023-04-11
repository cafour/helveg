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
    private readonly ConcurrentDictionary<string, Framework> frameworks
        = new();

    public MSBuildMinerOptions Options { get; }

    MinerOptions IMiner.Options => Options;

    public MSBuildMiner(MSBuildMinerOptions options, ILogger<MSBuildMiner>? logger = null)
    {
        Options = options;
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<MSBuildMiner>();
    }

    public async Task Mine(Workspace workspace, CancellationToken cancellationToken = default)
    {
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
            Id = $"Solution-{Interlocked.Increment(ref counter)}",
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
                return solution with
                {
                    Projects = ImmutableArray.Create(project with
                    {
                        ContainingSolution = solution.Id
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
                Projects = projects
                    .Select(p => p with
                    {
                        ContainingSolution = solution.Id
                    })
                    .ToImmutableArray()
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
        var msbuildProject = MSB.Evaluation.Project.FromFile(path, new MSB.Definition.ProjectOptions
        {
            ProjectCollection = collection
        });

        var projectName = msbuildProject.GetPropertyValue("MSBuildProjectName");

        var project = new Project
        {
            Id = path,
            Path = path,
            Name = projectName
        };

        logger.LogInformation("Found the '{}' project.", project.Name);

        string[]? targetFrameworks = null;

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

        var dependenciesBuilder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<Dependency>>();
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

    private async Task<ImmutableArray<Dependency>> ResolveDependencies(
        MSB.Evaluation.Project msbuildProject,
        string targetFramework)
    {
        var projectName = msbuildProject.GetPropertyValue(CSConst.MSBuildProjectNameProperty);

        msbuildProject.SetProperty(CSConst.TargetFrameworkProperty, targetFramework);
        var instance = MSB.Execution.BuildManager.DefaultBuildManager.GetProjectInstanceForBuild(msbuildProject);

        var buildRequest = new MSB.Execution.BuildRequestData(
            instance,
            new[] { CSConst.RestoreTarget, CSConst.ResolveReferencesTarget });
        logger.LogDebug("Resolving references of '{}' for the '{}' target framework.", projectName, targetFramework);
        var buildResult = await BuildAsync(MSB.Execution.BuildManager.DefaultBuildManager, buildRequest);
        if (buildResult.OverallResult == MSB.Execution.BuildResultCode.Failure)
        {
            return ImmutableArray<Dependency>.Empty;
        }

        if (!buildResult.ResultsByTarget.TryGetValue(CSConst.ResolveReferencesTarget, out var targetResult))
        {
            return ImmutableArray<Dependency>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<Dependency>();
        foreach(var item in targetResult.Items)
        {
            //logger.LogDebug("================================");
            //foreach (var metadataName in item.MetadataNames)
            //{
            //    logger.LogDebug("{}={}", metadataName, item.GetMetadata(metadataName.ToString()));
            //}
            builder.Add(new Dependency
            {
                Name = item.GetMetadata("Identity")
            });
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
}
