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
    private int assemblyCounter = 0;

    private readonly ConcurrentDictionary<AssemblyId, (Compilation, SymbolTokenGenerator)> assemblyMap
        = new();

    public ConcurrentDictionary<ISymbol, SymbolToken> Tokens { get; }
        = new(SymbolEqualityComparer.Default);

    public void Track(Compilation compilation)
    {
        assemblyMap.GetOrAdd(
            compilation.Assembly.GetHelvegAssemblyId(),
            _ => (compilation, new SymbolTokenGenerator(Interlocked.Increment(ref assemblyCounter).ToString())));

        foreach (var reference in compilation.References)
        {
            var referencedSymbol = compilation.GetAssemblyOrModuleSymbol(reference);
            if (referencedSymbol is not null && referencedSymbol is IAssemblySymbol referencedAssembly)
            {
                assemblyMap.GetOrAdd(
                    referencedAssembly.GetHelvegAssemblyId(),
                    _ => (compilation, new SymbolTokenGenerator(Interlocked.Increment(ref assemblyCounter).ToString())));
            }
        }
    }

    public SymbolToken? Add(ISymbol symbol)
    {
        if (Tokens.ContainsKey(symbol))
        {
            return SymbolToken.Invalid;
        }

        var definition = GetDefinition(symbol)
            ?? throw new ArgumentException($"A definition for the '{symbol}' symbol could not be found.");

        var containingAssembly = symbol is IAssemblySymbol assembly ? assembly : symbol.ContainingAssembly;

        var (_, gen) = assemblyMap.GetValueOrDefault(containingAssembly.GetHelvegAssemblyId());

        var token = gen.GetToken(symbol.GetHelvegSymbolKind());

        return Tokens.AddOrUpdate(definition, _ => token, (_, _) => token);
    }

    public SymbolToken Get(ISymbol symbol)
    {
        var definition = GetDefinition(symbol);
        if (definition is null || !Tokens.TryGetValue(definition, out var token))
        {
            return SymbolToken.CreateError(symbol.GetHelvegSymbolKind());
        }

        return token;
    }

    private ISymbol? GetDefinition(ISymbol symbol)
    {
        var containingAssembly = symbol is IAssemblySymbol assembly ? assembly : symbol.ContainingAssembly;

        var (parentCompilation, _) = assemblyMap.GetValueOrDefault(containingAssembly.GetHelvegAssemblyId());
        if (parentCompilation is null)
        {
            return null;
        }

        var candidates = SymbolFinder.FindSimilarSymbols(symbol.OriginalDefinition, parentCompilation)
            .ToArray();
        if (candidates.Length == 0)
        {
            return null;
        }

        // TODO: Pick the best one or at the very least issue a warning.
        return candidates.First();
    }
}
