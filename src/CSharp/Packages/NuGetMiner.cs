using Helveg.CSharp.Projects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
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

        void AddPackage(Library library)
        {
            if (string.IsNullOrEmpty(library.PackageId))
            {
                return;
            }

            var libraryExtension = new LibraryExtension { Library = library };

            var existingIndex = repo.Packages.FindIndex(p => p.Name ==  library.PackageId);
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
                            ? ImmutableArray<string>.Empty
                            : ImmutableArray.Create<string>(library.PackageVersion),
                        Extensions = ImmutableArray.Create<IEntityExtension>(libraryExtension)
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
        }

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

            foreach (var library in edsHandle.Entity.Libraries)
            {
                // "steal" libraries from the EDS that are part of a package
                if (!string.IsNullOrEmpty(library.PackageId))
                {
                    AddPackage(library);
                }
            }

            // remove the libraries with PackageId, making the repo their one and only owner
            edsHandle.Entity = edsHandle.Entity with
            {
                Libraries = edsHandle.Entity.Libraries.RemoveAll(l => !string.IsNullOrEmpty(l.PackageId))
            };
        }

        workspace.TryAddRoot(repo);
    }
}
