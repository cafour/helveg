using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Helveg.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReferenceCountingAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(new DiagnosticDescriptor(
                id: "HELVEG0000",
                title: "Fake Helveg Diagnostic",
                messageFormat: "This diagnostic is not an error.",
                category: "HELVEG",
                defaultSeverity: DiagnosticSeverity.Hidden,
                isEnabledByDefault: true));

        public ConcurrentDictionary<AnalyzedTypeId, ConcurrentDictionary<AnalyzedTypeId, int>> ReferenceCounts { get; }
            = new ConcurrentDictionary<AnalyzedTypeId, ConcurrentDictionary<AnalyzedTypeId, int>>();

        public override void Initialize(AnalysisContext context)
        {
            ReferenceCounts.Clear();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(c => {

            });
            context.RegisterCodeBlockAction(c =>
            {
                if (c.OwningSymbol is null || c.OwningSymbol.ContainingType is null)
                {
                    return;
                }

                var id = c.OwningSymbol.ContainingType.GetAnalyzedId();
                ConcurrentDictionary<AnalyzedTypeId, int>? typeReferences = null;
                while (!ReferenceCounts.TryGetValue(id, out typeReferences))
                {
                    typeReferences = new ConcurrentDictionary<AnalyzedTypeId, int>();
                    ReferenceCounts.TryAdd(id, typeReferences);
                }
                var walker = new ReferenceCountingWalker(
                    typeReferences,
                    c.SemanticModel,
                    c.OwningSymbol.ContainingAssembly);
                walker.Visit(c.CodeBlock);
            });
        }
    }
}
