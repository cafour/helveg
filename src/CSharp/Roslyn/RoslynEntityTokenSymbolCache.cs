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
    private readonly EntityTokenGenerator gen;

    public ConcurrentDictionary<ISymbol, EntityToken> Tokens { get; }
        = new(SymbolEqualityComparer.Default);
    public ConcurrentDictionary<AssemblyIdentity, Compilation> Compilations { get; }
        = new();

    public RoslynEntityTokenSymbolCache(EntityTokenGenerator gen)
    {
        this.gen = gen;
    }

    public void TrackCompilation(Compilation compilation, bool includeReferences)
    {
        Compilations.AddOrUpdate(compilation.Assembly.Identity, compilation, (_, c) => c);

        if (includeReferences)
        {
            foreach(var reference in compilation.References)
            {
                var assemblyReference = compilation.GetAssemblyOrModuleSymbol(reference);
                if (assemblyReference is not IAssemblySymbol assemblyReferenceSymbol)
                {
                    throw new ArgumentException($"Could not resolve reference '{reference.Display}'.");
                }

                Compilations.AddOrUpdate(assemblyReferenceSymbol.Identity, compilation, (_, c) => c);
            }
        }
    }

    public void Add(ISymbol symbol, CancellationToken cancellationToken = default)
    {
        var token = gen.GetToken(symbol.GetEntityKind());
        Add(symbol, token, cancellationToken);
    }

    public void Add(ISymbol symbol, EntityToken token, CancellationToken cancellationToken = default)
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

    public EntityToken Get(ISymbol symbol, CancellationToken cancellationToken = default)
    {
        var definition = GetDefinition(symbol, cancellationToken);
        if (definition is null || !Tokens.TryGetValue(definition, out var token))
        {
            return EntityToken.CreateError(symbol.GetEntityKind());
        }

        return token;
    }

    public EntityToken Require(ISymbol symbol, CancellationToken cancellationToken = default)
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
        var assemblyIdentity = symbol is IAssemblySymbol assembly
            ? assembly.Identity
            : symbol.ContainingAssembly?.Identity;

        if (assemblyIdentity is null)
        {
            throw new ArgumentException("Symbol shared across assemblies cannot have entity tokens.");
        }

        var parentCompilation = Compilations.GetValueOrDefault(assemblyIdentity);
        if (parentCompilation is null)
        {
            return null;
        }

        var candidates = SymbolFinder.FindSimilarSymbols(symbol.OriginalDefinition, parentCompilation, cancellationToken)
            .ToArray();
        if (candidates.Length == 0 || candidates.Length > 1)
        {
            throw new ArgumentException($"Symbol '{symbol}' has either no or too many similar symbols in the " +
                $"tracked compilation.");
        }

        return candidates[0];
    }
}
