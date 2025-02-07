import { HelvegEvent } from "../common/event.ts";
import {
    EMPTY_GRAPH,
    HelvegEdgeAttributes,
    HelvegGraph,
    HelvegGraphAttributes,
    HelvegNodeAttributes,
    HelvegNodeProgramType,
    HelvegSigma,
    collapseNode,
    dropNode,
    expandNode,
    expandPathsTo,
    findRoots,
    hoveredNodeSymbol,
    toggleNode,
} from "../model/graph.ts";
import { Coordinates, SigmaNodeEventPayload } from "../deps/sigma.ts";
import { LogSeverity, ILogger, consoleLogger } from "../model/logger.ts";
import { ForceAtlas2Progress, ForceAtlas2Supervisor } from "../layout/forceAltas2Supervisor.ts";
import { wheelOfFortune } from "../layout/circular.ts";
import tidyTree from "../layout/tidyTree.ts";
import { configureSigma, initializeGraph, initializeSigma, initializeSupervisor, styleGraph } from "./initializers.ts";
import { DEFAULT_GLYPH_PROGRAM_OPTIONS, GlyphProgramOptions, createGlyphProgram } from "../rendering/node.glyph.ts";
import { DEFAULT_EXPORT_OPTIONS, ExportOptions, exportDiagram } from "../rendering/export.ts";
import { IFilterBuilderEntry, SearchMode, buildNodeFilter, filterNodes } from "../model/filter.ts";
import { bfsGraph } from "../model/traversal.ts";
import {
    EdgeStylist,
    NodeStylist,
    RelationStylist,
    fallbackNodeStylist,
    fallbackRelationStylist,
} from "../model/style.ts";
import { EMPTY_ICON_REGISTRY, IconRegistry } from "../global.ts";
import { DataModel } from "../model/data-model.ts";
import { EMPTY_DATA_MODEL } from "../model/const.ts";
import { ForceAtlas2Settings, inferSettings } from "graphology-layout-forceatlas2";
import { deepCompare } from "../common/deep-compare.ts";
import { FALLBACK_INSPECTOR, Inspector } from "../model/inspect.ts";
import Graph from "../deps/graphology.ts";
import type { ForceAtlas2Options } from "../layout/forceAtlas2Iterate.ts";
import { Full } from "../common/full.ts";
import { preprocess, PreprocessFunc } from "../model/preprocess.ts";

export interface DiagramRefreshOptions {
    selectedRelations?: string[];
    selectedKinds?: string[];
    expandedDepth?: number;
}

export interface DiagramOptions {
    glyphProgram: GlyphProgramOptions;
    mainRelation: string | null;
    preprocess: PreprocessFunc;
    logLevel: LogSeverity;
    nodeStylist: NodeStylist;
    relationStylist: RelationStylist;
    edgeStylist?: EdgeStylist;
    inspector: Inspector;
    nodeKindOrder: string[];
    iconRegistry: Readonly<IconRegistry>;
    refresh: DiagramRefreshOptions;
    forceAtlas2: ForceAtlas2Options;
    cursor: CursorOptions;
}

export interface CursorOptions {
    defaultCursor: string;
    hoverCursor: string;
    shiftHoverCursor?: string;
    controlHoverCursor?: string;
    altHoverCursor?: string;
}

export const DEFAULT_FORCE_ATLAS2_OPTIONS: Readonly<ForceAtlas2Options> = {
    ...(inferSettings(1024) as Full<ForceAtlas2Settings>),
    adjustSizes: true,
    barnesHutOptimize: false,
    strongGravityMode: false,
    barnesHutTheta: 0.5,
    edgeWeightInfluence: 1,
    linLogMode: false,
    outboundAttractionDistribution: false,
    slowDown: 3,
    autoStopAverageTraction: 1.0,
};

export const DEFAULT_CURSOR_OPTIONS: Readonly<CursorOptions> = {
    defaultCursor: "default",
    hoverCursor: "pointer",
};

export const DEFAULT_DIAGRAM_OPTIONS: Readonly<DiagramOptions> = {
    logLevel: LogSeverity.Info,
    mainRelation: null,
    preprocess: preprocess,
    glyphProgram: DEFAULT_GLYPH_PROGRAM_OPTIONS,
    nodeStylist: fallbackNodeStylist,
    relationStylist: fallbackRelationStylist,
    nodeKindOrder: [],
    inspector: FALLBACK_INSPECTOR,
    iconRegistry: EMPTY_ICON_REGISTRY,
    refresh: {},
    forceAtlas2: DEFAULT_FORCE_ATLAS2_OPTIONS,
    cursor: DEFAULT_CURSOR_OPTIONS,
};

export enum DiagramMode {
    Normal = "normal",
    Highlighting = "highlighting",
}

export enum DiagramStatus {
    Stopped = "stopped",
    Running = "running",
    RunningInBackground = "runningInBackground",
}

