/**
 * Data produced and consumed by Helveg, a software visualization toolkit.
 */
export interface DataModel {
    /**
     * Metadata about the analyzer that produced this data set.
     */
    analyzer: AnalyzerMetadata;
    /**
     * The creation time of this document.
     */
    createdOn: Date;
    /**
     * The analyzed data in the form of a multigraph.
     */
    data?: Multigraph;
    /**
     * The name of the data set.
     */
    name: string;
    [property: string]: any;
}

/**
 * Metadata about the analyzer that produced this data set.
 */
export interface AnalyzerMetadata {
    /**
     * Name of the analyzer.
     */
    name: string;
    /**
     * Version of the analyzer.
     */
    version: string;
    [property: string]: any;
}

/**
 * The analyzed data in the form of a multigraph.
 */
export interface Multigraph {
    /**
     * The nodes of the multigraph.
     */
    nodes: { [key: string]: MultigraphNode };
    /**
     * The relations of the multigraph.
     */
    relations: { [key: string]: MultigraphRelation };
    [property: string]: any;
}

/**
 * A node of the multigraph.
 */
export interface MultigraphNode {
    /**
     * Diagnostics attached to the node.
     */
    diagnostics?: MultigraphDiagnostic[];
    /**
     * The kind of the node.
     */
    kind?: string;
    /**
     * Name of the entity this node represents.
     */
    name?: string;
    [property: string]: any;
}

/**
 * A diagnostic message (warning, error, etc.) regarding a node.
 */
export interface MultigraphDiagnostic {
    id:       string;
    message?: string;
    severity: MultigraphDiagnosticSeverity;
    [property: string]: any;
}

export type MultigraphDiagnosticSeverity = "hidden" | "info" | "warning" | "error";

/**
 * A relation of the multigraph.
 */
export interface MultigraphRelation {
    /**
     * The edges of the relation.
     */
    edges?: MultigraphEdge[];
    /**
     * Whether or not the relation is transitive. That is a -> b -> c => a -> c.
     */
    isTransitive?: boolean;
    /**
     * The name of the relation.
     */
    name: string;
    [property: string]: any;
}

/**
 * An edge of the relation.
 */
export interface MultigraphEdge {
    /**
     * The id of the destination node.
     */
    dst?: string;
    /**
     * The id of the source node.
     */
    src?: string;
    [property: string]: any;
}
