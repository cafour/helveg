using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Roslyn;

internal static class RoslynExtensions
{
    public static bool IsOriginalDefinition(this ISymbol symbol)
    {
        return SymbolEqualityComparer.Default.Equals(symbol, symbol.OriginalDefinition);
    }

    public static AssemblyId ToHelvegAssemblyId(this AssemblyIdentity identity)
    {
        return new AssemblyId
        {
            Name = identity.Name,
            Version = identity.Version,
            CultureName = identity.CultureName,
            PublicKeyToken = string.Concat(identity.PublicKeyToken.Select(b => b.ToString("x")))
        };
    }

    public static MemberAccessibility ToHelvegAccessibility(this Accessibility value)
    {
        return value switch
        {
            Accessibility.Private => MemberAccessibility.Private,
            Accessibility.ProtectedAndInternal => MemberAccessibility.ProtectedAndInternal,
            Accessibility.Protected => MemberAccessibility.Protected,
            Accessibility.Internal => MemberAccessibility.Internal,
            Accessibility.ProtectedOrInternal => MemberAccessibility.ProtectedOrInternal,
            Accessibility.Public => MemberAccessibility.Public,
            _ => MemberAccessibility.Invalid
        };
    }

    public static TypeKind ToHelvegTypeKind(this Microsoft.CodeAnalysis.TypeKind value)
    {
        return value switch
        {
            Microsoft.CodeAnalysis.TypeKind.Unknown => TypeKind.Unknown,
            Microsoft.CodeAnalysis.TypeKind.Array => TypeKind.Array,
            Microsoft.CodeAnalysis.TypeKind.Class => TypeKind.Class,
            Microsoft.CodeAnalysis.TypeKind.Delegate => TypeKind.Delegate,
            Microsoft.CodeAnalysis.TypeKind.Dynamic => TypeKind.Dynamic,
            Microsoft.CodeAnalysis.TypeKind.Enum => TypeKind.Enum,
            Microsoft.CodeAnalysis.TypeKind.Error => TypeKind.Error,
            Microsoft.CodeAnalysis.TypeKind.Interface => TypeKind.Interface,
            Microsoft.CodeAnalysis.TypeKind.Module => TypeKind.Module,
            Microsoft.CodeAnalysis.TypeKind.Pointer => TypeKind.Pointer,
            Microsoft.CodeAnalysis.TypeKind.Struct => TypeKind.Struct,
            Microsoft.CodeAnalysis.TypeKind.TypeParameter => TypeKind.TypeParameter,
            Microsoft.CodeAnalysis.TypeKind.Submission => TypeKind.Submission,
            Microsoft.CodeAnalysis.TypeKind.FunctionPointer => TypeKind.FunctionPointer,
            _ => TypeKind.Unknown
        };
    }

    public static RefKind ToHelvegRefKind(this Microsoft.CodeAnalysis.RefKind value)
    {
        return value switch
        {
            Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
            Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
            Microsoft.CodeAnalysis.RefKind.Out => RefKind.Out,
            Microsoft.CodeAnalysis.RefKind.In => RefKind.In,
            _ => RefKind.None
        };
    }

    public static MethodKind ToHelvegMethodKind(this Microsoft.CodeAnalysis.MethodKind value)
    {
        return value switch
        {
            Microsoft.CodeAnalysis.MethodKind.AnonymousFunction => MethodKind.AnonymousFunction,
            Microsoft.CodeAnalysis.MethodKind.Constructor => MethodKind.Constructor,
            Microsoft.CodeAnalysis.MethodKind.Conversion => MethodKind.Conversion,
            Microsoft.CodeAnalysis.MethodKind.DelegateInvoke => MethodKind.DelegateInvoke,
            Microsoft.CodeAnalysis.MethodKind.Destructor => MethodKind.Destructor,
            Microsoft.CodeAnalysis.MethodKind.EventAdd => MethodKind.EventAdd,
            Microsoft.CodeAnalysis.MethodKind.EventRaise => MethodKind.EventRaise,
            Microsoft.CodeAnalysis.MethodKind.EventRemove => MethodKind.EventRemove,
            Microsoft.CodeAnalysis.MethodKind.ExplicitInterfaceImplementation => MethodKind.ExplicitInterfaceImplementation,
            Microsoft.CodeAnalysis.MethodKind.UserDefinedOperator => MethodKind.UserDefinedOperator,
            Microsoft.CodeAnalysis.MethodKind.Ordinary => MethodKind.Ordinary,
            Microsoft.CodeAnalysis.MethodKind.PropertyGet => MethodKind.PropertyGet,
            Microsoft.CodeAnalysis.MethodKind.PropertySet => MethodKind.PropertySet,
            Microsoft.CodeAnalysis.MethodKind.ReducedExtension => MethodKind.ReducedExtension,
            Microsoft.CodeAnalysis.MethodKind.StaticConstructor => MethodKind.StaticConstructor,
            Microsoft.CodeAnalysis.MethodKind.BuiltinOperator => MethodKind.BuiltinOperator,
            Microsoft.CodeAnalysis.MethodKind.DeclareMethod => MethodKind.DeclareMethod,
            Microsoft.CodeAnalysis.MethodKind.LocalFunction => MethodKind.LocalFunction,
            Microsoft.CodeAnalysis.MethodKind.FunctionPointerSignature => MethodKind.FunctionPointerSignature,
            _ => MethodKind.Invalid
        };
    }

    public static TypeNullability ToHelvegNullability(this NullableAnnotation value)
    {
        return value switch
        {
            NullableAnnotation.None => TypeNullability.None,
            NullableAnnotation.NotAnnotated => TypeNullability.NotAnnotated,
            NullableAnnotation.Annotated => TypeNullability.Annotated,
            _ => TypeNullability.None
        };
    }

    public static EntityKind GetEntityKind(this ISymbol symbol)
    {
        return symbol switch
        {
            IAssemblySymbol => EntityKind.Assembly,
            IModuleSymbol => EntityKind.Module,
            INamespaceSymbol => EntityKind.Namespace,
            ITypeParameterSymbol => EntityKind.TypeParameter,
            IFieldSymbol => EntityKind.Field,
            IEventSymbol => EntityKind.Event,
            IPropertySymbol => EntityKind.Property,
            IMethodSymbol => EntityKind.Method,
            INamedTypeSymbol => EntityKind.Type,
            IParameterSymbol => EntityKind.Parameter,
            _ => throw new ArgumentException($"Could not assign {nameof(EntityKind)} to a Roslyn symbol of type " +
                $"'{symbol.GetType()}'.")
        };
    }
}
