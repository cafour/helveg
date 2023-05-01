import { HelvegEvent } from "common/event";
import { type StartMessage, type ProgressMessage, MessageKind, type UpdateMessage, type Message } from "./forceAtlas2Messages";
import type Graph from "graphology";
import { createEdgeWeightGetter } from "graphology-utils/getters";
import type { ForceAtlas2Settings } from "graphology-layout-forceatlas2";
import helpers from "graphology-layout-forceatlas2/helpers";
import forceAtlas2WorkerCode from "inline-bundle:../layout/forceAtlas2Worker.ts";
import type { Attributes, EdgeMapper } from "graphology-types";
import type { HelvegGraph } from "model/graph";

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
    private worker: Worker | null = null;
    private running: boolean = false;
    private inBackground: boolean = false;
    private lastPerformanceTime: number = 0;
    private matrices: GraphMatrices = { nodes: new Float32Array(0), edges: new Float32Array(0) };

    constructor(private graph: Graph, private settings: ForceAtlas2Settings) {
        this.graph.on("nodeAdded", this.handleGraphUpdate);
        this.graph.on("edgeAdded", this.handleGraphUpdate);
        this.graph.on("nodeDropped", this.handleGraphUpdate);
        this.graph.on("edgeDropped", this.handleGraphUpdate);
        this.handleGraphUpdate();

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
        return this.running;
    }

    get isInBackground(): boolean {
        return this.inBackground;
    }

    async start(inBackground: boolean): Promise<void> {
        if (!this.worker) {
            this.worker = this.spawnWorker();
        }

        if (this.running && this.inBackground === inBackground) {
            return;
        }

        if (this.running) {
            await this.stop();
        }

        this.inBackground = inBackground;
        this.running = true;
        this.lastPerformanceTime = performance.now();
        this.started.trigger(inBackground);
        this.worker.postMessage({
            kind: MessageKind.Start,
            isSingleIteration: !inBackground,
            settings: this.settings,
            reportInterval: this.reportInterval,
            nodes: this.matrices.nodes.buffer,
        }, {
            transfer: [this.matrices.nodes.buffer]
        });
    }

    stop(): Promise<void> {
        if (!this.worker || !this.running) {
            return Promise.resolve();
        }

        let promise = new Promise<void>((resolve, reject) => {
            this.updated.subscribeOnce(() => {
                resolve();
            });
            setTimeout(() => reject(new Error("Timeout while waiting for the worker to stop.")), 1000);
        });

        this.worker.postMessage({
            kind: MessageKind.Stop
        });
        this.running = false;
        this.stopped.trigger();

        return promise;
    }

    kill(): void {
        if (this.worker) {
            this.worker.terminate();
            this.worker = null;
        }
        this.running = false;

        this.graph.removeListener("nodeAdded", this.handleGraphUpdate);
        this.graph.removeListener("edgeAdded", this.handleGraphUpdate);
        this.graph.removeListener("nodeDropped", this.handleGraphUpdate);
        this.graph.removeListener("edgeDropped", this.handleGraphUpdate);
    }

    private askForSingleIteration() {
        if (!this.worker) {
            console.warn("Worker is not initialized, cannot ask for single iteration.");
            return;
        }

        this.worker.postMessage(<StartMessage>{
            kind: MessageKind.Start,
            isSingleIteration: true,
            settings: this.settings,
            nodes: this.matrices.nodes.buffer,
            reportInterval: this.reportInterval
        }, {
            transfer: [this.matrices.nodes.buffer]
        });
    }

    private handleGraphUpdate() {
        if (this.worker) {
            this.kill();
            this.spawnWorker();
            if (this.running) {
                this.start(this.inBackground);
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
                    speed: this.reportInterval / ((newTime - this.lastPerformanceTime) / 1000.0)
                });
                this.lastPerformanceTime = newTime;
                return;
            default:
                console.warn("Ignoring a message of unknown kind.");
                return;
        }
    }

    private update(message: UpdateMessage) {
        this.matrices.nodes = new Float32Array(message.nodes);
        assignLayoutChanges(this.graph, this.matrices.nodes, null, true);
        if (!this.inBackground && this.running) {
            this.askForSingleIteration();
        }
        this.updated.trigger();
    }

    private spawnWorker(): Worker {
        var url = window.URL.createObjectURL(new Blob([forceAtlas2WorkerCode], { type: "text/javascript" }));
        let worker = new Worker(url);
        worker.onmessage = this.handleMessage.bind(this);

        this.matrices = graphToByteArrays(this.graph, createEdgeWeightGetter("weight").fromEntry);
        worker.postMessage({
            kind: MessageKind.Init,
            edges: this.matrices.edges.buffer
        }, {
            transfer: [this.matrices.edges.buffer]
        })

        return worker;
    }
}

const FLOATS_PER_NODE = 10;
const FLOATS_PER_EDGE = 3;

// Based on https://github.com/graphology/graphology/blob/master/src/layout-forceatlas2/helpers.js
function graphToByteArrays(
    graph: Graph,
    getEdgeWeight: EdgeMapper<number, Attributes, Attributes>): GraphMatrices {

    let order = graph.order;
    let size = graph.size;
    let index = {};
    let j;

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
        NodeMatrix[j] = attr.x;
        NodeMatrix[j + 1] = attr.y;
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
