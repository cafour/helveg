using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly ConcurrentDictionary<AssemblyId, (Compilation, SymbolTokenGenerator)> assemblyMap = new();
    private readonly ILogger logger;

    private int assemblyCounter = 0;

    public IEnumerable<AssemblyId> TrackedAssemblies => assemblyMap.Keys;

    public ConcurrentDictionary<ISymbol, NumericToken> Tokens { get; }
        = new(SymbolEqualityComparer.Default);

    public SymbolTokenMap(ILogger? logger = null)
    {
        this.logger = logger ?? NullLogger.Instance;
    }

    public void TrackAndVisit(
        IAssemblySymbol assemblySymbol,
        NumericToken parentToken,
        Compilation relatedCompilation,
        string? assemblyPath = null)
    {
        var id = AssemblyId.Create(assemblySymbol);
        if (assemblyPath is not null)
        {
            id = id with { Path = assemblyPath };
        }

        if (Track(id, parentToken, relatedCompilation))
        {
            new ActionSymbolVisitor(s => GetOrAdd(s)).Visit(assemblySymbol);
        }
    }

    public void TrackAndVisit(
        PortableExecutableReference reference,
        NumericToken parentToken,
        Compilation relatedCompilation,
        string? assemblyPath = null)
    {
        var symbol = relatedCompilation.GetAssemblyOrModuleSymbol(reference);
        if (symbol is not IAssemblySymbol assemblySymbol)
        {
            logger.LogError("Reference '{}' cannot be tracked since no corresponding assembly " +
                "symbol could be obtained from the provided Compilation.", reference.Display);
            return;
        }

        var id = AssemblyId.Create(assemblySymbol, reference);
        if (assemblyPath is not null)
        {
            id = id with { Path = assemblyPath };
        }
        
        if (Track(id, parentToken, relatedCompilation))
        {
            new ActionSymbolVisitor(s => GetOrAdd(s)).Visit(assemblySymbol);
        }
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

    private bool Track(AssemblyId assemblyId, NumericToken parentToken, Compilation relatedCompilation)
    {
        var hasAdded = false;
        assemblyMap.GetOrAdd(assemblyId, _ =>
        {
            hasAdded = true;
            logger.LogDebug("Tracking '{}'.", assemblyId.ToDisplayString());
            return (
                relatedCompilation,
                new SymbolTokenGenerator(parentToken.Derive(Interlocked.Increment(ref assemblyCounter)))
            );
        });
        return hasAdded;
    }
}
