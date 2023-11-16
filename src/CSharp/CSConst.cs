using Helveg.CSharp.Symbols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

    /// <summary>
    /// Finds a solution or project file at the specified path. Preferes solutions before projects if ambiguous.
    /// </summary>
    public static string? FindSource(string path, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        if (Directory.Exists(path))
        {
            var solutionFiles = Directory.GetFiles(path, $"*{SolutionFileExtension}");
            if (solutionFiles.Length > 1)
            {
                logger.LogError(
                    "The '{}' directory contains multiple solution files. Provide a path to one them.",
                    path);
                return null;
            }
            else if (solutionFiles.Length == 1)
            {
                path = solutionFiles[0];
            }
            else
            {
                var csprojFiles = Directory.GetFiles(path, $"*{CSConst.ProjectFileExtension}");
                if (csprojFiles.Length > 1)
                {
                    logger.LogError(
                        "The '{}' directory contains multiple C# project files. Provide a path to one them.",
                        path);
                    return null;
                }
                else if (csprojFiles.Length == 1)
                {
                    path = csprojFiles[0];
                }
                else
                {
                    logger.LogCritical(
                        "The '{}' directory contains no solution nor C# project files.",
                        path);
                    return null;
                }
            }
        }

        if (!File.Exists(path))
        {
            logger.LogError("The source file '{}' does not exist.", path);
            return null;
        }

        var fileExtension = Path.GetExtension(path);
        if (fileExtension != SolutionFileExtension && fileExtension != ProjectFileExtension)
        {
            logger.LogError("The source file '{}' is not a solution nor a C# project file.", path);
            return null;
        }

        return path;
    }

    public static MemberAccessibility GetEffectiveAccessibility(MemberAccessibility member, MemberAccessibility? parent)
    {
        if (parent is null)
        {
            return member;
        }
        
        return member switch
        {
            MemberAccessibility.Invalid => MemberAccessibility.Invalid,
            MemberAccessibility.Private => MemberAccessibility.Private,
            MemberAccessibility.ProtectedAndInternal => parent == MemberAccessibility.Private
                ? MemberAccessibility.Private
                : MemberAccessibility.ProtectedAndInternal,
            MemberAccessibility.Protected or MemberAccessibility.ProtectedOrInternal => parent switch
            {
                MemberAccessibility.Private => MemberAccessibility.Private,
                MemberAccessibility.Internal => MemberAccessibility.ProtectedAndInternal,
                _ => member
            },
            MemberAccessibility.Public => parent switch
            {
                MemberAccessibility.Invalid => MemberAccessibility.Public,
                _ => parent.Value
            },
            _ => member
        };
    }
}
