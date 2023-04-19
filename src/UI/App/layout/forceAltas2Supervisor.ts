import { HelvegEvent } from "common/event";
import { type StartMessage, type ProgressMessage, MessageKind, type UpdateMessage, type Message } from "./forceAtlas2Messages";
import type Graph from "graphology";
import { createEdgeWeightGetter } from "graphology-utils/getters";
import type { ForceAtlas2Settings } from "graphology-layout-forceatlas2";
import helpers from "graphology-layout-forceatlas2/helpers";
import forceAtlas2WorkerCode from "inline-bundle:../layout/forceAtlas2Worker.ts";

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
    private matrices = {
        nodes: new Float32Array(),
        edges: new Float32Array()
    };

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
        var validationError = helpers.validateSettings(this.settings);
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
        if (!this.worker) {
            return Promise.resolve();
        }

        let promise: Promise<void>;
        
        if (this.inBackground) {
            promise = new Promise<void>((resolve, reject) => {
                this.updated.subscribeOnce(() => {
                    resolve();
                });
                setTimeout(() => reject(new Error("Timeout while waiting for the worker to stop.")), 1000);
            });
        }
        else {
            promise = Promise.resolve();
        }

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
        helpers.assignLayoutChanges(this.graph, this.matrices.nodes, null);
        if (!this.inBackground && this.running) {
            this.askForSingleIteration();
        }
        this.updated.trigger();
    }

    private spawnWorker(): Worker {
        var url = window.URL.createObjectURL(new Blob([forceAtlas2WorkerCode], { type: "text/javascript" }));
        let worker = new Worker(url);
        worker.onmessage = this.handleMessage.bind(this);

        this.matrices = helpers.graphToByteArrays(this.graph, createEdgeWeightGetter("weight").fromEntry);
        worker.postMessage({
            kind: MessageKind.Init,
            edges: this.matrices.edges.buffer
        }, {
            transfer: [this.matrices.edges.buffer]
        })

        return worker;
    }
}
