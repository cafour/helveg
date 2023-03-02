using Helveg.CSharp;
using Microsoft.Build.Locator;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.Playground;
public static class Program
{
    public static async Task Main(string[] args)
    {
        var vsInstance = MSBuildLocator.RegisterDefaults();
        var provider = new DefaultEntityWorkspaceProvider();
        var workspace = await provider.GetWorkspace(@"C:\dev\helveg\Helveg.sln");
        if (workspace is not null)
        {
            Console.WriteLine(workspace);

            var typeCounts = workspace.Solution.Projects
                .Select(p => p.Assembly)
                .SelectMany(a => a.GetAllTypes())
                .Concat(workspace.Solution.ExternalDependencies
                    .Select(e => e.Assembly)
                    .SelectMany(a => a.GetAllTypes()))
                .GroupBy(t => t.MetadataName) 
                .Select(t => (name: t.Key, count: t.Count()))
                .OrderBy(p => p.count);
            foreach (var typeCount in typeCounts)
            {
                Console.WriteLine($"{typeCount.count}\t{typeCount.name}");
            }


            using var analysisStream = new FileStream("./analysis.json", FileMode.Create, FileAccess.ReadWrite);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            await JsonSerializer.SerializeAsync(analysisStream, workspace, options);

            var visualizationVisitor = new VisualizationEntityVisitor();
            visualizationVisitor.Visit(workspace);
            var multigraph = visualizationVisitor.Build();

            using var multigraphStream = new FileStream("./multigraph.json", FileMode.Create, FileAccess.ReadWrite);
            await JsonSerializer.SerializeAsync(multigraphStream, multigraph, options);
        }
    }
}