export interface DiagramStats {
    iterationCount: number;
    speed: number;
    globalTraction: number;
    globalSwinging: number;
    averageTraction: number;
    averageSwinging: number;
}

const DEFAULT_DIAGRAM_STATS: Readonly<DiagramStats> = {
    iterationCount: 0,
    speed: 0,
    globalSwinging: 0,
    globalTraction: 0,
    averageTraction: 0,
    averageSwinging: 0,
};

export interface RemoveOptions {
    isTransitive: boolean;
}

export const DEFAULT_REMOVE_OPTIONS: RemoveOptions = {
    isTransitive: true,
};

export interface AutoLayoutOptions {
    roughIterationFactor: number;
    adjustIterationFactor: number;
}

export const DEFAULT_AUTO_LAYOUT_OPTIONS: AutoLayoutOptions = {
    roughIterationFactor: 1.5,
    adjustIterationFactor: 10,
};

export interface ModifierKeyState {
    control: boolean;
    alt: boolean;
    shift: boolean;
}

export interface ModifierKeyStateChange {
    old: Readonly<ModifierKeyState>;
    new: Readonly<ModifierKeyState>;
}

export class Diagram {
    private _element: HTMLElement;
    get element(): HTMLElement {
        return this._element;
    }

    private _sigmaElement: HTMLElement;
    get sigmaElement(): HTMLElement {
        return this._sigmaElement;
    }

    private _options: DiagramOptions;
    get options(): Readonly<DiagramOptions> {
        return this._options;
    }

    private _modelGraph: HelvegGraph = EMPTY_GRAPH;
    get modelGraph(): Readonly<HelvegGraph> {
        return this._modelGraph;
    }

    private _model: DataModel = EMPTY_DATA_MODEL;
    get model(): DataModel {
        return this._model;
    }
    set model(value: DataModel) {
        this._model = value;
        this._modelGraph = this.options.preprocess(this._model);
        this._nodeKeys = this._modelGraph
            .mapNodes((_n, na) => na.model)
            .flatMap((n) => Object.keys(n))
            .filter((v, i, a) => a.indexOf(v) === i);
        this._nodeKeyTypes = {};
        this._nodeKeys.forEach((k) => {
            this._nodeKeyTypes[k] = "string";
            const propTypes = new Set(
                this.modelGraph
                    .filterNodes((_n, na) => na.model[k] !== undefined)
                    .map((n) => typeof this.modelGraph.getNodeAttribute(n, "model")[k])
            );
            if (propTypes.size == 1) {
                this._nodeKeyTypes[k] = propTypes.values().next().value ?? "string";
            }
        });
        this.events.modelChanged.trigger(value);

        // NB: Runs without awaiting.
        this.refreshGraph(this._lastRefreshOptions);
        this.refreshSigma();
    }

    private _stats: DiagramStats = structuredClone(DEFAULT_DIAGRAM_STATS);
    get stats(): DiagramStats {
        return this._stats;
    }
    private set stats(value: DiagramStats) {
        this._stats = value;
        this.events.statsChanged.trigger(value);
    }

    private _mainRelation: string | null;
    get mainRelation(): string | null {
        return this._mainRelation;
    }
    set mainRelation(value: string | null) {
        this._mainRelation = value;
        this.events.mainRelationChanged.trigger(value);
    }

    private _status: DiagramStatus = DiagramStatus.Stopped;
    get status(): DiagramStatus {
        return this._status;
    }
    private set status(value: DiagramStatus) {
        this._status = value;
        this.events.statusChanged.trigger(value);
    }

    private _mode: DiagramMode = DiagramMode.Normal;
    get mode(): DiagramMode {
        return this._mode;
    }
    private set mode(value: DiagramMode) {
        this._mode = value;
        this._options.glyphProgram.showOnlyHighlighted = this._mode === DiagramMode.Highlighting;
        this.events.modeChanged.trigger(value);
    }

    private _shouldFixateSelectedNode: boolean = true;
    get shouldFixateSelectedNode(): boolean {
        return this._shouldFixateSelectedNode;
    }
    set shouldFixateSelectedNode(value: boolean) {
        this._shouldFixateSelectedNode = value;
        if (this._selectedNode != null) {
            this._graph?.setNodeAttribute(this._selectedNode, "fixed", value);
        }
    }

    private _selectedNode: string | null = null;
    get selectedNode(): string | null {
        return this._selectedNode;
    }
    set selectedNode(value: string | null) {
        if (this.shouldFixateSelectedNode && this._selectedNode != null) {
            this._graph?.setNodeAttribute(this._selectedNode, "fixed", false);
        }

        this._selectedNode = value;

        if (this.shouldFixateSelectedNode && this._selectedNode != null) {
            this._graph?.setNodeAttribute(this._selectedNode, "fixed", true);
        }

        this.events.nodeSelected.trigger(value);
    }

    get hoveredNode(): string | null {
        return this._sigma?.[hoveredNodeSymbol] ?? null;
    }

