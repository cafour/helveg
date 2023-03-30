export interface Multigraph {
    id: string,
    label: string | null,
    nodes: MultigraphNode[],
    relations: Relation[]
}

export interface MultigraphNode {
    id: string,
    label: string | null,
    properties: Record<string, string>
}

export interface Relation {
    id: string,
    label: string | null,
    edges: MultigraphEdge[]
}

export interface MultigraphEdge {
    src: string,
    dst: string,
    label: string | null,
    properties: Record<string, string>
}

