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

namespace Helveg.CSharp.Projects;

public class MSBuildMiner : IMiner
{
    private readonly ILogger<MSBuildMiner> logger;

    public MSBuildMinerOptions Options { get; }

    MinerOptions IMiner.Options => Options;

    public MSBuildMiner(MSBuildMinerOptions options, ILogger<MSBuildMiner>? logger = null)
    {
        Options = options;
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<MSBuildMiner>();
    }

    public async Task Mine(Workspace workspace, CancellationToken cancellationToken = default)
    {
        var solution = await GetSolution(workspace.Target.Path, cancellationToken);
        workspace.AddRoot(solution);
    }

    private async Task<Solution> GetSolution(string path, CancellationToken cancellationToken = default)
    {
        var absolutePath = new FileInfo(path).FullName;
        var solution = new Solution
        {
            Id = Path.GetFileName(path),
            Path = absolutePath,
            Name = Path.GetFileNameWithoutExtension(path)
        };

        if (!File.Exists(absolutePath))
        {
            solution = solution with
            {
                Diagnostics = solution.Diagnostics.Add(Diagnostic.Error(
                    "SolutionDoesNotExist",
                    "The solution file does not exist."))
            };
            logger.LogError("The solution file at '{}' does not exist.", absolutePath);
            return solution;
        }

        var msbuildSolution = MSB.Construction.SolutionFile.Parse(path);
        logger.LogInformation("Found the '{}' solution.", solution.Name);
        var projectPaths = msbuildSolution.ProjectsInOrder
            .Where(p => p.ProjectType != MSB.Construction.SolutionProjectType.SolutionFolder)
            .Select(p => p.AbsolutePath);

        var projectCollection = new MSB.Evaluation.ProjectCollection(Options.MSBuildProperties);
        var projects = await Task.WhenAll(projectPaths
            .Select(p => GetProject(p, projectCollection, cancellationToken)));
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

    private Task<Project> GetProject(
        string path,
        MSB.Evaluation.ProjectCollection collection,
        CancellationToken cancellationToken = default)
    {
        var msbuildProject = MSB.Evaluation.Project.FromFile(path, new MSB.Definition.ProjectOptions
        {
            ProjectCollection = collection
        });

        var project = new Project
        {
            Id = path,
            Path = path,
            Name = msbuildProject.GetPropertyValue("MSBuildProjectName")
        };

        logger.LogInformation("Found the '{}' project.", project.Name);

        return Task.FromResult(project);
    }
}