    private _draggedNode: string | null = null;
    get draggedNode(): string | null {
        return this._draggedNode;
    }
    private set draggedNode(value: string | null) {
        this._draggedNode = value;
    }

    private _logger: ILogger;
    get logger(): ILogger {
        return this._logger;
    }

    get nodeStylist(): NodeStylist {
        return this._options.nodeStylist;
    }
    set nodeStylist(value: NodeStylist) {
        this._options.nodeStylist = value;
        this.restyleGraph();
        this._sigma?.scheduleRefresh();
        this.events.nodeStylistChanged.trigger();
    }

    get edgeStylist(): EdgeStylist | undefined {
        return this._options.edgeStylist;
    }
    set edgeStylist(value: EdgeStylist | undefined) {
        this._options.edgeStylist = value;
        this.restyleGraph();
        this._sigma?.scheduleRefresh();
        this.events.edgeStylistChanged.trigger();
    }

    get relationStylist(): RelationStylist {
        return this._options.relationStylist;
    }
    set relationStylist(value: RelationStylist) {
        this._options.relationStylist = value;
        this.restyleGraph();
        this._sigma?.scheduleRefresh();
        this.events.relationStylistChanged.trigger();
    }

    get forceAtlas2Options(): ForceAtlas2Options {
        return this._options.forceAtlas2;
    }
    set forceAtlas2Options(value: ForceAtlas2Options) {
        this._options.forceAtlas2 = value;
        this.refreshSupervisor(true);
    }

    private _graph?: HelvegGraph;
    get graph() {
        return this._graph;
    }
    private _sigma?: HelvegSigma;
    private _supervisor?: ForceAtlas2Supervisor;
    private _glyphProgram: HelvegNodeProgramType;

    get glyphProgramOptions(): GlyphProgramOptions {
        return this.options.glyphProgram;
    }
    set glyphProgramOptions(value: GlyphProgramOptions) {
        const lastOptions = this._options.glyphProgram;

        if (deepCompare(lastOptions, value)) {
            return;
        }

        Object.assign(this._options.glyphProgram, value);

        this.reconfigureSigma();
        this.restyleGraph();
    }

    get cursorOptions(): Readonly<CursorOptions> {
        return this.options.cursor;
    }
    set cursorOptions(value: CursorOptions) {
        this._options.cursor = value;
    }

    // used when building JS filters
    // TODO: likely move to its own place somewhere else
    private _nodeKeys: string[] = [];
    get nodeKeys(): readonly string[] {
        return this._nodeKeys;
    }

    private _nodeKeyTypes: Record<string, string> = {};
    get nodeKeyTypes(): Record<string, string> {
        return this._nodeKeyTypes;
    }

    // used for refreshing
    private _lastRefreshOptions: DiagramRefreshOptions;

    private _modifierKeyState: ModifierKeyState = {
        alt: false,
        control: false,
        shift: false,
    };
    get modifierKeyState(): Readonly<ModifierKeyState> {
        return this._modifierKeyState;
    }
    private set modifierKeyState(newState: ModifierKeyState) {
        if (!deepCompare(this._modifierKeyState, newState)) {
            const oldState = { ...this._modifierKeyState };
            Object.assign(this._modifierKeyState, newState);
            this.events.modifierKeysChanged.trigger({ old: oldState, new: this._modifierKeyState });
        }
    }

    constructor(element: HTMLElement, options?: Partial<DiagramOptions>) {
        this._element = element;
        this._element.style.position = "relative";

        this._sigmaElement = document.createElement("div");
        this._sigmaElement.classList.add("sigma");
        this._sigmaElement.style.width = "100%";
        this._sigmaElement.style.height = "100%";
        this._element.appendChild(this._sigmaElement);
        window.addEventListener("keydown", this.onKeyUpDown.bind(this));
        window.addEventListener("keyup", this.onKeyUpDown.bind(this));
        window.addEventListener("mousemove", this.updateModifierKeyState.bind(this));
        window.addEventListener("focus", () => this.updateModifierKeyState({}));
        window.addEventListener("blur", () => this.updateModifierKeyState({}));

        this._options = { ...DEFAULT_DIAGRAM_OPTIONS, ...options };
        this._logger = consoleLogger("diagram", this._options.logLevel);
        this._mainRelation = this._options.mainRelation;
        this._glyphProgram = createGlyphProgram(this.options.glyphProgram, this._logger);
        this._lastRefreshOptions = this._options.refresh;

        this.element.style.width = "100%";
        this.element.style.height = "100%";
    }

