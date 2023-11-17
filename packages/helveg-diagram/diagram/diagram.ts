import { HelvegEvent } from "../common/event.ts";
import { HelvegGraph, expandPathsTo, findRoots, getNodeKinds, toggleNode } from "../model/graph.ts";
import { Coordinates, NodeProgramConstructor, Sigma, SigmaNodeEventPayload } from "../deps/sigma.ts";
import { LogSeverity, ILogger, consoleLogger } from "../model/logger.ts";
import { ForceAtlas2Progress, ForceAtlas2Supervisor } from "../layout/forceAltas2Supervisor.ts";
import { wheelOfFortune } from "../layout/circular.ts";
import tidyTree from "../layout/tidyTree.ts";
import { HelvegSigma, configureSigma, initializeGraph, initializeSigma, initializeSupervisor, styleGraph } from "./initializers.ts";
import { DEFAULT_GLYPH_PROGRAM_OPTIONS, GlyphProgramOptions, createGlyphProgram } from "../rendering/node.glyph.ts";
import { DEFAULT_EXPORT_OPTIONS, ExportOptions, exportDiagram } from "../rendering/export.ts";
import { SearchMode, buildNodeFilter, filterNodes } from "../model/filter.ts";
import { bfsGraph } from "../model/traversal.ts";
import { EdgeStylist, NodeStylist, RelationStylist, fallbackEdgeStylist, fallbackNodeStylist, fallbackRelationStylist } from "../model/style.ts";
import { EMPTY_ICON_REGISTRY, IconRegistry } from "../global.ts";
import { DataModel } from "../model/data-model.ts";
import { EMPTY_DATA_MODEL } from "../model/const.ts";
import { inferSettings } from "graphology-layout-forceatlas2";

export interface DiagramRefreshOptions {
    selectedRelations?: string[],
    selectedKinds?: string[],
    expandedDepth?: number
}

export interface DiagramOptions {
    glyphProgram: GlyphProgramOptions,
    mainRelation: string | null,
    logLevel: LogSeverity,
    nodeStylist: NodeStylist,
    relationStylist: RelationStylist,
    edgeStylist?: EdgeStylist,
    iconRegistry: Readonly<IconRegistry>,
    refresh: DiagramRefreshOptions,
    forceAtlas2: ForceAtlas2Options
}

export interface ForceAtlas2Options {
    linLogMode?: boolean;
    outboundAttractionDistribution?: boolean;
    adjustSizes?: boolean;
    edgeWeightInfluence?: number;
    scalingRatio?: number;
    strongGravityMode?: boolean;
    gravity?: number;
    slowDown?: number;
    barnesHutOptimize?: boolean;
    barnesHutTheta?: number;
}

export const DEFAULT_FORCE_ATLAS2_OPTIONS: Readonly<ForceAtlas2Options> = {
    ...inferSettings(1024),
    adjustSizes: true,
    barnesHutOptimize: false,
    barnesHutTheta: 0.5,
    edgeWeightInfluence: 1,
    linLogMode: false,
    outboundAttractionDistribution: false,
};

export const DEFAULT_DIAGRAM_OPTIONS: Readonly<DiagramOptions> = {
    logLevel: LogSeverity.Info,
    mainRelation: null,
    glyphProgram: DEFAULT_GLYPH_PROGRAM_OPTIONS,
    nodeStylist: fallbackNodeStylist,
    relationStylist: fallbackRelationStylist,
    iconRegistry: EMPTY_ICON_REGISTRY,
    refresh: {},
    forceAtlas2: DEFAULT_FORCE_ATLAS2_OPTIONS
};

export enum DiagramMode {
    Normal = "normal",
    Highlighting = "highlighting"
}

export enum DiagramStatus {
    Stopped = "stopped",
    Running = "running",
    RunningInBackground = "runningInBackground"
}

export interface DiagramStats {
    iterationCount: number;
    speed: number;
}

export interface CutOptions {
    isTransitive: boolean;
    relation: string | null;
}

export const DEFAULT_CUT_OPTIONS: CutOptions = {
    isTransitive: true,
    relation: null
};

export class Diagram {
    private _element: HTMLElement;
    get element(): HTMLElement { return this._element; }

    private _sigmaElement: HTMLElement;
    get sigmaElement(): HTMLElement { return this._sigmaElement; }

    private _options: DiagramOptions;
    get options(): Readonly<DiagramOptions> { return this._options; }

    private _model: DataModel = EMPTY_DATA_MODEL;
    get model(): DataModel { return this._model; }
    set model(value: DataModel) {
        this._model = value;
        if (this._model.data) {
            this._nodeKeys = Object.values(this._model.data.nodes)
                .flatMap(n => Object.keys(n))
                .filter((v, i, a) => a.indexOf(v) === i);
        }
        this.events.modelChanged.trigger(value);

        // NB: Runs without awaiting.
        this.refreshGraph(this._lastRefreshOptions);
        this.refreshSigma();
    }

