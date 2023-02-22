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

public class RoslynWorkspaceProvider : IHelWorkspaceCSProvider
{
    private readonly EntityTokenGenerator tokenGenerator = new();
    private readonly EntityTokenSymbolVisitor visitor;

    public RoslynWorkspaceProvider()
    {
        visitor = new(tokenGenerator);
    }

    public async Task<HelWorkspaceCS> GetWorkspace(string path, CancellationToken cancellationToken = default)
    {
        var workspace = MSBuildWorkspace.Create();
        await workspace.OpenSolutionAsync(path, cancellationToken: cancellationToken);

        return new HelWorkspaceCS
        {
            Solution = await GetSolution(workspace.CurrentSolution, cancellationToken)
        };
    }

    private async Task<HelSolutionCS> GetSolution(Solution solution, CancellationToken cancellationToken = default)
    {
        var helSolution = new HelSolutionCS
        {
            Token = tokenGenerator.GetToken(HelEntityKindCS.Solution),
            Name = solution.FilePath ?? IHelEntityCS.InvalidName,
            FullName = solution.FilePath
        };

        var projects = (await Task.WhenAll(solution.Projects
            .Select(p => GetProject(p, cancellationToken))))
            .Select(p => p with { ContainingSolution = helSolution.GetReference() })
            .ToImmutableArray();

        return helSolution with { Projects = projects };
    }

    private async Task<HelProjectCS> GetProject(Project project, CancellationToken cancellationToken = default)
    {
        var transcriber = new RoslynSymbolTranscriber(project, tokenGenerator, visitor);
        return await transcriber.Transcribe(cancellationToken);
    }

    private class SymbolErrorReferenceRewriter : HelEntityRewriterCS
    {
        private HelEntityLocator locator = null!;
        private readonly ConcurrentDictionary<HelEntityTokenCS, ISymbol> underlyingSymbols;
        private readonly ConcurrentDictionary<HelEntityTokenCS, ISymbol> errorReferences;

        public SymbolErrorReferenceRewriter(
            ConcurrentDictionary<HelEntityTokenCS, ISymbol> underlyingSymbols,
            ConcurrentDictionary<HelEntityTokenCS, ISymbol> errorReferences)
        {
            this.underlyingSymbols = underlyingSymbols;
            this.errorReferences = errorReferences;
        }

        public override HelSolutionCS RewriteSolution(HelSolutionCS solution)
        {
            locator = new HelEntityLocator(solution);
            var newSolution = base.RewriteSolution(solution);
            locator = null!;
            return newSolution;
        }
    }
}
