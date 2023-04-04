using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

internal class SymbolTokenMap
{
    private readonly ConcurrentDictionary<string, Compilation> compilations
        = new();
    private readonly ConcurrentDictionary<string, SymbolTokenGenerator> generators
        = new();

    public ConcurrentDictionary<ISymbol, SymbolToken> Tokens { get; }
        = new(SymbolEqualityComparer.Default);

    public void TrackCompilation(Compilation compilation)
    {
        throw new NotImplementedException();
        //Compilations.AddOrUpdate(compilation., compilation, (_, c) => c);

        //if (includeReferences)
        //{
        //    foreach(var reference in compilation.References)
        //    {
        //        var assemblyReference = compilation.GetAssemblyOrModuleSymbol(reference);
        //        if (assemblyReference is not IAssemblySymbol assemblyReferenceSymbol)
        //        {
        //            throw new ArgumentException($"Could not resolve reference '{reference.Display}'.");
        //        }

        //        Compilations.AddOrUpdate(assemblyReferenceSymbol.Identity, compilation, (_, c) => c);
        //    }
        //}
    }

    public void Add(ISymbol symbol)
    {
        throw new NotImplementedException();
        //var token = gen.GetToken(symbol.GetEntityKind());
        //Add(symbol, token, cancellationToken);
    }

    public void Add(ISymbol symbol, SymbolToken token, CancellationToken cancellationToken = default)
    {
        if (Tokens.ContainsKey(symbol))
        {
            return;
        }

        var definition = GetDefinition(symbol, cancellationToken);
        if (definition is null)
        {
            throw new ArgumentException($"A definition for the '{symbol}' symbol could not be found.");
        }

        Tokens.AddOrUpdate(definition, token, (_, _) => token);
    }

    public SymbolToken Get(ISymbol symbol, CancellationToken cancellationToken = default)
    {
        var definition = GetDefinition(symbol, cancellationToken);
        if (definition is null || !Tokens.TryGetValue(definition, out var token))
        {
            return SymbolToken.CreateError(symbol.GetHelvegSymbolKind());
        }

        return token;
    }

    public SymbolToken Require(ISymbol symbol, CancellationToken cancellationToken = default)
    {
        var token = Get(symbol, cancellationToken);
        if (token.IsError)
        {
            throw new InvalidOperationException($"Symbol '{symbol}' does not have a token even though it is " +
                $"required. This could be a bug.");
        }

        return token;
    }

    private ISymbol? GetDefinition(ISymbol symbol, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        //var assemblyIdentity = (symbol is IAssemblySymbol assembly
        //    ? assembly.Identity
        //    : symbol.ContainingAssembly?.Identity)
        //        ?? throw new ArgumentException("Symbol shared across assemblies cannot have entity tokens.");

        //var parentCompilation = compilations.GetValueOrDefault(assemblyIdentity);
        //if (parentCompilation is null)
        //{
        //    return null;
        //}

        //var candidates = SymbolFinder.FindSimilarSymbols(symbol.OriginalDefinition, parentCompilation, cancellationToken)
        //    .ToArray();
        //if (candidates.Length == 0 || candidates.Length > 1)
        //{
        //    throw new ArgumentException($"Symbol '{symbol}' has either no or too many similar symbols in the " +
        //        $"tracked compilation.");
        //}

        //return candidates[0];
    }
}
