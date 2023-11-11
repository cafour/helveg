using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

namespace Helveg.Engineering.Roslyn;

[Generator]
public class JsonSchemaSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var file in context.AdditionalFiles)
        {
            if (file is not null && context.AnalyzerConfigOptions.GetOptions(file)
                .TryGetPropertyValue("build_metadata.IsJsonSchema", false))
            {
                var sourceText = file.GetText();
                if (sourceText is null)
                {
                    continue;
                }

                var schema = JsonSchema.FromJsonAsync(sourceText.ToString()).GetAwaiter().GetResult();
                var generator = new CSharpGenerator(schema);
                var code = generator.GenerateFile();
                context.AddSource(Path.GetFileNameWithoutExtension(file.Path), code);
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }
}