    public readonly events = {
        statusChanged: new HelvegEvent<DiagramStatus>("helveg.diagram.statusChanged"),
        modeChanged: new HelvegEvent<DiagramMode>("helveg.diagram.modeChanged"),
        modelChanged: new HelvegEvent<DataModel>("helveg.diagram.modelChanged"),
        graphChanged: new HelvegEvent<HelvegGraph | undefined>("helveg.diagram.graphChanged"),
        statsChanged: new HelvegEvent<DiagramStats>("helveg.diagram.statsChanged"),
        nodeSelected: new HelvegEvent<string | null>("helveg.diagram.nodeSelected"),

        nodeDown: new HelvegEvent<{ nodeId: string; coords: Coordinates }>("helveg.diagram.nodeDown"),
        nodeUp: new HelvegEvent<{ nodeId: string; coords: Coordinates }>("helveg.diagram.nodeUp"),
        nodeDoubleClicked: new HelvegEvent<{ nodeId: string; coords: Coordinates }>("helveg.diagram.nodeDoubleClicked"),
        nodeMove: new HelvegEvent<{ nodeId: string; coords: Coordinates }>("helveg.diagram.nodeMove"),

        stageDown: new HelvegEvent<Coordinates>("helveg.diagram.stageDown"),
        stageUp: new HelvegEvent<Coordinates>("helveg.diagram.stageUp"),
        stageDoubleClicked: new HelvegEvent<Coordinates>("helveg.diagram.stageDoubleClicked"),
        stageMove: new HelvegEvent<Coordinates>("helveg.diagram.stageMove"),

        mainRelationChanged: new HelvegEvent<string | null>("helveg.diagram.mainRelationChanged"),
        modifierKeysChanged: new HelvegEvent<ModifierKeyStateChange>("helveg.diagram.modifierKeysChanged"),
        nodeStylistChanged: new HelvegEvent<void>("helveg.diagram.nodeStylistChanged"),
        edgeStylistChanged: new HelvegEvent<void>("helveg.diagram.edgeStylistChanged"),
        relationStylistChanged: new HelvegEvent<void>("helveg.diagram.relationStylistChanged"),
    } as const;

    private _finiteBackgroundLayoutExecutor:
        | {
              resolve: () => void;
              reject: (reason?: any) => void;
          }
        | undefined;

    public async resetLayout(): Promise<void> {
        if (!this._graph) {
            this._logger.debug("Cannot reset layout since the graph is not initialized.");
            return;
        }

        if (!this._model) {
            this._logger.debug("Cannot reset layout since the model is not initialized.");
            return;
        }

        if (this._supervisor?.isRunning) {
            await this.stopLayout();
        }

        await this.refreshSupervisor();

        if (!this.mainRelation) {
            this._logger.error("Cannot reset layout since no relation is selected as main.");
            return;
        }

        this._logger.debug("Resetting the layout.");

        let roots = findRoots(this._graph, this.mainRelation);
        let i = 0;
        let { radius, theta } = roots.size <= 1 ? { radius: 0, theta: 0 } : wheelOfFortune(1000, roots.size);
        radius = Math.min(radius, 4000);
        for (let root of roots) {
            tidyTree(this._graph, root, {
                radius: 1000,
                relation: this.mainRelation,
                offset: {
                    x: radius * Math.cos(theta * i),
                    y: radius * Math.sin(theta * i),
                },
            });
            i++;
        }

        this.stats = structuredClone(DEFAULT_DIAGRAM_STATS);
        if (this._sigma) {
            this._sigma.scheduleRefresh();
        }
    }

    public async runLayout(
        inBackground: boolean,
        iterationCount?: number,
        options?: Partial<ForceAtlas2Options>
    ): Promise<void> {
        if (!this._graph) {
            this._logger.debug("Cannot run since the graph is not initialized.");
            return;
        }

        if (!this._supervisor) {
            this._logger.debug("Cannot run since the supervisor is not initialized.");
            return;
        }

        this._logger.debug("Running the layout.");

        this._graph.forEachNode((n, a) => (a.inInitialPosition = false));

        this._supervisor.start(inBackground, iterationCount, options);

        this.refreshStatus();

        if (inBackground) {
            this._sigma?.kill();
            this._sigma = undefined;

            if (iterationCount && iterationCount >= 0) {
                await new Promise<void>((resolve, reject) => {
                    this._finiteBackgroundLayoutExecutor = {
                        resolve,
                        reject,
                    };
                });
            }
        } else if (!this._sigma) {
            this.refreshSigma();
        }
    }

    public async stopLayout(): Promise<void> {
        if (!this._graph) {
            this._logger.debug("Cannot stop since the graph is not initialized.");
            return;
        }

        this._logger.debug("Stopping the layout.");

        if (this._supervisor?.isRunning) {
            this._supervisor.stop();
            this.refreshStatus();

            if (!this._sigma) {
                this.refreshSigma();
            }
        }

        if (this._finiteBackgroundLayoutExecutor) {
            this._finiteBackgroundLayoutExecutor.reject("stopLayout");
        }
    }

