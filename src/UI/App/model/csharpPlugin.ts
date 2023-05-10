import type { HelvegOptions } from "model/options";
import { expandNode, findRoots, type HelvegGraph } from "./graph";
import { DiagnosticSeverity, type Edge, type Node, type NodeProperties } from "./multigraph";
import type { HelvegPlugin } from "./plugin";
import { bfs } from "./traversal";
import type { VisualizationModel } from "./visualization";
import { FireStatus, OutlineStyle, type NodeStyle, type NodeStyleGenerator, type EdgeStyleGenerator, type EdgeStyle, FALLBACK_EDGE_STYLE } from "./style";
import type { UIExtension } from "./uiExtensions";
import { AppPanels } from "./const";
import CSharpGlyphsSubpanel from "components/CSharpGlyphsSubpanel.svelte";
import CSharpKindsSubpanel from "components/CSharpKindsSubpanel.svelte";

export enum EntityKind {
    Solution = "Solution",
    Project = "Project",
    ExternalDependencySource = "ExternalDependencySource",
    Framework = "Framework",
    PackageRepository = "PackageRepository",
    Package = "Package",
    Library = "Library",
    Assembly = "Assembly",
    Module = "Module",
    Namespace = "Namespace",
    Type = "Type",
    Field = "Field",
    Property = "Property",
    Event = "Event",
    Method = "Method",
    TypeParameter = "TypeParameter",
    Parameter = "Parameter"
}

export enum DefaultEntityKindIcons {
    Solution = "csharp:Solution",
    Project = "csharp:CSProjectNode",
    ExternalDependencySource = "csharp:ReferenceGroup",
    Framework = "csharp:Framework",
    PackageRepository = "csharp:NuGet",
    Package = "csharp:Package",
    Library = "csharp:Library",
    Assembly = "csharp:Assembly",
    Module = "csharp:Module",
    Namespace = "csharp:Namespace",
    Type = "csharp:Class",
    TypeParameter = "csharp:Type",
    Field = "csharp:Field",
    Method = "csharp:Method",
    Property = "csharp:Property",
    Event = "csharp:Event",
    Parameter = "csharp:LocalVariable",
}

export enum Relations {
    Declares = "declares",
    InheritsFrom = "inheritsFrom",
    TypeOf = "typeOf",
    Returns = "returns",
    Overrides = "overrides",
    AssociatedWith = "associatedWith",
    DependsOn = "dependsOn"
}

export enum DefaultRelationColors {
    Declares = "#dfdfdf",
    InheritsFrom = "#c3e5de",
    TypeOf = "#dcdcaa",
    Returns = "#d1c3e5",
    Overrides = "#e5c3c8",
    AssociatedWith = "#e5ccb7",
    DependsOn = "#c3d6e5",
}

enum MemberAccessibility {
    Invalid = "Invalid",
    Private = "Private",
    ProtectedAndInternal = "ProtectedAndInternal",
    Protected = "Protected",
    Internal = "Internal",
    ProtectedOrInternal = "ProtectedOrInternal",
    Public = "Public"
}

enum TypeKind {
    Unknown = "Unknown",
    Array = "Array",
    Class = "Class",
    Delegate = "Delegate",
    Dynamic = "Dynamic",
    Enum = "Enum",
    Error = "Error",
    Interface = "Interface",
    Module = "Module",
    Pointer = "Pointer",
    Struct = "Struct",
    TypeParameter = "TypeParameter",
    Submission = "Submission",
    FunctionPointer = "FunctionPointer"
}

enum MethodKind {
    Invalid = "Invalid",
    AnonymousFunction = "AnonymousFunction",
    Constructor = "Constructor",
    Conversion = "Conversion",
    DelegateInvoke = "DelegateInvoke",
    Destructor = "Destructor",
    EventAdd = "EventAdd",
    EventRaise = "EventRaise",
    EventRemove = "EventRemove",
    ExplicitInterfaceImplementation = "ExplicitInterfaceImplementation",
    UserDefinedOperator = "UserDefinedOperator",
    Ordinary = "Ordinary",
    PropertyGet = "PropertyGet",
    PropertySet = "PropertySet",
    ReducedExtension = "ReducedExtension",
    StaticConstructor = "StaticConstructor",
    BuiltinOperator = "BuiltinOperator",
    DeclareMethod = "DeclareMethod",
    LocalFunction = "LocalFunction",
    FunctionPointerSignature = "FunctionPointerSignature"
}

