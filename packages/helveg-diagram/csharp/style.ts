import { MultigraphNode } from "../model/data-model.ts";
import { EdgeStyle, FALLBACK_EDGE_STYLE, FireStatus, NodeStyle, NodeStylist, OutlineStyle, RelationStylist } from "../model/style";
import { CSharpNode, DefaultRelationColors, EntityKind, MemberAccessibility, MethodKind, Palette, Relations, TypeKind } from "./model";
import chroma from "chroma-js";

export const csharpNodeStylist: NodeStylist = (node: MultigraphNode) => {
    return { ...FALLBACK_NODE_STYLE, ...resolveNodeStyle(node as Partial<CSharpNode>) };
}

export const DEFAULT_CSHARP_RELATION_COLORS: Record<string, string> =
{
    [Relations.Declares]: DefaultRelationColors.Declares,
    [Relations.AssociatedWith]: DefaultRelationColors.AssociatedWith,
    [Relations.DependsOn]: DefaultRelationColors.DependsOn,
    [Relations.InheritsFrom]: DefaultRelationColors.InheritsFrom,
    [Relations.Overrides]: DefaultRelationColors.Overrides,
    [Relations.References]: DefaultRelationColors.References,
    [Relations.Returns]: DefaultRelationColors.Returns,
    [Relations.TypeOf]: DefaultRelationColors.TypeOf
};

export function createCsharpRelationStylist(colors: Record<string, string>): RelationStylist {
    return (r) => { return { ...FALLBACK_EDGE_STYLE, ...resolveRelationStyle(r, colors) } };
}

export const csharpRelationStylist: RelationStylist = createCsharpRelationStylist(DEFAULT_CSHARP_RELATION_COLORS);

const FALLBACK_NODE_STYLE: NodeStyle = {
    icon: "vscode:pie-chart",
    size: 5,
    color: Palette.Gray600,
    outlines: [],
    slices: { solid: 0, stroked: 0, width: 0 },
    fire: FireStatus.None
};

const ENTITY_KIND_STYLES = new Map<EntityKind, Partial<NodeStyle>>()
    .set(EntityKind.Solution, {
        icon: "helveg:Solution",
        size: 55,
        color: Palette.Purple300,
        backgroundColor: chroma(Palette.Purple300).brighten(1.5).desaturate(1).hex(),
    })
    .set(EntityKind.Project, {
        icon: "helveg:Project",
        size: 45,
        color: Palette.Purple200,
        backgroundColor: chroma(Palette.Purple200).brighten(1.5).desaturate(1).hex(),
    })
    .set(EntityKind.Framework, {
        icon: "helveg:Framework",
        size: 50,
        color: Palette.Red200,
        backgroundColor: chroma(Palette.Red200).brighten(2).hex(),
    })
    .set(EntityKind.ExternalDependencySource, {
        icon: "helveg:ExternalDependencySource",
        size: 50,
        color: Palette.Gray600,
        backgroundColor: chroma(Palette.Gray600).brighten(2).hex(),
    })
    .set(EntityKind.PackageRepository, {
        icon: "helveg:PackageRepository",
        size: 50,
        color: Palette.Green300,
        backgroundColor: chroma(Palette.Green300).brighten(2).hex(),
    })
    .set(EntityKind.Package, {
        icon: "helveg:Package",
        size: 45,
        color: Palette.Green200,
        backgroundColor: chroma(Palette.Green200).brighten(2).hex(),
    })
    .set(EntityKind.Library, {
        icon: "helveg:Library",
        size: 40,
        color: Palette.Gray500,
        backgroundColor: chroma(Palette.Gray500).brighten(2).hex(),
    })
    .set(EntityKind.Assembly, {
        icon: "helveg:Assembly",
        size: 40,
        color: Palette.Gray600,
        backgroundColor: chroma(Palette.Gray600).brighten(2).hex(),
    })
    .set(EntityKind.Module, {
        icon: "helveg:Module",
        size: 35,
        color: Palette.Gray500,
        backgroundColor: chroma(Palette.Gray500).brighten(2).hex(),
    })
    .set(EntityKind.Namespace, {
        icon: "helveg:Namespace",
        size: 30,
        color: Palette.Gray600,
        backgroundColor: chroma(Palette.Gray600).brighten(3).hex(),
    })
    .set(EntityKind.Type, {
        icon: "helveg:Type",
        size: 15,
        color: Palette.Beige300
    })
    .set(EntityKind.TypeParameter, {
        icon: "helveg:TypeParameter",
        size: 10,
        color: Palette.Beige600,
        backgroundColor: chroma(Palette.Beige600).brighten(2).desaturate(1).hex(),
    })
    .set(EntityKind.Field, {
        icon: "helveg:Field",
        size: 10,
        color: Palette.Blue100,
        backgroundColor: chroma(Palette.Blue100).brighten(1.5).desaturate(1).hex(),
    })
    .set(EntityKind.Property, {
        icon: "helveg:Property",
        size: 10,
        color: Palette.Green100,
        backgroundColor: chroma(Palette.Green100).brighten(1.5).desaturate(1).hex(),
    })
    .set(EntityKind.Method, {
        icon: "helveg:Method",
        size: 10,
        color: Palette.Purple100,
        backgroundColor: chroma(Palette.Purple100).brighten(1.5).desaturate(1).hex(),
    })
    .set(EntityKind.Event, {
        icon: "helveg:Event",
        size: 10,
        color: Palette.Red100,
        backgroundColor: chroma(Palette.Red100).brighten(1.5).desaturate(1).hex(),
    })
    .set(EntityKind.Parameter, {
        icon: "helveg:Parameter",
        size: 5,
        color: Palette.Pink100,
        backgroundColor: chroma(Palette.Pink100).brighten(1.5).desaturate(1).hex(),
    });


