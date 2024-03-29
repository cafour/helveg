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
    return (r) => { return {...FALLBACK_EDGE_STYLE, ...resolveRelationStyle(r, colors)}};
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
                icon: "vs:Solution",
                size: 55,
                color: VSColor.DarkPurple,
                outlines: []
            };
        case EntityKind.Project:
            return {
                icon: "vs:CSProjectNode",
                size: 45,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.Framework:
            return {
                icon: "vs:Framework",
                size: 50,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.ExternalDependencySource:
            return {
                icon: "vs:ReferenceGroup",
                size: 50,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.PackageRepository:
            return {
                icon: "nuget:NuGet",
                size: 50,
                color: VSColor.NuGetBlue,
                outlines: []
            };
        case EntityKind.Package:
            return {
                icon: "vs:Package",
                size: 45,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.Library:
            return {
                icon: "vs:Library",
                size: 40,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.Assembly:
            return {
                icon: "vs:Assembly",
                size: 40,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.Module:
            return {
                icon: "vs:Module",
                size: 35,
                color: VSColor.Purple,
                outlines: []
            };
        case EntityKind.Namespace:
            return {
                icon: "vs:Namespace",
                size: 30,
                color: VSColor.DarkGray,
                outlines: []
            };
        case EntityKind.Type:
            base.size = 15;
            switch (node.typeKind) {
                case TypeKind.Class:
                    base.icon = "vs:Class";
                    base.color = VSColor.DarkYellow;
                    break;
                case TypeKind.Interface:
                    base.icon = "vs:Interface";
                    base.color = VSColor.Blue;
                    break;
                case TypeKind.Enum:
                    base.icon = "vs:Enumeration";
                    base.color = VSColor.DarkYellow;
                    break;
                case TypeKind.Struct:
                    base.icon = "vs:Structure";
                    base.color = VSColor.Blue;
                    break;
                case TypeKind.Delegate:
                    base.icon = "vs:Delegate";
                    base.color = VSColor.Purple;
                    break;
                default:
                    base.icon = "vs:Type";
                    base.color = VSColor.Blue;
                    break;
            }
            let instanceCount = node.instanceMemberCount ?? 0;
            let staticCount = node.staticMemberCount ?? 0;
            base.outlines = [
                { style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 },
                { style: OutlineStyle.Solid, width: instanceCount },
                { style: OutlineStyle.Dashed, width: staticCount }
            ];
            break;
        case EntityKind.TypeParameter:
            return {
                icon: "vs:Type",
                size: node.declaringKind === EntityKind.Method ? 5 : 10,
                color: VSColor.Blue,
                outlines: []
            };
        case EntityKind.Field:
            if (node.isEnumItem) {
                return {
                    icon: "vs:EnumerationItem",
                    size: 10,
                    color: VSColor.Blue
                }
            }

            base.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            base.size = 10;
            if (node.isConst) {
                base.icon = "vs:Constant";
                base.color = VSColor.DarkGray;
            } else {
                base.icon = "vs:Field";
                base.color = VSColor.Blue;
            }
            break;
        case EntityKind.Method:
            if (node.methodKind === MethodKind.BuiltinOperator
                || node.methodKind === MethodKind.UserDefinedOperator) {
                base.icon = "vs:Operator";
                base.color = VSColor.Blue;
            }
            else {
                base.icon = "vs:Method";
                base.color = VSColor.Purple;
            }

            base.size = 10;
            base.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
        case EntityKind.Property:
            base.icon = "vs:Property";
            base.size = 10;
            base.color = VSColor.DarkGray;
            base.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
        case EntityKind.Event:
            base.icon = "vs:Event";
            base.size = 10;
            base.color = VSColor.DarkYellow;
            base.outlines = [{ style: node.isStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
        case EntityKind.Parameter:
            return {
                icon: "vs:LocalVariable",
                color: VSColor.Blue,
                size: 5,
                outlines: []
            };
        default:
            return {
                icon: "vs:ExplodedDoughnutChart",
                size: 5,
                color: VSColor.DarkGray,
                outlines: []
            };
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
    colors = {...DEFAULT_CSHARP_RELATION_COLORS, ...colors};
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