    private _stats: DiagramStats = { iterationCount: -1, speed: -1.0 };
    get stats(): DiagramStats { return this._stats; }
    private set stats(value: DiagramStats) { this._stats = value; this.events.statsChanged.trigger(value); }

    private _mainRelation: string | null;
    get mainRelation(): string | null { return this._mainRelation; }
    set mainRelation(value: string | null) {
        this._mainRelation = value;
        this.events.mainRelationChanged.trigger(value);
    }

    private _status: DiagramStatus = DiagramStatus.Stopped;
    get status(): DiagramStatus { return this._status; }
    private set status(value: DiagramStatus) { this._status = value; this.events.statusChanged.trigger(value); };

    private _mode: DiagramMode = DiagramMode.Normal;
    get mode(): DiagramMode { return this._mode; }
    private set mode(value: DiagramMode) {
        this._mode = value;
        this._options.glyphProgram.showOnlyHighlighted = this._mode === DiagramMode.Highlighting;
        this.events.modeChanged.trigger(value);
    }

    private _selectedNode: string | null = null;
    get selectedNode(): string | null { return this._selectedNode; }
    set selectedNode(value: string | null) { this._selectedNode = value; this.events.nodeSelected.trigger(value); }

    private _canDragNodes: boolean = false;
    get canDragNodes(): boolean { return this._canDragNodes; }
    set canDragNodes(value: boolean) { this._canDragNodes = value; }

    private _draggedNode: string | null = null;
    get draggedNode(): string | null { return this._draggedNode; };
    private set draggedNode(value: string | null) { this._draggedNode = value; }

    private _logger: ILogger;
    get logger(): ILogger { return this._logger; }

    get nodeStylist(): NodeStylist { return this._options.nodeStylist; }
    set nodeStylist(value: NodeStylist) {
        this._options.nodeStylist = value;
        this.restyleGraph();
    }

    get edgeStylist(): EdgeStylist | undefined { return this._options.edgeStylist; }
    set edgeStylist(value: EdgeStylist | undefined) {
        this._options.edgeStylist = value;
        this.restyleGraph();
    };

    get relationStylist(): RelationStylist { return this._options.relationStylist; }
    set relationStylist(value: RelationStylist) {
        this._options.relationStylist = value;
        this.restyleGraph();
    }

    get forceAtlas2Options(): ForceAtlas2Options { return this._options.forceAtlas2; }
    set forceAtlas2Options(value: ForceAtlas2Options) {
        this._options.forceAtlas2 = value;
        this.refreshSupervisor(true);
    }

    private _graph?: HelvegGraph;
    private _sigma?: HelvegSigma;
    private _supervisor?: ForceAtlas2Supervisor;
    private _glyphProgram: NodeProgramConstructor;

    get glyphProgramOptions(): GlyphProgramOptions { return this.options.glyphProgram; }
    set glyphProgramOptions(value: GlyphProgramOptions) {
        this._options.glyphProgram = value;
        this.reconfigureSigma();
        this.restyleGraph();
    }

    // used when building JS filters
    // TODO: likely move to its own place somewhere else
    private _nodeKeys: string[] = [];

    // used for refreshing
    private _lastRefreshOptions: DiagramRefreshOptions;

    constructor(element: HTMLElement, options?: Partial<DiagramOptions>) {
        this._element = element;
        this._element.style.position = "relative";

        this._sigmaElement = document.createElement("div");
        this._sigmaElement.classList.add("sigma");
        this._sigmaElement.style.width = "100%";
        this._sigmaElement.style.height = "100%";
        this._element.appendChild(this._sigmaElement);

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
        statsChanged: new HelvegEvent<DiagramStats>("helveg.diagram.statsChanged"),
        nodeSelected: new HelvegEvent<string | null>("helveg.diagram.nodeSelected"),
        nodeClicked: new HelvegEvent<string>("helveg.diagram.nodeClicked"),
        mainRelationChanged: new HelvegEvent<string | null>("helveg.diagram.mainRelationChanged"),
    } as const;

    // NB: private state for gestures
    private _gestures = {
        hasPanned: false,
        isCaptorDown: false
    };

