using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

namespace Helveg.CSharp;

public class RoslynWorkspaceProvider : EntityWorkspaceProvider
{
    private readonly EntityTokenGenerator tokenGenerator = new();
    private readonly RoslynEntityTokenSymbolVisitor visitor;

    public RoslynWorkspaceProvider()
    {
        visitor = new(tokenGenerator);
    }

    public async Task<EntityWorkspace> GetWorkspace(string path, CancellationToken cancellationToken = default)
    {
        var workspace = MSBuildWorkspace.Create();
        await workspace.OpenSolutionAsync(path, cancellationToken: cancellationToken);

        return new EntityWorkspace
        {
            Solution = await GetSolution(workspace.CurrentSolution, cancellationToken)
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

        var projects = (await Task.WhenAll(solution.Projects
            .Select(p => GetProject(p, cancellationToken))))
            .Select(p => p with { ContainingSolution = helSolution.GetReference() })
            .ToImmutableArray();

        return helSolution with { Projects = projects };
    }

    private async Task<ProjectDefinition> GetProject(Project project, CancellationToken cancellationToken = default)
    {
        var transcriber = new RoslynSymbolTranscriber(project, tokenGenerator, visitor);
        return await transcriber.Transcribe(cancellationToken);
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
