using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Helveg.CSharp.Symbols;

/// <summary>
/// Transcribes metadata from Roslyn's <see cref="ISymbol"/>s of a given <see cref="Compilation"/> to
/// Helveg.CSharp data types.
/// </summary>
internal class RoslynSymbolTranscriber
{
    private readonly WeakReference<Compilation?> compilationRef = new(null);
    private readonly WeakReference<Compilation?> compareToRef = new(null);
    private readonly SymbolTokenMap tokenMap;
    private ImmutableDictionary<ISymbol, ImmutableArray<Diagnostic>> symbolDiagnostics
        = ImmutableDictionary<ISymbol, ImmutableArray<Diagnostic>>.Empty;

    public SymbolAnalysisScope Scope { get; }

    public RoslynSymbolTranscriber(SymbolTokenMap tokenMap, SymbolAnalysisScope scope)
    {
        this.tokenMap = tokenMap;
        Scope = scope;
    }

    public AssemblyDefinition Transcribe(
        AssemblyId assemblyId,
        Compilation? compareTo = null)
    {
        var compilation = tokenMap.GetCompilation(assemblyId);
        if (compilation is null)
        {
            return AssemblyDefinition.Invalid;
        }

        compilationRef.SetTarget(compilation);
        symbolDiagnostics = GetDiagnostics();

        if (compareTo is not null)
        {
            compareToRef.SetTarget(compareTo);
        }

        if (AssemblyId.Create(compilation.Assembly) == assemblyId)
        {
            return GetAssembly(compilation.Assembly);
        }

        var reference = compilation.GetReferencedAssembly(assemblyId);
        if (reference is null)
        {
            return AssemblyDefinition.Invalid;
        }

        symbolDiagnostics = ImmutableDictionary<ISymbol, ImmutableArray<Diagnostic>>.Empty;
        return GetAssembly(reference);
    }

    private AssemblyDefinition GetAssembly(IAssemblySymbol assembly)
    {
        var helAssembly = PopulateDefinition(assembly, new AssemblyDefinition()) with
        {
            Identity = AssemblyId.Create(assembly)
        };

        return helAssembly with
        {
            Modules = assembly.Modules
                .Select(m => GetModule(m, helAssembly.Reference))
                .Concat(TryGetSimilarSymbol(assembly, compareToRef, out var compareToAssembly)
                    ? compareToAssembly.Modules
                        .Where(m => !TryGetSimilarSymbol(m, compilationRef, out _))
                        .Select(m => GetModule(m, helAssembly.Reference))
                    : Enumerable.Empty<ModuleDefinition>())
                .DistinctBy(e => e.Token)
                .ToImmutableArray()
        };
    }

    private ModuleDefinition GetModule(
        IModuleSymbol module,
        AssemblyReference containingAssembly)
    {
        var helModule = PopulateDefinition(module, new ModuleDefinition()) with
        {
            ReferencedAssemblies = module.ReferencedAssemblySymbols
                .Select(GetAssemblyReference)
                .ToImmutableArray(),
            ContainingAssembly = containingAssembly
        };

        return helModule with
        {
            GlobalNamespace = GetNamespace(module.GlobalNamespace, helModule.Reference, null)
        };
    }

    private NamespaceDefinition GetNamespace(
        INamespaceSymbol @namespace,
        ModuleReference containingModule,
        NamespaceReference? containingNamespace)
    {
        var helNamespace = PopulateDefinition(@namespace, new NamespaceDefinition()) with
        {
            Token = tokenMap.GetOrAdd(@namespace),
            Name = @namespace.Name,
            ContainingModule = containingModule,
            ContainingNamespace = containingNamespace
        };

        TryGetSimilarSymbol(@namespace, compareToRef, out var compareToNamespace);

        return helNamespace with
        {
            Types = @namespace.GetTypeMembers()
                .Where(t => t.IsInAnalysisScope(Scope))
                .Select(t => GetType(t, helNamespace.Reference))
                .Concat(compareToNamespace is not null
                    ? compareToNamespace.GetTypeMembers()
                        .Where(m => !TryGetSimilarSymbol(m, compilationRef, out _))
                        .Select(m => GetType(m, helNamespace.Reference))
                    : Enumerable.Empty<TypeDefinition>())
                .DistinctBy(e => e.Token)
                .ToImmutableArray(),
            Namespaces = @namespace.GetNamespaceMembers()
                .Select(n => GetNamespace(n, containingModule, helNamespace.Reference))
                .Concat(compareToNamespace is not null
                    ? compareToNamespace.GetNamespaceMembers()
                        .Where(m => !TryGetSimilarSymbol(m, compilationRef, out _))
                        .Select(m => GetNamespace(m, containingModule, helNamespace.Reference))
                    : Enumerable.Empty<NamespaceDefinition>())
                .DistinctBy(e => e.Token)
                .ToImmutableArray()
        };
    }

