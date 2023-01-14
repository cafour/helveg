using System;
using Microsoft.CodeAnalysis;

namespace Helveg.Analysis
{
    public static class RoslynExtensions
    {
        public static AnalyzedTypeId GetAnalyzedId(this ITypeSymbol type)
        {
            return type switch
            {
                IArrayTypeSymbol array => GetAnalyzedId(array.ElementType),
                IDynamicTypeSymbol _ => AnalyzedTypeId.Dynamic,
                IErrorTypeSymbol _ => AnalyzedTypeId.Error,
                INamedTypeSymbol named => GetAnalyzedId(named),
                IPointerTypeSymbol pointer => GetAnalyzedId(pointer.PointedAtType),
                _ => throw new NotSupportedException(
                    $"ITypeSymbols of type '{type.GetType().Name}' are not supported."),
            };
        }

        public static AnalyzedTypeId GetAnalyzedId(this INamedTypeSymbol namedType)
        {
            return namedType.ContainingType is object
                ? new AnalyzedTypeId(namedType.Name, GetAnalyzedId(namedType.ContainingType), namedType.Arity)
                : new AnalyzedTypeId(
                    name: namedType.Name,
                    @namespace: namedType.ContainingNamespace?.ToDisplayString() ?? "global",
                    arity: namedType.Arity);
        }

        public static AnalyzedTypeKind GetAnalyzedKind(this ITypeSymbol type)
        {
            return type.TypeKind switch
            {
                TypeKind.Class => AnalyzedTypeKind.Class,
                TypeKind.Struct => AnalyzedTypeKind.Struct,
                TypeKind.Interface => AnalyzedTypeKind.Interface,
                TypeKind.Delegate => AnalyzedTypeKind.Delegate,
                TypeKind.Enum => AnalyzedTypeKind.Enum,
                _ => AnalyzedTypeKind.None
            };
        }

        public static Diagnosis GetDiagnosis(this DiagnosticSeverity severity)
        {
            return severity switch
            {
                DiagnosticSeverity.Error => Diagnosis.Error,
                DiagnosticSeverity.Warning => Diagnosis.Warning,
                _ => Diagnosis.None
            };
        }
    }
}
