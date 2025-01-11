import { Sigma, RenderParams, ProgramInfo, InstancedProgramDefinition, floatColor } from "../deps/sigma.ts";
import { HelvegNodeAttributes } from "../model/graph.ts";
import { FALLBACK_NODE_ICON, FALLBACK_NODE_STYLE } from "../model/style.ts";
import { EMPTY_ICON_ATLAS, IconAtlas, IconAtlasEntryStatus } from "./iconAtlas.ts";
import vertSrc from "./shaders/node.icon.vert";
import fragSrc from "./shaders/node.icon.frag";
import { HelvegNodeProgram, HelvegNodeProgramType } from "../diagram/initializers.ts";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_correctionRatio", "u_matrix", "u_atlas", "u_invert"];

export interface IconProgramOptions {
    iconAtlas: Readonly<IconAtlas>;
    showOnlyHighlighted: boolean;
}

export const DEFAULT_ICON_PROGRAM_OPTIONS: IconProgramOptions = {
    iconAtlas: EMPTY_ICON_ATLAS,
    showOnlyHighlighted: false
};

export default function createIconProgram(options: IconProgramOptions): HelvegNodeProgramType {
    return class extends IconProgram {
        constructor(gl: WebGLRenderingContext, pickingBuffer: WebGLFramebuffer, renderer: Sigma) {
            super(gl, pickingBuffer, renderer, options);
        }
    };
}

export class IconProgram extends HelvegNodeProgram<typeof UNIFORMS[number]> {
    texture: WebGLTexture;

    constructor(
        gl: WebGLRenderingContext,
        pickingBuffer: WebGLFramebuffer,
        renderer: Sigma,
        private options: IconProgramOptions
    ) {
        super(gl, pickingBuffer, renderer);

        this.texture = gl.createTexture() as WebGLTexture;
        options.iconAtlas.redrawn.subscribe(a => {
            this.rebindTexture(a, gl);
            this.renderer.scheduleRefresh();
        });
        this.rebindTexture(options.iconAtlas, gl);
    }

    getDefinition(): InstancedProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 3,
            VERTEX_SHADER_SOURCE: vertSrc,
            FRAGMENT_SHADER_SOURCE: fragSrc,
            METHOD: WebGL2RenderingContext.TRIANGLES,
            UNIFORMS,
            ATTRIBUTES: [
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_iconSize", size: 1, type: FLOAT },
                {
                    name: "a_color", size: 4,
                    type: UNSIGNED_BYTE,
                    normalized: true,
                },
                { name: "a_texture", size: 4, type: FLOAT },
            ],
            // NB: Data for an equilateral triangle that the donut is carved from.
            CONSTANT_ATTRIBUTES: [{ name: "a_angle", size: 1, type: FLOAT }],
            CONSTANT_DATA: [[(0 * Math.PI) / 3], [(2 * Math.PI) / 3], [(4 * Math.PI) / 3]],
        };
    }

    processVisibleItem(nodeIndex: number, offset: number, data: HelvegNodeAttributes): void {
        const array = this.array;
        this.options.iconAtlas.tryAddIcon(data.icon ?? FALLBACK_NODE_ICON);

        const useColor = (!this.options.showOnlyHighlighted || data.highlighted === true);

        array[offset++] = data.x ?? 0;
        array[offset++] = data.y ?? 0;
        array[offset++] = data.iconSize ?? data.baseSize ?? data.size ?? 1;
        array[offset++] = floatColor(
            useColor ? data.color ?? FALLBACK_NODE_STYLE.color : "#777777"
        );

        let atlasEntry = this.options.iconAtlas.entries[data.icon ?? FALLBACK_NODE_ICON];
        if (atlasEntry && atlasEntry.status === IconAtlasEntryStatus.Rendered) {
            array[offset++] = atlasEntry.x / this.options.iconAtlas.width;
            array[offset++] = atlasEntry.y / this.options.iconAtlas.height;
            array[offset++] = this.options.iconAtlas.iconSize / this.options.iconAtlas.width;
            array[offset++] = this.options.iconAtlas.iconSize / this.options.iconAtlas.height;
        } else {
            // the icon is not ready yet, so don't render it
            array[offset++] = 0;
            array[offset++] = 0;
            array[offset++] = 0;
            array[offset++] = 0;
        }

    }

    setUniforms(params: RenderParams, programInfo: ProgramInfo): void {
        const { gl, uniformLocations } = programInfo;
        const { u_sizeRatio, u_pixelRatio, u_correctionRatio, u_matrix, u_atlas, u_invert } = uniformLocations;

        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniform1f(u_correctionRatio, params.correctionRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1i(u_atlas, 0);
        gl.uniform1f(u_invert, 1);
    }

    protected renderProgram(params: RenderParams, programInfo: ProgramInfo): void {
        if (programInfo.isPicking) {
            return;
        }

        const { gl } = programInfo;
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, this.texture);
        gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
        super.renderProgram(params, programInfo);
    }


    private rebindTexture(iconAtlas: Readonly<IconAtlas>, gl: WebGLRenderingContext) {
        gl.bindTexture(gl.TEXTURE_2D, this.texture);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, iconAtlas.texture);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR_MIPMAP_LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
        gl.generateMipmap(gl.TEXTURE_2D);
    }
}
