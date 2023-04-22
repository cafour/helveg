import type { GlyphStyle, NodeStyle, Outlines } from "./glyph";
import { OutlineStyle } from "./glyph";
import type { GraphNode } from "./multigraph";
import type { VisualizationPlugin, VisualizationPluginContext } from "./plugin";

enum EntityKind {
    Solution = "csharp:Solution",
    Project = "csharp:Project",
    ExternalDependencySource = "csharp:ExternalDependencySource",
    Framework = "csharp:Framework",
    Package = "csharp:Package",
    AssemblyDependency = "csharp:AssemblyDependency",
    AssemblyDefinition = "csharp:AssemblyDefinition",
    ModuleDefinition = "csharp:ModuleDefinition",
    NamespaceDefinition = "csharp:NamespaceDefinition",
    TypeDefinition = "csharp:TypeDefinition",
    FieldDefinition = "csharp:FieldDefinition",
    PropertyDefinition = "csharp:PropertyDefinition",
    EventDefinition = "csharp:EventDefinition",
    MethodDefinition = "csharp:MethodDefinition",
    TypeParameterDefinition = "csharp:TypeParameterDefinition",
    ParameterDefinition = "csharp:ParameterDefinition"
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
    Blue = "#005dba"
}

interface CSharpNodeProperties {
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
    outlines: []
};

export default class CSharpPlugin implements VisualizationPlugin {
    name: string = "csharp";

    setup(context: VisualizationPluginContext): void {
        context.dataOptions.defaultIcons[EntityKind.Solution] = "csharp:Solution";
        context.dataOptions.defaultIcons[EntityKind.Project] = "csharp:CSProjectNode";
        context.dataOptions.defaultIcons[EntityKind.ExternalDependencySource] = "csharp:ReferenceGroup";
        context.dataOptions.defaultIcons[EntityKind.Framework] = "csharp:Framework";
        context.dataOptions.defaultIcons[EntityKind.Package] = "csharp:Package";
        context.dataOptions.defaultIcons[EntityKind.AssemblyDependency] = "csharp:Reference";
        context.dataOptions.defaultIcons[EntityKind.AssemblyDefinition] = "csharp:Assembly";
        context.dataOptions.defaultIcons[EntityKind.ModuleDefinition] = "csharp:Module";
        context.dataOptions.defaultIcons[EntityKind.NamespaceDefinition] = "csharp:Namespace";
        context.dataOptions.defaultIcons[EntityKind.TypeDefinition] = "csharp:Class";
        context.dataOptions.defaultIcons[EntityKind.TypeParameterDefinition] = "csharp:Type";
        context.dataOptions.defaultIcons[EntityKind.FieldDefinition] = "csharp:Field";
        context.dataOptions.defaultIcons[EntityKind.MethodDefinition] = "csharp:Method";
        context.dataOptions.defaultIcons[EntityKind.PropertyDefinition] = "csharp:Property";
        context.dataOptions.defaultIcons[EntityKind.EventDefinition] = "csharp:Event";
        context.dataOptions.defaultIcons[EntityKind.ParameterDefinition] = "csharp:LocalVariable";

        let plugin = this;
        let glyphStyle = <GlyphStyle>{
            apply(node: GraphNode) {
                if (!(Object.values(EntityKind).includes(node.properties["Kind"] as EntityKind))) {
                    return;
                }

                let base = plugin.resolveBaseStyle(node.properties as CSharpNodeProperties);
                return {
                    ...FALLBACK_STYLE,
                    ...base
                };
            }
        }

        for (const kind of Object.values(EntityKind)) {
            context.glyphOptions.styles[kind] = glyphStyle;
            context.dataOptions.kinds.push(kind);
        }

        context.dataOptions.selectedKinds.push(
            EntityKind.Solution,
            EntityKind.Project,
            EntityKind.Framework,
            EntityKind.ExternalDependencySource,
            EntityKind.Package,
            EntityKind.NamespaceDefinition,
            EntityKind.TypeDefinition,
            EntityKind.TypeParameterDefinition,
            EntityKind.FieldDefinition,
            EntityKind.MethodDefinition,
            EntityKind.PropertyDefinition,
            EntityKind.EventDefinition);
    }

    private resolveBaseStyle(props: CSharpNodeProperties): Partial<NodeStyle> {
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
                    size: 50,
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
            case EntityKind.Package:
                return {
                    icon: "csharp:Package",
                    size: 50,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.AssemblyDependency:
                return {
                    icon: "csharp:Reference",
                    size: 45,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.AssemblyDefinition:
                return {
                    icon: "csharp:Assembly",
                    size: 40,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.ModuleDefinition:
                return {
                    icon: "csharp:Module",
                    size: 35,
                    color: VSColor.Purple,
                    outlines: []
                };
            case EntityKind.NamespaceDefinition:
                return {
                    icon: "csharp:Namespace",
                    size: 30,
                    color: VSColor.DarkGray,
                    outlines: []
                };
            case EntityKind.TypeDefinition:
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
                    { style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 1 },
                    { style: OutlineStyle.Solid, width: Math.max(1, Math.sqrt(instanceCount)) },
                    { style: OutlineStyle.Dashed, width: Math.max(1, Math.sqrt(staticCount)) }
                ];
                break;
            case EntityKind.TypeParameterDefinition:
                return {
                    icon: "csharp:Type",
                    size: props.DeclaringKind === EntityKind.MethodDefinition ? 5 : 10,
                    color: VSColor.Blue,
                    outlines: []
                };
            case EntityKind.FieldDefinition:
                if (props.IsEnumItem) {
                    return {
                        icon: "csharp:EnumerationItem",
                        size: 10,
                        color: VSColor.Blue
                    }
                }

                base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 1 }];
                base.size = 10;
                if (props.IsConst) {
                    base.icon = "csharp:Constant";
                    base.color = VSColor.DarkGray;
                } else {
                    base.icon = "csharp:Field";
                    base.color = VSColor.Blue;
                }
                break;
            case EntityKind.MethodDefinition:
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
                base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 1 }];
                break;
            case EntityKind.PropertyDefinition:
                base.icon = "csharp:Property";
                base.size = 10;
                base.color = VSColor.DarkGray;
                base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 1 }];
                break;
            case EntityKind.EventDefinition:
                base.icon = "csharp:Event";
                base.size = 10;
                base.color = VSColor.DarkYellow;
                base.outlines = [{ style: props.IsStatic ? OutlineStyle.Dashed : OutlineStyle.Solid, width: 1 }];
                break;
            case EntityKind.ParameterDefinition:
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

}
