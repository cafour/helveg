import iterate from "graphology-layout-forceatlas2/iterate";
import { MessageKind, type Message, type StartMessage, type StopMessage, type UpdateMessage, type InitMessage, type ProgressMessage } from "./forceAtlas2Messages";

let globalIterationCount = 0;
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
    if (message.iterationCount !== undefined && message.iterationCount >= 0) {
        for(let i = 0; i < message.iterationCount; ++i) {
            iterate(message.settings, nodes, edges);
            if ((i % message.reportInterval) === 0) {
                report(i);
            }
        }
        update();
        postMessage(<StopMessage>{
            kind: MessageKind.Stop
        });
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
        globalIterationCount++;
        if ((globalIterationCount % message.reportInterval) === 0) {
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

function report(iterationCount?: number) {
    self.postMessage(<ProgressMessage>{
        kind: MessageKind.Progress,
        iterationCount: iterationCount ?? globalIterationCount
    });
}