function resolveNodeStyle(node: Partial<CSharpNode>): Partial<NodeStyle> {
    let style: NodeStyle = { ...FALLBACK_NODE_STYLE, ...(node.kind && ENTITY_KIND_STYLES.get(node.kind)) };

    const hasErrors = (node.diagnostics ?? [])
        ?.filter(d => d.severity === "error")
        .length > 0;
    const hasWarnings = (node.diagnostics ?? [])
        ?.filter(d => d.severity === "warning")
        .length > 0;
    style.fire = hasErrors ? FireStatus.Flame
        : hasWarnings ? FireStatus.Smoke
            : FireStatus.None;

    switch (node.kind) {
        case EntityKind.Type:
            style.size = 15;
            switch (node.typeKind) {
                case TypeKind.Class:
                    style.icon = node.isRecord ? "helveg:RecordClass" : "helveg:Class";
                    style.color = Palette.Beige200;
                    break;
                case TypeKind.Interface:
                    style.icon = "helveg:Interface";
                    style.color = Palette.Beige100;
                    break;
                case TypeKind.Enum:
                    style.icon = "helveg:Enum";
                    style.color = Palette.Beige400;
                    break;
                case TypeKind.Struct:
                    style.icon = node.isRecord ? "helveg:RecordStruct" : "helveg:Struct";
                    style.color = Palette.Beige500;
                    break;
                case TypeKind.Delegate:
                    style.icon = "helveg:Delegate";
                    style.color = Palette.Beige300;
                    break;
            }
            const instanceMemberCount = node.instanceMemberCount ?? 0;
            const staticMemberCount = node.staticMemberCount ?? 0;
            style.outlines = [
                { style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 },
                { style: OutlineStyle.Solid, width: instanceMemberCount },
                { style: OutlineStyle.Dashed, width: staticMemberCount }
            ];
            style.slices = {
                solid: staticMemberCount,
                stroked: instanceMemberCount,
                width: staticMemberCount + instanceMemberCount
            };
            break;
        case EntityKind.TypeParameter:
            style.size = node.declaringKind === EntityKind.Method ? 5 : 10
            break;
        case EntityKind.Field:
            style.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            if (node.isEnumItem) {
                style.icon = "helveg:EnumItem";
            } else if (node.isConst) {
                style.icon = "helveg:Constant";
            }
            break;
        case EntityKind.Method:
            if (node.methodKind === MethodKind.BuiltinOperator
                || node.methodKind === MethodKind.UserDefinedOperator) {
                style.icon = "helveg:Operator";
            }
            else if (node.methodKind === MethodKind.Constructor) {
                style.icon = "helveg:Constructor";
            }
            else if (node.methodKind === MethodKind.Destructor) {
                style.icon = "helveg:Destructor";
            }


            style.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
        case EntityKind.Property:
            if (node.isIndexer) {
                style.icon = "helveg:Indexer";
            }

            style.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
        case EntityKind.Event:
            style.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
    }

    if (node.isStatic && (
        node.typeKind === "Class"
        || node.typeKind === "Struct"
        || (node.kind === "Field" && !node.isConst)
        || node.kind === "Property"
        || node.kind === "Event"
        || node.kind === "Method"
    )) {
        style.icon += "Static";
    }

    switch (node.accessibility) {
        case MemberAccessibility.Internal:
            style.icon += "Internal";
            break;
        case MemberAccessibility.Private:
            style.icon += "Private";
            break;
        case MemberAccessibility.Protected:
        case MemberAccessibility.ProtectedAndInternal:
        case MemberAccessibility.ProtectedOrInternal:
            style.icon += "Protected";
            break;
    }
    return style;
}

function resolveRelationStyle(relation: string, colors?: Record<string, string>): EdgeStyle {
    colors = { ...DEFAULT_CSHARP_RELATION_COLORS, ...colors };
    switch (relation) {
        case Relations.Declares:
            return {
                color: colors[Relations.Declares],
                width: 2,
                type: "arrow"
            };
        case Relations.InheritsFrom:
            return {
                color: colors[Relations.InheritsFrom],
                width: 4,
                type: "arrow"
            };
        case Relations.TypeOf:
            return {
                color: colors[Relations.TypeOf],
                width: 4,
                type: "arrow"
            };
        case Relations.Overrides:
            return {
                color: colors[Relations.Overrides],
                width: 4,
                type: "arrow"
            };
        case Relations.Returns:
            return {
                color: colors[Relations.Returns],
                width: 4,
                type: "arrow"
            };
        case Relations.DependsOn:
            return {
                color: colors[Relations.DependsOn],
                width: 6,
                type: "arrow"
            };
        case Relations.References:
            return {
                color: colors[Relations.References],
                width: 4,
                type: "arrow"
            };
        default:
            return FALLBACK_EDGE_STYLE;
    }
}
