import { ForceAtlas2Settings } from "../deps/graphology-layout-forceatlas2.ts";

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
    iterationCount: number;
    speed: number;
}

export interface InitMessage extends Message {
    kind: MessageKind.Init;
    edges: ArrayBufferLike;
}

export interface StartMessage extends Message {
    kind: MessageKind.Start;
    settings: ForceAtlas2Settings;
    iterationCount?: number;
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
