import Graph from "graphology";
import { DEFAULT_DATA_OPTIONS, DEFAULT_EXPORT_OPTIONS, DEFAULT_GLYPH_OPTIONS, DEFAULT_LAYOUT_OPTIONS, SearchMode, type DataOptions, type ExportOptions, type LayoutOptions, type ToolOptions, DEFAULT_TOOL_OPTIONS, type AppearanceOptions, DEFAULT_APPEARANCE_OPTIONS, SearchScope } from "./options";
import { EMPTY_MODEL, type VisualizationModel } from "./visualization";
import { Sigma } from "sigma";
import { ForceAtlas2Supervisor, type ForceAtlas2Progress } from "layout/forceAltas2Supervisor";
import { IconAtlas } from "rendering/iconAtlas";
import { HelvegEvent } from "common/event";
import type { SigmaNodeEventPayload, SigmaStageEventPayload } from "sigma/sigma";
import { createGlyphProgram, type GlyphProgramOptions } from "rendering/node.glyph";
import forceAtlas2 from "graphology-layout-forceatlas2";
import type { NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import { exportDiagram } from "rendering/export";
import tidyTree from "layout/tidyTree";
import type { HelvegInstance } from "./instance";
import { buildNodeFilter, filterNodes } from "./filter";
import type { Coordinates } from "sigma/types";
import { findRoots, toggleNode, type HelvegGraph, type HelvegNodeAttributes } from "./graph";
import { bfs } from "./traversal";
import { wheellOfFortune } from "layout/circular";
import { OutlineStyle, type NodeStyleRegistry, type Outlines, getOutlinesTotalWidth, EdgeStyleRegistry } from "./style";
import { DEFAULT_SETTINGS } from "sigma/settings";

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

    get appearanceOptions(): AppearanceOptions;
    set appearanceOptions(value: AppearanceOptions);

    get layoutOptions(): LayoutOptions;
    set layoutOptions(value: LayoutOptions);

    get toolOptions(): ToolOptions;
    set toolOptions(value: ToolOptions);

    get status(): StructuralStatus;
    get statusChanged(): HelvegEvent<StructuralStatus>;

    get mode(): StructuralDiagramMode;
    get modeChanged(): HelvegEvent<StructuralDiagramMode>;

    get stats(): StructuralDiagramStats;
    get statsChanged(): HelvegEvent<StructuralDiagramStats>;

    get selectedNodeId(): string | null;
    get nodeSelected(): HelvegEvent<string | null>;

    get canDragNodes(): boolean;
    set canDragNodes(value: boolean);

    get draggedNodeId(): string | null;

    get nodeClicked(): HelvegEvent<string>;

    exportPositions(): Record<string, Coordinates>;
    importPositions(value: Record<string, Coordinates>);

    resetLayout(): Promise<void>;
    runLayout(inBackground: boolean): Promise<void>;
    stopLayout(): Promise<void>;
    save(options?: ExportOptions): void;
    highlight(searchText: string | null, searchMode: SearchMode, searchScope: SearchScope): void;
    highlightNode(nodeId: string | null, includeSubtree: boolean, includeNeighbors: boolean): void;
    isolate(searchText: string | null, searchMode: SearchMode, searchScope: SearchScope): Promise<void>;
    refresh(): Promise<void>;
    cut(nodeId: string): Promise<void>;
    toggleNode(nodeId: string): Promise<void>;
}

/**
 * An implementation of StructuralDiagram that is tied to an HTMLElement but not to a specific UI framework.
 */
