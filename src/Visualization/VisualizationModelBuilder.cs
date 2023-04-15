using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public class VisualizationModelBuilder
{
    private DocumentInfo? documentInfo;
    private Multigraph? multigraph;
    private readonly Dictionary<string, INodeFilter> nodeFilters = new();

    public static VisualizationModelBuilder CreateDefault()
    {
        return new();
    }

    public VisualizationModelBuilder SetDocumentInfo(DocumentInfo documentInfo)
    {
        this.documentInfo = documentInfo;
        return this;
    }

    public VisualizationModelBuilder SetMultigraph(Multigraph multigraph)
    {
        this.multigraph = multigraph;
        return this;
    }

    public VisualizationModelBuilder AddNodeFilter(string nodeFilterName, INodeFilter nodeFilter)
    {
        nodeFilters[nodeFilterName] = nodeFilter;
        return this;
    }

    public VisualizationModel Build()
    {
        if (documentInfo is null)
        {
            throw new InvalidOperationException("A document info must be set first.");
        }

        if (multigraph is null)
        {
            throw new InvalidOperationException("A multigraph must be set first.");
        }

        return new(
            DocumentInfo: documentInfo,
            Multigraph: multigraph,
            NodeFilters: nodeFilters.ToImmutableDictionary());
    }
}
