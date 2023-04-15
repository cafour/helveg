export interface Multigraph {
    id: string,
    label: string | null,
    nodes: Record<string, MultigraphNode>
    relations: Record<string, Relation>
}

export interface MultigraphNode {
    label: string | null,
    properties: Record<string, string>
}

export interface Relation {
    label: string | null,
    edges: MultigraphEdge[]
}

export interface MultigraphEdge {
    src: string,
    dst: string,
    label: string | null,
    properties: Record<string, string>
}

