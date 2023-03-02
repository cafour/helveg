export interface Multigraph {
    Id: string,
    Label: string | null,
    Nodes: MultigraphNode[],
    Relations: Relation[]
}

export interface MultigraphNode {
    Id: string,
    Label: string | null,
    Properties: MetadataProperty[]
}

export interface Relation {
    Id: string,
    Label: string | null,
    Edges: MultigraphEdge[]
}

export interface MultigraphEdge {
    Src: string,
    Dst: string,
    Label: string | null,
    Properties: MetadataProperty[]
}

export interface MetadataProperty {
    Key: string,
    Value: string
}
