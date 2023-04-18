import type { ForceAtlas2Settings } from "graphology-layout-forceatlas2";

export enum MessageKind {
    Init = "init",
    Progress = "progress",
    Start = "start",
    Stop = "stop",
    Update = "update"
}

export interface Message {
    kind: MessageKind;
}

export interface ProgressMessage extends Message {
    kind: MessageKind.Progress;
    iterations: number;
    speed: number;
}

export interface InitMessage extends Message {
    kind: MessageKind.Init;
    edges: ArrayBufferLike;
}

export interface StartMessage extends Message {
    kind: MessageKind.Start;
    settings: ForceAtlas2Settings;
    isSingleIteration: boolean;
    reportInterval: number;
    nodes: ArrayBufferLike;
}

export interface UpdateMessage extends Message {
    kind: MessageKind.Update;
    nodes: ArrayBufferLike;
}

export interface StopMessage extends Message {
    kind: MessageKind.Stop;
}
