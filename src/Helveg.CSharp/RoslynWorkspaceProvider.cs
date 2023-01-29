using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Helveg.CSharp;

public class RoslynWorkspaceProvider : IHelWorkspaceCSProvider
{
    public async Task<HelWorkspaceCS> GetWorkspace(string path, CancellationToken cancellationToken = default)
    {
        var workspace = MSBuildWorkspace.Create();
        await workspace.OpenSolutionAsync(path, cancellationToken: cancellationToken);

        return new HelWorkspaceCS
        {
            Solution = await GetSolution(workspace.CurrentSolution, cancellationToken)
        };
    }

    private async Task<HelSolutionCS> GetSolution(Solution solution, CancellationToken cancellationToken = default)
    {
        var projects = (await Task.WhenAll(solution.Projects.Select(p => GetProject(p, cancellationToken))))
            .ToImmutableArray();

        return new HelSolutionCS
        {
            Name = solution.FilePath ?? IHelEntityCS.InvalidName,
            Projects = projects
        };
    }

    private async Task<HelProjectCS> GetProject(Project project, CancellationToken cancellationToken = default)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation is null)
        {
            return HelProjectCS.Invalid;
        }

        return new HelProjectCS
        {
            Name = project.Name,
            Assembly = await GetAssembly(compilation.Assembly, cancellationToken)
        };
    }
    
    private async Task<HelAssemblyCS> GetAssembly(
        IAssemblySymbol assembly,
        CancellationToken cancellationToken = default)
    {
        return new HelAssemblyCS
        {
            Name = assembly.Name
        };
    }
}
