using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record VisualizationModel(
    DocumentInfo DocumentInfo,
    Multigraph Multigraph,
    ImmutableDictionary<string, INodeFilter> NodeFilters);
