import { Sigma, ProgramDefinition, RenderParams, ProgramInfo } from "../deps/sigma.ts";
import { HelvegNodeAttributes } from "../model/graph.ts";
import { FALLBACK_NODE_ICON } from "../model/style.ts";
import { IconAtlasEntryStatus, IconAtlas } from "./iconAtlas.ts";
import { PizzaProgramOptions } from "./pizza.ts";
import vertSrc from "./shaders/node.pizzaTopping.vert";
import fragSrc from "./shaders/node.pizzaTopping.frag";
import { FALLBACK_PIZZA_ICON } from "../global.ts";
import { HelvegNodeProgram } from "../diagram/initializers.ts";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_atlas"];

export class PizzaToppingProgram extends HelvegNodeProgram<typeof UNIFORMS[number]> {
    texture: WebGLTexture;

    constructor(
        gl: WebGLRenderingContext,
        pickingBuffer: WebGLFramebuffer,
        renderer: Sigma,
        private options: PizzaProgramOptions
    ) {
        super(gl, pickingBuffer, renderer);

        this.texture = gl.createTexture() as WebGLTexture;
        options.iconAtlas.redrawn.subscribe(a => {
            this.rebindTexture(a, gl);
            renderer.scheduleRefresh();
        });
        this.rebindTexture(options.iconAtlas, gl);
    }

    getDefinition(): ProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 1,
            VERTEX_SHADER_SOURCE: vertSrc,
            FRAGMENT_SHADER_SOURCE: fragSrc,
            METHOD: WebGL2RenderingContext.POINTS,
            UNIFORMS,
            ATTRIBUTES: [
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_size", size: 1, type: FLOAT },
                { name: "a_texture", size: 4, type: FLOAT }
            ],
        };
    }

    processVisibleItem(nodeIndex: number, offset: number, data: HelvegNodeAttributes): void {
        const array = this.array;
        const pizzaIcon = (data.kind ? this.options.pizzaToppings[data.kind] : FALLBACK_PIZZA_ICON)
            ?? FALLBACK_PIZZA_ICON;

        this.options.iconAtlas.tryAddIcon(pizzaIcon);

        const isVisible = !this.options.showOnlyHighlighted || data.highlighted === true;

        array[offset++] = data.x ?? 0;
        array[offset++] = data.y ?? 0;
        array[offset++] = isVisible ? data.size ?? 1 : 0;

        let atlasEntry = this.options.iconAtlas.entries[pizzaIcon];
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
        const { u_sizeRatio, u_pixelRatio, u_matrix, u_atlas } = uniformLocations;

        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, this.texture);
        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1i(u_atlas, 0);
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
