import { LineStyle, Outline, StaticGlyphStyle } from "./glyph";
import type { VisualizationPlugin, VisualizationPluginContext } from "./plugin";

export default class CSharpPlugin implements VisualizationPlugin {
    name: string = "csharp";

    setup(context: VisualizationPluginContext): void {
        context.glyphOptions.styles["csharp:Solution"] = new StaticGlyphStyle({
            icon: "csharp:Solution",
            size: 9,
            color: "#ac162c",
            outlines: [new Outline(LineStyle.Solid, "#ac162c", 1)]
        });
        context.dataOptions.defaultIcons["csharp:Solution"] = "csharp:Solution";
        context.dataOptions.kinds.push("csharp:Solution");

        context.glyphOptions.styles["csharp:Project"] = new StaticGlyphStyle({
            icon: "csharp:CSProjectNode",
            size: 8,
            color: "#57a64a",
            outlines: [new Outline(LineStyle.Solid, "#57a64a", 1)]
        });
        context.dataOptions.defaultIcons["csharp:Project"] = "csharp:CSProjectNode";
        context.dataOptions.kinds.push("csharp:Project");

        context.glyphOptions.styles["csharp:ExternalDependencySource"] = new StaticGlyphStyle({
            icon: "csharp:ReferenceGroup",
            size: 8,
            color: "#002440",
            outlines: [new Outline(LineStyle.Solid, "#002440", 1)]
        });
        context.dataOptions.defaultIcons["csharp:ExternalDependencySource"] = "csharp:ReferenceGroup";
        context.dataOptions.kinds.push("csharp:ExternalDependencySource");

        context.glyphOptions.styles["csharp:Framework"] = new StaticGlyphStyle({
            icon: "csharp:Framework",
            size: 8,
            color: "#002440",
            outlines: [new Outline(LineStyle.Solid, "#002440", 1)]
        });
        context.dataOptions.defaultIcons["csharp:Framework"] = "csharp:Framework";
        context.dataOptions.kinds.push("csharp:Framework");

        context.glyphOptions.styles["csharp:Package"] = new StaticGlyphStyle({
            icon: "csharp:Package",
            size: 7,
            color: "#002440",
            outlines: [new Outline(LineStyle.Solid, "#002440", 1)]
        });
        context.dataOptions.defaultIcons["csharp:Package"] = "csharp:Package";
        context.dataOptions.kinds.push("csharp:Package");

        context.glyphOptions.styles["csharp:AssemblyDependency"] = new StaticGlyphStyle({
            icon: "csharp:Reference",
            size: 7,
            color: "#002440",
            outlines: [new Outline(LineStyle.Solid, "#002440", 1)]
        });
        context.dataOptions.defaultIcons["csharp:AssemblyDependency"] = "csharp:Reference";
        context.dataOptions.kinds.push("csharp:AssemblyDependency");

        context.glyphOptions.styles["csharp:ModuleDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Module",
            size: 6,
            color: "#57a64a",
            outlines: [new Outline(LineStyle.Solid, "#57a64a", 1)]
        });
        context.dataOptions.defaultIcons["csharp:ModuleDefinition"] = "csharp:Module";
        context.dataOptions.kinds.push("csharp:ModuleDefinition");

        context.glyphOptions.styles["csharp:NamespaceDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Namespace",
            size: 5,
            color: "#dcdcdc",
            outlines: [new Outline(LineStyle.Solid, "#dcdcdc", 1)]
        });
        context.dataOptions.defaultIcons["csharp:NamespaceDefinition"] = "csharp:Namespace";
        context.dataOptions.kinds.push("csharp:NamespaceDefinition");

        context.glyphOptions.styles["csharp:TypeDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Class",
            size: 4,
            color: "#4ec9b0",
            outlines: [new Outline(LineStyle.Solid, "#4ec9b0", 1)]
        });
        context.dataOptions.defaultIcons["csharp:TypeDefinition"] = "csharp:Class";
        context.dataOptions.kinds.push("csharp:TypeDefinition");

        context.glyphOptions.styles["csharp:TypeParameterDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Type",
            size: 2,
            color: "#4ec9b0",
            outlines: [new Outline(LineStyle.Solid, "#4ec9b0", 1)]
        });
        context.dataOptions.defaultIcons["csharp:TypeParameterDefinition"] = "csharp:Type";
        context.dataOptions.kinds.push("csharp:TypeParameterDefinition");

        context.glyphOptions.styles["csharp:FieldDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Field",
            size: 3,
            color: "#dcdcaa",
            outlines: [new Outline(LineStyle.Solid, "#dcdcaa", 1)]
        });
        context.dataOptions.defaultIcons["csharp:FieldDefinition"] = "csharp:Field";
        context.dataOptions.kinds.push("csharp:FieldDefinition");

        context.glyphOptions.styles["csharp:MethodDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Method",
            size: 3,
            color: "#dcdcaa",
            outlines: [new Outline(LineStyle.Solid, "#dcdcaa", 1)]
        });
        context.dataOptions.defaultIcons["csharp:MethodDefinition"] = "csharp:Method";
        context.dataOptions.kinds.push("csharp:MethodDefinition");

        context.glyphOptions.styles["csharp:PropertyDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Property",
            size: 3,
            color: "#dcdcaa",
            outlines: [new Outline(LineStyle.Solid, "#dcdcaa", 1)]
        });
        context.dataOptions.defaultIcons["csharp:PropertyDefinition"] = "csharp:Property";
        context.dataOptions.kinds.push("csharp:PropertyDefinition");

        context.glyphOptions.styles["csharp:EventDefinition"] = new StaticGlyphStyle({
            icon: "csharp:Event",
            size: 3,
            color: "#dcdcaa",
            outlines: [new Outline(LineStyle.Solid, "#dcdcaa", 1)]
        });
        context.dataOptions.defaultIcons["csharp:EventDefinition"] = "csharp:Event";
        context.dataOptions.kinds.push("csharp:EventDefinition");

        context.glyphOptions.styles["csharp:ParameterDefinition"] = new StaticGlyphStyle({
            icon: "csharp:LocalVariable",
            size: 2,
            color: "#9cdcfe",
            outlines: [new Outline(LineStyle.Solid, "#9cdcfe", 1)]
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
