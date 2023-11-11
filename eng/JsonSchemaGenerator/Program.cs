using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Json.Schema;
using Json.Schema.CodeGeneration;
using Json.Schema.CodeGeneration.Language;

namespace Helveg.Engineering.JsonSchemaGenerator;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCmd = new RootCommand("Generate POCOs from a JSON schema.");

        var schemaArg = new Argument<FileInfo>("schema");
        rootCmd.Add(schemaArg);

        var outputArg = new Argument<string>("output");
        rootCmd.Add(outputArg);

        rootCmd.SetHandler(GeneratePocos, schemaArg, outputArg);
        return await rootCmd.InvokeAsync(args);
    }

    public static async Task GeneratePocos(FileInfo schemaPath, string outputPath)
    {
        var schema = JsonSchema.FromFile(schemaPath.FullName);
        var generatedCode = schema.GenerateCode(CodeWriters.CSharp);
        await File.WriteAllTextAsync(outputPath, generatedCode);
    }
}
