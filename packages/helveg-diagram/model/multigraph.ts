export interface Multigraph {
    id: string,
    nodes: Record<string, MultigraphNode>
    relations: Record<string, MultigraphRelation>
}

export type MultigraphDiagnosticSeverity = "unknown" | "hidden" | "info" | "warning" | "error";

export interface MultigraphDiagnostic {
    id: string,
    message: string,
    severity: MultigraphDiagnosticSeverity,
}

export type MultigraphNode = Record<string, unknown | undefined> & {
    label?: string;
    kind?: string;
    diagnostics?: MultigraphDiagnostic[];
}

export interface MultigraphRelation {
    edges: Record<string, MultigraphEdge>,
    isTransitive?: boolean;
}

export interface MultigraphEdgeProperties {
    Label?: string,
    Style?: string
}

export type MultigraphEdge = Record<string, unknown | undefined> & {
    src: string;
    dst: string;
}
