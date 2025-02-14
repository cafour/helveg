using Helveg.CSharp.Projects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Packages;

public class NuGetMiner : IMiner
{
    private readonly ILogger<NuGetMiner> logger;

    private int counter = 0;

    public NuGetMinerOptions Options { get; }

    MinerOptions IMiner.Options => Options;

    public NuGetMiner(NuGetMinerOptions options, ILogger<NuGetMiner>? logger = null)
    {
        Options = options;
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<NuGetMiner>();
    }

    public async Task Mine(Workspace workspace, CancellationToken cancellationToken = default)
    {
        var repo = new PackageRepository
        {
            Index = 1,
            Name = "NuGet"
        };

        var externalSources = workspace.Roots.Values.OfType<ExternalDependencySource>().ToImmutableArray();
        foreach (var eds in externalSources)
        {
            using var edsHandle = await workspace.GetRootExclusively<ExternalDependencySource>(eds.Id, cancellationToken);
            if (edsHandle.Entity is null)
            {
                logger.LogError($"{nameof(ExternalDependencySource)} '{{}}' no longer exists. " +
                    $"This is likely due to a race condition.", eds.Id);
                continue;
            }

            logger.LogInformation("Sorting NuGet packages.");
            foreach (var library in edsHandle.Entity.Libraries)
            {
                // "steal" libraries from the EDS that are part of a package
                if (!string.IsNullOrEmpty(library.PackageId))
                {
                    repo = AddPackage(repo, library);
                }
            }

            // remove the libraries with PackageId, making the repo their one and only owner
            edsHandle.Entity = edsHandle.Entity with
            {
                Libraries = edsHandle.Entity.Libraries.RemoveAll(l => !string.IsNullOrEmpty(l.PackageId))
            };
        }

        // NB: This solution of finding NuGet packages is quite dirty and for whatever reason doesn't work on, for
        //     example the PolySharp dependency in the Helveg repo. Though there could be another reason.
        //     so TODO: Fix missing PolySharp dependency in Helveg output on Helveg.
        using (var solutionHandle = await workspace.GetSolutionExclusively(logger, cancellationToken))
        {
            if (solutionHandle is null || solutionHandle.Entity is null)
            {
                return;
            }

            solutionHandle.Entity = solutionHandle.Entity with
            {
                Projects = [.. solutionHandle.Entity.Projects.Select(p => SetPackageDependencyTokens(p, repo))]
            };
        }

        workspace.TryAddRoot(repo);
    }

    private PackageRepository AddPackage(PackageRepository repo, Library library)
    {
        if (string.IsNullOrEmpty(library.PackageId))
        {
            return repo;
        }

        var libraryExtension = new LibraryExtension { Library = library };

        var existingIndex = repo.Packages.FindIndex(p => p.Name == library.PackageId);
        if (existingIndex < 0)
        {
            logger.LogDebug("Discovered the '{}' package.", library.PackageId);
            repo = repo with
            {
                Packages = repo.Packages.Add(new Package
                {
                    Token = repo.Token.Derive(++counter),
                    Name = library.PackageId,
                    Versions = string.IsNullOrEmpty(library.PackageVersion)
                        ? []
                        : [library.PackageVersion],
                    Extensions = [libraryExtension]
                })
            };
        }
        else
        {
            var package = repo.Packages[existingIndex];
            package = package with
            {
                Versions = string.IsNullOrEmpty(library.PackageVersion) || package.Versions.Contains(library.PackageVersion)
                    ? package.Versions
                    : package.Versions.Add(library.PackageVersion),
                Extensions = package.Extensions.Add(libraryExtension)
            };
            repo = repo with
            {
                Packages = repo.Packages.SetItem(existingIndex, package)
            };
        }
        return repo;
    }

    private static Project SetPackageDependencyTokens(Project project, PackageRepository repo)
    {
        return project with
        {
            PackageDependencies = project.PackageDependencies.Select(pair =>
            {
                var deps = pair.Value.Select(d =>
                {
                    var package = repo.Packages
                        .Where(p => p.Name.Equals(d.Name, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault();
                    if (package is null)
                    {
                        return d;
                    }

                    return d with
                    {
                        Token = package.Token
                    };
                })
                .ToImmutableArray();
                return new KeyValuePair<string, ImmutableArray<PackageDependency>>(pair.Key, deps);
            })
            .ToImmutableDictionary()
        };
    }
}
