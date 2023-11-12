using Helveg.CSharp.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

/// <summary>
/// Constants and readonly static data related to C#.
/// </summary>
public static class CSConst
{
    public const string CSharpNamespace = "csharp";
    
    public const string NodeStyle = "csharp:Entity";
    public const string RelationStyle = "csharp:Relation";

    public const string SolutionFileExtension = ".sln";
    public const string ProjectFileExtension = ".csproj";

    public const string MSBuildProjectNameProperty = "MSBuildProjectName";
    public const string TargetFrameworkProperty = "TargetFramework";
    public const string TargetFrameworksProperty = "TargetFrameworks";
    public const string RestoreTarget = "Restore";
    public const string ResolveReferencesTarget = "ResolveReferences";

    public const string GlobalNamespaceName = "global";

    public const string DefinitionSuffix = "Definition";

    public static readonly NumericToken InvalidToken = NumericToken.CreateInvalid(CSharpNamespace);
    public static readonly NumericToken NoneToken = NumericToken.CreateNone(CSharpNamespace);

    public static string KindOf<T>()
        where T : IEntity
    {
        return KindOf(typeof(T));
    }

    public static string KindOf(Type type)
    {
        var kind = type.Name;
        if (kind.Length > DefinitionSuffix.Length && kind.EndsWith(DefinitionSuffix, StringComparison.Ordinal))
        {
            kind = kind[0..^(DefinitionSuffix.Length)];
        }
        return kind;
    }
}
