import { HelvegEvent } from "../common/event.ts";
import { HelvegGraph, expandPathsTo, findRoots, getNodeKinds, toggleNode } from "../model/graph.ts";
import { LogSeverity, ILogger, consoleLogger } from "../model/logger.ts";
import { ForceAtlas2Progress, ForceAtlas2Supervisor } from "../layout/forceAltas2Supervisor.ts";
import { wheelOfFortune } from "../layout/circular.ts";
import tidyTree from "../layout/tidyTree.ts";
import { SearchMode, buildNodeFilter, filterNodes } from "../model/filter.ts";
import { bfsGraph } from "../model/traversal.ts";
import { EdgeStylist, NodeStylist, RelationStylist, fallbackEdgeStylist, fallbackNodeStylist, fallbackRelationStylist } from "../model/style.ts";
import { EMPTY_ICON_REGISTRY, IconRegistry } from "../global.ts";
import { DataModel } from "../model/data-model.ts";
import { Colors, EMPTY_DATA_MODEL } from "../model/const.ts";
import { initializeGraph, styleGraph } from "./initializers.ts";

export type Coordinates = { x: number, y: number };

export interface ExportOptions {
    fileName: string | null;
    includeEdges: boolean;
    includeNodes: boolean;
    includeLabels: boolean;
    includeEffects: boolean;
    includePizzaDough: boolean;
    includeHighlights: boolean;
    backgroundColor: string;
    opacity: number;
    scale: number;
}

export const DEFAULT_EXPORT_OPTIONS: ExportOptions = {
    fileName: null, // let the diagram decide the name
    includeEdges: true,
    includeNodes: true,
    includeLabels: true,
    includeEffects: true,
    includePizzaDough: true,
    includeHighlights: true,
    backgroundColor: Colors.White,
    opacity: 0,
    scale: 1
}

export interface DiagramRefreshOptions {
    selectedRelations?: string[],
    selectedKinds?: string[],
    expandedDepth?: number
}

export interface DiagramOptions {
    // glyphProgram: GlyphProgramOptions,
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
    // glyphProgram: DEFAULT_GLYPH_PROGRAM_OPTIONS,
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
}

export const DEFAULT_CUT_OPTIONS: CutOptions = {
    isTransitive: true
};

export class Diagram {
    private _element: HTMLElement;
    get element(): HTMLElement { return this._element; }

    private _options: DiagramOptions;
    get options(): Readonly<DiagramOptions> { return this._options; }

    private _model: DataModel = EMPTY_DATA_MODEL;
    get model(): DataModel { return this._model; }
    set model(value: DataModel) {
        throw new Error("This function has been unimplemented.");
    }

    private _stats: DiagramStats = { iterationCount: -1, speed: -1.0 };
    get stats(): DiagramStats { return this._stats; }
    private set stats(value: DiagramStats) { throw new Error("This function has been unimplemented."); }

    private _mainRelation: string | null;
    get mainRelation(): string | null { return this._mainRelation; }
    set mainRelation(value: string | null) {
        throw new Error("This function has been unimplemented.");
    }

    private _status: DiagramStatus = DiagramStatus.Stopped;
    get status(): DiagramStatus { return this._status; }
    private set status(value: DiagramStatus) { throw new Error("This function has been unimplemented."); };

    private _mode: DiagramMode = DiagramMode.Normal;
    get mode(): DiagramMode { return this._mode; }
    private set mode(value: DiagramMode) {
        throw new Error("This function has been unimplemented.");
    }

    private _selectedNode: string | null = null;
    get selectedNode(): string | null { return this._selectedNode; }
    set selectedNode(value: string | null) { throw new Error("This function has been unimplemented."); }

    get hoveredNode(): string | null { throw new Error("This function has been unimplemented."); }

    private _canDragNodes: boolean = false;
    get canDragNodes(): boolean { return this._canDragNodes; }
    set canDragNodes(value: boolean) { throw new Error("This function has been unimplemented."); }

    private _draggedNode: string | null = null;
    get draggedNode(): string | null { return this._draggedNode; };

    private _logger: ILogger;
    get logger(): ILogger { return this._logger; }

    get nodeStylist(): NodeStylist { return this._options.nodeStylist; }
    set nodeStylist(value: NodeStylist) {
        throw new Error("This function has been unimplemented.");
    }

    get edgeStylist(): EdgeStylist | undefined { return this._options.edgeStylist; }
    set edgeStylist(value: EdgeStylist | undefined) {
        throw new Error("This function has been unimplemented.");
    };

    get relationStylist(): RelationStylist { return this._options.relationStylist; }
    set relationStylist(value: RelationStylist) {
        throw new Error("This function has been unimplemented.");
    }

    // used for refreshing
    private _lastRefreshOptions: DiagramRefreshOptions;

    constructor(element: HTMLElement, options?: Partial<DiagramOptions>) {
        this._element = element;
        this._element.style.position = "relative";

        this._options = { ...DEFAULT_DIAGRAM_OPTIONS, ...options };
        this._logger = consoleLogger("diagram", this._options.logLevel);
        this._mainRelation = this._options.mainRelation;
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

    async resetLayout(): Promise<void> {
        throw new Error("This function has been unimplemented.");
    }

    async runLayout(inBackground: boolean): Promise<void> {
        throw new Error("This function has been unimplemented.");
    }

    async stopLayout(): Promise<void> {
        throw new Error("This function has been unimplemented.");
    }

    save(options?: ExportOptions): void {
        throw new Error("This function has been unimplemented.");
    }

    exportPositions(): Record<string, Coordinates> {
        throw new Error("This function has been unimplemented.");
    }

    importPositions(value: Record<string, Coordinates>) {
        throw new Error("This function has been unimplemented.");
    }

    highlight(searchText: string | null, searchMode: SearchMode, expandedOnly: boolean = false): string[] {
        throw new Error("This function has been unimplemented.");
    }

    highlightNode(nodeId: string | null, includeSubtree: boolean, includeNeighbors: boolean) {
        throw new Error("This function has been unimplemented.");
    }

    async isolate(searchText: string | null, searchMode: SearchMode): Promise<void> {
        throw new Error("This function has been unimplemented.");
    }


    async refresh(options?: DiagramRefreshOptions): Promise<void> {
        throw new Error("This function has been unimplemented.");
    }

    reset(): Promise<void> {
        throw new Error("This function has been unimplemented.");
    }

    async cut(nodeId: string, options?: CutOptions): Promise<void> {
        throw new Error("This function has been unimplemented.");
    }

    async toggleNode(nodeId: string): Promise<void> {
        throw new Error("This function has been unimplemented.");
    }
}
