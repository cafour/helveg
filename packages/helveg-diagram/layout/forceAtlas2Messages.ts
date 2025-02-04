import { ForceAtlas2Metadata, type ForceAtlas2Options } from "./forceAtlas2Iterate.ts";

export enum MessageKind {
    Init = "init",
    Progress = "progress",
    Start = "start",
    Stop = "stop",
    Update = "update"
}

export enum StopReason {
    FiniteIterationsDone = "finiteIterationsDone",
    AutoStop = "autoStop",
}

export interface Message {
    kind: MessageKind;
}

export interface ProgressMessage extends Message {
    kind: MessageKind.Progress;
    iterationCount: number;
    speed: number;
    metadata: ForceAtlas2Metadata;
}

export interface InitMessage extends Message {
    kind: MessageKind.Init;
    edges: ArrayBufferLike;
}

export interface StartMessage extends Message {
    kind: MessageKind.Start;
    settings: ForceAtlas2Options;
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
    reason: StopReason;
}
