import { OutlineStyle, type Outline, StaticGlyphStyle } from "./glyph";
import type { VisualizationPlugin, VisualizationPluginContext } from "./plugin";

export default class CSharpPlugin implements VisualizationPlugin {
    name: string = "csharp";

    setup(context: VisualizationPluginContext): void {
        context.glyphOptions.styles["csharp:Solution"] = new StaticGlyphStyle({
            icon: "csharp:Solution",
            size: 55,
            color: "#ac162c",
            outlines: []
        });
        context.dataOptions.defaultIcons["csharp:Solution"] = "csharp:Solution";
        context.dataOptions.kinds.push("csharp:Solution");

        context.glyphOptions.styles["csharp:Project"] = new StaticGlyphStyle({
            icon: "csharp:CSProjectNode",
            size: 50,
            color: "#57a64a",
            outlines: []
        });
        context.dataOptions.defaultIcons["csharp:Project"] = "csharp:CSProjectNode";
        context.dataOptions.kinds.push("csharp:Project");

        context.glyphOptions.styles["csharp:ExternalDependencySource"] = new StaticGlyphStyle({
            icon: "csharp:ReferenceGroup",
            size: 60,
            color: "#002440",
            outlines: []
        });
        context.dataOptions.defaultIcons["csharp:ExternalDependencySource"] = "csharp:ReferenceGroup";
        context.dataOptions.kinds.push("csharp:ExternalDependencySource");

        context.glyphOptions.styles["csharp:Framework"] = new StaticGlyphStyle({
            icon: "csharp:Framework",
            size: 55,
            color: "#002440",
            outlines: []
        });
        context.dataOptions.defaultIcons["csharp:Framework"] = "csharp:Framework";
        context.dataOptions.kinds.push("csharp:Framework");

        context.glyphOptions.styles["csharp:Package"] = new StaticGlyphStyle({
            icon: "csharp:Package",
            size: 50,
            color: "#002440",
            outlines: []
        });
        context.dataOptions.defaultIcons["csharp:Package"] = "csharp:Package";
        context.dataOptions.kinds.push("csharp:Package");

        context.glyphOptions.styles["csharp:AssemblyDependency"] = new StaticGlyphStyle({
            icon: "csharp:Reference",
            size: 45,
            color: "#002440",
            outlines: []
        });
        context.dataOptions.defaultIcons["csharp:AssemblyDependency"] = "csharp:Reference";
        context.dataOptions.kinds.push("csharp:AssemblyDependency");
        
        context.glyphOptions.styles["csharp:AssemblyDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Assembly",
            size: 40,
            color: "#002440",
            outlines: []
        });
        context.dataOptions.defaultIcons["csharp:AssemblyDefinition"] = "csharp:Assembly";
        context.dataOptions.kinds.push("csharp:AssemblyDefinition");

        context.glyphOptions.styles["csharp:ModuleDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Module",
            size: 35,
            color: "#57a64a",
            outlines: []
        });
        context.dataOptions.defaultIcons["csharp:ModuleDefinition"] = "csharp:Module";
        context.dataOptions.kinds.push("csharp:ModuleDefinition");

        context.glyphOptions.styles["csharp:NamespaceDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Namespace",
            size: 30,
            color: "#dcdcdc",
            outlines: []
        });
        context.dataOptions.defaultIcons["csharp:NamespaceDefinition"] = "csharp:Namespace";
        context.dataOptions.kinds.push("csharp:NamespaceDefinition");

        context.glyphOptions.styles["csharp:TypeDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Class",
            size: 15,
            color: "#4ec9b0",
            outlines: [{style: OutlineStyle.Solid, width: 1}]
        });
        context.dataOptions.defaultIcons["csharp:TypeDefinition"] = "csharp:Class";
        context.dataOptions.kinds.push("csharp:TypeDefinition");

        context.glyphOptions.styles["csharp:TypeParameterDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Type",
            size: 15,
            color: "#4ec9b0",
            outlines: []
        });
        context.dataOptions.defaultIcons["csharp:TypeParameterDefinition"] = "csharp:Type";
        context.dataOptions.kinds.push("csharp:TypeParameterDefinition");

        context.glyphOptions.styles["csharp:FieldDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Field",
            size: 10,
            color: "#dcdcaa",
            outlines: [{style: OutlineStyle.Solid, width: 1}]
        });
        context.dataOptions.defaultIcons["csharp:FieldDefinition"] = "csharp:Field";
        context.dataOptions.kinds.push("csharp:FieldDefinition");

        context.glyphOptions.styles["csharp:MethodDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Method",
            size: 10,
            color: "#dcdcaa",
            outlines: [{style: OutlineStyle.Solid, width: 1}]
        });
        context.dataOptions.defaultIcons["csharp:MethodDefinition"] = "csharp:Method";
        context.dataOptions.kinds.push("csharp:MethodDefinition");

        context.glyphOptions.styles["csharp:PropertyDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Property",
            size: 10,
            color: "#dcdcaa",
            outlines: [{style: OutlineStyle.Solid, width: 1}]
        });
        context.dataOptions.defaultIcons["csharp:PropertyDefinition"] = "csharp:Property";
        context.dataOptions.kinds.push("csharp:PropertyDefinition");

        context.glyphOptions.styles["csharp:EventDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Event",
            size: 10,
            color: "#dcdcaa",
            outlines: [{style: OutlineStyle.Solid, width: 1}]
        });
        context.dataOptions.defaultIcons["csharp:EventDefinition"] = "csharp:Event";
        context.dataOptions.kinds.push("csharp:EventDefinition");

        context.glyphOptions.styles["csharp:ParameterDefinition"] = new StaticGlyphStyle({
            icon: "csharp:LocalVariable",
            size: 5,
            color: "#9cdcfe",
            outlines: []
        });
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
    }

}