    public async autoLayout(options?: AutoLayoutOptions): Promise<void> {
        options = { ...DEFAULT_AUTO_LAYOUT_OPTIONS, ...options };

        if (!this._graph) {
            this._logger.debug("Cannot auto-layout since the graph is not initialized.");
            return;
        }

        let visibleNodeCount = 0;
        this._graph.forEachNode((_n, na) => {
            if (!na.hidden) {
                visibleNodeCount++;
            }
        });

        await this.resetLayout();

        // rough phase: strong gravity, barnes-hut optimize
        const roughIterationCount = Math.max(100, Math.sqrt(visibleNodeCount) * options.roughIterationFactor);
        this.logger?.debug(`AutoLayout: Running ${Math.floor(roughIterationCount)} rough iterations.`);
        await this.runLayout(true, roughIterationCount, {
            adjustSizes: false,
            strongGravityMode: true,
            barnesHutOptimize: true,
            slowDown: 1,
        });

        // adjust phase: weak gravity, adjust sizes
        const adjustIterationCount = Math.max(600, Math.sqrt(visibleNodeCount) * options.adjustIterationFactor);
        this.logger?.debug(`AutoLayout: Running ${Math.floor(adjustIterationCount)} adjust iterations.`);
        await this.runLayout(true, adjustIterationCount, {
            adjustSizes: true,
            strongGravityMode: false,
            barnesHutOptimize: false,
            slowDown: 2,
            autoStopAverageTraction: 2,
        });
    }

    public save(options?: ExportOptions): void {
        if (!this._graph) {
            this._logger.debug("Cannot save since the graph is not initialized.");
            return;
        }

        if (!this._sigma) {
            this._logger.debug("Cannot save since the sigma instance is not initialized.");
            return;
        }

        this._logger.debug("Saving the diagram.");

        options = { ...DEFAULT_EXPORT_OPTIONS, ...options };
        options.fileName ??= `${this._model.name}-export.png`;
        if (this._sigma) {
            exportDiagram(this._sigma, options);
        }
    }

    public exportPositions(): Record<string, Coordinates> {
        const result: Record<string, Coordinates> = {};
        if (this._graph) {
            this._graph.forEachNode((node, attributes) => {
                result[node] = {
                    x: attributes.x ?? 0,
                    y: attributes.y ?? 0,
                };
            });
        }
        return result;
    }

    public importPositions(value: Record<string, Coordinates>) {
        this._graph?.forEachNode((node, attributes) => {
            if (value[node]) {
                attributes.x = value[node].x;
                attributes.y = value[node].y;
            }
        });
    }

    public highlight(
        searchText: string | null,
        searchMode: SearchMode,
        expandedOnly: boolean = false,
        filterBuilder?: IFilterBuilderEntry[]
    ): string[] {
        if (!this._graph) {
            this._logger.warn("Cannot highlight nodes since the graph is not initialized.");
            return [];
        }

        let results: string[] = [];
        try {
            let filter = buildNodeFilter(searchText, searchMode, this._nodeKeys, filterBuilder);
            if (filter === null) {
                this.mode = DiagramMode.Normal;
                this._graph.forEachNode((_, a) => (a.highlighted = false));
                this._sigma?.refresh();
                return [];
            }

            this.mode = DiagramMode.Highlighting;

            if (this._model.data) {
                Object.entries(this._model.data.nodes).forEach(([id, node]) => {
                    if (this._graph?.hasNode(id) && (!expandedOnly || !this._graph.getNodeAttribute(id, "hidden"))) {
                        const isHighlighted = filter!(node);
                        if (isHighlighted) {
                            results.push(id);
                            if (!expandedOnly) {
                                expandPathsTo(this._graph, id, this.mainRelation ?? undefined);
                            }
                        }
                        this._graph.setNodeAttribute(id, "highlighted", isHighlighted);
                    }
                });
            }

            this._sigma?.refresh();
        } catch (e: any) {
            this._logger.warn(e?.message ?? e?.toString() ?? "Something bad happened while highlighting nodes.");
            return [];
        }

        this._logger.info(`Highlighting ${results.length} nodes.`);
        return results;
    }

    public async highlightNode(
        nodeId: string | null,
        includeSubtree: boolean,
        includeNeighbors: boolean
    ): Promise<void> {
        if (nodeId === null) {
            this._logger.debug("Clearing node highlights.");
            this._graph?.forEachNode((_, a) => (a.highlighted = false));
            this.mode = DiagramMode.Normal;
            this._sigma?.refresh();
            return;
        }

        if (!this._graph || !this._graph?.hasNode(nodeId)) {
            return;
        }

        this._graph.forEachNode((n) => this._graph?.setNodeAttribute(n, "highlighted", false));
        this._graph.setNodeAttribute(nodeId, "highlighted", true);

        if (includeSubtree) {
            bfsGraph(this._graph, nodeId, {
                relation: this.mainRelation,
                callback: (_, attr) => {
                    attr!.highlighted = true;
                },
            });
        }

        if (includeNeighbors) {
            for (let neighbor of this._graph.neighbors(nodeId)) {
                this._graph.setNodeAttribute(neighbor, "highlighted", true);
            }
        }

        const nodeAttributes = this._graph.getNodeAttributes(nodeId);
        if (nodeAttributes.collapsed) {
            await this.refreshSupervisor(true, () =>
                expandPathsTo(this._graph!, nodeId, this.options.mainRelation ?? undefined)
            );
        }

        this.mode = DiagramMode.Highlighting;
        this._sigma?.refresh();
    }

