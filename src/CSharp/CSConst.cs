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
    public const string CSharpPrefix = "csharp";

    public const string DeclaresId = "declares";
    public const string DeclaresLabel = "Declares";

    public const string InheritsFromId = "inheritsFrom";
    public const string InheritsFromLabel = "Inherits from";

    public const string ComposedOfId = "composedOf";
    public const string ComposedOfLabel = "Composed of";

    public const string TypeOfId = "typeOf";
    public const string TypeOfLabel = "Type of";

    public const string ReturnsId = "returns";
    public const string ReturnsLabel = "Returns";

    public const string SolutionFileExtension = ".sln";
    public const string ProjectFileExtension = ".csproj";

    public const string MSBuildProjectNameProperty = "MSBuildProjectName";
    public const string TargetFrameworkProperty = "TargetFramework";
    public const string TargetFrameworksProperty = "TargetFrameworks";
    public const string RestoreTarget = "Restore";
    public const string ResolveReferencesTarget = "ResolveReferences";
}
