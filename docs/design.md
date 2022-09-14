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

## Glyphs

* [Glyph-based Visualization: Foundations, Design Guidelines,
Techniques and Applications](https://vis.uib.no/wp-content/papercite-data/pdfs/Borgo13GlyphBased.pdf)
* [MatchPad : Interactive Glyph-Based Visualization for Real-Time Sports Performance Analysis](https://www.researchgate.net/publication/262355245_MatchPad_Interactive_Glyph-Based_Visualization_for_Real-Time_Sports_Performance_Analysis)
* [Diffusion Tensor Visualization with Glyph Packing](https://ieeexplore.ieee.org/document/4015499)
* [Systematising glyph design for visualization](https://ora.ox.ac.uk/objects/uuid:b98ccce1-038f-4c0a-a259-7f53dfe06ac7)

## Dilemmas

* Desktop rendering API (i.e. Silk, Stride, ...) vs WebGL
  * A desktop rendering API would be faster and could render large projects
  * WebGL is the only supported renderer inside VSCode
    * To integrate Helveg with VSCode would require some IPC in the case of a desktop rendering API.
      * This would not work if Helveg were run inside browser-only VS Code.
