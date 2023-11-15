using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;
using System.Linq;
using Helveg.CSharp;
using Helveg.CSharp.Projects;
using Helveg.CSharp.Symbols;
using Helveg.UI;

namespace Helveg.CommandLine;

public enum ConfigPreset
{
    Dev,
    Docs
}

public record Config(
    ConfigPreset Preset,
    FileSystemInfo? Source,
    FileSystemInfo? CompareTo,
    AnalysisScope ProjectAnalysis,
    AnalysisScope ExternalAnalysis,
    UIMode Mode,
    string? Name,
    DirectoryInfo? OutDir,
    ImmutableDictionary<string, string> BuildProperties,
    bool Force,
    int InitialDepth,
    ImmutableArray<string> InitialRelations,
    ImmutableArray<string> InitialKinds
)
{
    public static readonly Config Dev;

    public static readonly Config Docs;

    public static readonly Option<ConfigPreset> PresetOpt = new(
        aliases: new[] { "--preset" },
        description: "Preset of default values for the other options.",
        getDefaultValue: () => ConfigPreset.Dev
    );

    public static readonly Argument<FileSystemInfo> SourceArg = new(
        name: "SOURCE",
        description: "Path to a project or a solution.",
        getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory))
    {
        Arity = ArgumentArity.ExactlyOne
    };
    
    public static readonly Option<FileSystemInfo?> CompareToOpt = new(
        aliases: new[] {"--compare-to"},
        description: "The target project or solution for comparison."
    );

    public static readonly Option<AnalysisScope?> ProjectAnalysisOpt = new(
        aliases: new[] { "-pa", "--project-analysis" },
        description: "Scope of the internal project analysis."
    );

    public static readonly Option<AnalysisScope?> ExternalAnalysisOpt = new(
        aliases: new[] { "-ea", "--external-analysis" },
        description: "Scope of the analysis of external dependencies."
    );

    public static readonly Option<UIMode?> ModeOpt = new(
        aliases: new[] { "-m", "--mode" },
        description: "UI mode to use."
    );

    public static readonly Option<bool> DryRunOpt = new(
        aliases: new[] { "--dry-run" },
        description: "Don't output anything. Equivalent to --mode None."
    );

    public static readonly Option<string?> NameOpt = new(
        aliases: new[] { "-n", "--name" },
        description: "Name of the resulting data set / visualization. Defaults to the SOURCE file name."
    );

    public static readonly Option<DirectoryInfo> OutDirOpt = new(
        aliases: new[] { "--outdir" },
        description: "Output directory.",
        getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory)
    );

    public static readonly Option<Dictionary<string, string>?> BuildPropertiesOpt = new(
        aliases: new[] { "-p", "--properties" },
        parseArgument: a => a.Tokens
            .Select(t => t.Value.Split(new[] { '=' }, 2))
            .ToDictionary(p => p[0], p => p[1]),
        description: "Set an MSBuild property for the loading of projects"
    );

    public static readonly Option<bool?> ForceOpt = new(
        aliases: new[] { "-f", "--force" },
        description: "Overwrite previous results."
    );
    
    public static readonly Option<int?> InitialDepthOpt = new(
        aliases: new[] { "--initial-depth" },
        description: "Initial expanded depth of the diagram."
    );
    
    public static readonly Option<List<string>?> InitialRelationsOpt = new(
        aliases: new[] { "--initial-relation" },
        description: "Initial selected relations of the diagram."
    );
    
    public static readonly Option<List<string>?> InitialKindsOpt = new(
        aliases: new[] { "--initial-kind" },
        description: "Initial selected kinds of the diagram."
    );

    static Config()
    {
        BuildPropertiesOpt.AddValidator(r =>
        {
            foreach (var token in r.Tokens)
            {
                if (!token.Value.Contains('='))
                {
                    r.ErrorMessage = "MSBuild properties must follow the '<n>=<v>' format.";
                }
            }
        });

        var initialKinds = ImmutableArray.Create(
            CSConst.KindOf<Solution>(),
            CSConst.KindOf<Project>(),
            CSConst.KindOf<NamespaceDefinition>(),
            CSConst.KindOf<TypeDefinition>(),
            CSConst.KindOf<MethodDefinition>(),
            CSConst.KindOf<PropertyDefinition>(),
            CSConst.KindOf<EventDefinition>(),
            CSConst.KindOf<FieldDefinition>(),
            CSConst.KindOf<TypeParameterDefinition>(),
            CSConst.KindOf<ParameterDefinition>()
        );

        Dev = new(
            Preset: ConfigPreset.Dev,
            Source: null,
            CompareTo: null,
            ProjectAnalysis: AnalysisScope.Explicit,
            ExternalAnalysis: AnalysisScope.WithoutSymbols,
            Mode: UIMode.SingleFile,
            Name: null,
            OutDir: null,
            BuildProperties: ImmutableDictionary<string, string>.Empty,
            Force: false,
            InitialDepth: 1,
            InitialRelations: ImmutableArray.Create(CSRelations.Declares),
            InitialKinds: initialKinds
        );

        Docs = new(
            Preset: ConfigPreset.Docs,
            Source: null,
            CompareTo: null,
            ProjectAnalysis: AnalysisScope.PublicApi,
            ExternalAnalysis: AnalysisScope.None,
            Mode: UIMode.DataOnly,
            Name: null,
            OutDir: null,
            BuildProperties: ImmutableDictionary<string, string>.Empty,
            Force: false,
            InitialDepth: 1,
            InitialRelations: ImmutableArray.Create(CSRelations.Declares),
            InitialKinds: initialKinds
        );
    }

    public class Binder : BinderBase<Config>
    {
        protected override Config GetBoundValue(BindingContext bindingContext)
        {
            T? Value<T>(IValueDescriptor<T> descriptor)
            {
                return descriptor switch
                {
                    Option<T> opt => bindingContext.ParseResult.GetValueForOption<T>(opt),
                    Argument<T> arg => bindingContext.ParseResult.GetValueForArgument<T>(arg),
                    _ => throw new NotSupportedException()
                };
            }

            var preset = Value(PresetOpt);
            var config = preset switch
            {
                ConfigPreset.Dev => Dev,
                ConfigPreset.Docs => Docs,
                _ => throw new NotSupportedException($"Preset '{preset}' is not supported.")
            };
            
            var initialRelations = Value(InitialRelationsOpt);
            var initialKinds = Value(InitialKindsOpt);
            
            config = config with
            {
                Source = Value(SourceArg),
                CompareTo = Value(CompareToOpt),
                ProjectAnalysis = Value(ProjectAnalysisOpt) ?? config.ProjectAnalysis,
                ExternalAnalysis = Value(ExternalAnalysisOpt) ?? config.ExternalAnalysis,
                Mode = Value(ModeOpt) ?? config.Mode,
                Name = Value(NameOpt),
                OutDir = Value(OutDirOpt),
                BuildProperties = Value(BuildPropertiesOpt)?.ToImmutableDictionary()
                    ?? config.BuildProperties,
                Force = Value(ForceOpt) ?? config.Force,
                InitialDepth = Value(InitialDepthOpt) ?? config.InitialDepth,
                InitialRelations = initialRelations?.Count > 0
                    ? initialRelations.ToImmutableArray()
                    : config.InitialRelations,
                InitialKinds = initialKinds?.Count > 0
                    ? initialKinds.ToImmutableArray()
                    : config.InitialKinds
            };

            if (Value(DryRunOpt))
            {
                config = config with
                {
                    Mode = UIMode.None
                };
            }

            return config;
        }
    }
}
