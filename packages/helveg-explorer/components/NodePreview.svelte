<script lang="ts">
    import { getContext, onMount } from "svelte";
    import {
        type AbstractNodeProgram,
        Diagram,
        type HelvegNodeAttributes,
        type RenderParams,
    } from "../deps/helveg-diagram.ts";
    import {
        createDonutProgram,
        createIconProgram,
        type NodeDisplayData,
    } from "../deps/helveg-diagram.ts";

    export let node: HelvegNodeAttributes | undefined = undefined;
    $: renderNode(node);

    let canvas: HTMLCanvasElement;
    const observer = new ResizeObserver(() => resizeCanvas());
    let diagram = getContext<Diagram>("diagram");

    let gl: WebGLRenderingContext | null = null;
    let donutProgram: AbstractNodeProgram;
    let iconProgram: AbstractNodeProgram;

    function resizeCanvas() {
        canvas.width = canvas.clientWidth * window.devicePixelRatio;
        canvas.height = canvas.clientHeight * window.devicePixelRatio;
        renderNode(node);
    }

    onMount(() => {
        // canvas.addEventListener("resize", () => resizeCanvas());
        // resizeCanvas();
        observer.observe(canvas);

        gl = canvas.getContext("webgl2", {
            preserveDrawingBuffer: false,
            antialias: false,
        });
        if (!gl) {
            throw Error("Cannot obtain a WebGL2 context.");
        }
        const pickingBuffer = gl.createFramebuffer();
        const donutProgramType = createDonutProgram(diagram.glyphProgramOptions);
        donutProgram = new donutProgramType(gl, null, null!);
        donutProgram.reallocate(1);
        const iconProgramType = createIconProgram(diagram.glyphProgramOptions);
        iconProgram = new iconProgramType(gl, null!, null!);
        iconProgram.reallocate(1);
    });

    function renderNode(node?: HelvegNodeAttributes) {
        if (!gl || gl.drawingBufferWidth <= 1 || gl.drawingBufferHeight <= 1) {
            return;
        }

        gl.clear(gl.COLOR_BUFFER_BIT);

        if (!node) {
            return;
        }

        const nodeData: HelvegNodeAttributes & NodeDisplayData = {
            ...node,
            x: 0,
            y: 0,
            label: null,
            hidden: false,
            highlighted: true,
            collapsed: false,
        };
        
        const params: RenderParams = {
            // NB: sigma's program later multiplies width and height below with pixelRatio that's why we don't
            width: canvas.clientWidth,
            height: canvas.clientHeight,
            sizeRatio: (nodeData.size * 2) / canvas.width,
            zoomRatio: 1,
            pixelRatio: window.devicePixelRatio,
            cameraAngle: 0,
            correctionRatio: 1,
            downSizingRatio: 1,
            minEdgeThickness: 0,
            antiAliasingFeather: 0,
            matrix: new Float32Array([
                ...[1 / canvas.width, 0, 0],
                ...[0, 1 / canvas.height, 0],
                ...[0, 0, 1],
            ]),
            invMatrix: new Float32Array([
                ...[canvas.width, 0, 0],
                ...[0, canvas.height, 0],
                ...[0, 0, 1],
            ]),
        };
        donutProgram.process(0, 0, nodeData);
        donutProgram.render(params);
        iconProgram.process(0, 0, nodeData);
        iconProgram.render(params);
    }
</script>

<canvas bind:this={canvas} style="width: 100%; aspect-ratio: 1/1;"></canvas>