enum VSColor {
    DarkGray = "#212121",
    DarkPurple = "#68217a",
    Purple = "#6936aa",
    DarkYellow = "#996f00",
    Blue = "#005dba",
    NuGetBlue = "#004880"
}

export interface CSharpNodeProperties extends NodeProperties {
    Kind: EntityKind,
    TypeKind?: TypeKind,
    Accessibility?: MemberAccessibility,
    MethodKind?: MethodKind,
    IsConst?: boolean,
    IsEnumItem?: boolean,
    DeclaringKind?: EntityKind,
    InstanceMemberCount?: number,
    StaticMemberCount?: number,
    IsStatic?: boolean
}

const FALLBACK_STYLE: NodeStyle = {
    icon: "csharp:ExplodedDoughnutChart",
    color: VSColor.DarkGray,
    size: 5,
    outlines: [],
    fire: FireStatus.None
};

export interface CSharpDataOptions {
    includedKinds: string[];
    autoExpandedKinds: string[];
}

const DEFAULT_CSHARP_DATA_OPTIONS: CSharpDataOptions = {
    includedKinds: [
        EntityKind.Solution,
        EntityKind.Project,
        EntityKind.Namespace,
        EntityKind.Type,
        EntityKind.Field,
        EntityKind.Method,
        EntityKind.Property,
        EntityKind.Event
    ],
    autoExpandedKinds: [
        EntityKind.Solution,
        EntityKind.Project,
        EntityKind.Namespace
    ]
}

export enum CSharpGlyphSizingMode {
    Linear = "linear",
    Sqrt = "sqrt",
    Log = "log"
}

export interface CSharpGlyphOptions {
    sizingMode: CSharpGlyphSizingMode;
}

const DEFAULT_CSHARP_GLYPH_OPTIONS: CSharpGlyphOptions = {
    sizingMode: CSharpGlyphSizingMode.Linear
};

declare module "model/options" {
    export interface DataOptions {
        csharp?: CSharpDataOptions;
    }
    
    export interface GlyphOptions {
        csharp?: CSharpGlyphOptions;
    }
}

export default function csharp(options: HelvegOptions): CSharpPlugin {
    return new CSharpPlugin(options);
}

export class CSharpPlugin implements HelvegPlugin {
    name: string = "csharp";
    csharpDataOptions: CSharpDataOptions = { ...DEFAULT_CSHARP_DATA_OPTIONS };
    csharpGlyphOptions: CSharpGlyphOptions = { ...DEFAULT_CSHARP_GLYPH_OPTIONS };
    nodeStyles: Map<string, NodeStyleGenerator> = new Map();
    edgeStyles: Map<string, EdgeStyleGenerator> = new Map();
    uiExtensions: Map<string, UIExtension> = new Map();

    constructor(options: HelvegOptions) {
        let plugin = this;
        this.nodeStyles.set("Entity", (node: Node) => {
            let props = node.properties as CSharpNodeProperties;
            if (!(Object.values(EntityKind).includes(props.Kind))) {
                return FALLBACK_STYLE;
            }

            let base = plugin.resolveNodeStyle(props, this.csharpGlyphOptions);
            let fire = !props.Diagnostics ? FireStatus.None
                : props.Diagnostics.filter(d => d.severity === DiagnosticSeverity.Error).length > 0
                    ? FireStatus.Flame
                    : props.Diagnostics.filter(d => d.severity === DiagnosticSeverity.Warning).length > 0
                        ? FireStatus.Smoke
                        : FireStatus.None;
            return {
                ...FALLBACK_STYLE,
                ...base,
                fire
            };
        });
        this.edgeStyles.set("Relation", o => this.resolveEdgeStyle(o));
        
        this.uiExtensions.set("Glyphs", {
            targetPanel: AppPanels.Appearance,
            component: CSharpGlyphsSubpanel
        });
        this.uiExtensions.set("Kinds", {
            targetPanel: AppPanels.Data,
            component: CSharpKindsSubpanel
        });

        options.layout.tidyTree.relation ??= Relations.Declares;
        options.tool.cut.relation ??= Relations.Declares;
        options.tool.toggle.relation ??= Relations.Declares;
        options.data.selectedRelations.push(Relations.Declares);
        options.data.csharp = this.csharpDataOptions;
        options.glyph.csharp = this.csharpGlyphOptions;
    }

