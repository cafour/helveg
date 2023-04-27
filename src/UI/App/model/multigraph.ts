export interface Multigraph {
    id: string,
    label: string | null,
    nodes: Record<string, GraphNode>
    relations: Record<string, GraphRelation>
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
    Diagnostics?: Diagnostic[]
}

export interface GraphNode {
    label: string | null,
    properties: NodeProperties & Record<string, any>
}

export interface GraphRelation {
    label: string | null,
    edges: GraphEdge[]
}

export interface GraphEdge {
    src: string,
    dst: string,
    label: string | null,
    properties: Record<string, string>
}
