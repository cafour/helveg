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

namespace Helveg.CSharp;

public class DefaultEntityWorkspaceProvider : IEntityWorkspaceProvider
{
    private readonly EntityTokenGenerator tokenGenerator = new();
    private readonly RoslynEntityTokenDocumentVisitor documentVisitor;
    private readonly RoslynEntityTokenSymbolVisitor symbolVisitor;

    public DefaultEntityWorkspaceProvider()
    {
        documentVisitor = new(tokenGenerator);
        symbolVisitor = new(tokenGenerator);
    }

    public async Task<EntityWorkspace> GetWorkspace(string path, CancellationToken cancellationToken = default)
    {
        var workspace = MSBuildWorkspace.Create();
        await workspace.OpenSolutionAsync(path, cancellationToken: cancellationToken);

        documentVisitor.VisitSolution(workspace.CurrentSolution);

        return new EntityWorkspace
        {
            Solution = await GetSolution(workspace.CurrentSolution, cancellationToken),
            CreatedAt = DateTimeOffset.UtcNow,
            Name = Path.GetFileNameWithoutExtension(path)
        };
    }

    private async Task<SolutionDefinition> GetSolution(Solution solution, CancellationToken cancellationToken = default)
    {
        var helSolution = new SolutionDefinition
        {
            Token = tokenGenerator.GetToken(EntityKind.Solution),
            Name = solution.FilePath ?? IEntityDefinition.InvalidName,
            FullName = solution.FilePath
        };

        var externalDependencies = new ConcurrentDictionary<EntityToken, ExternalDependencyDefinition>();

        var projects = (await Task.WhenAll(solution.Projects
            .Select(p => GetProject(p, externalDependencies, cancellationToken))))
            .Select(p => p with { ContainingSolution = helSolution.GetReference() })
            .ToImmutableArray();

        return helSolution with
        {
            Projects = projects,
            ExternalDependencies = externalDependencies.Values
                .Select(e => e with { ContainingSolution = helSolution.GetReference() })
                .ToImmutableArray()
        };
    }

    private async Task<ProjectDefinition> GetProject(
        Project project,
        ConcurrentDictionary<EntityToken, ExternalDependencyDefinition> externalDependencies,
        CancellationToken cancellationToken = default)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            return ProjectDefinition.Invalid;
        }

        var transcriber = new RoslynSymbolTranscriber(compilation, tokenGenerator, symbolVisitor);
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
                        return ProjectReference.Invalid with { Hint = r.ToString() };
                    }
                    else
                    {
                        return new ProjectReference { Token = token, Hint = r.ToString() };
                    }
                })
                .ToImmutableArray(),
            ExternalDependencies = project.MetadataReferences
                .Select(r =>
                {
                    var token = documentVisitor.GetMetadataReferenceToken(r);
                    if (token.IsError)
                    {
                        return ExternalDependencyReference.Invalid with { Hint = r.ToString() };
                    }
                    else
                    {
                        return new ExternalDependencyReference { Token = token, Hint = r.ToString() };
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

        return helProject;
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
