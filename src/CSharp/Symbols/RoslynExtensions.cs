using MCA = Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

internal static class RoslynExtensions
{
    public static bool IsOriginalDefinition(this MCA.ISymbol symbol)
    {
        return MCA.SymbolEqualityComparer.Default.Equals(symbol, symbol.OriginalDefinition);
    }

    public static bool IsInAnalysisScope(this MCA.ISymbol symbol, SymbolAnalysisScope scope)
    {
        var isExplicit = !symbol.IsImplicitlyDeclared && (symbol.CanBeReferencedByName
                || (symbol is MCA.IMethodSymbol method && method.MethodKind == MCA.MethodKind.Constructor));
        return scope switch
        {
            SymbolAnalysisScope.All => true,
            SymbolAnalysisScope.PublicApi => isExplicit
                && (symbol.DeclaredAccessibility == MCA.Accessibility.Public
                    || symbol.DeclaredAccessibility == MCA.Accessibility.Protected)
                && (symbol.ContainingType is null || symbol.ContainingType.IsInAnalysisScope(scope)),
            SymbolAnalysisScope.Explicit => isExplicit,
            _ => false
        };
    }

    public static MemberAccessibility ToHelvegAccessibility(this MCA.Accessibility value)
    {
        return value switch
        {
            MCA.Accessibility.Private => MemberAccessibility.Private,
            MCA.Accessibility.ProtectedAndInternal => MemberAccessibility.ProtectedAndInternal,
            MCA.Accessibility.Protected => MemberAccessibility.Protected,
            MCA.Accessibility.Internal => MemberAccessibility.Internal,
            MCA.Accessibility.ProtectedOrInternal => MemberAccessibility.ProtectedOrInternal,
            MCA.Accessibility.Public => MemberAccessibility.Public,
            _ => MemberAccessibility.Invalid
        };
    }

    public static TypeKind ToHelvegTypeKind(this MCA.TypeKind value)
    {
        return value switch
        {
            MCA.TypeKind.Unknown => TypeKind.Unknown,
            MCA.TypeKind.Array => TypeKind.Array,
            MCA.TypeKind.Class => TypeKind.Class,
            MCA.TypeKind.Delegate => TypeKind.Delegate,
            MCA.TypeKind.Dynamic => TypeKind.Dynamic,
            MCA.TypeKind.Enum => TypeKind.Enum,
            MCA.TypeKind.Error => TypeKind.Error,
            MCA.TypeKind.Interface => TypeKind.Interface,
            MCA.TypeKind.Module => TypeKind.Module,
            MCA.TypeKind.Pointer => TypeKind.Pointer,
            MCA.TypeKind.Struct => TypeKind.Struct,
            MCA.TypeKind.TypeParameter => TypeKind.TypeParameter,
            MCA.TypeKind.Submission => TypeKind.Submission,
            MCA.TypeKind.FunctionPointer => TypeKind.FunctionPointer,
            _ => TypeKind.Unknown
        };
    }

    public static RefKind ToHelvegRefKind(this MCA.RefKind value)
    {
        return value switch
        {
            MCA.RefKind.None => RefKind.None,
            MCA.RefKind.Ref => RefKind.Ref,
            MCA.RefKind.Out => RefKind.Out,
            MCA.RefKind.In => RefKind.In,
            _ => RefKind.None
        };
    }

    public static MethodKind ToHelvegMethodKind(this MCA.MethodKind value)
    {
        return value switch
        {
            MCA.MethodKind.AnonymousFunction => MethodKind.AnonymousFunction,
            MCA.MethodKind.Constructor => MethodKind.Constructor,
            MCA.MethodKind.Conversion => MethodKind.Conversion,
            MCA.MethodKind.DelegateInvoke => MethodKind.DelegateInvoke,
            MCA.MethodKind.Destructor => MethodKind.Destructor,
            MCA.MethodKind.EventAdd => MethodKind.EventAdd,
            MCA.MethodKind.EventRaise => MethodKind.EventRaise,
            MCA.MethodKind.EventRemove => MethodKind.EventRemove,
            MCA.MethodKind.ExplicitInterfaceImplementation => MethodKind.ExplicitInterfaceImplementation,
            MCA.MethodKind.UserDefinedOperator => MethodKind.UserDefinedOperator,
            MCA.MethodKind.Ordinary => MethodKind.Ordinary,
            MCA.MethodKind.PropertyGet => MethodKind.PropertyGet,
            MCA.MethodKind.PropertySet => MethodKind.PropertySet,
            MCA.MethodKind.ReducedExtension => MethodKind.ReducedExtension,
            MCA.MethodKind.StaticConstructor => MethodKind.StaticConstructor,
            MCA.MethodKind.BuiltinOperator => MethodKind.BuiltinOperator,
            MCA.MethodKind.DeclareMethod => MethodKind.DeclareMethod,
            MCA.MethodKind.LocalFunction => MethodKind.LocalFunction,
            MCA.MethodKind.FunctionPointerSignature => MethodKind.FunctionPointerSignature,
            _ => MethodKind.Invalid
        };
    }

    public static TypeNullability ToHelvegNullability(this MCA.NullableAnnotation value)
    {
        return value switch
        {
            MCA.NullableAnnotation.None => TypeNullability.None,
            MCA.NullableAnnotation.NotAnnotated => TypeNullability.NotAnnotated,
            MCA.NullableAnnotation.Annotated => TypeNullability.Annotated,
            _ => TypeNullability.None
        };
    }

    public static MCA.IAssemblySymbol? GetReferencedAssembly(this MCA.Compilation compilation, AssemblyId id)
    {
        return (MCA.IAssemblySymbol?)compilation.References.Select(compilation.GetAssemblyOrModuleSymbol)
            .Where(a => a is not null && a is MCA.IAssemblySymbol assembly && AssemblyId.Create(assembly) == id)
            .FirstOrDefault();
    }

    public static DiagnosticSeverity ToHelvegSeverity(this MCA.DiagnosticSeverity value)
    {
        return value switch
        {
            MCA.DiagnosticSeverity.Hidden => DiagnosticSeverity.Hidden,
            MCA.DiagnosticSeverity.Info => DiagnosticSeverity.Info,
            MCA.DiagnosticSeverity.Warning => DiagnosticSeverity.Warning,
            MCA.DiagnosticSeverity.Error => DiagnosticSeverity.Error,
            _ => DiagnosticSeverity.Unknown
        };
    }
}
