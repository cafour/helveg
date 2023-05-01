export interface Multigraph {
    id: string,
    nodes: Record<string, Node>
    relations: Record<string, Relation>
}

export enum DiagnosticSeverity {
    Unknown = "Unknown",
    Hidden = "Hidden",
    Info = "Info",
    Warning = "Warning",
    Error = "Error"
}

export interface Diagnostic {
    id: string,
    message: string,
    severity: DiagnosticSeverity,
}

export interface NodeProperties {
    Label?: string,
    Style?: string,
    Diagnostics?: Diagnostic[]
}

export interface Node {
    properties: NodeProperties & Record<string, any>
}

export interface Relation {
    edges: Record<string, Edge>
}

export interface EdgeProperties {
    Label?: string,
    Style?: string
}

export interface Edge {
    src: string,
    dst: string,
    properties: EdgeProperties & Record<string, any>
}
