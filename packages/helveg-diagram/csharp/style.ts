import { MultigraphNode } from "../model/data-model.ts";
import {
    EdgeStyle,
    FALLBACK_EDGE_STYLE,
    FireStatus,
    NodeStyle,
    NodeStylist,
    OutlineStyle,
    RelationStylist,
} from "../model/style";
import {
    CSharpNode,
    DefaultRelationColors,
    EntityKind,
    LimitedTypeKind,
    MemberAccessibility,
    MethodKind,
    NodeColorSchema,
    Relations,
    TypeKind,
} from "./model";
import chroma from "chroma-js";

export enum Palette {
    Gray600 = "#202020",
    Gray500 = "#404040",
    Gray400 = "#606060",

    // i want hue: H=60-90, C=20-70, L=40-80, improve for colorblind, 6 colors, soft
    Beige600 = "#9a7b4c",
    Beige500 = "#a78368",
    Beige400 = "#b98448",
    Beige300 = "#bd8937",
    Beige200 = "#d3982d",
    Beige100 = "#d1ac78",

    // i want hue: H=90-60, C=20-85, L=40-70, improve for colorblind, 5 colors, soft
    // Red100 = "#d55d3a",
    Red100 = "#d55d3a", // overriden to match a darken version of the Explorer main accent color
    Green100 = "#6d964d",
    Blue100 = "#3198d7",
    Purple100 = "#926fcd",
    Pink100 = "#cc5e8b",

    // i want hue: H=0-360, C=20-70, L=10-60, improve for colorblind, 3 colors, soft
    // Red200 = "#a14644",
    Red200 = "#ac162c", // overriden to match the Explorer main accent color
    Green200 = "#5b713a",
    Purple200 = "#6f519c",

    // i want hue: H=0-360, C=20-70, L=10-40, improve for colorblind, 3 colors, soft
    // Red300 = "#7c2c30",
    Red300 = "#750003", // overriden to match the Explorer main accent color
    Green300 = "#3f4f21",
    Purple300 = "#503374",

    Red400 = "#470000",
}

export const DEFAULT_CSHARP_RELATION_COLORS: Record<string, string> = {
    [Relations.Declares]: DefaultRelationColors.Declares,
    [Relations.AssociatedWith]: DefaultRelationColors.AssociatedWith,
    [Relations.DependsOn]: DefaultRelationColors.DependsOn,
    [Relations.InheritsFrom]: DefaultRelationColors.InheritsFrom,
    [Relations.Overrides]: DefaultRelationColors.Overrides,
    [Relations.References]: DefaultRelationColors.References,
    [Relations.Returns]: DefaultRelationColors.Returns,
    [Relations.TypeOf]: DefaultRelationColors.TypeOf,
};

export function createCsharpRelationStylist(
    colors: Record<string, string>,
): RelationStylist {
    return (r) => {
        return { ...FALLBACK_EDGE_STYLE, ...resolveRelationStyle(r, colors) };
    };
}

export const csharpRelationStylist: RelationStylist =
    createCsharpRelationStylist(DEFAULT_CSHARP_RELATION_COLORS);

export const UNIVERSAL_NODE_COLOR_SCHEMA: Readonly<NodeColorSchema> = {
    entities: {
        [EntityKind.Solution]: {
            foreground: Palette.Red400,
            background: chroma(Palette.Red400).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.Project]: {
            foreground: Palette.Red300,
            background: chroma(Palette.Red300).brighten(2).desaturate(2).hex()
        },
        [EntityKind.ExternalDependencySource]: {
            foreground: Palette.Gray600,
            background: chroma(Palette.Gray600).brighten(2).hex(),
        },
        [EntityKind.Framework]: {
            foreground: Palette.Purple300,
            background: chroma(Palette.Purple300).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.PackageRepository]: {
            foreground: Palette.Green300,
            background: chroma(Palette.Green300).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.Package]: {
            foreground: Palette.Green200,
            background: chroma(Palette.Green200).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.Library]: {
            foreground: Palette.Gray500,
            background: chroma(Palette.Gray500).brighten(2).hex(),
        },
        [EntityKind.Assembly]: {
            foreground: Palette.Gray600,
            background: chroma(Palette.Gray600).brighten(2).hex(),
        },
        [EntityKind.Module]: {
            foreground: Palette.Gray500,
            background: chroma(Palette.Gray500).brighten(2).hex(),
        },
        [EntityKind.Namespace]: {
            foreground: Palette.Gray600,
            background: chroma(Palette.Gray600).brighten(3).hex(),
        },
        [EntityKind.Type]: {
            foreground: Palette.Beige300,
            background: chroma(Palette.Beige300).brighten(1).desaturate(1).hex(),
        },
        [EntityKind.Field]: {
            foreground: Palette.Blue100,
            background: chroma(Palette.Blue100).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.Property]: {
            foreground: Palette.Green100,
            background: chroma(Palette.Green100).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.Event]: {
            foreground: Palette.Red100,
            background: chroma(Palette.Red100).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.Method]: {
            foreground: Palette.Purple100,
            background: chroma(Palette.Purple100).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.TypeParameter]: {
            foreground: Palette.Beige600,
            background: chroma(Palette.Beige600).brighten(2).desaturate(1).hex(),
        },
        [EntityKind.Parameter]: {
            foreground: Palette.Pink100,
            background: chroma(Palette.Pink100).brighten(1.5).desaturate(1).hex(),
        },
    },
    types: {
        [TypeKind.Class]: {
            foreground: Palette.Beige200,
            background: chroma(Palette.Beige200).brighten(1).desaturate(1).hex(),
        },
        [TypeKind.Struct]: {
            foreground: Palette.Beige500,
            background: chroma(Palette.Beige500).brighten(1).hex(),
        },
        [TypeKind.Interface]: {
            foreground: Palette.Beige100,
            background: chroma(Palette.Beige100).brighten(0.5).desaturate(0.5).hex(),
        },
        [TypeKind.Enum]: {
            foreground: Palette.Beige400,
            background: chroma(Palette.Beige400).brighten(1).desaturate(1).hex(),
        },
        [TypeKind.Delegate]: {
            foreground: Palette.Beige300,
            background: chroma(Palette.Beige300).brighten(1).desaturate(1).hex(),
        },
    },
};

