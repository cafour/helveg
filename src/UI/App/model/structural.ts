import Graph from "graphology";
import type { GraphNode } from "./multigraph";
import { DEFAULT_DATA_OPTIONS, DEFAULT_EXPORT_OPTIONS, DEFAULT_GLYPH_OPTIONS, DEFAULT_LAYOUT_OPTIONS, type DataOptions, type ExportOptions, type GlyphOptions, type LayoutOptions } from "./options";
import type { HelvegPlugin, HelvegPluginContext } from "./plugin";
import { EMPTY_MODEL, type VisualizationModel } from "./visualization";
import { Sigma } from "sigma";
import { ForceAtlas2Supervisor, type ForceAtlas2Progress } from "layout/forceAltas2Supervisor";
import { IconAtlas } from "rendering/iconAtlas";
import { HelvegEvent } from "common/event";
import { OutlineStyle, getOutlinesTotalWidth, type Outlines, type GlyphStyle, GlyphStyleRepository } from "./glyph";
import type { SigmaNodeEventPayload } from "sigma/sigma";
import { createGlyphProgram, type GlyphProgramOptions } from "rendering/node.glyph";
import forceAtlas2 from "graphology-layout-forceatlas2";
import type { NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import { exportDiagram } from "rendering/export";
import tidyTree from "layout/tidyTree";

export enum StructuralStatus {
    Stopped,
    Running,
    RunningInBackground
}

export class StructuralState {
    selectedNode: GraphNode | null = null;
    dataOptions: DataOptions = { ...DEFAULT_DATA_OPTIONS };
    glyphOptions: GlyphOptions = { ...DEFAULT_GLYPH_OPTIONS };
    layoutOptions: LayoutOptions = { ...DEFAULT_LAYOUT_OPTIONS };
    exportOptions: ExportOptions = { ...DEFAULT_EXPORT_OPTIONS };
    status: StructuralStatus = StructuralStatus.Stopped;

    applyPlugin(plugin: HelvegPlugin) {
        let context: HelvegPluginContext = {
            dataOptions: this.dataOptions,
            glyphOptions: this.glyphOptions
        };

        plugin.setup(context);
    }
}

export interface StructuralDiagramStats {
    iterationCount: number;
    speed: number;
}

export interface StructuralDiagram {
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

    save(options?: ExportOptions): void;
    resetLayout(): Promise<void>;
    runLayout(inBackground: boolean): Promise<void>;
    stopLayout(): Promise<void>;
}

/**
 * An implementation of StructuralDiagram that is tied to an HTMLElement but not to a specific UI framework.
 */
export class DefaultStructuralDiagram implements StructuralDiagram {
    private _element: HTMLElement | null = null;
    private _model: VisualizationModel = EMPTY_MODEL;
    private _dataOptions: DataOptions = DEFAULT_DATA_OPTIONS;
    private _glyphOptions: GlyphOptions = DEFAULT_GLYPH_OPTIONS
    private _styleRepository: GlyphStyleRepository;
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

    private _graph: Graph | null = null;
    private _sigma: Sigma | null = null;
    private _supervisor: ForceAtlas2Supervisor | null = null;
    private _iconAtlas: IconAtlas = new IconAtlas();
    private _glyphProgramOptions: GlyphProgramOptions = {
        gap: 0.5,
        iconAtlas: this._iconAtlas,
        showIcons: true,
        showOutlines: true
    };
    private _glyphProgram = createGlyphProgram(this._glyphProgramOptions);

    constructor(styleRepository: GlyphStyleRepository) {
        this._styleRepository = styleRepository;
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
        
        DEBUG && console.log("Refreshing the sigma instance.");

        if (this._sigma) {
            this._sigma.kill();
        }

        this._sigma = initializeSigma(this._element, this._graph, this._glyphProgram, this.onNodeClick);
        configureSigma(this._sigma, this._glyphOptions);
    }

    private refreshGraph(): void {
        if (!this._model) {
            return;
        }

        DEBUG && console.log(`Refreshing the graph to match the '${this._model.multigraph.label}' model.`);
        
        this._graph = initializeGraph(this._model, this._dataOptions);
        stylizeGraph(this._graph, this._model, this._styleRepository);
        
        if (this._supervisor) {
            this._supervisor.kill();
            this._supervisor = null;
        }

        this._sigma?.setGraph(this._graph);

        this._supervisor = initializeSupervisor(this._graph, this.onSupervisorProgress);
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
}

function initializeGraph(
    model: VisualizationModel,
    dataOptions: DataOptions,
): Graph {

    const graph = new Graph();
    for (const nodeId in model.multigraph.nodes) {
        const node = model.multigraph.nodes[nodeId];
        graph.addNode(nodeId, {
            label: node.label || nodeId,
            x: 0,
            y: 0
        });
    }

    for (const relationId in dataOptions.selectedRelations) {
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

function stylizeGraph(graph: Graph, model: VisualizationModel, styleRepository: GlyphStyleRepository) {
    graph.forEachNode((node, attributes) => {
        const style = styleRepository.getNodeStyle(model.multigraph.nodes[node]);

        const outlines = [
            { width: style.size, style: OutlineStyle.Solid },
            ...style.outlines.slice(0, 3),
        ] as Outlines;
        attributes.size = outlines.length > 0
            ? getOutlinesTotalWidth(outlines)
            : style.size;
        attributes.iconSize = style.size;
        attributes.color = style.color;
        attributes.type = "glyph";
        attributes.icon = style.icon;
        attributes.outlines = outlines;
    });
}


function initializeSigma(
    element: HTMLElement,
    graph: Graph,
    glyphProgram: NodeProgramConstructor,
    onClick: (payload: SigmaNodeEventPayload) => void,
): Sigma {

    const sigma = new Sigma(graph, element, {
        nodeProgramClasses: {
            glyph: glyphProgram,
        },
        labelFont: "'Cascadia Mono', 'Consolas', monospace",
        itemSizesReference: "positions",
    });
    sigma.on("clickNode", onClick);
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
