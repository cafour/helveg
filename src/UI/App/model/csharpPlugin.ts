import type { GlyphStyle, NodeStyle } from "./glyph";
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
    DeclaringKind?: EntityKind
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
        context.dataOptions.defaultIcons["csharp:Solution"] = "csharp:Solution";
        context.dataOptions.kinds.push("csharp:Solution");
        context.dataOptions.defaultIcons["csharp:Project"] = "csharp:CSProjectNode";
        context.dataOptions.kinds.push("csharp:Project");
        context.dataOptions.defaultIcons["csharp:ExternalDependencySource"] = "csharp:ReferenceGroup";
        context.dataOptions.kinds.push("csharp:ExternalDependencySource");
        context.dataOptions.defaultIcons["csharp:Framework"] = "csharp:Framework";
        context.dataOptions.kinds.push("csharp:Framework");
        context.dataOptions.defaultIcons["csharp:Package"] = "csharp:Package";
        context.dataOptions.kinds.push("csharp:Package");
        context.dataOptions.defaultIcons["csharp:AssemblyDependency"] = "csharp:Reference";
        context.dataOptions.kinds.push("csharp:AssemblyDependency");
        context.dataOptions.defaultIcons["csharp:AssemblyDefinition"] = "csharp:Assembly";
        context.dataOptions.kinds.push("csharp:AssemblyDefinition");
        context.dataOptions.defaultIcons["csharp:ModuleDefinition"] = "csharp:Module";
        context.dataOptions.kinds.push("csharp:ModuleDefinition");
        context.dataOptions.defaultIcons["csharp:NamespaceDefinition"] = "csharp:Namespace";
        context.dataOptions.kinds.push("csharp:NamespaceDefinition");
        context.dataOptions.defaultIcons["csharp:TypeDefinition"] = "csharp:Class";
        context.dataOptions.kinds.push("csharp:TypeDefinition");
        context.dataOptions.defaultIcons["csharp:TypeParameterDefinition"] = "csharp:Type";
        context.dataOptions.kinds.push("csharp:TypeParameterDefinition");
        context.dataOptions.defaultIcons["csharp:FieldDefinition"] = "csharp:Field";
        context.dataOptions.kinds.push("csharp:FieldDefinition");
        context.dataOptions.defaultIcons["csharp:MethodDefinition"] = "csharp:Method";
        context.dataOptions.kinds.push("csharp:MethodDefinition");
        context.dataOptions.defaultIcons["csharp:PropertyDefinition"] = "csharp:Property";
        context.dataOptions.kinds.push("csharp:PropertyDefinition");
        context.dataOptions.defaultIcons["csharp:EventDefinition"] = "csharp:Event";
        context.dataOptions.kinds.push("csharp:EventDefinition");
        context.dataOptions.defaultIcons["csharp:ParameterDefinition"] = "csharp:LocalVariable";
        context.dataOptions.kinds.push("csharp:ParameterDefinition");

        context.dataOptions.selectedKinds.push(
            "csharp:Solution",
            "csharp:Project",
            "csharp:Framework",
            "csharp:ExternalDependencySource",
            "csharp:Package",
            "csharp:AssemblyDependency",
            "csharp:NamespaceDefinition",
            "csharp:TypeDefinition",
            "csharp:TypeParameterDefinition",
            "csharp:FieldDefinition",
            "csharp:MethodDefinition",
            "csharp:PropertyDefinition",
            "csharp:EventDefinition");

        let plugin = this;
        let glyphStyle = <GlyphStyle>{
            apply(node: GraphNode){
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
        }
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
                base.size = 25;
                base.outlines = [{ style: OutlineStyle.Solid, width: 1 }];
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
                }
                break;
            case EntityKind.TypeParameterDefinition:
                return {
                    icon: "csharp:Type",
                    size: props.DeclaringKind === EntityKind.MethodDefinition ? 10 : 20,
                    color: VSColor.Blue,
                    outlines: []
                };
            case EntityKind.FieldDefinition:
                if (props.IsEnumItem) {
                    return {
                        icon: "csharp:EnumerationItem",
                        size: 15,
                        color: VSColor.Blue
                    }
                }

                base.outlines = [{ style: OutlineStyle.Solid, width: 1 }];
                base.size = 15;
                if (props.IsConst) {
                    base.icon = "csharp:Constant";
                    base.color = VSColor.DarkGray;
                } else {
                    base.icon = "csharp:Field";
                    base.color = VSColor.Blue;
                }
                break;
            case EntityKind.MethodDefinition:
                base.icon = "csharp:Method";
                base.size = 15;
                base.color = VSColor.Purple;
                base.outlines = [{ style: OutlineStyle.Solid, width: 1 }];
                break;
            case EntityKind.PropertyDefinition:
                base.icon = "csharp:Property";
                base.size = 15;
                base.color = VSColor.DarkGray;
                base.outlines = [{ style: OutlineStyle.Solid, width: 1 }];
                break;
            case EntityKind.EventDefinition:
                base.icon = "csharp:Event";
                base.size = 15;
                base.color = VSColor.DarkYellow;
                base.outlines = [{ style: OutlineStyle.Solid, width: 1 }];
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

        switch(props.Accessibility) {
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