export function createCsharpNodeStylist(colorSchema: NodeColorSchema) {
    return (node: MultigraphNode) => {
        return {
            ...FALLBACK_NODE_STYLE,
            ...resolveNodeStyle(colorSchema, node as Partial<CSharpNode>),
        }
    };
}

export const csharpNodeStylist: NodeStylist = createCsharpNodeStylist(UNIVERSAL_NODE_COLOR_SCHEMA);

const FALLBACK_NODE_STYLE: NodeStyle = {
    icon: "vscode:pie-chart",
    size: 5,
    color: Palette.Gray600,
    outlines: [],
    slices: { solid: 0, stroked: 0, width: 0 },
    fire: FireStatus.None,
};

const PARTIAL_STYLES = new Map<EntityKind | TypeKind, Partial<NodeStyle>>()
    .set(EntityKind.Solution, {
        icon: "helveg:Solution",
        size: 55,
    })
    .set(EntityKind.Project, {
        icon: "helveg:Project",
        size: 45,
    })
    .set(EntityKind.Framework, {
        icon: "helveg:Framework",
        size: 50,
    })
    .set(EntityKind.ExternalDependencySource, {
        icon: "helveg:ExternalDependencySource",
        size: 50,
    })
    .set(EntityKind.PackageRepository, {
        icon: "helveg:PackageRepository",
        size: 50,
    })
    .set(EntityKind.Package, {
        icon: "helveg:Package",
        size: 45,
    })
    .set(EntityKind.Library, {
        icon: "helveg:Library",
        size: 40,
    })
    .set(EntityKind.Assembly, {
        icon: "helveg:Assembly",
        size: 40,
    })
    .set(EntityKind.Module, {
        icon: "helveg:Module",
        size: 35,
    })
    .set(EntityKind.Namespace, {
        icon: "helveg:Namespace",
        size: 30,
    })
    .set(EntityKind.Type, {
        icon: "helveg:Type",
        size: 15,
    })
    .set(TypeKind.Class, {
        icon: "helveg:Class",
        size: 15,
    })
    .set(TypeKind.Struct, {
        icon: "helveg:Struct",
        size: 15,
    })
    .set(TypeKind.Interface, {
        icon: "helveg:Interface",
        size: 15,
    })
    .set(TypeKind.Enum, {
        icon: "helveg:Enum",
        size: 15,
    })
    .set(TypeKind.Delegate, {
        icon: "helveg:Delegate",
        size: 15,
    })
    .set(EntityKind.TypeParameter, {
        icon: "helveg:TypeParameter",
        size: 10,
    })
    .set(EntityKind.Field, {
        icon: "helveg:Field",
        size: 10,
    })
    .set(EntityKind.Property, {
        icon: "helveg:Property",
        size: 10,
    })
    .set(EntityKind.Method, {
        icon: "helveg:Method",
        size: 10,
    })
    .set(EntityKind.Event, {
        icon: "helveg:Event",
        size: 10,
    })
    .set(EntityKind.Parameter, {
        icon: "helveg:Parameter",
        size: 5,
    });

