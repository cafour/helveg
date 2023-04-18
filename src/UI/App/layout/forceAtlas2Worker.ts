import iterate from "graphology-layout-forceatlas2/iterate";
import { MessageKind, type Message, type StartMessage, type StopMessage, type UpdateMessage, type InitMessage } from "./forceAtlas2Messages";

let edges: Float32Array;

self.onmessage = e => {
    let message = e.data as Message;
    switch(message.kind) {
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
    if (message.isSingleIteration) {
        console.log("iteration");
        let nodes = new Float32Array(message.nodes);
        iterate(message.settings, nodes, edges);
        self.postMessage({
            kind: MessageKind.Update,
            nodes: nodes.buffer
        }, {
            transfer: [nodes.buffer]
        });
    }
}

function stop(message: StopMessage) {
    
}

function report() {
    
}