    onVisualize(model: Readonly<VisualizationModel>, graph: HelvegGraph): void {

        let includedNodes = new Set<string>();
        let excludedNodes = new Set<string>();

        // 1. split nodes into included and excluded
        Object.entries(model.multigraph.nodes)
            .forEach(([id, node]) => {
                if (this.csharpDataOptions.includedKinds.includes(node.properties.Kind)) {
                    includedNodes.add(id);
                }
                else {
                    excludedNodes.add(id);
                }
            });

        // 2. drop nodes that are not included but add transitive "declares" edges to keep the tree connected
        includedNodes.forEach(id => {
            // 2.1. find nodes that are reachable from the current node, stop at included nodes
            let reachableNodes = bfs(graph, id, {
                relation: Relations.Declares,
                callback: n => n === id || !includedNodes.has(n)
            });

            // 2.2 add the transitive edges and remove the unincluded nodes
            reachableNodes.forEach(child => {
                if (child === id) {
                    // obviously, the node doesn't declare itself
                    return;
                }

                if (includedNodes.has(child)) {
                    let edgeKey = `declares;${id};${child}`;
                    if (!graph.hasEdge(edgeKey)) {
                        graph.addDirectedEdgeWithKey(edgeKey, id, child, {
                            relation: Relations.Declares,
                            style: "csharp:Relation",
                            type: "arrow"
                        });
                    }
                }
                else {
                    graph.dropNode(child);
                }
            });
        });

        // 3. drop all remaining unincluded nodes (they should only exist if they were roots to begin with)
        excludedNodes.forEach(id => graph.hasNode(id) && graph.dropNode(id));

        // 4. collapse all nodes
        graph.forEachNode((node, attr) => {
            attr.collapsed = true;
            attr.hidden = true;
        });

        // 5. expand nodes that are auto-expanded, stop at first non-auto-expanded node
        let roots = findRoots(graph, Relations.Declares);
        for (let root of roots) {
            graph.setNodeAttribute(root, "hidden", false);
            bfs(graph, root, {
                relation: Relations.Declares,
                callback: n => {
                    let kind = model.multigraph.nodes[n].properties.Kind;
                    if (this.csharpDataOptions.autoExpandedKinds.includes(kind)) {
                        expandNode(graph, n, false, Relations.Declares);
                    }
                }
            })
        }
    }