function resolveNodeStyle(colorSchema: NodeColorSchema, node: Partial<CSharpNode>): Partial<NodeStyle> {
    let style: NodeStyle = {
        ...FALLBACK_NODE_STYLE,
        ...(node.kind && PARTIAL_STYLES.get(node.kind)),
    };

    if (node.kind && colorSchema.entities[node.kind]) {
        style.color = colorSchema.entities[node.kind].foreground;
        style.backgroundColor = colorSchema.entities[node.kind].background;
    }

    const hasErrors = (node.diagnostics ?? [])
        ?.filter((d) => d.severity === "error")
        .length > 0;
    const hasWarnings = (node.diagnostics ?? [])
        ?.filter((d) => d.severity === "warning")
        .length > 0;
    style.fire = hasErrors
        ? FireStatus.Flame
        : hasWarnings
            ? FireStatus.Smoke
            : FireStatus.None;

    switch (node.kind) {
        case EntityKind.Type:
            if (node.typeKind) {
                if (PARTIAL_STYLES.has(node.typeKind)) {
                    Object.assign(style, PARTIAL_STYLES.get(node.typeKind));
                }

                const typeKind = node.typeKind as unknown as LimitedTypeKind;
                if (colorSchema.types[typeKind]) {
                    style.color = colorSchema.types[typeKind].foreground;
                    style.backgroundColor = colorSchema.types[typeKind].background;
                }
            }

            switch (node.typeKind) {
                case TypeKind.Class:
                    if (node.isRecord) {
                        style.icon = "helveg:RecordClass";
                    }
                    break;
                case TypeKind.Struct:
                    if (node.isRecord) {
                        style.icon = "helveg:RecordStruct";
                    }
                    break;
            }
            const instanceMemberCount = node.instanceMemberCount ?? 0;
            const staticMemberCount = node.staticMemberCount ?? 0;
            style.outlines = [
                {
                    style: node.isStatic
                        ? OutlineStyle.Dashed
                        : OutlineStyle.Solid,
                    width: 2,
                },
                { style: OutlineStyle.Solid, width: instanceMemberCount },
                { style: OutlineStyle.Dashed, width: staticMemberCount },
            ];
            style.slices = {
                solid: staticMemberCount,
                stroked: instanceMemberCount,
                width: staticMemberCount + instanceMemberCount,
            };
            break;
        case EntityKind.TypeParameter:
            style.size = node.declaringKind === EntityKind.Method ? 5 : 10;
            break;
        case EntityKind.Field:
            style.outlines = [{
                style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid,
                width: 2,
            }];
            if (node.isEnumItem) {
                style.icon = "helveg:EnumItem";
            } else if (node.isConst) {
                style.icon = "helveg:Constant";
            }
            break;
        case EntityKind.Method:
            if (node.methodKind === MethodKind.BuiltinOperator ||
                node.methodKind === MethodKind.UserDefinedOperator
            ) {
                style.icon = "helveg:Operator";
            } else if (node.methodKind === MethodKind.Constructor) {
                style.icon = "helveg:Constructor";
            } else if (node.methodKind === MethodKind.Destructor) {
                style.icon = "helveg:Destructor";
            }

            style.outlines = [{
                style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid,
                width: 2,
            }];
            break;
        case EntityKind.Property:
            if (node.isIndexer) {
                style.icon = "helveg:Indexer";
            }

            style.outlines = [{
                style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid,
                width: 2,
            }];
            break;
        case EntityKind.Event:
            style.outlines = [{
                style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid,
                width: 2,
            }];
            break;
    }

    if (
        node.isStatic && (
            node.typeKind === "Class" ||
            node.typeKind === "Struct" ||
            (node.kind === "Field" && !node.isConst) ||
            node.kind === "Property" ||
            node.kind === "Event" ||
            node.kind === "Method"
        )
    ) {
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

function resolveRelationStyle(
    relation: string,
    colors?: Record<string, string>,
): EdgeStyle {
    colors = { ...DEFAULT_CSHARP_RELATION_COLORS, ...colors };
    switch (relation) {
        case Relations.Declares:
            return {
                color: colors[Relations.Declares],
                width: 2,
                type: "arrow",
            };
        case Relations.InheritsFrom:
            return {
                color: colors[Relations.InheritsFrom],
                width: 4,
                type: "arrow",
            };
        case Relations.TypeOf:
            return {
                color: colors[Relations.TypeOf],
                width: 4,
                type: "arrow",
            };
        case Relations.Overrides:
            return {
                color: colors[Relations.Overrides],
                width: 4,
                type: "arrow",
            };
        case Relations.Returns:
            return {
                color: colors[Relations.Returns],
                width: 4,
                type: "arrow",
            };
        case Relations.DependsOn:
            return {
                color: colors[Relations.DependsOn],
                width: 6,
                type: "arrow",
            };
        case Relations.References:
            return {
                color: colors[Relations.References],
                width: 4,
                type: "arrow",
            };
        default:
            return FALLBACK_EDGE_STYLE;
    }
}