    private TypeDefinition GetType(INamedTypeSymbol type,
        NamespaceReference containingNamespace,
        TypeReference? containingType = null)
    {
        if (!type.IsOriginalDefinition())
        {
            type = type.OriginalDefinition;
        }

        var helType = PopulateMember(type, new TypeDefinition()) with
        {
            MetadataName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ContainingNamespace = containingNamespace,
            ContainingType = containingType,
            TypeKind = type.TypeKind.ToHelvegTypeKind(),
            IsReferenceType = type.IsReferenceType,
            IsValueType = type.IsValueType,
            IsAnonymousType = type.IsAnonymousType,
            IsTupleType = type.IsTupleType,
            IsNativeIntegerType = type.IsNativeIntegerType,
            IsRefLikeType = type.IsRefLikeType,
            IsUnmanagedType = type.IsUnmanagedType,
            IsReadOnly = type.IsReadOnly,
            IsRecord = type.IsRecord,
            IsImplicitClass = type.IsImplicitClass,
            BaseType = type.BaseType is null ? null : GetTypeReference(type.BaseType),
            Interfaces = type.Interfaces.Select(GetTypeReference).ToImmutableArray(),
        };

        var reference = helType.Reference;

        var members = type.GetMembers()
            .Where(s => s.IsInAnalysisScope(Scope));

        TryGetSimilarSymbol(type, compareToRef, out var compareToType);

        var compareToMembers = compareToType?.GetMembers()
            .Where(s => s.IsInAnalysisScope(Scope)
                && !TryGetSimilarSymbol(s, compilationRef, out _))
            .ToImmutableArray() ?? ImmutableArray<ISymbol>.Empty;

        return helType with
        {
            TypeParameters = type.TypeParameters
                .Select(p => GetTypeParameter(p, reference, null, containingNamespace))
                .Concat(compareToType is not null
                    ? compareToType.TypeParameters
                        .Where(m => !TryGetSimilarSymbol(m, compilationRef, out _))
                        .Select(m => GetTypeParameter(m, reference, null, containingNamespace))
                    : Enumerable.Empty<TypeParameterDefinition>())
                .DistinctBy(e => e.Token)
                .ToImmutableArray(),
            NestedTypes = type.GetTypeMembers()
                .Where(t => t.IsInAnalysisScope(Scope))
                .Select(t => GetType(t, containingNamespace, reference))
                .Concat(compareToType is not null
                    ? compareToType.GetTypeMembers()
                        .Where(m => m.IsInAnalysisScope(Scope) && !TryGetSimilarSymbol(m, compilationRef, out _))
                        .Select(m => GetType(m, containingNamespace, reference))
                    : Enumerable.Empty<TypeDefinition>())
                .DistinctBy(e => e.Token)
                .ToImmutableArray(),
            // TODO: Does GetMembers() contain nested types as well?
            Fields = members
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Field)
                .Cast<IFieldSymbol>()
                .Select(f => GetField(f, reference, containingNamespace))
                .Concat(compareToMembers
                    .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Field)
                    .Cast<IFieldSymbol>()
                    .Select(m => GetField(m, reference, containingNamespace)))
                .DistinctBy(e => e.Token)
                .ToImmutableArray(),
            Events = members
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Event)
                .Cast<IEventSymbol>()
                .Select(e => GetEvent(e, reference, containingNamespace))
                .Concat(compareToMembers
                    .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Event)
                    .Cast<IEventSymbol>()
                    .Select(m => GetEvent(m, reference, containingNamespace)))
                .DistinctBy(e => e.Token)
                .ToImmutableArray(),
            Properties = members
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .Select(p => GetProperty(p, reference, containingNamespace))
                .Concat(compareToMembers
                    .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Property)
                    .Cast<IPropertySymbol>()
                    .Select(m => GetProperty(m, reference, containingNamespace)))
                .DistinctBy(e => e.Token)
                .ToImmutableArray(),
            Methods = members
                .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Method)
                .Cast<IMethodSymbol>()
                .Select(m => GetMethod(m, reference, containingNamespace))
                .Concat(compareToMembers
                    .Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Method)
                    .Cast<IMethodSymbol>()
                    .Select(m => GetMethod(m, reference, containingNamespace)))
                .DistinctBy(e => e.Token)
                .ToImmutableArray(),

        };
    }

    private TypeParameterDefinition GetTypeParameter(
        ITypeParameterSymbol symbol,
        TypeReference? declaringType,
        MethodReference? declaringMethod,
        NamespaceReference containingNamespace)
    {
        if (declaringType is null && declaringMethod is null)
        {
            throw new ArgumentException($"Either '{nameof(declaringType)}' or '{nameof(declaringMethod)}' " +
                "must not be null.");
        }

        var helTypeParameter = PopulateDefinition(symbol, new TypeParameterDefinition()) with
        {
            DeclaringType = declaringType,
            DeclaringMethod = declaringMethod,
            ContainingNamespace = containingNamespace
        };

        return helTypeParameter;
    }

    private EventDefinition GetEvent(
        IEventSymbol symbol,
        TypeReference containingType,
        NamespaceReference containingNamespace)
    {
        var helEvent = PopulateMember(symbol, new EventDefinition()) with
        {
            EventType = GetTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            AddMethod = symbol.AddMethod is null ? null : GetMethodReference(symbol.AddMethod),
            RemoveMethod = symbol.RemoveMethod is null ? null : GetMethodReference(symbol.RemoveMethod),
            RaiseMethod = symbol.RaiseMethod is null ? null : GetMethodReference(symbol.RaiseMethod)
        };

        return helEvent;
    }

    private FieldDefinition GetField(
        IFieldSymbol symbol,
        TypeReference containingType,
        NamespaceReference containingNamespace)
    {
        var helField = PopulateMember(symbol, new FieldDefinition()) with
        {
            FieldType = GetTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            AssociatedEvent = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IEventSymbol e
                ? GetEventReference(e)
                : null,
            AssociatedProperty = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IPropertySymbol p
                ? GetPropertyReference(p)
                : null,
            IsVolatile = symbol.IsVolatile,
            IsReadOnly = symbol.IsReadOnly,
            IsRequired = symbol.IsRequired,
            IsConst = symbol.IsConst,
            RefKind = symbol.RefKind.ToHelvegRefKind(),
            IsEnumItem = symbol.ContainingType.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum
        };

        return helField;
    }

    private PropertyDefinition GetProperty(
        IPropertySymbol symbol,
        TypeReference containingType,
        NamespaceReference containingNamespace)
    {
        var helProperty = PopulateMember(symbol, new PropertyDefinition()) with
        {
            PropertyType = GetTypeReference(symbol.Type),
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            GetMethod = symbol.GetMethod is null ? null : GetMethodReference(symbol.GetMethod),
            SetMethod = symbol.SetMethod is null ? null : GetMethodReference(symbol.SetMethod),
            IsIndexer = symbol.IsIndexer,
            IsRequired = symbol.IsRequired,
            OverriddenProperty = symbol.OverriddenProperty is null
                ? null
                : GetPropertyReference(symbol.OverriddenProperty),
            RefKind = symbol.RefKind.ToHelvegRefKind()
        };

        return helProperty with
        {
            Parameters = symbol.Parameters
                .Select(p => GetParameter(p, null, helProperty.Reference))
                .ToImmutableArray()
        };
    }

    private MethodDefinition GetMethod(
        IMethodSymbol symbol,
        TypeReference containingType,
        NamespaceReference containingNamespace)
    {
        if (!symbol.IsOriginalDefinition())
        {
            symbol = symbol.OriginalDefinition;
        }

        var helMethod = PopulateMember(symbol, new MethodDefinition()) with
        {
            AssociatedEvent = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IEventSymbol e
                ? GetEventReference(e)
                : null,
            AssociatedProperty = symbol.AssociatedSymbol is not null && symbol.AssociatedSymbol is IPropertySymbol p
                ? GetPropertyReference(p)
                : null,
            ContainingType = containingType,
            ContainingNamespace = containingNamespace,
            IsAsync = symbol.IsAsync,
            IsExtensionMethod = symbol.IsExtensionMethod,
            IsInitOnly = symbol.IsInitOnly,
            IsReadOnly = symbol.IsReadOnly,
            MethodKind = symbol.MethodKind.ToHelvegMethodKind(),
            OverridenMethod = symbol.OverriddenMethod is null ? null : GetMethodReference(symbol.OverriddenMethod),
            ReceiverType = symbol.ReceiverType is null ? null : GetTypeReference(symbol.ReceiverType),
            RefKind = symbol.RefKind.ToHelvegRefKind(),
            ReturnType = GetTypeReference(symbol.ReturnType)
        };

        return helMethod with
        {
            ExplicitInterfaceImplementations = symbol.ExplicitInterfaceImplementations
                .Select(GetMethodReference)
                .ToImmutableArray(),
            TypeParameters = symbol.TypeParameters
                .Select(p => GetTypeParameter(p, null, helMethod.Reference, containingNamespace))
                .ToImmutableArray(),
            Parameters = symbol.Parameters
                .Select(p => GetParameter(p, helMethod.Reference, null))
                .ToImmutableArray()
        };
    }

    private ParameterDefinition GetParameter(
        IParameterSymbol symbol,
        MethodReference? declaringMethod,
        PropertyReference? declaringProperty)
    {
        var helParameter = PopulateDefinition(symbol, new ParameterDefinition()) with
        {
            Ordinal = symbol.Ordinal,
            DeclaringMethod = declaringMethod,
            DeclaringProperty = declaringProperty,
            HasExplicitDefaultValue = symbol.HasExplicitDefaultValue,
            IsDiscard = symbol.IsDiscard,
            IsOptional = symbol.IsOptional,
            IsParams = symbol.IsParams,
            IsThis = symbol.IsThis,
            ParameterType = GetTypeReference(symbol.Type),
            RefKind = symbol.RefKind.ToHelvegRefKind()
        };

        return helParameter;
    }

    private TMember PopulateMember<TMember>(ISymbol symbol, TMember helMember)
        where TMember : MemberDefinition
    {
        return PopulateDefinition(symbol, helMember) with
        {
            Accessibility = symbol.DeclaredAccessibility.ToHelvegAccessibility(),
            CanBeReferencedByName = symbol.CanBeReferencedByName,
            IsAbstract = symbol.IsAbstract,
            IsExtern = symbol.IsExtern,
            IsImplicitlyDeclared = symbol.IsImplicitlyDeclared,
            IsOverride = symbol.IsOverride,
            IsSealed = symbol.IsSealed,
            IsStatic = symbol.IsStatic,
            IsVirtual = symbol.IsVirtual
        };
    }

    private T PopulateDefinition<T>(ISymbol symbol, T definition)
        where T : SymbolDefinition
    {
        var comment = symbol.GetDocumentationCommentXml();
        if (!string.IsNullOrEmpty(comment))
        {
            comment = MarkdownCommentVisitor.ToMarkdown(comment);
        }
        var diff = DiffStatus.Unmodified;
        if (compareToRef.TryGetTarget(out var compareToCompilation))
        {
            var compareToSymbols = SymbolFinder.FindSimilarSymbols(symbol, compareToCompilation).ToArray();
            if (compareToSymbols.Length == 0)
            {
                diff = DiffStatus.Added;
            }
            else if (compilationRef.TryGetTarget(out var compilation))
            {
                var symbols = SymbolFinder.FindSimilarSymbols(symbol, compilation).ToArray();
                if (symbols.Length == 0)
                {
                    diff = DiffStatus.Deleted;
                }
            }
        }
        return definition with
        {
            Token = tokenMap.GetOrAdd(symbol),
            Name = symbol.Name,
            Diagnostics = symbolDiagnostics.TryGetValue(symbol, out var diagnostics)
                ? diagnostics
                : ImmutableArray<Diagnostic>.Empty,
            Comments = !string.IsNullOrEmpty(comment)
                ? ImmutableArray.Create(new Comment(
                    Format: CommentFormat.Markdown,
                    Content: comment))
                : ImmutableArray<Comment>.Empty,
            DiffStatus = diff
        };
    }

    private AssemblyReference GetAssemblyReference(IAssemblySymbol symbol)
    {
        var reference = new AssemblyReference
        {
            Token = tokenMap.GetOrAdd(symbol),
            Hint = symbol.ToDisplayString()
        };
        return reference;
    }

    private ModuleReference GetModuleReference(IModuleSymbol symbol)
    {
        var reference = new ModuleReference
        {
            Token = tokenMap.GetOrAdd(symbol),
            Hint = symbol.ToDisplayString()
        };
        return reference;
    }

    private NamespaceReference GetNamespaceReference(INamespaceSymbol symbol)
    {

        var reference = new NamespaceReference
        {
            Token = tokenMap.GetOrAdd(symbol),
            Hint = symbol.ToDisplayString()
        };

        if (symbol.NamespaceKind != NamespaceKind.Module)
        {
            reference = reference with
            {
                Diagnostics = ImmutableArray.Create(Diagnostic.Warning(
                    "NonModuleNamespace",
                    $"Only 'module' namespaces should be turned into a {nameof(NamespaceReference)}."))
            };
        }

        return reference;
    }

    private TypeReference GetTypeReference(ITypeSymbol symbol)
    {
        if (!compilationRef.TryGetTarget(out var compilation) || compilation is null)
        {
            return TypeReference.Invalid;
        }

        TypeReference? reference;
        // TODO: IDynamicTypeSymbol
        // TODO: IErrorTypeSymbol
        switch (symbol)
        {
            case IArrayTypeSymbol arrayType:
                var arrayToken = tokenMap.GetOrAdd(compilation.GetSpecialType(SpecialType.System_Array));
                reference = new ArrayTypeReference
                {
                    Token = arrayToken,
                    ElementType = GetTypeReference(arrayType.ElementType),
                    TypeKind = TypeKind.Array,
                    Sizes = arrayType.Sizes,
                    Rank = arrayType.Rank,
                };
                break;
            case INamedTypeSymbol namedType:
                reference = new TypeReference
                {
                    Token = tokenMap.GetOrAdd(symbol.OriginalDefinition),
                };
                if (!namedType.IsOriginalDefinition())
                {
                    reference = reference with
                    {
                        TypeArguments = namedType.TypeArguments
                            .Select(GetTypeReference)
                            .ToImmutableArray()
                    };
                }
                break;
            case IFunctionPointerTypeSymbol fpType:
                var fnPtrToken = tokenMap.GetOrAdd(compilation.GetSpecialType(SpecialType.System_IntPtr));
                reference = new FunctionPointerTypeReference
                {
                    Token = fnPtrToken,
                    Signature = GetMethodReference(fpType.Signature)
                };
                break;
            case IPointerTypeSymbol pointerType:
                var intPtrToken = tokenMap.GetOrAdd(compilation.GetSpecialType(SpecialType.System_IntPtr));
                reference = new PointerTypeReference
                {
                    Token = intPtrToken,
                    PointedAtType = GetTypeReference(pointerType.PointedAtType)
                };
                break;
            case ITypeParameterSymbol typeParameter:
                reference = new TypeParameterReference
                {
                    Token = tokenMap.GetOrAdd(typeParameter)
                };
                break;

            default:
                reference = TypeReference.Invalid;
                break;
        }
        reference = reference with
        {
            Hint = symbol.ToDisplayString(),
            TypeKind = symbol.TypeKind.ToHelvegTypeKind(),
            Nullability = symbol.NullableAnnotation.ToHelvegNullability()
        };
        return reference;
    }

    private MethodReference GetMethodReference(IMethodSymbol symbol)
    {
        var isOriginal = symbol.IsOriginalDefinition();

        var reference = new MethodReference
        {
            Token = tokenMap.GetOrAdd(symbol),
            TypeArguments = isOriginal
                ? ImmutableArray<TypeReference>.Empty
                : symbol.TypeArguments
                    .Select(GetTypeReference)
                    .ToImmutableArray(),
        };

        return reference;
    }

    private EventReference GetEventReference(IEventSymbol symbol)
    {
        var reference = new EventReference
        {
            Token = tokenMap.GetOrAdd(symbol)
        };
        return reference;
    }

    private PropertyReference GetPropertyReference(IPropertySymbol symbol)
    {
        var reference = new PropertyReference
        {
            Token = tokenMap.GetOrAdd(symbol)
        };
        return reference;
    }

    private FieldReference GetFieldReference(IFieldSymbol symbol)
    {
        var reference = new FieldReference
        {
            Token = tokenMap.GetOrAdd(symbol)
        };
        return reference;
    }

    private ImmutableDictionary<ISymbol, ImmutableArray<Diagnostic>> GetDiagnostics()
    {
        if (!compilationRef.TryGetTarget(out var compilation) || compilation is null)
        {
            return ImmutableDictionary<ISymbol, ImmutableArray<Diagnostic>>.Empty;
        }

        var roslynDiagnostics = compilation.GetDiagnostics();

        var builder = ImmutableDictionary.CreateBuilder<ISymbol, ImmutableArray<Diagnostic>>(
            SymbolEqualityComparer.Default);

        foreach (var roslynDiagnostic in roslynDiagnostics)
        {
            if (roslynDiagnostic.Location.SourceTree is null)
            {
                // TODO: We should probably report metadata diagnostics somewhere.
                continue;
            }

            ISymbol? relatedSymbol = null;

            var semanticModel = compilation.GetSemanticModel(
                roslynDiagnostic.Location.SourceTree,
                ignoreAccessibility: true);
            if (roslynDiagnostic.Location.SourceTree.TryGetRoot(out var root))
            {
                // 1. Try to find a declaration closeby.
                var syntaxNode = root.FindNode(roslynDiagnostic.Location.SourceSpan)
                    .FirstAncestorOrSelf<SyntaxNode>(n =>
                    {
                        relatedSymbol = semanticModel.GetDeclaredSymbol(n);
                        return relatedSymbol is not null;
                    });

                // 2. Try to find any enclosing symbol.
                relatedSymbol ??= semanticModel.GetEnclosingSymbol(
                        (roslynDiagnostic.Location.SourceSpan.Start + roslynDiagnostic.Location.SourceSpan.End) / 2);
            }

            // 3. If everything else fails, this diagnostic will be associated with the assembly itself.
            relatedSymbol ??= compilation.Assembly;

            var helvegDiagnostic = new Diagnostic(
                roslynDiagnostic.Id,
                roslynDiagnostic.GetMessage(),
                roslynDiagnostic.Severity.ToHelvegSeverity());

            var existing = builder.GetValueOrDefault(relatedSymbol);
            existing = existing.IsDefault ? ImmutableArray<Diagnostic>.Empty : existing;

            if (!existing.Contains(helvegDiagnostic))
            {
                builder[relatedSymbol] = existing.Add(helvegDiagnostic);
            }
        }

        return builder.ToImmutable();
    }

    private static bool TryGetSimilarSymbol<TSymbol>(
        TSymbol symbol,
        WeakReference<Compilation?> searchedCompilation,
        [NotNullWhen(true)] out TSymbol? foundSymbol)
        where TSymbol : class, ISymbol
    {
        if (!searchedCompilation.TryGetTarget(out var compilation))
        {
            foundSymbol = null;
            return false;
        }

        var candidateSymbols = SymbolFinder.FindSimilarSymbols(symbol, compilation).ToArray();
        if (candidateSymbols.Length == 0)
        {
            foundSymbol = null;
            return false;
        }

        foundSymbol = candidateSymbols[0];
        return true;
    }
}