    private resolveNodeStyle(
        props: CSharpNodeProperties,
        csharpGlyphOptions: CSharpGlyphOptions): Partial<NodeStyle> {

        let getSize = this.getSizingFunc(csharpGlyphOptions.sizingMode);
            
        let base: Partial<NodeStyle> = {};
        switch (props.Kind) {
            case EntityKind.Solution:
                return {
                    icon: "csharp:Solution",
                    size: 55,
                    color: VSColor.DarkPurple,
                    outlines: []
                };
            case EntityKind.Project:
                return {
                    icon: "csharp:CSProjectNode",
                    size: 45,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.Framework:
                return {
                    icon: "csharp:Framework",
                    size: 50,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.ExternalDependencySource:
                return {
                    icon: "csharp:ReferenceGroup",
                    size: 50,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.PackageRepository:
                return {
                    icon: "csharp:NuGet",
                    size: 50,
                    color: VSColor.NuGetBlue,
                    outlines: []
                };
            case EntityKind.Package:
                return {
                    icon: "csharp:Package",
                    size: 45,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.Library:
                return {
                    icon: "csharp:Library",
                    size: 40,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.Assembly:
                return {
                    icon: "csharp:Assembly",
                    size: 40,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.Module:
                return {
                    icon: "csharp:Module",
                    size: 35,
                    color: VSColor.Purple,
                    outlines: []
                };
            case EntityKind.Namespace:
                return {
                    icon: "csharp:Namespace",
                    size: 30,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.Type:
                base.size = 15;
                base.fire = FireStatus.Flame;
                switch (props.TypeKind) {
                    case TypeKind.Class:
                        base.icon = "csharp:Class";
                        base.color = VSColor.DarkYellow;
                        break;
                    case TypeKind.Interface:
                        base.icon = "csharp:Interface";
                        base.color = VSColor.Blue;
                        break;
                    case TypeKind.Enum:
                        base.icon = "csharp:Enumeration";
                        base.color = VSColor.DarkYellow;
                        break;
                    case TypeKind.Struct:
                        base.icon = "csharp:Structure";
                        base.color = VSColor.Blue;
                        break;
                    case TypeKind.Delegate:
                        base.icon = "csharp:Delegate";
                        base.color = VSColor.Purple;
                        break;
                    default:
                        base.icon = "csharp:Type";
                        base.color = VSColor.Blue;
                        break;
                }
                let instanceCount = props.InstanceMemberCount ?? 0;
                let staticCount = props.StaticMemberCount ?? 0;
                base.outlines = [
                    { style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 },
                    { style: OutlineStyle.Solid, width: getSize(instanceCount) },
                    { style: OutlineStyle.Dashed, width: getSize(staticCount) }
                ];
                break;
            case EntityKind.TypeParameter:
                return {
                    icon: "csharp:Type",
                    size: props.DeclaringKind === EntityKind.Method ? 5 : 10,
                    color: VSColor.Blue,
                    outlines: []
                };
            case EntityKind.Field:
                if (props.IsEnumItem) {
                    return {
                        icon: "csharp:EnumerationItem",
                        size: 10,
                        color: VSColor.Blue
                    }
                }

                base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
                base.size = 10;
                if (props.IsConst) {
                    base.icon = "csharp:Constant";
                    base.color = VSColor.DarkGray;
                } else {
                    base.icon = "csharp:Field";
                    base.color = VSColor.Blue;
                }
                break;
            case EntityKind.Method:
                if (props.MethodKind === MethodKind.BuiltinOperator
                    || props.MethodKind === MethodKind.UserDefinedOperator) {
                    base.icon = "csharp:Operator";
                    base.color = VSColor.Blue;
                }
                else {
                    base.icon = "csharp:Method";
                    base.color = VSColor.Purple;
                }

                base.size = 10;
                base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
                base.fire = FireStatus.Smoke;
                break;
            case EntityKind.Property:
                base.icon = "csharp:Property";
                base.size = 10;
                base.color = VSColor.DarkGray;
                base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
                break;
            case EntityKind.Event:
                base.icon = "csharp:Event";
                base.size = 10;
                base.color = VSColor.DarkYellow;
                base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
                break;
            case EntityKind.Parameter:
                return {
                    icon: "csharp:LocalVariable",
                    color: VSColor.Blue,
                    size: 5,
                    outlines: []
                };
            default:
                return {
                    icon: "csharp:ExplodedDoughnutChart",
                    size: 5,
                    color: VSColor.DarkGray,
                    outlines: []
                };
        }

        switch (props.Accessibility) {
            case MemberAccessibility.Internal:
                base.icon += "Internal";
                break;
            case MemberAccessibility.Private:
                base.icon += "Private";
                break;
            case MemberAccessibility.Protected:
            case MemberAccessibility.ProtectedAndInternal:
            case MemberAccessibility.ProtectedOrInternal:
                base.icon += "Protected";
                break;
        }
        return base;
    }

    private getSizingFunc(mode: CSharpGlyphSizingMode): (value: number) => number {
        switch (mode) {
            case CSharpGlyphSizingMode.Linear:
                return (value: number) => value;
            case CSharpGlyphSizingMode.Log:
                return (value: number) => Math.log(Math.max(1, value));
            case CSharpGlyphSizingMode.Sqrt:
                return (value: number) => Math.sqrt(value);
            default:
                return (_: number) => 1;
        }
    }
    
    private resolveEdgeStyle(object: { relation: string, edge: Edge }): EdgeStyle {
        switch (object.relation) {
            case Relations.Declares:
                return {
                    color: DefaultRelationColors.Declares,
                    width: 2
                };
            case Relations.InheritsFrom:
                return {
                    color: DefaultRelationColors.InheritsFrom,
                    width: 4
                };
            case Relations.TypeOf:
                return {
                    color: DefaultRelationColors.TypeOf,
                    width: 4
                };
            case Relations.Overrides:
                return {
                    color: DefaultRelationColors.Overrides,
                    width: 4
                };
            case Relations.Returns:
                return {
                    color: DefaultRelationColors.Returns,
                    width: 4
                };
            case Relations.DependsOn:
                return {
                    color: DefaultRelationColors.DependsOn,
                    width: 6
                };
            default:
                return FALLBACK_EDGE_STYLE;
        }
    }
}
