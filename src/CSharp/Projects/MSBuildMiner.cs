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

namespace Helveg.CSharp.Projects;

public class MSBuildMiner : IMiner
{
    private readonly ILogger<MSBuildMiner> logger;
    private int counter = 0;

    public MSBuildMinerOptions Options { get; }

    MinerOptions IMiner.Options => Options;

    public MSBuildMiner(MSBuildMinerOptions options, ILogger<MSBuildMiner>? logger = null)
    {
        Options = options;
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<MSBuildMiner>();
    }

    public Task Mine(Workspace workspace, CancellationToken cancellationToken = default)
    {
        var solution = GetSolution(workspace.Source.Path, cancellationToken);
        if (solution is null)
        {
            return Task.CompletedTask;
        }

        if (!workspace.TryAddRoot(solution))
        {
            logger.LogError("Failed to add the '{}' root to the mining workspace.", solution.Id);
        }

        return Task.CompletedTask;
    }

    private Solution? GetSolution(string path, CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(path))
        {
            var solutionFiles = Directory.GetFiles(path, "*.sln");
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
                var csprojFiles = Directory.GetFiles(path, "*.csproj");
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
        if (fileExtension != ".sln" && fileExtension != ".csproj")
        {
            logger.LogCritical("The source file '{}' is not a solution nor a C# project file.", path);
            return null;
        }

        var absolutePath = new FileInfo(path).FullName;
        var solution = new Solution
        {
            Id = $"Solution-{Interlocked.Increment(ref counter)}",
            Path = fileExtension == ".sln" ? path : null,
            Name = Path.GetFileNameWithoutExtension(path)
        };

        var projectCollection = new MSB.Evaluation.ProjectCollection(Options.MSBuildProperties);
        if (fileExtension == ".csproj")
        {
            var project = GetProject(path, projectCollection);
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

        var buildParams = new MSB.Execution.BuildParameters(projectCollection);
        MSB.Execution.BuildManager.DefaultBuildManager.BeginBuild(buildParams);

        var projects = projectPaths
            .Select(p => GetProject(p, projectCollection))
            .ToArray();

        MSB.Execution.BuildManager.DefaultBuildManager.EndBuild();
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

    private Project GetProject(
        string path,
        MSB.Evaluation.ProjectCollection collection)
    {
        var msbuildProject = MSB.Evaluation.Project.FromFile(path, new MSB.Definition.ProjectOptions
        {
            ProjectCollection = collection
        });

        var projectName = msbuildProject.GetPropertyValue("MSBuildProjectName");

        //msbuildProject.SetProperty("TargetFramework", "net7.0");

        //var instance = msbuildProject.CreateProjectInstance();

        //var buildRequest = new MSB.Execution.BuildRequestData(
        //    instance,
        //    new[] { "ResolveReferences" });
        //var buildResult = MSB.Execution.BuildManager.DefaultBuildManager.BuildRequest(buildRequest);

        var project = new Project
        {
            Id = path,
            Path = path,
            Name = projectName
        };

        logger.LogInformation("Found the '{}' project.", project.Name);

        return project;
    }
}
