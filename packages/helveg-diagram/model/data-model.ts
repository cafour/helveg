// To parse this data:
//
//   import { Convert, DataModel } from "./file";
//
//   const dataModel = Convert.toDataModel(json);
//
// These functions will throw an error if the JSON doesn't
// match the expected interface, even if the JSON is valid.

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

// Converts JSON strings to/from your types
// and asserts the results of JSON.parse at runtime
export class Convert {
    public static toDataModel(json: string): DataModel {
        return cast(JSON.parse(json), r("DataModel"));
    }

    public static dataModelToJson(value: DataModel): string {
        return JSON.stringify(uncast(value, r("DataModel")), null, 2);
    }
}

function invalidValue(typ: any, val: any, key: any, parent: any = ''): never {
    const prettyTyp = prettyTypeName(typ);
    const parentText = parent ? ` on ${parent}` : '';
    const keyText = key ? ` for key "${key}"` : '';
    throw Error(`Invalid value${keyText}${parentText}. Expected ${prettyTyp} but got ${JSON.stringify(val)}`);
}

function prettyTypeName(typ: any): string {
    if (Array.isArray(typ)) {
        if (typ.length === 2 && typ[0] === undefined) {
            return `an optional ${prettyTypeName(typ[1])}`;
        } else {
            return `one of [${typ.map(a => { return prettyTypeName(a); }).join(", ")}]`;
        }
    } else if (typeof typ === "object" && typ.literal !== undefined) {
        return typ.literal;
    } else {
        return typeof typ;
    }
}

function jsonToJSProps(typ: any): any {
    if (typ.jsonToJS === undefined) {
        const map: any = {};
        typ.props.forEach((p: any) => map[p.json] = { key: p.js, typ: p.typ });
        typ.jsonToJS = map;
    }
    return typ.jsonToJS;
}

function jsToJSONProps(typ: any): any {
    if (typ.jsToJSON === undefined) {
        const map: any = {};
        typ.props.forEach((p: any) => map[p.js] = { key: p.json, typ: p.typ });
        typ.jsToJSON = map;
    }
    return typ.jsToJSON;
}

function transform(val: any, typ: any, getProps: any, key: any = '', parent: any = ''): any {
    function transformPrimitive(typ: string, val: any): any {
        if (typeof typ === typeof val) return val;
        return invalidValue(typ, val, key, parent);
    }

    function transformUnion(typs: any[], val: any): any {
        // val must validate against one typ in typs
        const l = typs.length;
        for (let i = 0; i < l; i++) {
            const typ = typs[i];
            try {
                return transform(val, typ, getProps);
            } catch (_) {}
        }
        return invalidValue(typs, val, key, parent);
    }

    function transformEnum(cases: string[], val: any): any {
        if (cases.indexOf(val) !== -1) return val;
        return invalidValue(cases.map(a => { return l(a); }), val, key, parent);
    }

    function transformArray(typ: any, val: any): any {
        // val must be an array with no invalid elements
        if (!Array.isArray(val)) return invalidValue(l("array"), val, key, parent);
        return val.map(el => transform(el, typ, getProps));
    }

    function transformDate(val: any): any {
        if (val === null) {
            return null;
        }
        const d = new Date(val);
        if (isNaN(d.valueOf())) {
            return invalidValue(l("Date"), val, key, parent);
        }
        return d;
    }

    function transformObject(props: { [k: string]: any }, additional: any, val: any): any {
        if (val === null || typeof val !== "object" || Array.isArray(val)) {
            return invalidValue(l(ref || "object"), val, key, parent);
        }
        const result: any = {};
        Object.getOwnPropertyNames(props).forEach(key => {
            const prop = props[key];
            const v = Object.prototype.hasOwnProperty.call(val, key) ? val[key] : undefined;
            result[prop.key] = transform(v, prop.typ, getProps, key, ref);
        });
        Object.getOwnPropertyNames(val).forEach(key => {
            if (!Object.prototype.hasOwnProperty.call(props, key)) {
                result[key] = transform(val[key], additional, getProps, key, ref);
            }
        });
        return result;
    }

    if (typ === "any") return val;
    if (typ === null) {
        if (val === null) return val;
        return invalidValue(typ, val, key, parent);
    }
    if (typ === false) return invalidValue(typ, val, key, parent);
    let ref: any = undefined;
    while (typeof typ === "object" && typ.ref !== undefined) {
        ref = typ.ref;
        typ = typeMap[typ.ref];
    }
    if (Array.isArray(typ)) return transformEnum(typ, val);
    if (typeof typ === "object") {
        return typ.hasOwnProperty("unionMembers") ? transformUnion(typ.unionMembers, val)
            : typ.hasOwnProperty("arrayItems")    ? transformArray(typ.arrayItems, val)
            : typ.hasOwnProperty("props")         ? transformObject(getProps(typ), typ.additional, val)
            : invalidValue(typ, val, key, parent);
    }
    // Numbers can be parsed by Date but shouldn't be.
    if (typ === Date && typeof val !== "number") return transformDate(val);
    return transformPrimitive(typ, val);
}

function cast<T>(val: any, typ: any): T {
    return transform(val, typ, jsonToJSProps);
}

function uncast<T>(val: T, typ: any): any {
    return transform(val, typ, jsToJSONProps);
}

function l(typ: any) {
    return { literal: typ };
}

function a(typ: any) {
    return { arrayItems: typ };
}

function u(...typs: any[]) {
    return { unionMembers: typs };
}

function o(props: any[], additional: any) {
    return { props, additional };
}

function m(additional: any) {
    return { props: [], additional };
}

function r(name: string) {
    return { ref: name };
}

const typeMap: any = {
    "DataModel": o([
        { json: "analyzer", js: "analyzer", typ: r("AnalyzerMetadata") },
        { json: "createdOn", js: "createdOn", typ: Date },
        { json: "data", js: "data", typ: u(undefined, r("Multigraph")) },
        { json: "name", js: "name", typ: "" },
    ], "any"),
    "AnalyzerMetadata": o([
        { json: "name", js: "name", typ: "" },
        { json: "version", js: "version", typ: "" },
    ], "any"),
    "Multigraph": o([
        { json: "nodes", js: "nodes", typ: m(r("MultigraphNode")) },
        { json: "relations", js: "relations", typ: m(r("MultigraphRelation")) },
    ], "any"),
    "MultigraphNode": o([
        { json: "diagnostics", js: "diagnostics", typ: u(undefined, a(r("MultigraphDiagnostic"))) },
        { json: "kind", js: "kind", typ: u(undefined, "") },
        { json: "name", js: "name", typ: u(undefined, "") },
    ], "any"),
    "MultigraphDiagnostic": o([
        { json: "id", js: "id", typ: "" },
        { json: "message", js: "message", typ: u(undefined, "") },
        { json: "severity", js: "severity", typ: r("MultigraphDiagnosticSeverity") },
    ], "any"),
    "MultigraphRelation": o([
        { json: "edges", js: "edges", typ: u(undefined, a(r("MultigraphEdge"))) },
        { json: "isTransitive", js: "isTransitive", typ: u(undefined, true) },
        { json: "name", js: "name", typ: "" },
    ], "any"),
    "MultigraphEdge": o([
        { json: "dst", js: "dst", typ: u(undefined, "") },
        { json: "src", js: "src", typ: u(undefined, "") },
    ], "any"),
    "MultigraphDiagnosticSeverity": [
        "error",
        "hidden",
        "info",
        "warning",
    ],
};
