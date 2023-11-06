export interface Multigraph {
    id: string,
    nodes: Record<string, MultigraphNode>
    relations: Record<string, MultigraphRelation>
}

export enum MultigraphDiagnosticSeverity {
    Unknown = "Unknown",
    Hidden = "Hidden",
    Info = "Info",
    Warning = "Warning",
    Error = "Error"
}

export interface MultigraphDiagnostic {
    id: string,
    message: string,
    severity: MultigraphDiagnosticSeverity,
}

export interface MultigraphNodeProperties {
    Label?: string,
    Style?: string,
    Diagnostics?: MultigraphDiagnostic[]
}

export interface MultigraphNode {
    properties: MultigraphNodeProperties & Record<string, any>
}

export interface MultigraphRelation {
    edges: Record<string, MultigraphEdge>
}

export interface MultigraphEdgeProperties {
    Label?: string,
    Style?: string
}

export interface MultigraphEdge {
    src: string,
    dst: string,
    properties: MultigraphEdgeProperties & Record<string, any>
}
