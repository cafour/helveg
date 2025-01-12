import { HelvegEvent } from "../common/event.ts";
import { ForceAtlas2Settings, helpers } from "../deps/graphology-layout-forceatlas2.ts";
import { createEdgeWeightGetter } from "../deps/graphology-utils.ts";
import Graph, { Attributes, EdgeMapper } from "../deps/graphology.ts";
import { HelvegGraph } from "../global.ts";
import { ILogger } from "../model/logger.ts";
import { MessageKind, StartMessage, Message, UpdateMessage, ProgressMessage } from "./forceAtlas2Messages.ts";
import forceAtlas2WorkerCode from "inline-bundle:./forceAtlas2Worker.ts";

type GraphMatrices = { nodes: Float32Array, edges: Float32Array };

export interface ForceAtlas2Progress {
    iterationCount: number;

    /**
     * The number of iterations per second.
     */
    speed: number;
}

export class ForceAtlas2Supervisor {
    public reportInterval: number = 100;
    private _worker: Worker | null = null;
    private _running: boolean = false;
    private _inBackground: boolean = false;
    private _lastPerformanceTime: number = 0;
    private _matrices: GraphMatrices = { nodes: new Float32Array(0), edges: new Float32Array(0) };

    constructor(private graph: HelvegGraph, private settings: ForceAtlas2Settings, private logger?: ILogger) {
        this.graph.on("nodeAdded", this.handleGraphChange);
        this.graph.on("edgeAdded", this.handleGraphChange);
        this.graph.on("nodeDropped", this.handleGraphChange);
        this.graph.on("edgeDropped", this.handleGraphChange);
        this.handleGraphChange();

        this.settings = helpers.assign({}, {
            linLogMode: false,
            outboundAttractionDistribution: false,
            adjustSizes: false,
            edgeWeightInfluence: 1,
            scalingRatio: 1,
            strongGravityMode: false,
            gravity: 1,
            slowDown: 1,
            barnesHutOptimize: false,
            barnesHutTheta: 0.5
        }, this.settings);
        let validationError = helpers.validateSettings(this.settings);
        if (validationError) {
            throw new Error(validationError);
        }
    }

    progress: HelvegEvent<ForceAtlas2Progress>
        = new HelvegEvent<ForceAtlas2Progress>("helveg.ForceAtlas2Supervisor.progress");
    started: HelvegEvent<boolean>
        = new HelvegEvent<boolean>("helveg.ForceAtlas2Supervisor.started");
    stopped: HelvegEvent<void>
        = new HelvegEvent<void>("helveg.ForceAtlas2Supervisor.stopped");
    updated: HelvegEvent<void>
        = new HelvegEvent<void>("helveg.ForceAtlas2Supervisor.updated");

    get isRunning(): boolean {
        return this._running;
    }

    get isInBackground(): boolean {
        return this._inBackground;
    }

    async start(inBackground: boolean, iterationCount?: number, settings?: ForceAtlas2Settings): Promise<void> {
        if (this._running && this._inBackground === inBackground) {
            return;
        }

        if (!inBackground && iterationCount) {
            throw new Error("The iterationCount parameter is currently supported only in background mode.");
        }

        await this.stop();

        this._inBackground = inBackground;
        this._running = true;
        this._lastPerformanceTime = performance.now();
        this.started.trigger(inBackground);

        if (!this._worker) {
            this._worker = this.spawnWorker();
        }

        this._worker.postMessage({
            kind: MessageKind.Start,
            iterationCount: this.isInBackground ? iterationCount : 1,
            settings: { ...this.settings, ...settings },
            reportInterval: this.reportInterval,
            nodes: this._matrices.nodes.buffer,
        }, {
            transfer: [this._matrices.nodes.buffer]
        });
    }

    stop(): Promise<void> {
        if (!this._worker || !this._running) {
            this.logger?.debug("Worker is not running, nothing to stop.");
            return Promise.resolve();
        }

        let promise = new Promise<void>((resolve, reject) => {
            // NB: running = false needs to be set to prevent `update` from asking for more iterations.
            this._running = false;
            let killTimeout = setTimeout(() => {
                this.kill();
                this.logger?.warn("Timed out while waiting for the worker to stop.");
                this.stopped.trigger();
                resolve();
            }, 1000);
            this.updated.subscribeOnce(() => {
                clearTimeout(killTimeout);
                this.kill();

                this.stopped.trigger();

                this.logger?.debug("Worker stopped.");
                resolve();
            });
        });

        this.logger?.debug("Stopping the worker.");
        this._worker.postMessage({
            kind: MessageKind.Stop
        });

        return promise;
    }

