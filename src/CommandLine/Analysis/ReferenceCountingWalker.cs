using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Helveg.Analysis
{
    public class ReferenceCountingWalker : CSharpSyntaxWalker
    {
        private readonly ConcurrentDictionary<AnalyzedTypeId, int> typeReferences;
        private readonly SemanticModel semanticModel;
        private readonly IAssemblySymbol scope;

        public ReferenceCountingWalker(
            ConcurrentDictionary<AnalyzedTypeId, int> typeReferences,
            SemanticModel semanticModel,
            IAssemblySymbol scope)
        {
            this.typeReferences = typeReferences;
            this.semanticModel = semanticModel;
            this.scope = scope;
        }

        public override void DefaultVisit(SyntaxNode node)
        {
            // recurse first
            base.DefaultVisit(node);

            if (!(node is TypeSyntax || node is NameSyntax))
            {
                return;
            }

            var symbolInfo = semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is INamedTypeSymbol typeSymbol
                && SymbolEqualityComparer.Default.Equals(typeSymbol.ContainingAssembly, scope))
            {
                var id = typeSymbol.GetAnalyzedId();
                if (!typeReferences.TryGetValue(id, out var current))
                {
                    typeReferences.TryAdd(id, 0);
                }
                while(!typeReferences.TryUpdate(id, current + 1, current))
                {
                    typeReferences.TryGetValue(id, out current);
                }
            }
        }
    }
}
