export interface Multigraph {
    id: string,
    label: string | null,
    nodes: Record<string, GraphNode>
    relations: Record<string, GraphRelation>
}

export interface GraphNode {
    label: string | null,
    properties: Record<string, any>
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

