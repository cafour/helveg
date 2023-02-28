using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Helveg.CSharp.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Helveg.CSharp;

public class DefaultEntityWorkspaceProvider : IEntityWorkspaceProvider
{
    private readonly EntityTokenGenerator tokenGenerator = new();
    private readonly RoslynEntityTokenDocumentVisitor documentVisitor;
    private readonly ILogger<DefaultEntityWorkspaceProvider> logger;

    public DefaultEntityWorkspaceProvider(ILogger<DefaultEntityWorkspaceProvider>? logger = null)
    {
        documentVisitor = new(tokenGenerator);
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<DefaultEntityWorkspaceProvider>();
    }

    public async Task<EntityWorkspace> GetWorkspace(
        string path,
        EntityWorkspaceAnalysisOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        options ??= EntityWorkspaceAnalysisOptions.Default;

        var workspace = MSBuildWorkspace.Create(options.MSBuildProperties);
        var file = new FileInfo(path);
        try
        {
            switch (file.Extension)
            {
                case ".sln":
                    await workspace.OpenSolutionAsync(file.FullName, cancellationToken: cancellationToken);
                    break;
                case ".csproj":
                    await workspace.OpenProjectAsync(file.FullName, cancellationToken: cancellationToken);
                    break;
                default:
                    logger.LogCritical($"File extension '{file.Extension}' is not supported.");
                    return EntityWorkspace.Invalid;
            }
        }
        catch (Exception e)
        {
            logger.LogCritical($"MSBuild failed to load the '{file}' project or solution. "
                + "Run with '--verbose' for more information.");
            logger.LogDebug(e, "MSBuildWorkspace threw an exception.");
            return EntityWorkspace.Invalid;
        }
        LogMSBuildDiagnostics(workspace);
        if (workspace.Diagnostics.Any(d => d.Kind == WorkspaceDiagnosticKind.Failure))
        {
            return EntityWorkspace.Invalid;
        }

        documentVisitor.VisitSolution(workspace.CurrentSolution);

        return new EntityWorkspace
        {
            Solution = await GetSolution(workspace.CurrentSolution, options, cancellationToken),
            CreatedAt = DateTimeOffset.UtcNow,
            Name = Path.GetFileNameWithoutExtension(path)
        };
    }

    private async Task<SolutionDefinition> GetSolution(
        Solution solution,
        EntityWorkspaceAnalysisOptions options,
        CancellationToken cancellationToken = default)
    {
        var tokenCache = new RoslynEntityTokenSymbolCache(solution, tokenGenerator);
        var visitor = new RoslynEntityTokenSymbolVisitor(tokenCache);

        var helSolution = new SolutionDefinition
        {
            Token = tokenGenerator.GetToken(EntityKind.Solution),
            Name = solution.FilePath ?? IEntityDefinition.InvalidName,
            FullName = solution.FilePath
        };

        var externalDependencies = new ConcurrentDictionary<EntityToken, ExternalDependencyDefinition>();

        var projects = (await Task.WhenAll(solution.Projects
            .Select(p => GetProject(p, options, externalDependencies, tokenCache, visitor, cancellationToken))))
            .Select(p => p with { ContainingSolution = helSolution.GetReference() })
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
                .Select(e => e with { ContainingSolution = helSolution.GetReference() })
                .ToImmutableArray()
            };
        }

        return helSolution;
    }

    private async Task<ProjectDefinition> GetProject(
        Project project,
        EntityWorkspaceAnalysisOptions options,
        ConcurrentDictionary<EntityToken, ExternalDependencyDefinition> externalDependencies,
        RoslynEntityTokenSymbolCache tokens,
        RoslynEntityTokenSymbolVisitor symbolVisitor,
        CancellationToken cancellationToken = default)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            return ProjectDefinition.Invalid;
        }

        await symbolVisitor.VisitAssembly(compilation.Assembly);

        if (options.IncludeExternalDepedencies)
        {
            foreach(var reference in compilation.References)
            {
                var referenceSymbol = compilation.GetAssemblyOrModuleSymbol(reference);
                if (referenceSymbol is not IAssemblySymbol referencedAssembly)
                {
                    throw new ArgumentException($"Could not obtain an assembly symbol for '{reference.Display}'.");
                }

                await symbolVisitor.VisitAssembly(referencedAssembly);
            }
        }

        var transcriber = new RoslynSymbolTranscriber(compilation, tokens);
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
                            Name = reference.Display ?? IEntityDefinition.InvalidName
                        };
                    }
                    return new ExternalDependencyDefinition
                    {
                        Name = reference.Display ?? IEntityDefinition.InvalidName,
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

    private class SymbolErrorReferenceRewriter : EntityRewriter
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
