import Graph from "graphology";
import { DEFAULT_DATA_OPTIONS, DEFAULT_EXPORT_OPTIONS, DEFAULT_GLYPH_OPTIONS, DEFAULT_LAYOUT_OPTIONS, SearchMode, type DataOptions, type ExportOptions, type GlyphOptions, type LayoutOptions } from "./options";
import { EMPTY_MODEL, type VisualizationModel } from "./visualization";
import { Sigma } from "sigma";
import { ForceAtlas2Supervisor, type ForceAtlas2Progress } from "layout/forceAltas2Supervisor";
import { IconAtlas } from "rendering/iconAtlas";
import { HelvegEvent } from "common/event";
import { OutlineStyle, getOutlinesTotalWidth, type Outlines, type GlyphStyle, GlyphStyleRegistry } from "./glyph";
import type { SigmaNodeEventPayload } from "sigma/sigma";
import { createGlyphProgram, type GlyphProgramOptions } from "rendering/node.glyph";
import forceAtlas2 from "graphology-layout-forceatlas2";
import type { NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import { exportDiagram } from "rendering/export";
import tidyTree from "layout/tidyTree";
import type { HelvegInstance } from "./instance";
import { buildNodeFilter, filterNodes } from "./filter";
import type { NodeDisplayData } from "sigma/types";

export enum StructuralStatus {
    Stopped,
    Running,
    RunningInBackground
}

export interface StructuralDiagramStats {
    iterationCount: number;
    speed: number;
}

export enum StructuralDiagramMode {
    Normal = "normal",
    Highlighting = "highlighting"
}

export interface AbstractStructuralDiagram {
    get element(): HTMLElement | null;
    set element(value: HTMLElement | null);

    get model(): VisualizationModel;
    set model(value: VisualizationModel);

    get dataOptions(): DataOptions;
    set dataOptions(value: DataOptions);

    get glyphOptions(): GlyphOptions;
    set glyphOptions(value: GlyphOptions);

    get layoutOptions(): LayoutOptions;
    set layoutOptions(value: LayoutOptions);

    get status(): StructuralStatus;
    get statusChanged(): HelvegEvent<StructuralStatus>;

    get stats(): StructuralDiagramStats;
    get statsChanged(): HelvegEvent<StructuralDiagramStats>;

    get selectedNodeId(): string | null;
    get nodeSelected(): HelvegEvent<string | null>;

    resetLayout(): Promise<void>;
    runLayout(inBackground: boolean): Promise<void>;
    stopLayout(): Promise<void>;
    save(options?: ExportOptions): void;
    highlight(searchText: string | null, searchMode: SearchMode): void;
    isolate(searchText: string | null, searchMode: SearchMode): void;
    reset(): Promise<void>;
}

export interface HelvegNodeAttributes extends NodeDisplayData {
    style: string;
    icon: string;
    iconSize: number;
    outlines: Outlines;
}

type HelvegGraph = Graph<Partial<HelvegNodeAttributes>>;

/**
 * An implementation of StructuralDiagram that is tied to an HTMLElement but not to a specific UI framework.
 */
export class StructuralDiagram implements AbstractStructuralDiagram {
    private _element: HTMLElement | null = null;
    private _model: VisualizationModel = EMPTY_MODEL;
    private _nodeKeys: string[] = [];
    private _dataOptions: DataOptions = DEFAULT_DATA_OPTIONS;
    private _glyphOptions: GlyphOptions = DEFAULT_GLYPH_OPTIONS
    private _layoutOptions: LayoutOptions = DEFAULT_LAYOUT_OPTIONS;
    private _status: StructuralStatus = StructuralStatus.Stopped;
    private _statusChanged: HelvegEvent<StructuralStatus>
        = new HelvegEvent<StructuralStatus>("helveg.StructuralDiagram.statusChanged");
    private _stats: StructuralDiagramStats = {
        iterationCount: 0,
        speed: 0
    };
    private _statsChanged: HelvegEvent<StructuralDiagramStats>
        = new HelvegEvent<StructuralDiagramStats>("helveg.StructuralDiagram.statsChanged");
    private _selectedNodeId: string | null = null;
    private _nodeSelected: HelvegEvent<string | null>
        = new HelvegEvent<string | null>("helveg.StructuralDiagram.nodeSelected");

    private _graph: HelvegGraph | null = null;
    private _sigma: Sigma | null = null;
    private _supervisor: ForceAtlas2Supervisor | null = null;
    private _iconAtlas: IconAtlas;
    private _glyphProgramOptions: GlyphProgramOptions;
    private _glyphProgram: NodeProgramConstructor;

    constructor(private _instance: HelvegInstance) {
        this._iconAtlas = new IconAtlas(this._instance.icons);
        this._glyphProgramOptions = {
            gap: 0.5,
            iconAtlas: this._iconAtlas,
            showIcons: true,
            showOutlines: true,
            diagramMode: StructuralDiagramMode.Normal,
        };
        this._glyphProgram = createGlyphProgram(this._glyphProgramOptions);
    }

    async resetLayout(): Promise<void> {
        if (!this._graph) {
            DEBUG && console.warn("Cannot reset layout since the graph is not initialized.");
            return;
        }

        if (!this._model) {
            DEBUG && console.warn("Cannot reset layout since the model is not initialized.");
            return;
        }

        DEBUG && console.log("Resetting the layout.");

        if (this._supervisor?.isRunning) {
            await this.stopLayout();
        }
        if (this._supervisor) {
            this._supervisor.kill();
            this._supervisor = initializeSupervisor(this._graph, this.onSupervisorProgress.bind(this));
        }

        // TODO: add a setting for roots
        let solutionRoot = Object.entries(this._model.multigraph.nodes).find(
            ([k, v]) => v.properties["Kind"] === "csharp:Solution"
        )?.[0];
        // let frameworkRoots = Object.entries(this._model.multigraph.nodes).find(
        //     ([k, v]) => v.properties["Kind"] === "csharp:Framework"
        // )?.[0];
        if (solutionRoot) {
            tidyTree(this._graph, solutionRoot, 1000);
        }

        if (this._sigma) {
            this._sigma.scheduleRefresh();
        }
    }

    async runLayout(inBackground: boolean): Promise<void> {
        if (!this._graph) {
            DEBUG && console.warn("Cannot run since the graph is not initialized.");
            return;
        }

        if (!this._supervisor) {
            DEBUG && console.warn("Cannot run since the supervisor is not initialized.");
            return;
        }

        DEBUG && console.log("Running the layout.");

        this._supervisor.start(inBackground);

        this.status = inBackground
            ? StructuralStatus.RunningInBackground
            : StructuralStatus.Running;

        if (inBackground) {
            this._sigma?.kill();
            this._sigma = null;
        } else if (!this._sigma) {
            this.refreshSigma();
        }
    }

    async stopLayout(): Promise<void> {
        if (!this._graph) {
            DEBUG && console.warn("Cannot stop since the graph is not initialized.");
            return;
        }

        DEBUG && console.log("Stopping the layout.");

        if (this._supervisor?.isRunning) {
            await this._supervisor.stop();
            this.status = StructuralStatus.Stopped;

            if (!this._sigma) {
                this.refreshSigma();
            }
        }
    }

    save(options?: ExportOptions): void {
        if (!this._graph) {
            DEBUG && console.warn("Cannot save since the graph is not initialized.")
            return;
        }

        if (!this._sigma) {
            DEBUG && console.warn("Cannot save since the sigma instance is not initialized.")
            return;
        }

        DEBUG && console.log("Saving the diagram.");

        options = { ...DEFAULT_EXPORT_OPTIONS, ...options };
        options.fileName ??= `${this._model.multigraph.label}-export.png`;
        if (this._sigma) {
            exportDiagram(this._sigma, options);
        }
    }

    highlight(searchText: string | null, searchMode: SearchMode): void {
        if (!this._graph) {
            DEBUG && console.warn("Cannot highlight nodes since the graph is not initialized.");
            return;
        }

        try {
            let filter = buildNodeFilter(searchText, searchMode, this._nodeKeys);
            if (filter === null) {
                this._glyphProgramOptions.diagramMode = StructuralDiagramMode.Normal;
                this._graph.forEachNode((_, a) => a.highlighted = undefined);
                this._sigma?.refresh();
                return;
            }

            this._glyphProgramOptions.diagramMode = StructuralDiagramMode.Highlighting;

            Object.entries(this._model.multigraph.nodes).forEach(([id, node]) => {
                if (this._graph?.hasNode(id)) {
                    this._graph.setNodeAttribute(id, "highlighted", filter!(node));
                }
            });
            
            this._sigma?.refresh();
        }
        catch (e: any) {
            this._instance.logger.warn(e?.message 
                ?? e?.toString() 
                ?? "Something bad happened while highlighting nodes.");
            return;
        }

        this._instance.logger.info(`Highlighting ${this._graph.reduceNodes((count, _, attributes) =>
            attributes.highlighted ? count + 1 : count, 0)} nodes.`);
    }

    isolate(searchText: string | null, searchMode: SearchMode): void {
        if (!this._graph) {
            DEBUG && console.warn("Cannot isolate nodes since the graph is not initialized.");
            return;
        }

        try {
            let filter = buildNodeFilter(searchText, searchMode, this._nodeKeys);
            if (filter === null) {
                return;
            }
            for (let id of filterNodes(this._model.multigraph, filter, true)) {
                if (this._graph.hasNode(id)) {
                    this._graph.dropNode(id);
                }
            }
        }
        catch (e: any) {
            this._instance.logger.warn(e?.message 
                ?? e?.toString() 
                ?? "Something bad happened while isolating nodes.");
            return;
        }

        this._instance.logger.info(`Isolated ${this._graph.nodes().length} nodes.`);
    }

    async reset(): Promise<void> {
        this.refreshGraph();
        await this.resetLayout();
    }

    get element(): HTMLElement | null {
        return this._element;
    }

    set element(value: HTMLElement | null) {
        this._element = value;
        this.refreshSigma();
    }

    get model(): VisualizationModel {
        return this._model;
    }

    set model(value: VisualizationModel) {
        this._model = value;

        this.refreshGraph();
        this.refreshSigma();

        this._nodeKeys = Object.values(this._model.multigraph.nodes)
            .flatMap(n => Object.keys(n.properties))
            .filter((v, i, a) => a.indexOf(v) === i);
    }

    get dataOptions(): DataOptions {
        return this._dataOptions;
    }

    set dataOptions(value: DataOptions) {
        this._dataOptions = value;
        this.refreshGraph();
    }

    get glyphOptions(): GlyphOptions {
        return this._glyphOptions;
    }

    set glyphOptions(value: GlyphOptions) {
        this._glyphOptions = value;
        if (this._sigma) {
            configureSigma(this._sigma, this._glyphOptions);
        }

        this._glyphProgramOptions.showIcons = this._glyphOptions.showIcons;
        this._glyphProgramOptions.showOutlines = this._glyphOptions.showOutlines;
    }

    get layoutOptions(): LayoutOptions {
        return this._layoutOptions;
    }

    set layoutOptions(value: LayoutOptions) {
        this._layoutOptions = value;
    }

    get status(): StructuralStatus {
        return this._status;
    }

    get statusChanged(): HelvegEvent<StructuralStatus> {
        return this._statusChanged;
    }

    private set status(value: StructuralStatus) {
        if (this._status !== value) {
            this._status = value;
            this._statusChanged.trigger(value);
        }
    }

    get stats(): StructuralDiagramStats {
        return this._stats;
    }

    get statsChanged(): HelvegEvent<StructuralDiagramStats> {
        return this._statsChanged;
    }

    private set stats(value: StructuralDiagramStats) {
        this._stats = value;
        this._statsChanged.trigger(value);
    }

    get selectedNodeId(): string | null {
        return this._selectedNodeId;
    }

    get nodeSelected(): HelvegEvent<string | null> {
        return this._nodeSelected;
    }

    private set selectedNodeId(value: string | null) {
        if (this._selectedNodeId !== value) {
            this._selectedNodeId = value;
            this._nodeSelected.trigger(value);
        }
    }

    private refreshSigma(): void {
        if (!this._element || !this._graph) {
            return;
        }

        DEBUG && console.log("Refreshing the Sigma.js instance.");

        if (this._sigma) {
            this._sigma.kill();
        }

        this._sigma = initializeSigma(this._element, this._graph, this._glyphProgram, this.onNodeClick.bind(this));
        configureSigma(this._sigma, this._glyphOptions);
    }

    private refreshGraph(): void {
        if (!this._model || this._model.isEmpty) {
            return;
        }

        DEBUG && console.log(`Refreshing the graph to match the '${this._model.multigraph.label}' model.`);

        this._graph = initializeGraph(this._model, this._dataOptions);
        stylizeGraph(this._graph, this._model, this._instance.styles);

        if (this._supervisor) {
            this._supervisor.kill();
            this._supervisor = null;
        }

        this._sigma?.setGraph(this._graph);

        this._supervisor = initializeSupervisor(this._graph, this.onSupervisorProgress.bind(this));

        this._glyphProgramOptions.diagramMode = StructuralDiagramMode.Normal;
    }

    private onNodeClick(event: SigmaNodeEventPayload): void {
        this.selectedNodeId = event.node;
    }

    private onSupervisorProgress(message: ForceAtlas2Progress) {
        this.stats = {
            iterationCount: message.iterationCount,
            speed: message.speed
        };
    }

    // private nodeReducer(node: HelvegNodeAttributes, data: HelvegNodeAttributes): Partial<NodeDisplayData> {

    // }
}

function initializeGraph(
    model: VisualizationModel,
    dataOptions: DataOptions,
): HelvegGraph {

    const graph = new Graph<Partial<HelvegNodeAttributes>>();
    for (const nodeId in model.multigraph.nodes) {
        const node = model.multigraph.nodes[nodeId];
        graph.addNode(nodeId, {
            label: node.label || nodeId,
            x: 0,
            y: 0,
            style: "csharp:Entity",

        });
    }

    for (const relationId of dataOptions.selectedRelations) {
        const relation = model.multigraph.relations[relationId];
        if (!relation) {
            continue;
        }

        for (const edge of relation.edges) {
            try {
                graph.addDirectedEdge(edge.src, edge.dst);
            } catch (error) {
                // console.warn(`Failed to add an edge. edge=${edge}, error=${error}`);
            }
        }
    }

    return graph;
}

function stylizeGraph(graph: Graph, model: VisualizationModel, styleRepository: GlyphStyleRegistry) {
    graph.forEachNode((node, attributes) => {
        if (!attributes.style) {
            DEBUG && console.log(`Node '${node}' is missing a style attribute.`);
            return;
        }

        if (!model.multigraph.nodes[node]) {
            DEBUG && console.log(`Node '${node}' does not exist in the model.`);
            return;
        }

        const glyphStyle = styleRepository.get(attributes.style);
        if (!glyphStyle) {
            DEBUG && console.log(`Glyph style '${attributes.style}' could not be found.`);
            return;
        }

        const nodeStyle = glyphStyle.apply(model.multigraph.nodes[node]);
        if (!nodeStyle) {
            DEBUG && console.log(`Glyph style '${attributes.style}' could not be applied to node '${node}'.`);
            return;
        }

        const outlines = [
            { width: nodeStyle.size, style: OutlineStyle.Solid },
            ...nodeStyle.outlines.slice(0, 3),
        ] as Outlines;
        attributes.size = outlines.length > 0
            ? getOutlinesTotalWidth(outlines)
            : nodeStyle.size;
        attributes.iconSize = nodeStyle.size;
        attributes.color = nodeStyle.color;
        attributes.type = "glyph";
        attributes.icon = nodeStyle.icon;
        attributes.outlines = outlines;
    });
}

function initializeSigma(
    element: HTMLElement,
    graph: Graph,
    glyphProgram: NodeProgramConstructor,
    onClick?: (payload: SigmaNodeEventPayload) => void,
    nodeReducer?: (node: string, data: Partial<HelvegNodeAttributes>) => Partial<HelvegNodeAttributes>
): Sigma {

    const sigma = new Sigma(graph, element, {
        nodeProgramClasses: {
            glyph: glyphProgram,
        },
        labelFont: "'Cascadia Mono', 'Consolas', monospace",
        itemSizesReference: "positions"
    });

    if (onClick) {
        sigma.on("clickNode", onClick);
    }

    if (nodeReducer) {
        sigma.setSetting("nodeReducer", nodeReducer);
    }

    return sigma;
}

function configureSigma(
    sigma: Sigma,
    glyphOptions: GlyphOptions
) {
    sigma.setSetting("renderLabels", glyphOptions.showLabels);
}

function initializeSupervisor(
    graph: Graph,
    onSupervisorProgress: (ForceAtlas2Progress) => void
): ForceAtlas2Supervisor {

    let settings = forceAtlas2.inferSettings(graph);
    settings.adjustSizes = true;
    const supervisor = new ForceAtlas2Supervisor(graph, settings);
    supervisor.progress.subscribe(onSupervisorProgress);
    return supervisor;
}

// function collapseNode(graph: Graph, nodeId: string) {
//     graph.forEachOutNeighbor(nodeId, (neighborId) => {
//         collapseNode(graph, neighborId);
//         graph.dropNode(neighborId);
//     });
//     graph.setNodeAttribute(nodeId, "collapsed", true);
// }

// function expandNode(
//     graph: Graph,
//     nodeId: string,
//     recursive: boolean = false
// ) {
//     let declaresRelation = helveg.model.multigraph.relations["declares"];
//     let x = graph.getNodeAttribute(nodeId, "x");
//     let y = graph.getNodeAttribute(nodeId, "y");
//     let neighbours = [
//         ...declaresRelation.edges.filter((edge) => edge.src === nodeId),
//     ];
//     neighbours.forEach((edge, i) => {
//         addNode(graph, edge.dst);
//         graph.setNodeAttribute(
//             edge.dst,
//             "x",
//             x + Math.cos((i / neighbours.length) * 2 * Math.PI)
//         );
//         graph.setNodeAttribute(
//             edge.dst,
//             "y",
//             y + Math.sin((i / neighbours.length) * 2 * Math.PI)
//         );
//         graph.addDirectedEdge(edge.src, edge.dst);
//         if (recursive) {
//             expandNode(graph, edge.dst, true);
//         }
//     });

//     graph.setNodeAttribute(nodeId, "collapsed", false);
// }

// function toggleNode(graph: Graph, nodeId: string) {
//     const collapsed = <boolean | undefined>(
//         graph.getNodeAttribute(nodeId, "collapsed")
//     );
//     if (collapsed) {
//         expandNode(graph, nodeId);
//     } else {
//         collapseNode(graph, nodeId);
//     }
// }
