import { HelvegEvent } from "../common/event.ts";
import { ForceAtlas2Settings, helpers } from "../deps/graphology-layout-forceatlas2.ts";
import { createEdgeWeightGetter } from "../deps/graphology-utils.ts";
import Graph, { Attributes, EdgeMapper } from "../deps/graphology.ts";
import { HelvegGraph } from "../global.ts";
import { ILogger } from "../model/logger.ts";
import {
    MessageKind,
    StartMessage,
    Message,
    UpdateMessage,
    ProgressMessage,
    StopMessage,
    StopReason,
} from "./forceAtlas2Messages.ts";
import forceAtlas2WorkerCode from "inline-bundle:./forceAtlas2Worker.ts";

type GraphMatrices = {
    // NB: the counts variables are here because the matrices might currently be owned by the worker
    nodeCount: number;
    nodes: Float32Array;
    nodeIds: string[];
    edgeCount: number;
    edges: Float32Array;
};

export interface ForceAtlas2Progress {
    iterationCount: number;
    iterationsPerSecond: number;
    globalTraction: number;
    globalSwinging: number;
    averageTraction: number;
    averageSwinging: number;
}

export class ForceAtlas2Supervisor {
    public reportInterval: number = 100;
    private _lastWorker: Worker | null = null;
    private _worker: Worker | null = null;
    private _running: boolean = false;
    private _inBackground: boolean = false;
    private _lastPerformanceTime: number = 0;
    private _matrices: GraphMatrices = {
        nodeCount: 0,
        nodes: new Float32Array(0),
        nodeIds: [],
        edgeCount: 0,
        edges: new Float32Array(0),
    };

    get nodeIds(): Readonly<string[]> {
        return this._matrices.nodeIds;
    }

    constructor(private graph: HelvegGraph, private settings: ForceAtlas2Settings, private logger?: ILogger) {
        this.graph.on("nodeAdded", this.handleGraphChange);
        this.graph.on("edgeAdded", this.handleGraphChange);
        this.graph.on("nodeDropped", this.handleGraphChange);
        this.graph.on("edgeDropped", this.handleGraphChange);
        this.handleGraphChange();

        this.settings = helpers.assign(
            {},
            {
                linLogMode: false,
                outboundAttractionDistribution: false,
                adjustSizes: false,
                edgeWeightInfluence: 1,
                scalingRatio: 1,
                strongGravityMode: false,
                gravity: 1,
                slowDown: 1,
                barnesHutOptimize: false,
                barnesHutTheta: 0.5,
            },
            this.settings
        );
        let validationError = helpers.validateSettings(this.settings);
        if (validationError) {
            throw new Error(validationError);
        }
    }

    progress: HelvegEvent<ForceAtlas2Progress> = new HelvegEvent<ForceAtlas2Progress>(
        "helveg.ForceAtlas2Supervisor.progress"
    );
    started: HelvegEvent<boolean> = new HelvegEvent<boolean>("helveg.ForceAtlas2Supervisor.started");
    stopped: HelvegEvent<void> = new HelvegEvent<void>("helveg.ForceAtlas2Supervisor.stopped");
    updated: HelvegEvent<void> = new HelvegEvent<void>("helveg.ForceAtlas2Supervisor.updated");

    public get isRunning(): boolean {
        return this._running;
    }

    public get isInBackground(): boolean {
        return this._inBackground;
    }

    public start(inBackground: boolean, iterationCount?: number, settings?: ForceAtlas2Settings): void {
        if (this._running && this._inBackground === inBackground) {
            return;
        }

        if (!inBackground && iterationCount) {
            throw new Error("The iterationCount parameter is currently supported only in background mode.");
        }

        this.stop();

        // NB: last worker is only not null when the last stop operation is still in progress
        this._lastWorker?.terminate();
        this._lastWorker = null;

        this._inBackground = inBackground;
        this._running = true;
        this._lastPerformanceTime = performance.now();
        this.started.trigger(inBackground);

        if (!this._worker) {
            this._worker = this.spawnWorker();
        }

        this._worker.postMessage(
            {
                kind: MessageKind.Start,
                iterationCount: this.isInBackground ? iterationCount : 1,
                settings: { ...this.settings, ...settings },
                reportInterval: this.reportInterval,
                nodes: this._matrices.nodes.buffer,
            },
            {
                transfer: [this._matrices.nodes.buffer],
            }
        );
    }

