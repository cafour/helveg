using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Roslyn;

internal class RoslynEntityTokenSymbolCache
{
    private readonly Solution solution;
    private readonly EntityTokenGenerator gen;

    public ConcurrentDictionary<ISymbol, EntityToken> Tokens { get; }
        = new(SymbolEqualityComparer.Default);
    //public ConcurrentDictionary<AssemblyIdentity, Compilation> Compilations { get; }
    //    = new();

    public RoslynEntityTokenSymbolCache(Solution solution, EntityTokenGenerator gen)
    {
        this.solution = solution;
        this.gen = gen;
    }

    public Task Add(ISymbol symbol, CancellationToken cancellationToken = default)
    {
        var token = gen.GetToken(symbol.GetEntityKind());
        return Add(symbol, token, cancellationToken);
    }

    public async Task Add(ISymbol symbol, EntityToken token, CancellationToken cancellationToken = default)
    {
        var definition = await GetDefinition(symbol, cancellationToken);
        Tokens.AddOrUpdate(definition, token, (_, _) => token);
    }

    public async Task<EntityToken> GetAsync(ISymbol symbol, CancellationToken cancellationToken = default)
    {
        var definition = await GetDefinition(symbol, cancellationToken);
        if (!Tokens.TryGetValue(definition, out var token))
        {
            return EntityToken.CreateError(symbol.GetEntityKind());
        }

        return token;
    }

    public async Task<EntityToken> RequireAsync(ISymbol symbol, CancellationToken cancellationToken = default)
    {
        var token = await GetAsync(symbol, cancellationToken);
        if (token.IsError)
        {
            throw new InvalidOperationException($"Symbol '{symbol}' does not have a token even though it is " +
                $"required. This could be a bug.");
        }

        return token;
    }

    public EntityToken Get(ISymbol symbol)
    {
        return GetAsync(symbol).GetAwaiter().GetResult();
    }

    public EntityToken Require(ISymbol symbol)
    {
        return RequireAsync(symbol).GetAwaiter().GetResult();
    }

    private async Task<ISymbol> GetDefinition(ISymbol symbol, CancellationToken cancellationToken)
    {
        var definition = await SymbolFinder.FindSourceDefinitionAsync(symbol, solution, cancellationToken);
        if (definition is null)
        {
            throw new ArgumentException($"Could not find definition of '{symbol}'.");
        }
        return definition;
    }
}
