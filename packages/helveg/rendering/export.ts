import { DEFAULT_EXPORT_OPTIONS, type ExportOptions } from "model/options";
import { Sigma } from "sigma";
import FileSaver from "file-saver";

/**
 * A function that exports a diagram to a PNG file.
 * 
 * Based on the "png-snapshot" example from the Sigma.js repository.
 */
export function exportDiagram(sigma: Sigma, options?: ExportOptions) {
    options = { ...DEFAULT_EXPORT_OPTIONS, ...options };

    let { width, height } = sigma.getDimensions();

    const originalPixelRatio = window.devicePixelRatio || 1;
    const pixelRatio = originalPixelRatio * options.scale;
    window.devicePixelRatio = pixelRatio;
    try {

        const tmpRoot = document.createElement("div");
        tmpRoot.style.width = `${width}px`;
        tmpRoot.style.height = `${height}px`;
        tmpRoot.style.position = "absolute";
        tmpRoot.style.right = "101%";
        tmpRoot.style.bottom = "101%";
        document.body.appendChild(tmpRoot);

        const tmpRenderer = new Sigma(sigma.getGraph(), tmpRoot, sigma.getSettings());

        tmpRenderer.getCamera().setState(sigma.getCamera().getState());
        tmpRenderer.refresh();

        const exportCanvas = document.createElement("canvas");
        exportCanvas.setAttribute("width", (width * pixelRatio).toString());
        exportCanvas.setAttribute("height", (height * pixelRatio).toString());
        const ctx = exportCanvas.getContext("2d") as CanvasRenderingContext2D;

        // background
        ctx.fillStyle = options.backgroundColor;
        ctx.globalAlpha = options.opacity;
        ctx.fillRect(0, 0, width * pixelRatio, height * pixelRatio);
        ctx.globalAlpha = 1;

        // resolve layer names
        let layers: string[] = [];
        if (options.includeEdges) {
            layers.push("edges");
        }

        if (options.includeNodes) {
            layers.push("nodes");
        }

        if (options.includeLabels) {
            layers.push("labels");
        }
        
        if (options.includeHighlights) {
            layers.push("hovers");
            layers.push("hoverNodes");
        }

        if (options.includePizzaDough) {
            let pizzaCanvases = tmpRenderer.getContainer().getElementsByClassName("helveg-codepizza");
            if (pizzaCanvases.length === 0) {
                DEBUG && console.warn("Could not find the codepizza canvas.");
            }
            for (let pizzaCanvas of pizzaCanvases) {
                ctx.drawImage(
                    pizzaCanvas as HTMLCanvasElement,
                    0,
                    0,
                    width * pixelRatio,
                    height * pixelRatio,
                    0,
                    0,
                    width * pixelRatio,
                    height * pixelRatio
                );
            }
        }
        
        if (options.includeEffects) {
            let effectCanvases = tmpRenderer.getContainer().getElementsByClassName("helveg-effects");
            if (effectCanvases.length === 0) {
                DEBUG && console.warn("Could not find the effects canvas.");
            }
            for (let effectCanvas of effectCanvases) {
                ctx.drawImage(
                    effectCanvas as HTMLCanvasElement,
                    0,
                    0,
                    width * pixelRatio,
                    height * pixelRatio,
                    0,
                    0,
                    width * pixelRatio,
                    height * pixelRatio
                );
            }
        }

        // draw each layer onto the export canvas
        let canvases = tmpRenderer.getCanvases();
        layers = layers.filter((id) => !!canvases[id]);
        layers.forEach((id) => {
            ctx.drawImage(
                canvases[id],
                0,
                0,
                width * pixelRatio,
                height * pixelRatio,
                0,
                0,
                width * pixelRatio,
                height * pixelRatio,
            );
        });

        exportCanvas.toBlob((blob) => {
            if (blob) {
                console.log("blob");
                FileSaver.saveAs(blob, options?.fileName ?? "helveg-export.png");
            }

            tmpRenderer.kill();
            tmpRoot.remove();
        }, "image/png");
    }
    finally {
        window.devicePixelRatio = originalPixelRatio;
    }
}
