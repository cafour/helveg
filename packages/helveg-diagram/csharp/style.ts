import { MultigraphNode } from "../model/data-model.ts";
import { EdgeStyle, FALLBACK_EDGE_STYLE, FALLBACK_NODE_STYLE, FireStatus, NodeStyle, NodeStylist, OutlineStyle, RelationStylist } from "../model/style";
import { CSharpNode, DefaultRelationColors, EntityKind, MemberAccessibility, MethodKind, Relations, TypeKind, VSColor } from "./model";

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

function resolveNodeStyle(node: Partial<CSharpNode>): Partial<NodeStyle> {
    let base: Partial<NodeStyle> = {};

    const hasErrors = (node.diagnostics ?? [])
        ?.filter(d => d.severity === "error")
        .length > 0;
    const hasWarnings = (node.diagnostics ?? [])
        ?.filter(d => d.severity === "warning")
        .length > 0;
    base.fire = hasErrors ? FireStatus.Flame
        : hasWarnings ? FireStatus.Smoke
            : FireStatus.None;

    switch (node.kind) {
        case EntityKind.Solution:
            return {
                icon: "helveg:Solution",
                size: 55,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.Project:
            return {
                icon: "helveg:Project",
                size: 45,
                color: VSColor.Green,
                outlines: []
            };
        case EntityKind.Framework:
            return {
                icon: "helveg:Framework",
                size: 50,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.ExternalDependencySource:
            return {
                icon: "helveg:ExternalDependencySource",
                size: 50,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.PackageRepository:
            return {
                icon: "helveg:PackageRepository",
                size: 50,
                color: VSColor.NuGetBlue,
                outlines: []
            };
        case EntityKind.Package:
            return {
                icon: "helveg:Package",
                size: 45,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.Library:
            return {
                icon: "helveg:Library",
                size: 40,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.Assembly:
            return {
                icon: "helveg:Assembly",
                size: 40,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.Module:
            return {
                icon: "helveg:Module",
                size: 35,
                color: VSColor.Purple,
                outlines: []
            };
        case EntityKind.Namespace:
            return {
                icon: "helveg:Namespace",
                size: 30,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.Type:
            base.size = 15;
            switch (node.typeKind) {
                case TypeKind.Class:
                    base.icon = node.isRecord ? "helveg:RecordClass" : "helveg:Class";
                    base.color = VSColor.DarkYellow;
                    break;
                case TypeKind.Interface:
                    base.icon = "helveg:Interface";
                    base.color = VSColor.Blue;
                    break;
                case TypeKind.Enum:
                    base.icon = "helveg:Enum";
                    base.color = VSColor.DarkYellow;
                    break;
                case TypeKind.Struct:
                    base.icon = node.isRecord ? "helveg:RecordStruct" : "helveg:Struct";
                    base.color = VSColor.Blue;
                    break;
                case TypeKind.Delegate:
                    base.icon = "helveg:Delegate";
                    base.color = VSColor.Purple;
                    break;
                default:
                    base.icon = "helveg:Type";
                    base.color = VSColor.Blue;
                    break;
            }
            const instanceMemberCount = node.instanceMemberCount ?? 0;
            const staticMemberCount = node.staticMemberCount ?? 0;
            base.outlines = [
                { style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 },
                { style: OutlineStyle.Solid, width: instanceMemberCount },
                { style: OutlineStyle.Dashed, width: staticMemberCount }
            ];
            base.slices = {
                solid: staticMemberCount,
                stroked: instanceMemberCount,
                width: staticMemberCount + instanceMemberCount
            };
            break;
        case EntityKind.TypeParameter:
            return {
                icon: "helveg:TypeParameter",
                size: node.declaringKind === EntityKind.Method ? 5 : 10,
                color: VSColor.Blue,
                outlines: []
            };
        case EntityKind.Field:
            if (node.isEnumItem) {
                return {
                    icon: "helveg:EnumItem",
                    size: 10,
                    color: VSColor.Blue
                }
            }

            base.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            base.size = 10;
            if (node.isConst) {
                base.icon = "helveg:Constant";
                base.color = VSColor.DarkGray;
            } else {
                base.icon = "helveg:Field";
                base.color = VSColor.Blue;
            }
            break;
        case EntityKind.Method:
            if (node.methodKind === MethodKind.BuiltinOperator
                || node.methodKind === MethodKind.UserDefinedOperator) {
                base.icon = "helveg:Operator";
            }
            else if (node.methodKind === MethodKind.Constructor) {
                base.icon = "helveg:Constructor";
            }
            else {
                base.icon = "helveg:Method";
            }

            base.color = VSColor.Purple;
            base.size = 10;
            base.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
        case EntityKind.Property:
            base.icon = "helveg:Property";
            base.size = 10;
            base.color = VSColor.DarkGray;
            base.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
        case EntityKind.Event:
            base.icon = "helveg:Event";
            base.size = 10;
            base.color = VSColor.DarkYellow;
            base.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
        case EntityKind.Parameter:
            return {
                icon: "helveg:Parameter",
                color: VSColor.Blue,
                size: 5,
                outlines: []
            };
        default:
            return {
                icon: "vscode:pie-chart",
                size: 5,
                color: VSColor.DarkGray,
                outlines: []
            };
    }

    if (node.isStatic && (
        node.typeKind === "Class"
        || node.typeKind === "Struct"
        || (node.kind === "Field" && !node.isConst)
        || node.kind === "Property"
        || node.kind === "Event"
        || node.kind === "Method"
    )) {
        base.icon += "Static";
    }

    switch (node.accessibility) {
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