    async resetLayout(): Promise<void> {
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
                    y: radius * Math.sin(theta * i)
                }
            });
            i++;
        }

        this.stats = { iterationCount: 0, speed: 0 };
        if (this._sigma) {
            this._sigma.scheduleRefresh();
        }
    }

    async runLayout(inBackground: boolean): Promise<void> {
        if (!this._graph) {
            this._logger.debug("Cannot run since the graph is not initialized.");
            return;
        }

        if (!this._supervisor) {
            this._logger.debug("Cannot run since the supervisor is not initialized.");
            return;
        }

        this._logger.debug("Running the layout.");
        
        this._graph.forEachNode((n, a) => a.inInitialPosition = false);

        await this._supervisor.start(inBackground);

        this.refreshStatus();

        if (inBackground) {
            this._sigma?.kill();
            this._sigma = undefined;
        } else if (!this._sigma) {
            this.refreshSigma();
        }
    }

    async stopLayout(): Promise<void> {
        if (!this._graph) {
            this._logger.debug("Cannot stop since the graph is not initialized.");
            return;
        }

        this._logger.debug("Stopping the layout.");

        if (this._supervisor?.isRunning) {
            await this._supervisor.stop();
            this.refreshStatus();

            if (!this._sigma) {
                this.refreshSigma();
            }
        }
    }

    save(options?: ExportOptions): void {
        if (!this._graph) {
            this._logger.debug("Cannot save since the graph is not initialized.")
            return;
        }

        if (!this._sigma) {
            this._logger.debug("Cannot save since the sigma instance is not initialized.")
            return;
        }

        this._logger.debug("Saving the diagram.");

        options = { ...DEFAULT_EXPORT_OPTIONS, ...options };
        options.fileName ??= `${this._model.name}-export.png`;
        if (this._sigma) {
            exportDiagram(this._sigma, options);
        }
    }

    exportPositions(): Record<string, Coordinates> {
        const result: Record<string, Coordinates> = {};
        if (this._graph) {
            this._graph.forEachNode((node, attributes) => {
                result[node] = {
                    x: attributes.x ?? 0,
                    y: attributes.y ?? 0
                };
            });
        }
        return result;
    }

    importPositions(value: Record<string, Coordinates>) {
        this._graph?.forEachNode((node, attributes) => {
            if (value[node]) {
                attributes.x = value[node].x;
                attributes.y = value[node].y;
            }
        });
    }

    highlight(searchText: string | null, searchMode: SearchMode, expandedOnly: boolean = false): string[] {
        if (!this._graph) {
            this._logger.warn("Cannot highlight nodes since the graph is not initialized.");
            return [];
        }

        let results: string[] = [];
        try {
            let filter = buildNodeFilter(searchText, searchMode, this._nodeKeys);
            if (filter === null) {
                this.mode = DiagramMode.Normal;
                this._graph.forEachNode((_, a) => a.highlighted = undefined);
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
        }
        catch (e: any) {
            this._logger.warn(e?.message
                ?? e?.toString()
                ?? "Something bad happened while highlighting nodes.");
            return [];
        }

        this._logger.info(`Highlighting ${results.length} nodes.`);
        return results;
    }

    highlightNode(nodeId: string | null, includeSubtree: boolean, includeNeighbors: boolean) {
        if (nodeId === null) {
            this._logger.debug("Clearing node highlights.");
            this._graph?.forEachNode((_, a) => a.highlighted = undefined);
            this._sigma?.refresh();
            this.mode = DiagramMode.Normal;
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
                }
            });
        }

        if (includeNeighbors) {
            for (let neighbor of this._graph.neighbors(nodeId)) {
                this._graph.setNodeAttribute(neighbor, "highlighted", true);
            }
        }

        this.mode = DiagramMode.Highlighting;
        this._sigma?.refresh();
    }

    async isolate(searchText: string | null, searchMode: SearchMode): Promise<void> {
        if (!this._graph) {
            this._logger.warn("Cannot isolate nodes since the graph is not initialized.");
            return;
        }

        if (!this._model.data) {
            this._logger.warn("Cannot isolate nodes since the model contains no data.");
            return;
        }

        try {
            let filter = buildNodeFilter(searchText, searchMode, this._nodeKeys);
            if (filter === null) {
                return;
            }

            await this.refreshSupervisor(true, () => {
                if (!this._graph) {
                    return;
                }

                for (let id of filterNodes(this._model.data!, filter, true)) {
                    if (this._graph.hasNode(id)) {
                        this._graph.dropNode(id);
                    }
                }
            });

        }
        catch (e: any) {
            this._logger.warn(e?.message
                ?? e?.toString()
                ?? "Something bad happened while isolating nodes.");
            return;
        }

        this._logger.info(`Isolated ${this._graph.nodes().length} nodes.`);
    }


    async refresh(options?: DiagramRefreshOptions): Promise<void> {
        await this.refreshGraph(options ?? this._lastRefreshOptions);
        await this.resetLayout();
    }

    reset(): Promise<void> {
        this.mainRelation = this.options.mainRelation;
        return this.refresh(this.options.refresh);
    }

    async cut(nodeId: string, options?: CutOptions): Promise<void> {
        options = { ...DEFAULT_CUT_OPTIONS, ...options };

        if (!this._graph) {
            this._logger.warn("Cannot cut nodes since the graph is not initialized.");
            return;
        }

        if (!this._graph.hasNode(nodeId)) {
            throw new Error(`Cannot cut node '${nodeId}' since it does not exist in the graph.`);
        }

        if (!options.isTransitive) {
            this._graph.dropNode(nodeId);
            return;
        }

        let reachable = bfsGraph(this._graph, nodeId, { relation: options.relation });

        await this.refreshSupervisor(true, () => {
            reachable.forEach(id => this._graph?.dropNode(id));
        })

        this._logger.info(`Cut ${reachable.size} nodes.`);
    }

    async toggleNode(nodeId: string): Promise<void> {
        if (!this._graph) {
            this._logger.warn("Cannot toggle nodes since the graph is not initialized.");
            return;
        }

        await this.refreshSupervisor(true, () => toggleNode(this._graph!, nodeId));
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
        }
        finally {
            if (!this._graph) {
                return;
            }

            this._supervisor = initializeSupervisor(
                this._graph,
                this.onSupervisorProgress.bind(this),
                this.options.forceAtlas2,
                this.logger);
            if (shouldLayoutContinue
                && (lastStatus === DiagramStatus.Running || lastStatus === DiagramStatus.RunningInBackground)) {
                await this._supervisor.start(lastStatus === DiagramStatus.RunningInBackground);
                this.refreshStatus();
            }
        }
    }

    private refreshStatus() {
        this.status = this._supervisor?.isRunning === true && this._supervisor?.isInBackground === true
            ? DiagramStatus.RunningInBackground
            : this._supervisor?.isRunning === true
                ? DiagramStatus.Running
                : DiagramStatus.Stopped;
    }

    private onSupervisorProgress(message: ForceAtlas2Progress) {
        this.stats = {
            iterationCount: message.iterationCount,
            speed: message.speed
        };
    }

    private refreshSigma(): void {
        if (!this._sigmaElement || !this._graph) {
            return;
        }

        this._logger.debug("Refreshing the Sigma.js instance.");

        if (this._sigma) {
            this._sigma.kill();
        }

        this._sigma = initializeSigma(
            this._sigmaElement,
            this._graph,
            this._glyphProgram,
            this.onNodeClick.bind(this),
            this.onNodeDown.bind(this),
            undefined,
            this.onDown.bind(this),
            this.onUp.bind(this),
            this.onMove.bind(this)
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
            showOnlyHighlighted: this.mode == DiagramMode.Highlighting
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
            this._model,
            this.mainRelation ?? undefined,
            options.selectedRelations ?? (this.mainRelation ? [this.mainRelation] : []),
            options.selectedKinds,
            options.expandedDepth);
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
            this._model,
            this.options.glyphProgram,
            this.options.nodeStylist,
            this.options.relationStylist,
            this.options.edgeStylist,
            this.logger);
    }

    private onNodeClick(event: SigmaNodeEventPayload): void {
        this.selectedNode = event.node;
        this.events.nodeClicked.trigger(event.node);
    }

    private onNodeDown(event: SigmaNodeEventPayload): void {
        if (this._canDragNodes) {
            this.draggedNode = event.node;
            this._graph?.setNodeAttribute(event.node, "highlighted", true);
            this._graph?.setNodeAttribute(event.node, "fixed", true);
        }
    }

    private onDown(_event: Coordinates): void {
        this._gestures.isCaptorDown = true;
        this._gestures.hasPanned = false;
    }

    private onUp(_coords: Coordinates): void {
        if (this.draggedNode) {
            this._graph?.setNodeAttribute(this.draggedNode, "highlighted", false);
            this._graph?.setNodeAttribute(this.draggedNode, "fixed", false);
            this.draggedNode = null;
        }

        if (!this._gestures.hasPanned) {
            // deselect the node and possibly unhighlight nodes
            this.selectedNode = null;
        }

        this._gestures.hasPanned = false;
        this._gestures.isCaptorDown = false;
    }

    private onMove(coords: Coordinates): boolean {
        if (this._gestures.isCaptorDown) {
            this._gestures.hasPanned = true;
        }

        if (!this.draggedNode || !this._sigma || !this._graph) {
            return true;
        }

        const pos = this._sigma.viewportToGraph(coords)

        this._graph.setNodeAttribute(this.draggedNode, "x", pos.x);
        this._graph.setNodeAttribute(this.draggedNode, "y", pos.y);
        return false;
    }
}
