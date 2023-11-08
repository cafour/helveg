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
                { style: OutlineStyle.Solid, width: instanceCount },
                { style: OutlineStyle.Dashed, width: staticCount }
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
