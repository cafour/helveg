import iterate, { ForceAtlas2Metadata, PPN } from "./forceAtlas2Iterate.ts";
import {
    MessageKind,
    type Message,
    type StartMessage,
    type StopMessage,
    type UpdateMessage,
    type InitMessage,
    type ProgressMessage,
    StopReason,
} from "./forceAtlas2Messages";

let globalIterationCount = 0;
let nodes: Float32Array;
let edges: Float32Array;
let stopRequested: boolean = false;
let metadata: ForceAtlas2Metadata = {
    globalSwinging: 0,
    globalTraction: 0,
};

self.onmessage = (e) => {
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
        for (let i = 0; i < message.iterationCount; ++i) {
            metadata = iterate(message.settings, nodes, edges);
            globalIterationCount++;
            if (globalIterationCount % message.reportInterval === 0) {
                report();
            }

            if (
                message.settings.autoStopAverageTraction > 0 &&
                metadata.globalTraction / (nodes.length / PPN) < message.settings.autoStopAverageTraction
            ) {
                stop({ kind: MessageKind.Stop, reason: StopReason.AutoStop });
                return;
            }
        }
        stop(
            {
                kind: MessageKind.Stop,
                reason: StopReason.FiniteIterationsDone,
            },
            false
        );
    } else {
        runContinously(message);
    }
}

function runContinously(message: StartMessage) {
    var mc = new MessageChannel();
    mc.port1.onmessage = () => {
        if (stopRequested) {
            return;
        }
        metadata = iterate(message.settings, nodes, edges);
        globalIterationCount++;
        if (globalIterationCount % message.reportInterval === 0) {
            report();
        }

        if (
            message.settings.autoStopAverageTraction > 0 &&
            metadata.globalTraction / (nodes.length / PPN) < message.settings.autoStopAverageTraction
        ) {
            stop({ kind: MessageKind.Stop, reason: StopReason.AutoStop });
        }
        mc.port2.postMessage("");
    };
    mc.port2.postMessage("");
}

function stop(message: StopMessage, shouldReport = true) {
    stopRequested = true;
    if (shouldReport) {
        report();
    }
    update();
    postMessage(message);
}

function update() {
    self.postMessage(
        <UpdateMessage>{
            kind: MessageKind.Update,
            nodes: nodes.buffer,
        },
        {
            transfer: [nodes.buffer],
        }
    );
}

function report() {
    self.postMessage(<ProgressMessage>{
        kind: MessageKind.Progress,
        iterationCount: globalIterationCount,
        metadata: metadata,
    });
}
