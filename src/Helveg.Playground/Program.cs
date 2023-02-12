using Helveg.CSharp;
using Microsoft.Build.Locator;
using System;
using System.Threading.Tasks;

namespace Helveg.Playground;
public static class Program
{
    public static async Task Main(string[] args)
    {
        var vsInstance = MSBuildLocator.RegisterDefaults();
        var provider = new RoslynWorkspaceProvider();
        var workspace = await provider.GetWorkspace(@"C:\dev\helveg\Helveg.sln");
        Console.WriteLine(workspace);
    }
}