    public async isolate(
        searchText: string | null,
        searchMode: SearchMode,
        filterBuilder?: IFilterBuilderEntry[]
    ): Promise<void> {
        if (!this._graph) {
            this._logger.warn("Cannot isolate nodes since the graph is not initialized.");
            return;
        }

        if (!this._model.data) {
            this._logger.warn("Cannot isolate nodes since the model contains no data.");
            return;
        }

        let changed = false;
        try {
            let filter = buildNodeFilter(searchText, searchMode, this._nodeKeys, filterBuilder);
            if (filter === null) {
                return;
            }

            await this.refreshSupervisor(true, () => {
                if (!this._graph) {
                    return;
                }

                this.selectedNode = null;
                // NB: This awful construction is here so that the dropping of nodes does not take forever due to
                //     Sigma's and FA2-supervisor's event listeners.
                this._sigma?.setGraph(new Graph<HelvegNodeAttributes, HelvegEdgeAttributes, HelvegGraphAttributes>());
                // NB: Sigma doesn't clear its highlightedNodes correctly
                ((this._sigma as any)["highlightedNodes"] as Set<string>).clear();

                for (let id of filterNodes(this._model.data!, filter, true)) {
                    if (this._graph.hasNode(id)) {
                        dropNode(this._graph, id);
                        changed = true;
                    }
                }
            });
        } catch (e: any) {
            this._logger.warn(e?.message ?? e?.toString() ?? "Something bad happened while isolating nodes.");
            return;
        } finally {
            this._sigma?.setGraph(this._graph);
        }

        this._logger.info(`Isolated ${this._graph.nodes().length} nodes.`);
        this.events.graphChanged.trigger(this._graph);
    }

    public async refresh(options?: DiagramRefreshOptions): Promise<void> {
        await this.refreshGraph(options ?? this._lastRefreshOptions);
        await this.autoLayout();
    }

    public reset(): Promise<void> {
        this.mainRelation = this.options.mainRelation;
        return this.refresh(this.options.refresh);
    }

    async remove(nodeId: string, options?: RemoveOptions): Promise<void> {
        options = { ...DEFAULT_REMOVE_OPTIONS, ...options };

        if (!this._graph) {
            this._logger.warn("Cannot remove nodes since the graph is not initialized.");
            return;
        }

        if (!this._graph.hasNode(nodeId)) {
            throw new Error(`Cannot remove node '${nodeId}' since it does not exist in the graph.`);
        }

        // NB: pre-emptively unset selected node so that Sigma cannot complain that you deleted it
        //     TODO: Handle better.
        this.selectedNode = null;

        let removedCount = 0;
        if (!options.isTransitive && !this._graph.getNodeAttributes(nodeId).collapsed) {
            // NB: drop the node but preserve the transitive relations
            dropNode(this._graph, nodeId);
            removedCount = 1;
        } else {
            let reachable = bfsGraph(this._graph, nodeId, {
                relation: this.mainRelation,
            });

            await this.refreshSupervisor(true, () => {
                if (!this._graph) {
                    return;
                }

                // NB: This awful construction is here so that the dropping of nodes does not take forever due to
                //     Sigma's and FA2-supervisor's event listeners.
                this._sigma?.setGraph(new Graph<HelvegNodeAttributes, HelvegEdgeAttributes, HelvegGraphAttributes>());
                // NB: Sigma doesn't clear its highlightedNodes correctly
                ((this._sigma as any)["highlightedNodes"] as Set<string>).clear();
                reachable.forEach((id) => this._graph!.dropNode(id));
                this._graph!.forEachNode((_, a) => (a.highlighted = false));
                this._sigma?.setGraph(this._graph);
            });

            removedCount = reachable.size;
        }

        if (removedCount > 0) {
            this.events.graphChanged.trigger(this._graph);
        }

        this._logger.info(`Removed ${removedCount} nodes.`);
    }

    async toggleNode(nodeId: string): Promise<void> {
        if (!this._graph) {
            this._logger.warn("Cannot toggle nodes since the graph is not initialized.");
            return;
        }

        if (!this.mainRelation) {
            this._logger.warn("Cannot toggle nodes since the main relation is unset.");
            return;
        }

        await this.refreshSupervisor(true, () => toggleNode(this._graph!, nodeId, this.mainRelation!));
    }

