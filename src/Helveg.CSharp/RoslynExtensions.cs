using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

internal static class RoslynExtensions
{
    public static bool IsOriginalDefinition(this ISymbol symbol)
    {
        return SymbolEqualityComparer.Default.Equals(symbol, symbol.OriginalDefinition);
    }

    public static HelAssemblyIdCS ToHelvegAssemblyId(this AssemblyIdentity identity)
    {
        return new HelAssemblyIdCS
        {
            Name = identity.Name,
            Version = identity.Version,
            CultureName = identity.CultureName,
            PublicKeyToken = string.Concat(identity.PublicKeyToken.Select(b => b.ToString("x")))
        };
    }

    public static HelAccessibilityCS ToHelvegAccessibility(this Accessibility value)
    {
        return value switch
        {
            Accessibility.Private => HelAccessibilityCS.Private,
            Accessibility.ProtectedAndInternal => HelAccessibilityCS.ProtectedAndInternal,
            Accessibility.Protected => HelAccessibilityCS.Protected,
            Accessibility.Internal => HelAccessibilityCS.Internal,
            Accessibility.ProtectedOrInternal => HelAccessibilityCS.ProtectedOrInternal,
            Accessibility.Public => HelAccessibilityCS.Public,
            _ => HelAccessibilityCS.Invalid
        };
    }

    public static HelTypeKindCS ToHelvegTypeKind(this TypeKind value)
    {
        return value switch
        {
            TypeKind.Unknown => HelTypeKindCS.Unknown,
            TypeKind.Array => HelTypeKindCS.Array,
            TypeKind.Class => HelTypeKindCS.Class,
            TypeKind.Delegate => HelTypeKindCS.Delegate,
            TypeKind.Dynamic => HelTypeKindCS.Dynamic,
            TypeKind.Enum => HelTypeKindCS.Enum,
            TypeKind.Error => HelTypeKindCS.Error,
            TypeKind.Interface => HelTypeKindCS.Interface,
            TypeKind.Module => HelTypeKindCS.Module,
            TypeKind.Pointer => HelTypeKindCS.Pointer,
            TypeKind.Struct => HelTypeKindCS.Struct,
            TypeKind.TypeParameter => HelTypeKindCS.TypeParameter,
            TypeKind.Submission => HelTypeKindCS.Submission,
            TypeKind.FunctionPointer => HelTypeKindCS.FunctionPointer,
            _ => HelTypeKindCS.Unknown
        };
    }

    public static HelRefKindCS ToHelvegRefKind(this RefKind value)
    {
        return value switch
        {
            RefKind.None => HelRefKindCS.None,
            RefKind.Ref => HelRefKindCS.Ref,
            RefKind.Out => HelRefKindCS.Out,
            RefKind.In => HelRefKindCS.In,
            _ => HelRefKindCS.None
        };
    }

    public static HelMethodKindCS ToHelvegMethodKind(this MethodKind value)
    {
        return value switch
        {
            MethodKind.AnonymousFunction => HelMethodKindCS.AnonymousFunction,
            MethodKind.Constructor => HelMethodKindCS.Constructor,
            MethodKind.Conversion => HelMethodKindCS.Conversion,
            MethodKind.DelegateInvoke => HelMethodKindCS.DelegateInvoke,
            MethodKind.Destructor => HelMethodKindCS.Destructor,
            MethodKind.EventAdd => HelMethodKindCS.EventAdd,
            MethodKind.EventRaise => HelMethodKindCS.EventRaise,
            MethodKind.EventRemove => HelMethodKindCS.EventRemove,
            MethodKind.ExplicitInterfaceImplementation => HelMethodKindCS.ExplicitInterfaceImplementation,
            MethodKind.UserDefinedOperator => HelMethodKindCS.UserDefinedOperator,
            MethodKind.Ordinary => HelMethodKindCS.Ordinary,
            MethodKind.PropertyGet => HelMethodKindCS.PropertyGet,
            MethodKind.PropertySet => HelMethodKindCS.PropertySet,
            MethodKind.ReducedExtension => HelMethodKindCS.ReducedExtension,
            MethodKind.StaticConstructor => HelMethodKindCS.StaticConstructor,
            MethodKind.BuiltinOperator => HelMethodKindCS.BuiltinOperator,
            MethodKind.DeclareMethod => HelMethodKindCS.DeclareMethod,
            MethodKind.LocalFunction => HelMethodKindCS.LocalFunction,
            MethodKind.FunctionPointerSignature => HelMethodKindCS.FunctionPointerSignature,
            _ => HelMethodKindCS.Invalid
        };
    }

    public static HelNullabilityCS ToHelvegNullability(this NullableAnnotation value)
    {
        return value switch
        {
            NullableAnnotation.None => HelNullabilityCS.None,
            NullableAnnotation.NotAnnotated => HelNullabilityCS.NotAnnotated,
            NullableAnnotation.Annotated => HelNullabilityCS.Annotated,
            _ => HelNullabilityCS.None
        };
    }

    public static HelEntityKindCS GetEntityKind(this ISymbol symbol)
    {
        return symbol switch
        {
            IAssemblySymbol => HelEntityKindCS.Assembly,
            IModuleSymbol => HelEntityKindCS.Module,
            INamespaceSymbol => HelEntityKindCS.Namespace,
            ITypeParameterSymbol => HelEntityKindCS.TypeParameter,
            IFieldSymbol => HelEntityKindCS.Field,
            IEventSymbol => HelEntityKindCS.Event,
            IPropertySymbol => HelEntityKindCS.Property,
            IMethodSymbol => HelEntityKindCS.Method,
            INamedTypeSymbol => HelEntityKindCS.Type,
            _ => throw new ArgumentException($"Could not assign {nameof(HelEntityKindCS)} to a Roslyn symbol of type " +
                $"'{symbol.GetType()}'.")
        };
    }
}
