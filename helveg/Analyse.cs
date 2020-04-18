using System;
using System.Numerics;
using Microsoft.CodeAnalysis.MSBuild;

namespace Helveg
{
    public static class Analyse
    {
        public static float[,] ConstructGraph(string projectPath)
        {
            var workspace = MSBuildWorkspace.Create();
            var project = workspace.OpenProjectAsync(projectPath).GetAwaiter().GetResult();
            foreach(var diagnostic in workspace.Diagnostics)
            {
                Console.Write($"[{diagnostic.Kind}]: {diagnostic.Message}\n\n");
            }
            Console.WriteLine($"Total MSBuild Diagnostics: {workspace.Diagnostics.Count}");
            return new float[0, 0];
        }
    }
}