    async toggleAll(shouldExpand?: boolean): Promise<void> {
        if (!this._graph) {
            this._logger.warn("Cannot toggle nodes since the graph is not initialized.");
            return;
        }

        if (!this.mainRelation) {
            this._logger.warn("Cannot toggle nodes since the main relation is unset.");
            return;
        }

        await this.refreshSupervisor(true, () => {
            this.graph!.forEachNode((n) => toggleNode(this.graph!, n, this.mainRelation!, shouldExpand));
        });
    }

    async dig(collapse: boolean = false): Promise<void> {
        if (!this._graph) {
            this._logger.warn("Cannot toggle nodes since the graph is not initialized.");
            return;
        }

        if (!this.mainRelation) {
            this._logger.warn("Cannot toggle nodes since the main relation is unset.");
            return;
        }

        await this.refreshSupervisor(true, () => {
            if (collapse) {
                this.graph
                    ?.filterNodes((_n, na) => !na.hidden && (na.collapsed || na.childCount === 0))
                    .flatMap((n) =>
                        this.graph!.filterInEdges(n, (_e, ea) => ea.relation === this.mainRelation!).map((e) =>
                            this.graph!.source(e)
                        )
                    )
                    .forEach((n) => collapseNode(this.graph!, n, this.mainRelation!));
            } else {
                this.graph
                    ?.filterNodes((_n, na) => !na.hidden && na.collapsed)
                    .forEach((n) => expandNode(this.graph!, n, { relation: this.mainRelation! }));
            }
        });
    }

    /**
     * Kills the current instance of the FA2 supervisor and spawns a new one. Calls `modify` while no supervisor
     * is running. If `shouldLayoutContinue` is true, the newly spawned supervisor will be started if the original one
     * was running.
     */
    private async refreshSupervisor(shouldLayoutContinue: boolean = false, modify?: () => void): Promise<void> {
        let lastStatus = this.status;

        if (this._supervisor) {
            this._supervisor.kill();
            this._supervisor = undefined;
        }

        this.refreshStatus();

        try {
            if (modify) {
                modify();
            }
        } finally {
            if (!this._graph) {
                return;
            }

            try {
                this._sigma?.refresh();
            } catch {}

            this._supervisor = initializeSupervisor(
                this._graph,
                this.onSupervisorProgress.bind(this),
                this.onSupervisorStopped.bind(this),
                this.onSupervisorUpdated.bind(this),
                this.options.forceAtlas2,
                this.logger
            );
            if (
                shouldLayoutContinue &&
                (lastStatus === DiagramStatus.Running || lastStatus === DiagramStatus.RunningInBackground)
            ) {
                this._supervisor.start(lastStatus === DiagramStatus.RunningInBackground);
                this.refreshStatus();
            }
        }
    }

    private refreshStatus() {
        this.status =
            this._supervisor?.isRunning === true && this._supervisor?.isInBackground === true
                ? DiagramStatus.RunningInBackground
                : this._supervisor?.isRunning === true
                ? DiagramStatus.Running
                : DiagramStatus.Stopped;
    }

    private onSupervisorProgress(message: ForceAtlas2Progress) {
        this.stats = {
            iterationCount: message.iterationCount,
            speed: message.iterationsPerSecond,
            globalSwinging: message.globalSwinging,
            globalTraction: message.globalTraction,
            averageSwinging: message.averageSwinging,
            averageTraction: message.averageTraction,
        };
    }

    private onSupervisorStopped() {
        if (this._finiteBackgroundLayoutExecutor) {
            this._finiteBackgroundLayoutExecutor.resolve();
        }

        this.refreshStatus();
        if (!this._sigma) {
            this.refreshSigma();
        }
    }

    private onSupervisorUpdated() {
        this._sigma?.refresh({
            partialGraph: { nodes: (this._supervisor?.nodeIds as string[]) ?? [] },
            schedule: true,
            skipIndexation: false,
        });
    }

    private refreshSigma(): void {
        if (!this._sigmaElement || !this._graph) {
            return;
        }

        this._logger.debug("Refreshing the Sigma.js instance.");

        if (this._sigma) {
            this._sigma.kill();
        }

        this._sigma = initializeSigma(this._sigmaElement, this._graph, this._glyphProgram);
        const mouse = this._sigma.getMouseCaptor();
        const touch = this._sigma.getTouchCaptor();
        // mouse.on("mousedown", this.onDown.bind(this));
        // touch.on("touchdown", (e) => this.onDown(e.touches[0]));
        // mouse.on("mouseup", this.onUp.bind(this));
        // touch.on("touchup", (e) => this.onUp(e.touches[0]));
        mouse.on("mousemovebody", (e) => {
            if (this.onMove(e) === false) {
                // prevent Sigma from moving the camera
                e.preventSigmaDefault();
                e.original.preventDefault();
                e.original.stopPropagation();
            }
        });
        touch.on("touchmove", (e) => {
            if (e.touches.length == 1) {
                this.onMove(e.touches[0]);
                e.original.preventDefault();
            }
        });
        this._sigma.on("downNode", (e) =>
            this.events.nodeDown.trigger({
                nodeId: e.node,
                coords: e.event,
            })
        );
        this._sigma.on("upNode", (e) =>
            this.events.nodeUp.trigger({
                nodeId: e.node,
                coords: e.event,
            })
        );
        this._sigma.on("upStage", (e) => this.events.stageUp.trigger(e.event));
        this._sigma.on("downStage", (e) => this.events.stageDown.trigger(e.event));
        this._sigma.on("doubleClickStage", (e) => this.events.stageDoubleClicked.trigger(e.event));
        this._sigma.on("enterNode", this.updateCursor.bind(this));
        this._sigma.on("leaveNode", this.resetCursor.bind(this));
        this._sigma.on("doubleClickNode", (e) =>
            this.events.nodeDoubleClicked.trigger({
                nodeId: e.node,
                coords: e.event,
            })
        );
        this.reconfigureSigma();
    }