    public kill(): void {
        this._lastWorker?.terminate();
        this._lastWorker = null;
        this._worker?.terminate();
        this._worker = null;
        this._running = false;
        this.detach();
    }

    public stop(): Promise<void> {
        if (!this._worker || !this._running) {
            this.logger?.debug("Worker is not running, nothing to stop.");
            return Promise.resolve();
        }

        this._lastWorker = this._worker;

        // NB: running = false needs to be set to prevent `update` from asking for more iterations.
        this._running = false;
        this._worker = null;
        let stopPromise = Promise.resolve();

        // when running in background, we fire-and-forget wait for the last update
        if (this._inBackground) {
            stopPromise = new Promise<void>((resolve, reject) => {
                let killTimeout = setTimeout(() => {
                    if (this._lastWorker) {
                        this._lastWorker.terminate();
                        this._lastWorker = null;
                        this.logger?.warn("Timed out while waiting for the worker to stop.");
                    }
                }, 2_000);
                this.updated.subscribeOnce(() => {
                    clearTimeout(killTimeout);
                    if (this._lastWorker) {
                        this._lastWorker.terminate();
                        this._lastWorker = null;
                        this.logger?.debug("Worker stopped.");
                    }
                });
                this.logger?.debug("Stopping the background worker.");
                this._lastWorker?.postMessage({
                    kind: MessageKind.Stop,
                });
            });
        } else {
            this._lastWorker.terminate();
            this._lastWorker = null;
            this.logger?.debug("Worker stopped.");
        }
        this.detach();
        this.stopped.trigger();
        return stopPromise;
    }

    private detach(): void {
        this.graph.removeListener("nodeAdded", this.handleGraphChange);
        this.graph.removeListener("edgeAdded", this.handleGraphChange);
        this.graph.removeListener("nodeDropped", this.handleGraphChange);
        this.graph.removeListener("edgeDropped", this.handleGraphChange);
    }

    private askForSingleIteration(settings?: ForceAtlas2Settings) {
        if (!this._worker) {
            console.warn("Worker is not initialized, cannot ask for single iteration.");
            return;
        }

        this._worker.postMessage(
            <StartMessage>{
                kind: MessageKind.Start,
                iterationCount: 1,
                settings: { ...this.settings, ...settings },
                nodes: this._matrices.nodes.buffer,
                reportInterval: this.reportInterval,
            },
            {
                transfer: [this._matrices.nodes.buffer],
            }
        );
    }

    private handleGraphChange() {
        if (this._worker) {
            this.detach();
            this.spawnWorker();
            if (this._running) {
                this.start(this._inBackground);
            }
        }
    }

    private handleMessage(event: MessageEvent) {
        let message = event.data as Message;
        switch (message.kind) {
            case MessageKind.Update:
                this.update(message as UpdateMessage);
                return;
            case MessageKind.Progress:
                let progressMessage = message as ProgressMessage;
                let newTime = performance.now();
                this.progress.trigger({
                    iterationCount: progressMessage.iterationCount,
                    iterationsPerSecond: this.reportInterval / ((newTime - this._lastPerformanceTime) / 1000.0),
                    globalSwinging: progressMessage.metadata.globalSwinging,
                    globalTraction: progressMessage.metadata.globalTraction,
                    averageSwinging: progressMessage.metadata.globalSwinging / this._matrices.nodeCount,
                    averageTraction: progressMessage.metadata.globalTraction / this._matrices.nodeCount,
                });
                this._lastPerformanceTime = newTime;
                return;
            case MessageKind.Stop:
                const stopMessage = message as StopMessage;
                if (stopMessage.reason === StopReason.FiniteIterationsDone && !this._inBackground && this._running) {
                    // continue running in the foreground, one iteration at a time
                    this.askForSingleIteration();
                    return;
                }

                // the worker has finished
                this.kill();
                this.stopped.trigger();
                this.logger?.debug("Worker stopped.");
                return;
            default:
                console.warn("Ignoring a message of unknown kind.");
                return;
        }
    }

    private update(message: UpdateMessage) {
        this._matrices.nodes = new Float32Array(message.nodes);
        assignLayoutChanges(this.graph, this._matrices.nodes, this._matrices.nodeIds, true);
        this.updated.trigger();
    }

