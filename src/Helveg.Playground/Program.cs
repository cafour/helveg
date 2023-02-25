using Helveg.CSharp;
using Microsoft.Build.Locator;
using System;
using System.IO;
using System.Text.Json;
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
            using var stream = new FileStream("./analysis.json", FileMode.Create, FileAccess.ReadWrite);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            await JsonSerializer.SerializeAsync(stream, workspace, options);
        }
    }
}