    private reconfigureSigma(): void {
        if (!this._sigma) {
            return;
        }

        const tmpOptions: GlyphProgramOptions = {
            ...this.options.glyphProgram,
            showLabels: this.options.glyphProgram.showLabels && this.mode == DiagramMode.Normal,
            showOnlyHighlighted: this.mode == DiagramMode.Highlighting,
        };

        configureSigma(this._sigma, tmpOptions);
    }

    private async refreshGraph(options: DiagramRefreshOptions): Promise<void> {
        if (!this._model || !this._model.data) {
            this._logger.debug("Ignoring graph refresh since the model is empty.");
            return;
        }

        this._logger.debug(`Refreshing the graph to match the '${this._model.name}' model.`);

        this._graph = initializeGraph(
            this._modelGraph,
            this.mainRelation ?? undefined,
            options.selectedRelations ?? (this.mainRelation ? [this.mainRelation] : []),
            options.selectedKinds,
            options.expandedDepth
        );
        this.events.graphChanged.trigger(this._graph);
        this.restyleGraph();

        await this.refreshSupervisor(false, () => this._graph && this._sigma?.setGraph(this._graph));

        this.mode = DiagramMode.Normal;
    }

    private restyleGraph(): void {
        if (!this._graph || !this._model || !this._model.data) {
            return;
        }

        this._logger.debug(`Restyling the graph.`);

        styleGraph(
            this._graph,
            this.options.glyphProgram,
            this.options.nodeStylist,
            this.options.relationStylist,
            this.options.edgeStylist
        );
    }

    private updateModifierKeyState(e: { altKey?: boolean; ctrlKey?: boolean; shiftKey?: boolean }) {
        const newState: ModifierKeyState = {
            alt: e.altKey ?? false,
            control: e.ctrlKey ?? false,
            shift: e.shiftKey ?? false,
        };
        this.modifierKeyState = newState;

        if (this.hoveredNode != null) {
            this.updateCursor();
        }
    }

    private updateCursor(): void {
        if (this.cursorOptions.altHoverCursor && this._modifierKeyState.alt) {
            this.element.style.cursor = this.cursorOptions.altHoverCursor;
        } else if (this.cursorOptions.controlHoverCursor && this._modifierKeyState.control) {
            this.element.style.cursor = this.cursorOptions.controlHoverCursor;
        } else if (this.cursorOptions.shiftHoverCursor && this._modifierKeyState.shift) {
            this.element.style.cursor = this.cursorOptions.shiftHoverCursor;
        } else {
            this.element.style.cursor = this.cursorOptions.hoverCursor;
        }
    }

    private resetCursor(): void {
        this.element.style.cursor = this.cursorOptions.defaultCursor;
    }

    public dragNode(nodeId: string | null) {
        if (this.draggedNode !== nodeId && this.draggedNode) {
            this._graph?.setNodeAttribute(this.draggedNode, "highlighted", false);
            this._graph?.setNodeAttribute(this.draggedNode, "fixed", false);
        }

        this.draggedNode = nodeId;

        if (this.draggedNode !== null) {
            this._graph?.setNodeAttribute(nodeId, "highlighted", true);
            this._graph?.setNodeAttribute(nodeId, "fixed", true);
        }
    }

    private onMove(coords: Coordinates): boolean {
        if (this.draggedNode) {
            this.events.nodeMove.trigger({ nodeId: this.draggedNode, coords: coords });
        } else {
            this.events.stageMove.trigger(coords);
        }

        if (!this.draggedNode || !this._sigma || !this._graph) {
            return true;
        }

        const pos = this._sigma.viewportToGraph(coords);

        this._graph.setNodeAttribute(this.draggedNode, "x", pos.x);
        this._graph.setNodeAttribute(this.draggedNode, "y", pos.y);
        return false;
    }

    private onKeyUpDown(event: KeyboardEvent) {
        this.updateModifierKeyState(event);

        if (event.key === "Alt") {
            event.preventDefault();
        }

        if (this.hoveredNode) {
            this.updateCursor();
        }
    }
}
