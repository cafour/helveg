import { Attributes } from "graphology-types";
import { NodeDisplayData, RenderParams, AbstractNodeProgram, Sigma, getPixelRatio, NodeHoverDrawingFunction, NodeLabelDrawingFunction } from "../deps/sigma.ts";
import { HelvegNodeProgramType } from "../diagram/initializers.ts";
import { ILogger } from "../model/logger.ts";
import { FireProgram } from "./node.fire.ts";
import { GlyphProgramOptions } from "./node.glyph.ts";

interface ProgramCallbacks {
    process(nodeIndex: number, offset: number, data: NodeDisplayData): void;
    reallocate(capacity: number): void;
    render(params: RenderParams): void;
    kill(): void;
}

class ParamsReportingProgram implements AbstractNodeProgram {
    constructor(private callbacks: Partial<ProgramCallbacks>) {
    }

    drawLabel: NodeLabelDrawingFunction<Attributes, Attributes, Attributes> | undefined = undefined;
    drawHover: NodeHoverDrawingFunction<Attributes, Attributes, Attributes> | undefined = undefined;

    process(nodeIndex: number, offset: number, data: NodeDisplayData): void {
        if (this.callbacks.process) {
            this.callbacks.process(nodeIndex, offset, data);
        }
    }

    reallocate(capacity: number): void {
        if (this.callbacks.reallocate) {
            this.callbacks.reallocate(capacity);
        }
    }

    render(params: RenderParams): void {
        if (this.callbacks.render) {
            this.callbacks.render(params);
        }
    }

    kill(): void {
        if (this.callbacks.kill) {
            this.callbacks.kill();
        }
    }
}

export class SigmaEffectsExtension {
    // NB: This counter ensures that each instance of the extension gets its own canvas.
    private static counter: number = 0;

    private sigma: Sigma = null!;
    private canvas: HTMLCanvasElement = null!;
    private gl: WebGL2RenderingContext = null!;
    private renderParams: RenderParams | null = null;
    private renderFrame: number = 0;
    private _reportingProgram: HelvegNodeProgramType;
    private fireProgram: FireProgram = null!;

    constructor(private options: GlyphProgramOptions, private logger?: ILogger) {
        let self = this;
        this._reportingProgram = class extends ParamsReportingProgram {
            constructor(gl: WebGLRenderingContext, pickingBuffer: WebGLFramebuffer, renderer: Sigma) {
                self.initialize(renderer);
                super({
                    process: self.fireProgram.process.bind(self.fireProgram),
                    reallocate: self.fireProgram.reallocate.bind(self.fireProgram),
                    render: self.onSigmaRender.bind(self)
                });
            }
        }
    }

    get program(): HelvegNodeProgramType {
        return this._reportingProgram;
    }

    private initialize(sigma: Sigma) {
        if (this.sigma) {
            this.logger?.debug("The effects extension has already been initialized.");
            return;
        }

        this.sigma = sigma;
        this.sigma.addListener("kill", this.onSigmaKill.bind(this));
        this.sigma.addListener("resize", this.onSigmaResize.bind(this));

        this.canvas = document.createElement("canvas");
        this.canvas.classList.add("helveg-effects");
        // this.canvas.id = `"helveg-effects-${++SigmaEffectsExtension.counter}"`;
        this.canvas.style.position = "absolute";

        this.sigma.getContainer().prepend(this.canvas);

        let context = this.canvas.getContext("webgl2", {
            preserveDrawingBuffer: false,
            antialias: false
        });

        if (!context) {
            throw new Error("Unable to create a WebGL2 context.");
        }

        this.gl = context;
        this.fireProgram = new FireProgram(this.gl, null!, this.sigma, this.options);
        this.onSigmaResize();
    }

    private onSigmaResize() {
        let width = this.sigma.getContainer().offsetWidth;
        let height = this.sigma.getContainer().offsetHeight;
        let pixelRatio = getPixelRatio();

        this.canvas.setAttribute("width", width * pixelRatio + "px");
        this.canvas.setAttribute("height", height * pixelRatio + "px");
        this.canvas.style.width = width + "px";
        this.canvas.style.height = height + "px";
        this.gl.viewport(0, 0, width * pixelRatio, height * pixelRatio);
    }

    private onSigmaKill(): void {
        this.logger?.debug("Killing Helveg's effects extension of Sigma.js.");
        if (this.renderFrame) {
            cancelAnimationFrame(this.renderFrame);
            this.renderFrame = 0;
        }

        this.canvas.remove();
        this.canvas = null!;
        this.sigma = null!;
        this.fireProgram = null!;
    }

    private onSigmaRender(params: RenderParams): void {
        this.renderParams = params;
        this.gl.clear(this.gl.COLOR_BUFFER_BIT);

        if (this.options.showFire) {
            this.fireProgram.render(this.renderParams);
            this.requestRender();
        }
    }

    private requestRender() {
        if (this.options.showFire
            && this.options.isFireAnimated
            && this.options.particleCount > 0
            && this.renderFrame === 0) {
            this.renderFrame = requestAnimationFrame(() => {
                this.renderFrame = 0;
                if (this.renderParams) {
                    this.gl.clear(this.gl.COLOR_BUFFER_BIT);
                    this.fireProgram.render(this.renderParams);
                    this.requestRender();
                }
            });
        }
    }
}
