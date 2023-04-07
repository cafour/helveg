using Helveg.CSharp.Projects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
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
            await msbuildWorkspace.OpenSolutionAsync(solution.Path!, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogCritical($"MSBuild failed to load the '{solution.Path}' project or solution. "
                + "Run with '--verbose' for more information.");
            logger.LogDebug(e, "MSBuildWorkspace threw an exception.");
            return;
        }
        LogMSBuildDiagnostics(msbuildWorkspace);

        var assemblies = await Task.WhenAll(msbuildWorkspace.CurrentSolution.Projects
            .Select(async p => (name: p.Name, assembly: await GetAssembly(p, cancellationToken))));
        var projectAssemblies = assemblies
            .ToDictionary(p => p.name, p => p.assembly);
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
                    if (projectAssemblies.TryGetValue(p.Name, out var assembly))
                    {
                        var extensions = p.Extensions.Add(
                            new AssemblyExtension
                            {
                                Assembly = assembly with
                                {
                                    ContainingProject = p.Id
                                }
                            });
                        return p with
                        {
                            Extensions = p.Extensions.AddRange(extensions)
                        };
                    }

                    return p;
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

        logger.LogInformation("Found the '{}' assembly.", compilation.Assembly.Name);

        // TODO: Classify references into BCL, Packages, and Other.
        tokenMap.Track(compilation);

        // Phase 1: Discover all symbols within the assembly.
        logger.LogDebug("Visiting the '{}' assembly.", compilation.Assembly.Name);
        symbolVisitor.VisitAssembly(compilation.Assembly);

        foreach (var reference in compilation.References)
        {
            var referenceSymbol = compilation.GetAssemblyOrModuleSymbol(reference);
            if (referenceSymbol is not Microsoft.CodeAnalysis.IAssemblySymbol referencedAssembly)
            {
                throw new ArgumentException($"Could not obtain an assembly symbol for '{reference.Display}'.");
            }

            logger.LogDebug("Visiting a '{}' reference.", referencedAssembly.Name);
            symbolVisitor.VisitAssembly(referencedAssembly);
        }

        logger.LogDebug("Transcribing the '{}' assembly.", compilation.Assembly.Name);
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
