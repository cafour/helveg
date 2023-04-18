import { HelvegEvent } from "common/event";
import { type StartMessage, type ProgressMessage, MessageKind, type UpdateMessage, type Message } from "./forceAtlas2Messages";
import type Graph from "graphology";
import { createEdgeWeightGetter } from "graphology-utils/getters";
import type { ForceAtlas2Settings } from "graphology-layout-forceatlas2";
import helpers from "graphology-layout-forceatlas2/helpers";
import forceAtlas2WorkerCode from "inline-bundle:../layout/forceAtlas2Worker.ts";

export class ForceAtlas2Supervisor {
    private worker: Worker | null = null;
    private running: boolean = false;
    private inBackground: boolean = false;
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

    progress: HelvegEvent<ProgressMessage> = new HelvegEvent<ProgressMessage>("helveg.ForceAtlas2Supervisor.progress");
    started: HelvegEvent<boolean> = new HelvegEvent<boolean>("helveg.ForceAtlas2Supervisor.started");
    stopped: HelvegEvent<void> = new HelvegEvent<void>("helveg.ForceAtlas2Supervisor.stopped");

    get isRunning(): boolean {
        return this.running;
    }

    get isInBackground(): boolean {
        return this.inBackground;
    }

    start(inBackground: boolean): void {
        if (!this.worker) {
            this.worker = this.spawnWorker();
        }

        this.inBackground = inBackground;
        this.running = true;
        this.started.trigger(inBackground);
        this.worker.postMessage({
            kind: MessageKind.Start,
            isSingleIteration: !inBackground,
            settings: this.settings,
            nodes: this.matrices.nodes.buffer,
        }, {
            transfer: [this.matrices.nodes.buffer]
        });
    }

    stop(): void {
        if (!this.worker) {
            return;
        }

        this.worker.postMessage({
            kind: MessageKind.Stop
        });
        this.running = false;
        this.stopped.trigger();
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

        this.worker.postMessage({
            kind: MessageKind.Start,
            isSingleIteration: true,
            settings: this.settings,
            nodes: this.matrices.nodes.buffer,
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
