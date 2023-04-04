using Helveg.CSharp.Projects;
using Helveg.CSharp.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public class RoslynMiner : IMiner
{
    private readonly RoslynMinerOptions options;
    private readonly SymbolTokenGenerator tokenGenerator = new();
    private readonly ILogger<RoslynMiner> logger;

    private readonly RoslynEntityTokenSymbolCache tokenCache;
    private readonly RoslynEntityTokenSymbolVisitor symbolVisitor;
    private readonly RoslynEntityTokenDocumentVisitor documentVisitor;

    public RoslynMiner(RoslynMinerOptions options, ILogger<RoslynMiner>? logger = null)
    {
        this.options = options;
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<RoslynMiner>();
    }
    
    public async Task Mine(Workspace workspace, CancellationToken cancellationToken = default)
    {
        var msbuildWorkspace = MSBuildWorkspace.Create(options.MSBuildProperties);
        var file = new FileInfo(workspace.Target.Path);
        try
        {
            switch (file.Extension)
            {
                case ".sln":
                    await msbuildWorkspace.OpenSolutionAsync(file.FullName, cancellationToken: cancellationToken);
                    break;
                case ".csproj":
                    await msbuildWorkspace.OpenProjectAsync(file.FullName, cancellationToken: cancellationToken);
                    break;
                default:
                    logger.LogCritical($"File extension '{file.Extension}' is not supported.");
                    return;
            }
        }
        catch (Exception e)
        {
            logger.LogCritical($"MSBuild failed to load the '{file}' project or solution. "
                + "Run with '--verbose' for more information.");
            logger.LogDebug(e, "MSBuildWorkspace threw an exception.");
            return;
        }
        LogMSBuildDiagnostics(msbuildWorkspace);
        //if (workspace.Diagnostics.Any(d => d.Kind == WorkspaceDiagnosticKind.Failure))
        //{
        //    return EntityWorkspace.Invalid;
        //}

        documentVisitor.VisitSolution(msbuildWorkspace.CurrentSolution);

        workspace.AddRoot(await GetSolution(msbuildWorkspace.CurrentSolution, options, cancellationToken));
    }

    private async Task<SolutionDefinition> GetSolution(
        Solution solution,
        CancellationToken cancellationToken = default)
    {
        var helSolution = new SolutionDefinition
        {
            Token = tokenGenerator.GetToken(EntityKind.Solution),
            Name = solution.FilePath ?? CSharpConstants.InvalidName,
            FullName = solution.FilePath
        };

        var externalDependencies = new ConcurrentDictionary<EntityToken, ExternalDependencyDefinition>();

        var projects = (await Task.WhenAll(solution.Projects
            .Select(p => GetProject(p, options, externalDependencies, cancellationToken))))
            .Select(p => p with { ContainingSolution = helSolution.Reference })
            .ToImmutableArray();

        helSolution = helSolution with
        {
            Projects = projects
        };

        if (options.IncludeExternalDepedencies)
        {
            helSolution = helSolution with
            {
                ExternalDependencies = externalDependencies.Values
                .Select(e => e with { ContainingSolution = helSolution.Reference })
                .ToImmutableArray()
            };
        }

        return helSolution;
    }

    private async Task<ProjectDefinition> GetProject(
        Project project,
        EntityWorkspaceAnalysisOptions options,
        ConcurrentDictionary<EntityToken, ExternalDependencyDefinition> externalDependencies,
        CancellationToken cancellationToken = default)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            return ProjectDefinition.Invalid;
        }

        // TODO: Classify references into BCL, Packages, and Other.
        tokenCache.TrackCompilation(compilation, true);

        symbolVisitor.VisitAssembly(compilation.Assembly);

        if (true)
        {
            foreach (var reference in compilation.References)
            {
                var referenceSymbol = compilation.GetAssemblyOrModuleSymbol(reference);
                if (referenceSymbol is not IAssemblySymbol referencedAssembly)
                {
                    throw new ArgumentException($"Could not obtain an assembly symbol for '{reference.Display}'.");
                }

                symbolVisitor.VisitAssembly(referencedAssembly);
            }
        }

        var transcriber = new RoslynSymbolTranscriber(compilation, tokenCache);
        var helProject = new ProjectDefinition
        {
            Token = documentVisitor.RequireProjectToken(project.Id),
            Name = project.Name,
            FullName = project.FilePath,
            Assembly = transcriber.Transcribe(),
            ProjectDependencies = project.ProjectReferences
                .Select(r =>
                {
                    var token = documentVisitor.GetProjectToken(r.ProjectId);
                    if (token.IsError)
                    {
                        return ProjectReference.Invalid;
                    }
                    else
                    {
                        return new ProjectReference { Token = token };
                    }
                })
                .ToImmutableArray()
        };
        if (options.IncludeExternalDepedencies)
        {
            helProject = helProject with
            {
                ExternalDependencies = project.MetadataReferences
                .Select(r =>
                {
                    var token = documentVisitor.GetMetadataReferenceToken(r);
                    if (token.IsError)
                    {
                        return ExternalDependencyReference.Invalid with { Hint = r.Display };
                    }
                    else
                    {
                        return new ExternalDependencyReference { Token = token, Hint = r.Display };
                    }
                })
                .ToImmutableArray()
            };

            foreach (var reference in project.MetadataReferences)
            {
                var token = documentVisitor.RequireMetadataReferenceToken(reference);
                externalDependencies.AddOrUpdate(token, _ =>
                {
                    var assembly = transcriber.TranscribeReference(reference);
                    if (assembly is null)
                    {
                        return ExternalDependencyDefinition.Invalid with
                        {
                            Name = reference.Display ?? CSharpConstants.InvalidName
                        };
                    }
                    return new ExternalDependencyDefinition
                    {
                        Name = reference.Display ?? CSharpConstants.InvalidName,
                        Token = token,
                        Assembly = assembly
                    };
                }, (_, e) => e);
            }
        }

        return helProject;
    }

    private void LogMSBuildDiagnostics(MSBuildWorkspace workspace)
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

        if (workspace.Diagnostics.Any(d => d.Kind == WorkspaceDiagnosticKind.Failure))
        {
            logger.LogCritical($"Failed to load the project or solution. "
                + "Make sure it can be built with 'dotnet build'.");
        }
    }

    private class SymbolErrorReferenceRewriter : SymbolRewriter
    {
        private EntityLocator locator = null!;
        private readonly ConcurrentDictionary<EntityToken, ISymbol> underlyingSymbols;
        private readonly ConcurrentDictionary<EntityToken, ISymbol> errorReferences;

        public SymbolErrorReferenceRewriter(
            ConcurrentDictionary<EntityToken, ISymbol> underlyingSymbols,
            ConcurrentDictionary<EntityToken, ISymbol> errorReferences)
        {
            this.underlyingSymbols = underlyingSymbols;
            this.errorReferences = errorReferences;
        }

        public override SolutionDefinition RewriteSolution(SolutionDefinition solution)
        {
            locator = new EntityLocator(solution);
            var newSolution = base.RewriteSolution(solution);
            locator = null!;
            return newSolution;
        }
    }
}
