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

export interface Solution
{
    Path: string | null;
    Name: string | null;
    Projects: Record<string, Project> | null;
}

export interface Project
{
    Path: string | null;
    Name: string | null;
    LastWriteTime: string | null;
    Types: Record<string, Type> | null;
    ProjectReferences: string[] | null;
    PackageReferences: string[] | null;
}

export interface Type
{
    Id: string | null;
    Kind: TypeKind;
    Health: Diagnosis;
    Size: number;
    Relations: Record<string, number> | null;
    Family: number;
}

export enum TypeKind
{
    None,
    Class,
    Struct,
    Interface,
    Delegate,
    Enum,
}

export enum Diagnosis
{
    None = 0,
    Error = 1 << 0,
    Warning = 1 << 1,
}
