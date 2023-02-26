using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record Multigraph(
    string Id,
    string? Label,
    ImmutableArray<Node> Nodes,
    ImmutableArray<Relation> Relations);
