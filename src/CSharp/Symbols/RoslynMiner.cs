using Helveg.CSharp.Projects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public class RoslynMiner : IMiner
{
    private readonly ILogger<RoslynMiner> logger;

    private readonly SymbolTokenMap tokenMap;
    private readonly SymbolTrackingVisitor symbolVisitor;

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

        var assemblies = await Task.WhenAll(msbuildWorkspace.CurrentSolution.Projects
            .Select(async p => (key: p.FilePath ?? p.Name, assembly: await GetAssembly(p, cancellationToken))));
        var projectAssemblies = assemblies
            .GroupBy(p => p.key)
            .ToDictionary(g => g.Key, g => g.Select(p => p.assembly).ToArray());
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
                    if (!projectAssemblies.TryGetValue(p.Path ?? p.Name, out var assemblies))
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
    }

    private async Task<AssemblyDefinition> GetAssembly(
        Microsoft.CodeAnalysis.Project project,
        CancellationToken cancellationToken = default)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            logger.LogError("Failed to compile the '{}' project.", project.Name);
            return AssemblyDefinition.Invalid;
        }

        logger.LogInformation("Found the '{}' assembly.", compilation.Assembly.GetHelvegAssemblyId().ToDisplayString());

        // TODO: Classify references into BCL, Packages, and Other.
        tokenMap.Track(compilation);

        // Phase 1: Discover all symbols within the assembly.
        logger.LogDebug("Visiting the '{}' assembly.", compilation.Assembly.GetHelvegAssemblyId().ToDisplayString());
        symbolVisitor.VisitAssembly(compilation.Assembly);

        var errors = new List<Diagnostic>();

        foreach (var reference in compilation.References)
        {
            var referenceSymbol = compilation.GetAssemblyOrModuleSymbol(reference);
            if (referenceSymbol is not Microsoft.CodeAnalysis.IAssemblySymbol referencedAssembly)
            {
                var diagnostic = Diagnostic.Error(
                    "MissingAssemblySymbol",
                    $"Could not obtain an assembly symbol for a metadata reference of '{project.Name}'. " +
                    $"The reference is: '{reference}'.");
                errors.Add(diagnostic);
                logger.LogWarning(diagnostic.Message);
                continue;
            }

            logger.LogDebug("Visiting a '{}' reference.", referencedAssembly.GetHelvegAssemblyId().ToDisplayString());
            symbolVisitor.VisitAssembly(referencedAssembly);
        }

        // Phase 2: Transcribe the assembly into Helveg's structures.
        logger.LogDebug("Transcribing the '{}' assembly.", compilation.Assembly.GetHelvegAssemblyId().ToDisplayString());
        var transcriber = new RoslynSymbolTranscriber(compilation, tokenMap);
        return transcriber.Transcribe();
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
                logger.LogDebug(new EventId(0, "MSBuildWorkspace"), workspace.Diagnostics[i].Message);
            }
        }

        if (workspace.Diagnostics.Any(d => d.Kind == Microsoft.CodeAnalysis.WorkspaceDiagnosticKind.Failure))
        {
            logger.LogCritical($"Failed to load the project or solution. "
                + "Make sure it can be built with 'dotnet build'.");
        }
    }
}