    kill(): void {
        if (this._worker) {
            this._worker.terminate();
            this._worker = null;
        }
        this._running = false;

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

        this._worker.postMessage(<StartMessage>{
            kind: MessageKind.Start,
            iterationCount: 1,
            settings: { ...this.settings, ...settings },
            nodes: this._matrices.nodes.buffer,
            reportInterval: this.reportInterval
        }, {
            transfer: [this._matrices.nodes.buffer]
        });
    }

    private handleGraphChange() {
        if (this._worker) {
            this.kill();
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
                let iterationCount = (message as ProgressMessage).iterationCount;
                let newTime = performance.now();
                this.progress.trigger({
                    iterationCount: iterationCount,
                    speed: this.reportInterval / ((newTime - this._lastPerformanceTime) / 1000.0)
                });
                this._lastPerformanceTime = newTime;
                return;
            case MessageKind.Stop:
                if (this.isInBackground) {
                    // the worker has finished the assigned background iterations
                    this.kill();
                    this.stopped.trigger();
                    this.logger?.debug("Worker stopped.");
                } else if (this._running) {
                    // continue running in the foreground, one iteration at a time
                    this.askForSingleIteration();
                }
                return;
            default:
                console.warn("Ignoring a message of unknown kind.");
                return;
        }
    }

    private update(message: UpdateMessage) {
        this._matrices.nodes = new Float32Array(message.nodes);
        assignLayoutChanges(this.graph, this._matrices.nodes, null, true);
        this.updated.trigger();
    }

    private spawnWorker(): Worker {
        var url = window.URL.createObjectURL(new Blob([forceAtlas2WorkerCode], { type: "text/javascript" }));
        let worker = new Worker(url);
        worker.onmessage = this.handleMessage.bind(this);

        this._matrices = graphToByteArrays(this.graph, createEdgeWeightGetter("weight").fromEntry);
        worker.postMessage({
            kind: MessageKind.Init,
            edges: this._matrices.edges.buffer
        }, {
            transfer: [this._matrices.edges.buffer]
        })

        return worker;
    }
}

const FLOATS_PER_NODE = 10;
const FLOATS_PER_EDGE = 3;

// Based on https://github.com/graphology/graphology/blob/master/src/layout-forceatlas2/helpers.js
function graphToByteArrays(
    graph: HelvegGraph,
    getEdgeWeight: EdgeMapper<number, Attributes, Attributes>): GraphMatrices {

    let order = graph.filterNodes((n, a) => !a.hidden).length;
    let size = graph.filterEdges((e, a, s, t, sa, ta) => !sa.hidden && !ta.hidden).length;
    let index: Record<string, number> = {};
    let j: number;

    // NOTE: float32 could lead to issues if edge array needs to index large
    // number of nodes.
    let NodeMatrix = new Float32Array(order * FLOATS_PER_NODE);
    let EdgeMatrix = new Float32Array(size * FLOATS_PER_EDGE);

    // Iterate through nodes
    j = 0;
    graph.forEachNode((node, attr) => {
        // Completely ignore hidden nodes
        if (attr.hidden === true) {
            return;
        }

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
    });

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
        nodes: NodeMatrix,
        edges: EdgeMatrix
    };
};


// Based on https://github.com/graphology/graphology/blob/master/src/layout-forceatlas2/helpers.js
function assignLayoutChanges(
    graph: Graph,
    nodeMatrix: Float32Array,
    outputReducer: ((node: string, attr: Attributes) => Attributes) | null = null,
    isFixedVolatile: boolean = false) {
    var i = 0;

    graph.updateEachNodeAttributes((node, attr) => {
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

        return outputReducer ? outputReducer(node, attr) : attr;
    });
};

// Based on https://github.com/graphology/graphology/blob/master/src/layout-forceatlas2/helpers.js
function readGraphPositions(graph: Graph, nodeMatrix: Float32Array) {
    var i = 0;

    graph.forEachNode(function (_, attr) {
        nodeMatrix[i] = attr.x;
        nodeMatrix[i + 1] = attr.y;

        i += FLOATS_PER_NODE;
    });
};
