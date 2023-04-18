import { HelvegEvent } from "common/event";
import type { ProgressMessage } from "./forceAtlas2Messages";
import type Graph from "graphology";
import type { ForceAtlas2Settings } from "graphology-layout-forceatlas2";
import forceAtlas2WorkerCode from "inline-bundle:../layout/forceAtlas2Worker.ts";

export class ForceAtlas2Supervisor {

    constructor(private graph: Graph, private settings: ForceAtlas2Settings) {
        this.graph.on("nodeAdded", this.handleGraphUpdate);
        this.graph.on("edgeAdded", this.handleGraphUpdate);
        this.graph.on("nodeDropped", this.handleGraphUpdate);
        this.graph.on("edgeDropped", this.handleGraphUpdate);
    }

    progress: HelvegEvent<ProgressMessage> = new HelvegEvent<ProgressMessage>("ForceAtlas2Supervisor.progress");
    started: HelvegEvent<void> = new HelvegEvent<void>("ForceAtlas2Supervisor.started");
    stopped: HelvegEvent<void> = new HelvegEvent<void>("ForceAtlas2Supervisor.stopped");

    isRunning(): boolean {
        return false;
    }

    start(inBackground: boolean): void {

    }

    stop(): void {

    }

    kill(): void {

    }

    private handleGraphUpdate() {

    }

    private spawnWorker() {

    }
}