    private spawnWorker(): Worker {
        var url = window.URL.createObjectURL(new Blob([forceAtlas2WorkerCode], { type: "text/javascript" }));
        let worker = new Worker(url);
        worker.onmessage = this.handleMessage.bind(this);

        this._matrices = graphToByteArrays(this.graph, createEdgeWeightGetter("weight").fromEntry);
        worker.postMessage(
            {
                kind: MessageKind.Init,
                edges: this._matrices.edges.buffer,
            },
            {
                transfer: [this._matrices.edges.buffer],
            }
        );

        return worker;
    }
}

const FLOATS_PER_NODE = 10;
const FLOATS_PER_EDGE = 3;

// Based on https://github.com/graphology/graphology/blob/master/src/layout-forceatlas2/helpers.js
function graphToByteArrays(
    graph: HelvegGraph,
    getEdgeWeight: EdgeMapper<number, Attributes, Attributes>
): GraphMatrices {
    const nodeIds = graph.filterNodes((_n, a) => !a.hidden);
    let order = nodeIds.length;
    let size = graph.filterEdges((e, a, s, t, sa, ta) => !sa.hidden && !ta.hidden).length;
    let index: Record<string, number> = {};
    let j: number;

    // NOTE: float32 could lead to issues if edge array needs to index large
    // number of nodes.
    let NodeMatrix = new Float32Array(order * FLOATS_PER_NODE);
    let EdgeMatrix = new Float32Array(size * FLOATS_PER_EDGE);

    // Iterate through nodes
    j = 0;
    for (const node of nodeIds) {
        const attr = graph.getNodeAttributes(node);
        // Node index
        index[node] = j;

        // Populating byte array
        NodeMatrix[j] = attr.x ?? 0;
        NodeMatrix[j + 1] = attr.y ?? 0;
        NodeMatrix[j + 2] = 0; // dx
        NodeMatrix[j + 3] = 0; // dy
        NodeMatrix[j + 4] = 0; // old_dx
        NodeMatrix[j + 5] = 0; // old_dy
        NodeMatrix[j + 6] = 1; // mass
        NodeMatrix[j + 7] = 1; // convergence
        NodeMatrix[j + 8] = attr.size || 1;
        NodeMatrix[j + 9] = attr.fixed ? 1 : 0;
        j += FLOATS_PER_NODE;
    }

    // Iterate through edges
    j = 0;
    graph.forEachEdge((edge, attr, source, target, sa, ta, u) => {
        let sj = index[source];
        let tj = index[target];

        if (sa.hidden === true || ta.hidden === true) {
            return;
        }

        let weight = getEdgeWeight(edge, attr, source, target, sa, ta, u);

        // Incrementing mass to be a node's weighted degree
        NodeMatrix[sj + 6] += weight;
        NodeMatrix[tj + 6] += weight;

        // Populating byte array
        EdgeMatrix[j] = sj;
        EdgeMatrix[j + 1] = tj;
        EdgeMatrix[j + 2] = weight;
        j += FLOATS_PER_EDGE;
    });

    return {
        nodeCount: order,
        nodes: NodeMatrix,
        nodeIds: nodeIds,
        edgeCount: size,
        edges: EdgeMatrix,
    };
}

// Based on https://github.com/graphology/graphology/blob/master/src/layout-forceatlas2/helpers.js
function assignLayoutChanges(
    graph: Graph,
    nodeMatrix: Float32Array,
    nodeIds: string[],
    isFixedVolatile: boolean = false
) {
    let i = 0;
    for (const node of nodeIds) {
        const attr = graph.getNodeAttributes(node);
        if (attr.hidden === true) {
            return attr;
        }

        if (!attr.fixed) {
            attr.x = nodeMatrix[i];
            attr.y = nodeMatrix[i + 1];
        } else if (isFixedVolatile) {
            nodeMatrix[i] = attr.x;
            nodeMatrix[i + 1] = attr.y;
        }

        i += FLOATS_PER_NODE;
    }
}

// Based on https://github.com/graphology/graphology/blob/master/src/layout-forceatlas2/helpers.js
function readGraphPositions(graph: Graph, nodeMatrix: Float32Array) {
    var i = 0;

    graph.forEachNode(function (_, attr) {
        nodeMatrix[i] = attr.x;
        nodeMatrix[i + 1] = attr.y;

        i += FLOATS_PER_NODE;
    });
}
