import "./Index.scss";
import { Sigma } from "sigma";
import Graph from "graphology";
import circular from "graphology-layout/circular";
import forceAtlas2 from "graphology-layout-forceatlas2";
import { cropToLargestConnectedComponent } from "graphology-components";
import * as Model from "./Model";

// 1. Load data:
const dataElementId = "helveg-data";
const dataScript = document.getElementById(dataElementId);
if (dataScript == null) {
    throw new Error(`Could not find the '${dataElementId}' element.`);
}
const data = <Model.Multigraph>JSON.parse(dataScript.textContent!)
const graph = new Graph();

// 2. Build the graph:
for (const node of data.Nodes) {
    graph.addNode(node.Id, {
        label: node.Label || node.Id
    });
}

var containsRelation = data.Relations.filter(r => r.Id === "contains")[0];
for (const edge of containsRelation.Edges) {
    graph.addDirectedEdge(edge.Src, edge.Dst);
}

// 3. Only keep the main connected component:
cropToLargestConnectedComponent(graph);

// 4. Add colors to the nodes, based on node types:
const colors: Record<string, string> = {
    "Unknown": "#DCDCDC", // none -> VS label
    "Class": "#4EC9B0", // class -> VS class
    "Struct": "#86C691", // struct -> VS struct
    "Interface": "#B8D7A3", // interface -> VS interface
    "Delegate": "#4EC9B0", // delegate -> VS delegate
    "Enum": "#B8D7A3", // enum -> VS enum
};
graph.forEachNode((node, attributes) =>
    graph.setNodeAttribute(node, "color", colors[attributes.TypeKind]),
);

const degrees = graph.nodes().map((node) => graph.degree(node));
const minDegree = Math.min(...degrees);
const maxDegree = Math.max(...degrees);
const minSize = 2;
const maxSize = 15;
graph.forEachNode((node) => {
    const degree = graph.degree(node);
    graph.setNodeAttribute(
        node,
        "size",
        minSize + ((degree - minDegree) / (maxDegree - minDegree)) * (maxSize - minSize));
});

// 6. Position nodes on a circle, then run Force Atlas 2 for a while to get
//    proper graph layout:
circular.assign(graph);
const settings = forceAtlas2.inferSettings(graph);
forceAtlas2.assign(graph, { settings, iterations: 600 });

// 8. Finally, draw the graph using sigma:
const container = document.getElementById("sigma-container") as HTMLElement;
new Sigma(graph, container);