export class StructuralDiagram implements AbstractStructuralDiagram {
    private _element: HTMLElement | null = null;
    private _model: VisualizationModel = EMPTY_MODEL;
    private _nodeKeys: string[] = [];
    private _dataOptions: DataOptions = DEFAULT_DATA_OPTIONS;
    private _appearanceOptions: AppearanceOptions = DEFAULT_APPEARANCE_OPTIONS;
    private _layoutOptions: LayoutOptions = DEFAULT_LAYOUT_OPTIONS;
    private _toolOptions: ToolOptions = DEFAULT_TOOL_OPTIONS;
    private _status: StructuralStatus = StructuralStatus.Stopped;
    private _statusChanged = new HelvegEvent<StructuralStatus>("helveg.StructuralDiagram.statusChanged");
    private _mode: StructuralDiagramMode = StructuralDiagramMode.Normal;
    private _modeChanged = new HelvegEvent<StructuralDiagramMode>("helveg.StructuralDiagram.modeChanged");
    private _stats: StructuralDiagramStats = {
        iterationCount: 0,
        speed: 0
    };
    private _statsChanged = new HelvegEvent<StructuralDiagramStats>("helveg.StructuralDiagram.statsChanged");
    private _selectedNodeId: string | null = null;
    private _nodeSelected = new HelvegEvent<string | null>("helveg.StructuralDiagram.nodeSelected");
    private _canDragNodes: boolean = false;
    private _draggedNodeId: string | null = null;

    private _graph: HelvegGraph | null = null;
    private _sigma: Sigma | null = null;
    private _supervisor: ForceAtlas2Supervisor | null = null;
    private _iconAtlas: IconAtlas;
    private _glyphProgramOptions: GlyphProgramOptions;
    private _glyphProgram: NodeProgramConstructor;
    private _isCaptorDown = false;
    private _hasPanned = false;

    private _nodeClicked = new HelvegEvent<string>("helveg.StructuralDiagram.nodeClicked");

