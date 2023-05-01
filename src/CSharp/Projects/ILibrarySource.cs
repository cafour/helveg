using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public interface ILibrarySource : IEntity
{
    [JsonIgnore]
    NumericToken Token { get; }

    ImmutableArray<Library> Libraries { get; init; }

    ILibrarySource WithLibraries(ImmutableArray<Library> libraries);
}
