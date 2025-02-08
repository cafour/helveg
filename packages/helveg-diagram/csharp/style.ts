import { HelvegNodeAttributes } from "../global.ts";
import { MultigraphNode } from "../model/data-model.ts";
import {
    Contour,
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

export enum VSColor {
    DarkGray = "#212121",
    DarkPurple = "#68217a",
    Purple = "#6936aa",
    DarkYellow = "#996f00",
    Blue = "#005dba",
    NuGetBlue = "#004880",
    Green = "#1f801f",
}

export type RelationColorSchema = Record<Relations, string>;

export const DEFAULT_CSHARP_RELATION_COLORS: RelationColorSchema = {
    [Relations.Declares]: DefaultRelationColors.Declares,
    [Relations.AssociatedWith]: DefaultRelationColors.AssociatedWith,
    [Relations.DependsOn]: DefaultRelationColors.DependsOn,
    [Relations.InheritsFrom]: DefaultRelationColors.InheritsFrom,
    [Relations.Overrides]: DefaultRelationColors.Overrides,
    [Relations.References]: DefaultRelationColors.References,
    [Relations.Returns]: DefaultRelationColors.Returns,
    [Relations.TypeOf]: DefaultRelationColors.TypeOf,
};

export const CSHARP_RELATION_STYLIST: RelationStylist<RelationColorSchema> = (relation, schema) => {
    return {...FALLBACK_EDGE_STYLE, ...resolveRelationStyle(relation, schema)};
}

export const UNIVERSAL_NODE_COLOR_SCHEMA: Readonly<NodeColorSchema> = {
    entities: {
        [EntityKind.Solution]: {
            foreground: Palette.Red400,
            background: chroma(Palette.Red400).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.Project]: {
            foreground: Palette.Red300,
            background: chroma(Palette.Red300).brighten(2).desaturate(2).hex(),
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
            foreground: chroma(Palette.Blue100).darken(0.5).hex(),
            background: chroma(Palette.Blue100).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.Property]: {
            foreground: chroma(Palette.Green100).darken(0.5).hex(),
            background: chroma(Palette.Green100).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.Event]: {
            foreground: Palette.Red100,
            background: chroma(Palette.Red100).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.Method]: {
            foreground: chroma(Palette.Purple100).darken(0.5).hex(),
            background: chroma(Palette.Purple100).brighten(1.5).desaturate(1).hex(),
        },
        [EntityKind.TypeParameter]: {
            foreground: Palette.Beige600,
            background: chroma(Palette.Beige600).brighten(2).desaturate(1).hex(),
        },
        [EntityKind.Parameter]: {
            foreground: Palette.Pink100,
            background: chroma(Palette.Pink100).brighten(1.5).desaturate(1.5).hex(),
        },
    },
    types: {
        [TypeKind.Class]: {
            foreground: Palette.Beige200,
            background: chroma(Palette.Beige200).brighten(1).desaturate(2).hex(),
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

export const TYPE_FOCUS_COLOR_SCHEMA: Readonly<NodeColorSchema> = {
    entities: {
        ...UNIVERSAL_NODE_COLOR_SCHEMA.entities,
        [EntityKind.Solution]: {
            foreground: chroma(Palette.Red400).brighten(1).desaturate(1.5).hex(),
            background: chroma(Palette.Red400).brighten(3).desaturate(1.5).hex(),
        },
        [EntityKind.Project]: {
            foreground: chroma(Palette.Red300).brighten(1).desaturate(2).hex(),
            background: chroma(Palette.Red300).brighten(3).desaturate(2).hex(),
        },
        [EntityKind.Field]: {
            foreground: Palette.Gray500,
            background: chroma(Palette.Gray500).brighten(3).desaturate(1).hex(),
        },
        [EntityKind.Property]: {
            foreground: Palette.Gray500,
            background: chroma(Palette.Gray500).brighten(3).desaturate(1).hex(),
        },
        [EntityKind.Event]: {
            foreground: Palette.Gray500,
            background: chroma(Palette.Gray500).brighten(3).desaturate(1).hex(),
        },
        [EntityKind.Method]: {
            foreground: Palette.Gray500,
            background: chroma(Palette.Gray500).brighten(3).desaturate(1).hex(),
        },
        [EntityKind.Parameter]: {
            foreground: Palette.Gray500,
            background: chroma(Palette.Gray500).brighten(3).desaturate(1).hex(),
        },
    },
    types: {
        [TypeKind.Class]: {
            foreground: Palette.Beige200,
            background: chroma(Palette.Beige200).brighten(1).desaturate(2).hex(),
        },
        [TypeKind.Struct]: {
            foreground: chroma(Palette.Blue100).darken(0.5).hex(),
            background: chroma(Palette.Blue100).brighten(1.5).desaturate(1).hex(),
        },
        [TypeKind.Interface]: {
            foreground: chroma(Palette.Green100).darken(0.5).hex(),
            background: chroma(Palette.Green100).brighten(1.5).desaturate(1).hex(),
        },
        [TypeKind.Enum]: {
            foreground: chroma(Palette.Red100).desaturate(1).hex(),
            background: chroma(Palette.Red100).brighten(1.5).desaturate(1).hex(),
        },
        [TypeKind.Delegate]: {
            foreground: chroma(Palette.Purple100).darken(0.5).hex(),
            background: chroma(Palette.Purple100).brighten(1.5).desaturate(1).hex(),
        },
    },
};

export const VS_NODE_COLOR_SCHEMA: Readonly<NodeColorSchema> = {
    entities: {
        [EntityKind.Solution]: {
            foreground: VSColor.DarkPurple,
            background: chroma(VSColor.DarkPurple).brighten(3).desaturate(2).hex(),
        },
        [EntityKind.Project]: {
            foreground: VSColor.DarkGray,
            background: chroma(VSColor.DarkGray).brighten(4).desaturate(1).hex(),
        },
        [EntityKind.ExternalDependencySource]: {
            foreground: VSColor.DarkGray,
            background: chroma(VSColor.DarkGray).brighten(4).desaturate(1).hex(),
        },
        [EntityKind.Framework]: {
            foreground: VSColor.DarkGray,
            background: chroma(VSColor.DarkGray).brighten(4).desaturate(1).hex(),
        },
        [EntityKind.PackageRepository]: {
            foreground: VSColor.NuGetBlue,
            background: chroma(VSColor.NuGetBlue).brighten(3).desaturate(2).hex(),
        },
        [EntityKind.Package]: {
            foreground: VSColor.DarkGray,
            background: chroma(VSColor.DarkGray).brighten(4).desaturate(1).hex(),
        },
        [EntityKind.Library]: {
            foreground: VSColor.DarkGray,
            background: chroma(VSColor.DarkGray).brighten(4).desaturate(1).hex(),
        },
        [EntityKind.Assembly]: {
            foreground: VSColor.DarkGray,
            background: chroma(VSColor.DarkGray).brighten(4).desaturate(1).hex(),
        },
        [EntityKind.Module]: {
            foreground: VSColor.Purple,
            background: chroma(VSColor.Purple).brighten(2).desaturate(2).hex(),
        },
        [EntityKind.Namespace]: {
            foreground: VSColor.DarkGray,
            background: chroma(VSColor.DarkGray).brighten(4).desaturate(1).hex(),
        },
        [EntityKind.Type]: {
            foreground: VSColor.Blue,
            background: chroma(VSColor.Blue).brighten(2).desaturate(1).hex(),
        },
        [EntityKind.Field]: {
            foreground: VSColor.Blue,
            background: chroma(VSColor.Blue).brighten(2).desaturate(1).hex(),
        },
        [EntityKind.Property]: {
            foreground: VSColor.DarkGray,
            background: chroma(VSColor.DarkGray).brighten(4).desaturate(1).hex(),
        },
        [EntityKind.Event]: {
            foreground: VSColor.DarkYellow,
            background: chroma(VSColor.DarkYellow).brighten(2).desaturate(2).hex(),
        },
        [EntityKind.Method]: {
            foreground: VSColor.Purple,
            background: chroma(VSColor.Purple).brighten(2).desaturate(2).hex(),
        },
        [EntityKind.TypeParameter]: {
            foreground: VSColor.Blue,
            background: chroma(VSColor.Blue).brighten(2).desaturate(1).hex(),
        },
        [EntityKind.Parameter]: {
            foreground: VSColor.Blue,
            background: chroma(VSColor.Blue).brighten(2).desaturate(1).hex(),
        },
    },
    types: {
        [TypeKind.Class]: {
            foreground: VSColor.DarkYellow,
            background: chroma(VSColor.DarkYellow).brighten(2).desaturate(2).hex(),
        },
        [TypeKind.Struct]: {
            foreground: VSColor.Blue,
            background: chroma(VSColor.Blue).brighten(2).desaturate(1).hex(),
        },
        [TypeKind.Interface]: {
            foreground: VSColor.Blue,
            background: chroma(VSColor.Blue).brighten(2).desaturate(1).hex(),
        },
        [TypeKind.Enum]: {
            foreground: VSColor.DarkYellow,
            background: chroma(VSColor.DarkYellow).brighten(2).desaturate(2).hex(),
        },
        [TypeKind.Delegate]: {
            foreground: VSColor.Purple,
            background: chroma(VSColor.Purple).brighten(2).desaturate(2).hex(),
        },
    },
};

export const CSHARP_NODE_STYLIST: NodeStylist<NodeColorSchema> = (node, schema) => {
    return {
        ...FALLBACK_NODE_STYLE,
        ...resolveNodeStyle(schema, node)
    };
}

const FALLBACK_NODE_STYLE: NodeStyle = {
    icon: "vscode:pie-chart",
    size: 5,
    color: Palette.Gray600,
    outlines: [],
    slices: { solid: 0, stroked: 0, width: 0 },
    fire: FireStatus.None,
    contour: Contour.None,
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

function resolveNodeStyle(colorSchema: NodeColorSchema, node: HelvegNodeAttributes): Partial<NodeStyle> {
    const model = node.model as CSharpNode;

    let style: NodeStyle = {
        ...FALLBACK_NODE_STYLE,
        ...(model.kind && PARTIAL_STYLES.get(model.kind)),
    };

    if (model.kind && colorSchema.entities[model.kind]) {
        style.color = colorSchema.entities[model.kind].foreground;
        style.backgroundColor = colorSchema.entities[model.kind].background;
    }

    const hasErrors = (model.diagnostics ?? [])?.filter((d) => d.severity === "error").length > 0;
    const hasWarnings = (model.diagnostics ?? [])?.filter((d) => d.severity === "warning").length > 0;
    style.fire = hasErrors ? FireStatus.Flame : hasWarnings ? FireStatus.Smoke : FireStatus.None;

    switch (model.kind) {
        case EntityKind.Type:
            if (model.typeKind) {
                if (PARTIAL_STYLES.has(model.typeKind)) {
                    Object.assign(style, PARTIAL_STYLES.get(model.typeKind));
                }

                const typeKind = model.typeKind as unknown as LimitedTypeKind;
                if (colorSchema.types[typeKind]) {
                    style.color = colorSchema.types[typeKind].foreground;
                    style.backgroundColor = colorSchema.types[typeKind].background;
                }
            }

            switch (model.typeKind) {
                case TypeKind.Class:
                    if (model.isRecord) {
                        style.icon = "helveg:RecordClass";
                    }
                    break;
                case TypeKind.Struct:
                    if (model.isRecord) {
                        style.icon = "helveg:RecordStruct";
                    }
                    break;
            }
            const instanceMemberCount = model.instanceMemberCount ?? 0;
            const staticMemberCount = model.staticMemberCount ?? 0;
            style.outlines = [
                {
                    style: model.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid,
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
            style.size = model.declaringKind === EntityKind.Method ? 5 : 10;
            break;
        case EntityKind.Field:
            style.outlines = [
                {
                    style: model.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid,
                    width: 2,
                },
            ];
            if (model.isEnumItem) {
                style.icon = "helveg:EnumItem";
            } else if (model.isConst) {
                style.icon = "helveg:Constant";
            }
            break;
        case EntityKind.Method:
            if (
                model.methodKind === MethodKind.BuiltinOperator ||
                model.methodKind === MethodKind.UserDefinedOperator
            ) {
                style.icon = "helveg:Operator";
            } else if (model.methodKind === MethodKind.Constructor) {
                style.icon = "helveg:Constructor";
            } else if (model.methodKind === MethodKind.Destructor) {
                style.icon = "helveg:Destructor";
            }

            style.outlines = [
                {
                    style: model.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid,
                    width: 2,
                },
            ];
            break;
        case EntityKind.Property:
            if (model.isIndexer) {
                style.icon = "helveg:Indexer";
            }

            style.outlines = [
                {
                    style: model.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid,
                    width: 2,
                },
            ];
            break;
        case EntityKind.Event:
            style.outlines = [
                {
                    style: model.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid,
                    width: 2,
                },
            ];
            break;
    }

    if (
        model.isStatic &&
        (model.typeKind === "Class" ||
            model.typeKind === "Struct" ||
            (model.kind === "Field" && !model.isConst) ||
            model.kind === "Property" ||
            model.kind === "Event" ||
            model.kind === "Method")
    ) {
        style.icon += "Static";
    }

    if (
        model.isSealed &&
        (model.typeKind === TypeKind.Class || model.kind === EntityKind.Method || model.kind === EntityKind.Property)
    ) {
        style.contour = Contour.FullOctagon;
    }

    if (model.isAbstract && (model.typeKind === TypeKind.Class || model.kind !== EntityKind.Type)) {
        style.contour = Contour.DashedHexagon;
    }

    switch (model.accessibility) {
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

    if (
        model.kind === EntityKind.Solution ||
        model.kind === EntityKind.Project ||
        model.kind === EntityKind.Namespace ||
        model.kind === EntityKind.Assembly ||
        model.kind === EntityKind.Module
    ) {
        style.size += (model.treeHeight ?? 0) * 5;
    }
    return style;
}

function resolveRelationStyle(relation: string, colors?: RelationColorSchema): EdgeStyle {
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