    constructor(private _instance: HelvegInstance) {
        this._iconAtlas = new IconAtlas(this._instance.icons);
        this._glyphProgramOptions = {
            gap: 0.5,
            iconAtlas: this._iconAtlas,
            showIcons: true,
            showOutlines: true,
            showFire: true,
            isFireAnimated: true,
            particleCount: 32,
            diagramMode: StructuralDiagramMode.Normal,
            isPizzaEnabled: false,
            crustWidth: 20,
            sauceWidth: 40
        };
        this._glyphProgram = createGlyphProgram(this._glyphProgramOptions);
        this.appearanceOptions = _instance.options.appearance;
        this.layoutOptions = _instance.options.layout;
        this.toolOptions = _instance.options.tool;
        this.dataOptions = _instance.options.data;
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

        await this.refreshSupervisor();

        let roots = findRoots(this._graph, this.layoutOptions.tidyTree.relation ?? undefined);
        let i = 0;
        let { radius, theta } = roots.size <= 1 ? { radius: 0, theta: 0 } : wheellOfFortune(1000, roots.size);
        radius = Math.min(radius, 4000);
        for (let root of roots) {
            tidyTree(this._graph, root, {
                radius: 1000,
                relation: this.layoutOptions.tidyTree.relation,
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
            DEBUG && console.warn("Cannot run since the graph is not initialized.");
            return;
        }

        if (!this._supervisor) {
            DEBUG && console.warn("Cannot run since the supervisor is not initialized.");
            return;
        }

        DEBUG && console.log("Running the layout.");

        await this._supervisor.start(inBackground);

        this.refreshStatus();

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
            this.refreshStatus();

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
        options.fileName ??= `${this._model.documentInfo.name}-export.png`;
        if (this._sigma) {
            exportDiagram(this._sigma, options);
        }
    }

    highlight(searchText: string | null, searchMode: SearchMode, searchScope: SearchScope): void {
        if (!this._graph) {
            DEBUG && console.warn("Cannot highlight nodes since the graph is not initialized.");
            return;
        }

        try {
            let filter = buildNodeFilter(searchText, searchMode, this._nodeKeys);
            if (filter === null) {
                this.mode = StructuralDiagramMode.Normal;
                this._graph.forEachNode((_, a) => a.highlighted = undefined);
                this._sigma?.refresh();
                return;
            }

            this.mode = StructuralDiagramMode.Highlighting;

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

    highlightNode(nodeId: string | null, includeSubtree: boolean, includeNeighbors: boolean) {
        if (nodeId === null) {
            DEBUG && console.log("Clearing node highlights.");
            this._graph?.forEachNode((_, a) => a.highlighted = undefined);
            this._sigma?.refresh();
            this.mode = StructuralDiagramMode.Normal;
            return;
        }

        if (!this._graph?.hasNode(nodeId)) {
            return;
        }

        this._graph.forEachNode((n) => this._graph?.setNodeAttribute(n, "highlighted", false));
        this._graph.setNodeAttribute(nodeId, "highlighted", true);

        if (includeSubtree) {
            bfs(this._graph, nodeId, {
                relation: this.layoutOptions.tidyTree.relation,
                callback: (_, attr) => {
                    attr.highlighted = true;
                }
            });
        }

        if (includeNeighbors) {
            for (let neighbor of this._graph.neighbors(nodeId)) {
                this._graph.setNodeAttribute(neighbor, "highlighted", true);
            }
        }

        this.mode = StructuralDiagramMode.Highlighting;
        this._sigma?.refresh();
    }

    async isolate(searchText: string | null, searchMode: SearchMode, searchScope: SearchScope): Promise<void> {
        if (!this._graph) {
            DEBUG && console.warn("Cannot isolate nodes since the graph is not initialized.");
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

                for (let id of filterNodes(this._model.multigraph, filter, true)) {
                    if (this._graph.hasNode(id)) {
                        this._graph.dropNode(id);
                    }
                }
            });

        }
        catch (e: any) {
            this._instance.logger.warn(e?.message
                ?? e?.toString()
                ?? "Something bad happened while isolating nodes.");
            return;
        }

        this._instance.logger.info(`Isolated ${this._graph.nodes().length} nodes.`);
    }

    async refresh(): Promise<void> {
        await this.refreshGraph();
        await this.resetLayout();
    }

    async cut(nodeId: string): Promise<void> {
        if (!this._graph) {
            DEBUG && console.warn("Cannot cut nodes since the graph is not initialized.");
            return;
        }

        if (!this._graph.hasNode(nodeId)) {
            throw new Error(`Cannot cut node '${nodeId}' since it does not exist in the graph.`);
        }

        if (!this._toolOptions.cut.isTransitive) {
            this._graph.dropNode(nodeId);
            return;
        }

        let reachable = bfs(this._graph, nodeId, { relation: this._toolOptions.cut.relation });

        await this.refreshSupervisor(true, () => {
            reachable.forEach(id => this._graph?.dropNode(id));
        })

        this._instance.logger.info(`Cut ${reachable.size} nodes.`);
    }

    async toggleNode(nodeId: string): Promise<void> {
        if (!this._graph) {
            DEBUG && console.warn("Cannot toggle nodes since the graph is not initialized.");
            return;
        }

        await this.refreshSupervisor(true, () => toggleNode(this._graph!, nodeId));
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

        // NB: Runs without awaiting.
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
        DEBUG && console.log("DataOptions have changed. This has no effect until the next refresh().");
    }

    get appearanceOptions(): AppearanceOptions {
        return this._appearanceOptions;
    }

    set appearanceOptions(value: AppearanceOptions) {
        this._appearanceOptions = value;

        this._glyphProgramOptions.showIcons = this._appearanceOptions.glyph.showIcons;
        this._glyphProgramOptions.showOutlines = this._appearanceOptions.glyph.showOutlines;
        this._glyphProgramOptions.showFire = this._appearanceOptions.glyph.showFire;
        this._glyphProgramOptions.isFireAnimated = this._appearanceOptions.glyph.isFireAnimated;
        this._glyphProgramOptions.isPizzaEnabled = this._appearanceOptions.codePizza.isEnabled;
        this._glyphProgramOptions.crustWidth = this._appearanceOptions.codePizza.crustWidth;
        this._glyphProgramOptions.sauceWidth = this._appearanceOptions.codePizza.sauceWidth;

        this.reconfigureSigma();
        this.restyleGraph();
    }

    get layoutOptions(): LayoutOptions {
        return this._layoutOptions;
    }

    set layoutOptions(value: LayoutOptions) {
        this._layoutOptions = value;
        DEBUG && console.log("LayoutOptions have changed. This has no effect until the next refresh().");
    }

    get toolOptions(): ToolOptions {
        return this._toolOptions;
    }

    set toolOptions(value: ToolOptions) {
        this._toolOptions = value;
    }

    get status(): StructuralStatus {
        return this._status;
    }

    private set status(value: StructuralStatus) {
        if (this._status !== value) {
            this._status = value;
            this._statusChanged.trigger(value);
        }
    }

    get statusChanged(): HelvegEvent<StructuralStatus> {
        return this._statusChanged;
    }

    get mode(): StructuralDiagramMode {
        return this._mode;
    }

    private set mode(value: StructuralDiagramMode) {
        if (this._mode !== value) {
            this._mode = value;
            this._glyphProgramOptions.diagramMode = value;
            this.reconfigureSigma();
            this._modeChanged.trigger(value);
        }
    }

    get modeChanged(): HelvegEvent<StructuralDiagramMode> {
        return this._modeChanged;
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

    private set selectedNodeId(value: string | null) {
        if (this._selectedNodeId !== value) {
            this._selectedNodeId = value;
            this._nodeSelected.trigger(value);
        }
    }

    get nodeSelected(): HelvegEvent<string | null> {
        return this._nodeSelected;
    }

    get canDragNodes(): boolean {
        return this._canDragNodes;
    }

    set canDragNodes(value: boolean) {
        this._canDragNodes = value;
        this._draggedNodeId = null;
    }

    get draggedNodeId(): string | null {
        return this._draggedNodeId;
    }

    get nodeClicked(): HelvegEvent<string> {
        return this._nodeClicked;
    }

    exportPositions(): Record<string, Coordinates> {
        let result = {};
        if (this._graph) {
            this._graph.forEachNode((node, attributes) => {
                result[node] = {
                    x: attributes.x,
                    y: attributes.y
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

    private refreshSigma(): void {
        if (!this._element || !this._graph) {
            return;
        }

        DEBUG && console.log("Refreshing the Sigma.js instance.");

        if (this._sigma) {
            this._sigma.kill();
        }

        this._sigma = initializeSigma(
            this._element,
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

        let tmpAppearanceOptions = {
            ...this._appearanceOptions,
            showLabels: this._appearanceOptions.glyph.showLabels && this.mode == StructuralDiagramMode.Normal
        };

        configureSigma(this._sigma, tmpAppearanceOptions);
    }

    private async refreshGraph(): Promise<void> {
        if (!this._model || this._model.isEmpty) {
            return;
        }

        DEBUG && console.log(`Refreshing the graph to match the '${this._model.documentInfo.name}' model.`);

        this._graph = initializeGraph(this._model, this._dataOptions);
        for (let plugin of this._instance.plugins.getAll()) {
            if (plugin.onVisualize) {
                plugin.onVisualize(this._model, this._graph);
            }
        }
        this.restyleGraph();

        await this.refreshSupervisor(false, () => this._graph && this._sigma?.setGraph(this._graph));

        this._supervisor = initializeSupervisor(this._graph, this.onSupervisorProgress.bind(this));

        this.mode = StructuralDiagramMode.Normal;
    }

    private restyleGraph(): void {
        if (!this._graph || !this._model || this._model.isEmpty) {
            return;
        }

        DEBUG && console.log(`Restyling the graph.`);

        styleGraph(
            this._graph,
            this._model,
            this._appearanceOptions,
            this._instance.nodeStyles,
            this._instance.edgeStyles);
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
            this._supervisor = null;
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

            this._supervisor = initializeSupervisor(this._graph, this.onSupervisorProgress.bind(this));
            if (shouldLayoutContinue
                && (lastStatus === StructuralStatus.Running || lastStatus === StructuralStatus.RunningInBackground)) {
                await this._supervisor.start(lastStatus === StructuralStatus.RunningInBackground);
                this.refreshStatus();
            }
        }
    }

    private refreshStatus() {
        this.status = this._supervisor?.isRunning === true && this._supervisor?.isInBackground === true
            ? StructuralStatus.RunningInBackground
            : this._supervisor?.isRunning === true
                ? StructuralStatus.Running
                : StructuralStatus.Stopped;
    }

    private onNodeClick(event: SigmaNodeEventPayload): void {
        this.selectedNodeId = event.node;
        this._nodeClicked.trigger(event.node);
    }

    private onSupervisorProgress(message: ForceAtlas2Progress) {
        this.stats = {
            iterationCount: message.iterationCount,
            speed: message.speed
        };
    }

    private onNodeDown(event: SigmaNodeEventPayload): void {
        if (this._canDragNodes) {
            this._draggedNodeId = event.node;
            this._graph?.setNodeAttribute(event.node, "highlighted", true);
            this._graph?.setNodeAttribute(event.node, "fixed", true);
        }
    }

    private onDown(event: Coordinates): void {
        this._isCaptorDown = true;
        this._hasPanned = false;
    }

    private onUp(coords: Coordinates): void {
        if (this._draggedNodeId) {
            this._graph?.setNodeAttribute(this._draggedNodeId, "highlighted", false);
            this._graph?.setNodeAttribute(this._draggedNodeId, "fixed", false);
            this._draggedNodeId = null;
        }

        if (!this._hasPanned) {
            // deselect the node and possibly unhighlight nodes
            this.selectedNodeId = null;
        }

        this._hasPanned = false;
        this._isCaptorDown = false;
    }

    private onMove(coords: Coordinates): boolean {
        if (this._isCaptorDown) {
            this._hasPanned = true;
        }

        if (!this._draggedNodeId || !this._sigma || !this._graph) {
            return true;
        }


        const pos = this._sigma.viewportToGraph(coords)

        this._graph.setNodeAttribute(this._draggedNodeId, "x", pos.x);
        this._graph.setNodeAttribute(this._draggedNodeId, "y", pos.y);
        return false;
    }
}

function initializeGraph(
    model: VisualizationModel,
    dataOptions: DataOptions,
): HelvegGraph {

    const graph = new Graph<Partial<HelvegNodeAttributes>>({
        multi: true,
        allowSelfLoops: true,
        type: "directed"
    });
    for (const nodeId in model.multigraph.nodes) {
        const node = model.multigraph.nodes[nodeId];
        graph.addNode(nodeId, {
            label: node.properties.Label ?? nodeId,
            x: 0,
            y: 0,
            style: node.properties.Style
        });
    }

    for (const relationId of dataOptions.selectedRelations) {
        const relation = model.multigraph.relations[relationId];
        if (!relation) {
            continue;
        }

        for (let [id, edge] of Object.entries(relation.edges)) {
            try {
                graph.addDirectedEdgeWithKey(`${relationId};${id}`, edge.src, edge.dst, {
                    relation: relationId,
                    style: edge.properties.Style
                });
            } catch (error) {
                DEBUG && console.warn(`Failed to add an edge. edge=${edge}, error=${error}`);
            }
        }
    }

    return graph;
}

function styleGraph(
    graph: HelvegGraph,
    model: VisualizationModel,
    appearanceOptions: AppearanceOptions,
    nodeStyles: NodeStyleRegistry,
    edgeStyles: EdgeStyleRegistry) {

    graph.forEachNode((node, attributes) => {
        if (!attributes.style) {
            DEBUG && console.log(`Node '${node}' is missing a style attribute.`);
            return;
        }

        if (!model.multigraph.nodes[node]) {
            DEBUG && console.log(`Node '${node}' does not exist in the model.`);
            return;
        }

        const generator = nodeStyles.get(attributes.style);
        if (!generator) {
            DEBUG && console.log(`Node style '${attributes.style}' could not be found.`);
            return;
        }

        const style = generator(model.multigraph.nodes[node]);
        if (!style) {
            DEBUG && console.log(`Node style '${attributes.style}' could not be applied to node '${node}'.`);
            return;
        }

        const outlines = [
            { width: style.size, style: OutlineStyle.Solid },
            ...style.outlines.slice(0, 3),
        ] as Outlines;
        attributes.size = appearanceOptions.glyph.showOutlines && outlines.length > 0
            ? getOutlinesTotalWidth(outlines)
            : style.size;
        attributes.iconSize = style.size;
        attributes.color = style.color;
        attributes.type = "glyph";
        attributes.icon = style.icon;
        attributes.outlines = outlines;
        attributes.fire = style.fire;
    });

    graph.forEachEdge((edge, attributes) => {
        if (!attributes.style || !attributes.relation) {
            return;
        }

        let generator = edgeStyles.get(attributes.style);
        if (!generator) {
            DEBUG && console.log(`Edge style '${attributes.style}' could not be found.`);
            return;
        }

        const style = generator({
            relation: attributes.relation,
            edge: model.multigraph.relations[attributes.relation].edges[edge]
        });
        if (!style) {
            DEBUG && console.log(`Edge style '${attributes.style}' could not be applied to edge '${edge}'.`);
            return;
        }

        attributes.type = style.type;
        attributes.color = style.color;
        attributes.size = style.width;
    });
}

// HACK: Sigma does not allow to disable hovering on nodes, so we have to track it ourselves.
let isHoverEnabled = true;

function initializeSigma(
    element: HTMLElement,
    graph: Graph,
    glyphProgram: NodeProgramConstructor,
    onClick?: (payload: SigmaNodeEventPayload) => void,
    onNodeDown?: (payload: SigmaNodeEventPayload) => void,
    onStageDown?: (payload: SigmaStageEventPayload) => void,
    onDown?: (coords: Coordinates) => void,
    onUp?: (coords: Coordinates) => void,
    onMove?: (coords: Coordinates) => boolean | void
): Sigma {

    const sigma = new Sigma(graph, element, {
        nodeProgramClasses: {
            glyph: glyphProgram,
        },
        labelFont: "'Cascadia Mono', 'Consolas', monospace",
        edgeLabelFont: "'Cascadia Mono', 'Consolas', monospace",
        itemSizesReference: "positions"
    });

    if (onClick) {
        sigma.on("clickNode", onClick);
    }

    if (onNodeDown) {
        sigma.on("downNode", onNodeDown);
    }

    if (onStageDown) {
        sigma.on("downStage", onStageDown);
    }

    if (onDown) {
        sigma.getMouseCaptor().on("mousedown", e => onDown(e));
        sigma.getTouchCaptor().on("touchdown", e => onDown(e.touches[0]));
    }

    if (onUp) {
        sigma.getMouseCaptor().on("mouseup", onUp);
        sigma.getTouchCaptor().on("touchup", e => onUp(e.touches[0]));
    }

    if (onMove) {
        sigma.getMouseCaptor().on("mousemovebody", e => {
            if (onMove(e) === false) {
                // prevent Sigma from moving the camera
                e.preventSigmaDefault();
                e.original.preventDefault();
                e.original.stopPropagation();
            }

        });
        sigma.getTouchCaptor().on("touchmove", e => {
            if (e.touches.length == 1) {
                onMove(e.touches[0]);
                e.original.preventDefault();
            }
        });
    }

    sigma.on("enterNode", e => {
        if (!isHoverEnabled) {
            (sigma as any).hoveredNode = null;
        }
    })

    return sigma;
}

function configureSigma(
    sigma: Sigma,
    appearanceOptions: AppearanceOptions
) {
    sigma.setSetting("renderLabels", appearanceOptions.glyph.showLabels);
    if (appearanceOptions.codePizza.isEnabled) {
        sigma.setSetting("zoomToSizeRatioFunction", (cameraRatio) => cameraRatio);
        sigma.setSetting("hoverRenderer", () => { });
        isHoverEnabled = false;
    } else {
        sigma.setSetting("zoomToSizeRatioFunction", DEFAULT_SETTINGS.zoomToSizeRatioFunction);
        sigma.setSetting("hoverRenderer", DEFAULT_SETTINGS.hoverRenderer);
        isHoverEnabled = true;
    }
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
