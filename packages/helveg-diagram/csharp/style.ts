import { MultigraphEdge, MultigraphNode } from "../model/multigraph";
import { EdgeStyle, FALLBACK_EDGE_STYLE, FALLBACK_NODE_STYLE, NodeStyle, OutlineStyle } from "../model/style";
import { CSharpNodeProperties, DefaultRelationColors, EntityKind, MemberAccessibility, MethodKind, Relations, TypeKind, VSColor } from "./model";

export function csharpNodeStylist(node: MultigraphNode): NodeStyle {
    return { ...FALLBACK_NODE_STYLE, ...resolveNodeStyle(node.properties as Partial<CSharpNodeProperties>) };
}

export function csharpEdgeStylist(relation: string, edge: MultigraphEdge): EdgeStyle {
    return {...FALLBACK_EDGE_STYLE, ...resolveEdgeStyle(relation, edge)};
}

function resolveNodeStyle(props: Partial<CSharpNodeProperties>): Partial<NodeStyle> {

    let base: Partial<NodeStyle> = {};
    switch (props.Kind) {
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
            switch (props.TypeKind) {
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
            let instanceCount = props.InstanceMemberCount ?? 0;
            let staticCount = props.StaticMemberCount ?? 0;
            base.outlines = [
                { style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 },
                { style: OutlineStyle.Solid, width: instanceCount },
                { style: OutlineStyle.Dashed, width: staticCount }
            ];
            break;
        case EntityKind.TypeParameter:
            return {
                icon: "vs:Type",
                size: props.DeclaringKind === EntityKind.Method ? 5 : 10,
                color: VSColor.Blue,
                outlines: []
            };
        case EntityKind.Field:
            if (props.IsEnumItem) {
                return {
                    icon: "vs:EnumerationItem",
                    size: 10,
                    color: VSColor.Blue
                }
            }

            base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            base.size = 10;
            if (props.IsConst) {
                base.icon = "vs:Constant";
                base.color = VSColor.DarkGray;
            } else {
                base.icon = "vs:Field";
                base.color = VSColor.Blue;
            }
            break;
        case EntityKind.Method:
            if (props.MethodKind === MethodKind.BuiltinOperator
                || props.MethodKind === MethodKind.UserDefinedOperator) {
                base.icon = "vs:Operator";
                base.color = VSColor.Blue;
            }
            else {
                base.icon = "vs:Method";
                base.color = VSColor.Purple;
            }

            base.size = 10;
            base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
        case EntityKind.Property:
            base.icon = "vs:Property";
            base.size = 10;
            base.color = VSColor.DarkGray;
            base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
            break;
        case EntityKind.Event:
            base.icon = "vs:Event";
            base.size = 10;
            base.color = VSColor.DarkYellow;
            base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 2 }];
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

function resolveEdgeStyle(relation: string, edge: MultigraphEdge): EdgeStyle {
    switch (relation) {
        case Relations.Declares:
            return {
                color: DefaultRelationColors.Declares,
                width: 2,
                type: "arrow"
            };
        case Relations.InheritsFrom:
            return {
                color: DefaultRelationColors.InheritsFrom,
                width: 4,
                type: "arrow"
            };
        case Relations.TypeOf:
            return {
                color: DefaultRelationColors.TypeOf,
                width: 4,
                type: "arrow"
            };
        case Relations.Overrides:
            return {
                color: DefaultRelationColors.Overrides,
                width: 4,
                type: "arrow"
            };
        case Relations.Returns:
            return {
                color: DefaultRelationColors.Returns,
                width: 4,
                type: "arrow"
            };
        case Relations.DependsOn:
            return {
                color: DefaultRelationColors.DependsOn,
                width: 6,
                type: "arrow"
            };
        case Relations.References:
            return {
                color: DefaultRelationColors.DependsOn,
                width: 4,
                type: "arrow"
            };
        default:
            return FALLBACK_EDGE_STYLE;
    }
}
