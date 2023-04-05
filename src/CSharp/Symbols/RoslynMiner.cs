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
            await msbuildWorkspace.OpenSolutionAsync(solution.FullName!, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogCritical($"MSBuild failed to load the '{solution.FullName}' project or solution. "
                + "Run with '--verbose' for more information.");
            logger.LogDebug(e, "MSBuildWorkspace threw an exception.");
            return;
        }
        LogMSBuildDiagnostics(msbuildWorkspace);
        //if (workspace.Diagnostics.Any(d => d.Kind == WorkspaceDiagnosticKind.Failure))
        //{
        //    return EntityWorkspace.Invalid;
        //}
        var assemblies = await Task.WhenAll(msbuildWorkspace.CurrentSolution.Projects
            .Select(p => GetAssembly(p, cancellationToken)));
        var projectAssemblies = assemblies
            .Where(a => a.ContainingProject is not null)
            .GroupBy(a => a.ContainingProject)
            .ToDictionary(g => g.Key!, g => g.ToArray());
        workspace.SetRoot(solution with
        {
            Projects = solution.Projects
                .Select(p =>
                {
                    if (projectAssemblies.TryGetValue(p.Id, out var relatedAssemblies))
                    {
                        var extensions = relatedAssemblies
                            .Select(a => new AssemblyExtension
                            {
                                Assembly = a
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
            return AssemblyDefinition.Invalid;
        }

        // TODO: Classify references into BCL, Packages, and Other.
        tokenMap.Track(project);

        // Phase 1: Discover all symbols within the assembly.
        //symbolVisitor.VisitAssembly(compilation.Assembly);

        //foreach (var reference in compilation.References)
        //{
        //    var referenceSymbol = compilation.GetAssemblyOrModuleSymbol(reference);
        //    if (referenceSymbol is not Microsoft.CodeAnalysis.IAssemblySymbol referencedAssembly)
        //    {
        //        throw new ArgumentException($"Could not obtain an assembly symbol for '{reference.Display}'.");
        //    }

        //    symbolVisitor.VisitAssembly(referencedAssembly);
        //}

        //var transcriber = new RoslynSymbolTranscriber(compilation, tokenMap);
        //return transcriber.Transcribe();
        return AssemblyDefinition.Invalid;
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
