# Helveg Design Document (2022)

## Visualization Engines

* [ILNumerics](https://ilnumerics.net/visualization-engine.html)
* [Circos](http://circos.ca/)

## Visual Studio Extensibility

* [Add a tool window](https://docs.microsoft.com/en-us/visualstudio/extensibility/adding-a-tool-window?view=vs-2022)

## Rendering possibilities

* [WebGPU](https://developer.chrome.com/docs/web-platform/webgpu/)
  * **Not available!**
* [Silk.NET](https://github.com/dotnet/Silk.NET)
  * **Arguably too low-level**
  * Might target WebGL in the future
* [Stride](https://github.com/stride3d/stride/)
* [vvvv](https://vvvv.org/#)
  * Visualization library or smth
* [SVG.NET](https://github.com/svg-net/SVG)
  * Requires `System.Drawing` which is a little dumb

### JavaScript

* [JavaScript: Discover the Graph-Visualization Ecosystem](https://isquared.digital/blog/2020-03-24-viz-tools-pt2-1/)
* [sigma.js](https://www.sigmajs.org/)
  * Graph rendering
* [cola.js](https://ialab.it.monash.edu/webcola/)
  * Graph layouting
* [VivaGraphJS](https://github.com/anvaka/VivaGraphJS)
  * Modular graph rendering and layouting engine
* [deck.gl](https://deck.gl/)
  * High-performance large scale visualization


## Visual Style

* [Mini Metro](https://dinopoloclub.com/games/mini-metro/)
* Visual Studio Icons
  * Methods -> Purple Isometric Boxes -> Hexagons
  * Modules -> Multiple Boxes -> Squares
* Increasing complexity
  * Functions -> Dots
  * Types -> Squares
  * Modules -> Hexagons
* A 2D city
  * Functions -> Buildings
  * Classes -> Blocks

## Glyphs

* [Glyph-based Visualization: Foundations, Design Guidelines, Techniques and Applications](https://vis.uib.no/wp-content/papercite-data/pdfs/Borgo13GlyphBased.pdf)
* [MatchPad : Interactive Glyph-Based Visualization for Real-Time Sports Performance Analysis](https://www.researchgate.net/publication/262355245_MatchPad_Interactive_Glyph-Based_Visualization_for_Real-Time_Sports_Performance_Analysis)
* [Diffusion Tensor Visualization with Glyph Packing](https://ieeexplore.ieee.org/document/4015499)
* [Systematising glyph design for visualization](https://ora.ox.ac.uk/objects/uuid:b98ccce1-038f-4c0a-a259-7f53dfe06ac7)
* [MetaGlyph: Automatic Generation of Metaphoric Glyph-based Visualization](https://arxiv.org/pdf/2209.05739.pdf)
* [A Glyph Toolbox for Immersive Scientific Visualization](https://math.nist.gov/mcsd/savg/papers/NIST_SAVG_2002103000.pdf)

### Glyph libraries

* https://github.com/rwoldford/glyphs
* https://docs.bokeh.org/en/latest/docs/reference/models/glyphs.html

## Dilemmas

* Desktop rendering API (i.e. Silk, Stride, ...) vs WebGL
  * A desktop rendering API would be faster and could render large projects
  * WebGL is the only supported renderer inside VSCode
    * To integrate Helveg with VSCode would require some IPC in the case of a desktop rendering API.
      * This would not work if Helveg were run inside browser-only VS Code.

## Existing softvis tools

* [Dependencies Visualizer for Unity](https://assetstore.unity.com/packages/tools/utilities/dependencies-visualizer-178326)
* [Reference Viewer in Unreal Engine](https://docs.unrealengine.com/4.27/en-US/Basics/ContentBrowser/ReferenceViewer/)
* [ndepend](https://www.ndepend.com/)
* [UML Class Diagram in JetBrains Rider](https://www.jetbrains.com/help/idea/class-diagram.html)
* [Class Designer in Visual Studio](https://docs.microsoft.com/en-us/visualstudio/ide/class-designer/designing-and-viewing-classes-and-types?view=vs-2022)
* [PlantUML](https://github.com/plantuml/plantuml)
* [Code Connect](https://marketplace.visualstudio.com/items?itemName=CodeConnect.CodeConnectAlpha-12804)
* [Alive](https://www.youtube.com/watch?v=C40Ozwohgm8)

## Features

### Must-have

**High-level structural view (static analysis)**

* A (potentially very large) node graph.
* A nested hierarchy of nodes, `Solution -> Project/Assembly -> Namespace -> Type -> Field/Property/Method -> Variable`
  * `Project/Assembly` should be optional as sometimes you only care about namespaces.
* Filtering (regexes & other)
* Extension for Visual Studio / Visual Studio Code => Must be built as a web app
* Export of data and images (PNG, SVG)
* Support for use cases
  1. Large C# solutions => Some optimization required
  2. Unity projects => Support for other asset types and parsing of Unity references

**Dynamic analysis**

* Should enhance the node graph
* `dotnet dump` analysis ([clrmd](https://github.com/Microsoft/clrmd))
* Stacktrace visualization
  * View of all possible function call paths (sans reflection)
  * Highlight of a provided stacktrace

### Nice-to-have

* A minigame of some sort
* An extension for other IDEs
* Good performance
  * Would probably require a custom renderer, which is too time consuming
* Visualization of a program's "recording"
  * See _Alive_ in _Existing softvis tools_

## TODO

* Design glyphs for functions, classes, namespaces, assemblies, other assets, etc.
* Pick between Stride/Silk and WebGL.

## English

* https://www.linguee.com/
