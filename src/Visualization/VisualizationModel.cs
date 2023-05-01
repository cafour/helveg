using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record VisualizationModel
{
    public static VisualizationModel Invalid { get; } = new();

    public DocumentInfo DocumentInfo { get; init; } = DocumentInfo.Invalid;
    
    public Multigraph Multigraph { get; init; } = Multigraph.Invalid;

    [JsonIgnore]
    public bool IsValid => DocumentInfo is not null && DocumentInfo.Name != Const.Invalid;
}
