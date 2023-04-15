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

    public IEnumerable<AssemblyId> TrackedAssemblies => assemblyMap.Keys;

    public ConcurrentDictionary<ISymbol, NumericToken> Tokens { get; }
        = new(SymbolEqualityComparer.Default);

    //public void Track(Compilation compilation)
    //{
    //    assemblyMap.GetOrAdd(
    //        compilation.Assembly.GetHelvegAssemblyId(),
    //        _ => (compilation, new SymbolTokenGenerator(Interlocked.Increment(ref assemblyCounter).ToString())));

    //    foreach (var reference in compilation.References)
    //    {
    //        var referencedSymbol = compilation.GetAssemblyOrModuleSymbol(reference);
    //        if (referencedSymbol is not null && referencedSymbol is IAssemblySymbol referencedAssembly)
    //        {
    //            assemblyMap.GetOrAdd(
    //                referencedAssembly.GetHelvegAssemblyId(),
    //                _ => (compilation, new SymbolTokenGenerator(Interlocked.Increment(ref assemblyCounter).ToString())));
    //        }
    //    }
    //}

    public void Track(AssemblyId assemblyId, NumericToken parentToken, Compilation relatedCompilation)
    {
        assemblyMap.GetOrAdd(assemblyId, _ => (
            relatedCompilation,
            new SymbolTokenGenerator(parentToken.Derive(Interlocked.Increment(ref assemblyCounter)))
        ));
    }

    public NumericToken GetOrAdd(ISymbol symbol)
    {
        if (Tokens.TryGetValue(symbol, out var existingToken))
        {
            return existingToken;
        }

        var containingAssembly = symbol is IAssemblySymbol assembly
            ? assembly
            : symbol.ContainingAssembly;
        if (containingAssembly == null)
        {
            // The symbol is probably shared accross assemblies, which we do not support.
            return CSConst.InvalidToken;
        }

        var (parentCompilation, generator) = assemblyMap.GetValueOrDefault(AssemblyId.Create(containingAssembly));
        if (parentCompilation is null)
        {
            // The symbol comes from an assembly that isn't tracked, which isn't an error.
            return CSConst.NoneToken;
        }

        var candidates = SymbolFinder.FindSimilarSymbols(symbol.OriginalDefinition, parentCompilation)
            .ToArray();
        if (candidates.Length == 0)
        {
            // The symbols could not be found, which is unfortunate but not a critical error.
            return generator.None;
        }

        if (candidates.Length > 1)
        {
            // Since there is more than one result, report None instead of a possibly incorrect result.
            return generator.None;
        }

        return Tokens.AddOrUpdate(candidates[0], _ => generator.GetToken(), (_, existing) => existing);
    }

    public Compilation? GetCompilation(AssemblyId assemblyId)
    {
        return assemblyMap.GetValueOrDefault(assemblyId).Item1;
    }
}
