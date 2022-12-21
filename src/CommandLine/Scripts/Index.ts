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
const data = <Model.Solution>JSON.parse(dataScript.textContent!)
const graph = new Graph();

// 2. Build the graph:
if (data.Projects == null || Object.keys(data.Projects).length == 0) {
    throw new Error("Solution contains no projects.");
}

let edgeQueue = [];

for (const projectName of Object.keys(data.Projects)) {
    const project = data.Projects[projectName];
    if (project.Types == null || Object.keys(project.Types).length == 0) {
        console.warn(`The '${project.Name}' project contains no types!`);
        continue;
    }

    for (const typeName of Object.keys(project.Types)) {
        const type = project.Types[typeName];
        graph.addNode(type.Id, {
            label: type.Id,
            kind: type.Kind
        });
        if (type.Relations == null || Object.keys(type.Relations).length == 0) {
            continue;
        }

        for (const relationName of Object.keys(type.Relations)) {
            edgeQueue.push({ source: type.Id, target: relationName });
        }
    }
}

for (const edge of edgeQueue) {
    graph.addDirectedEdge(edge.source, edge.target);
}

// 3. Only keep the main connected component:
cropToLargestConnectedComponent(graph);

// 4. Add colors to the nodes, based on node types:
const colors: Record<Model.TypeKind, string> = {
    "0": "#DCDCDC", // none -> VS label
    "1": "#4EC9B0", // class -> VS class
    "2": "#86C691", // struct -> VS struct
    "3": "#B8D7A3", // interface -> VS interface
    "4": "#4EC9B0", // delegate -> VS delegate
    "5": "#B8D7A3", // enum -> VS enum
};
graph.forEachNode((node, attributes) =>
    graph.setNodeAttribute(node, "color", colors[<Model.TypeKind>attributes.kind]),
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
