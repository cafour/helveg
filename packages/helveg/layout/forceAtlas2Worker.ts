import iterate from "graphology-layout-forceatlas2/iterate";
import { MessageKind, type Message, type StartMessage, type StopMessage, type UpdateMessage, type InitMessage, type ProgressMessage } from "./forceAtlas2Messages";
import type { ForceAtlas2Settings } from "graphology-layout-forceatlas2";

let iterationCount = 0;
let nodes: Float32Array;
let edges: Float32Array;
let stopRequested: boolean = false;

self.onmessage = e => {
    let message = e.data as Message;
    switch (message.kind) {
        case MessageKind.Init:
            init(message as InitMessage);
            return;
        case MessageKind.Start:
            start(message as StartMessage);
            return;
        case MessageKind.Stop:
            stop(message as StopMessage);
            return;
        default:
            console.warn("Ignoring a message of unknown kind.");
            return;
    }
};

function init(message: InitMessage) {
    edges = new Float32Array(message.edges);
}

function start(message: StartMessage) {
    nodes = new Float32Array(message.nodes);
    stopRequested = false;
    if (message.isSingleIteration) {
        iterate(message.settings, nodes, edges);
        update();
        iterationCount++;
        if ((iterationCount % message.reportInterval) === 0) {
            report();
        }
    }
    else {
        runContinously(message);
    }
}

function runContinously(message: StartMessage)
{
    var mc = new MessageChannel();
    mc.port1.onmessage = () => {
        if (stopRequested) {
            update();
            return;
        }
        iterate(message.settings, nodes, edges);
        iterationCount++;
        if ((iterationCount % message.reportInterval) === 0) {
            report();
        }
        mc.port2.postMessage("");
    };
    mc.port2.postMessage("");
}

function stop(message: StopMessage) {
    stopRequested = true;
}

function update() {
    self.postMessage(<UpdateMessage>{
        kind: MessageKind.Update,
        nodes: nodes.buffer
    }, {
        transfer: [nodes.buffer]
    });
}

function report() {
    self.postMessage(<ProgressMessage>{
        kind: MessageKind.Progress,
        iterationCount: iterationCount
    });
}

