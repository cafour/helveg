<script lang="ts">
    import { getContext, onMount } from "svelte";
    import {
        type AbstractNodeProgram,
        Diagram,
        type HelvegNodeAttributes,
    } from "../deps/helveg-diagram.ts";
    import {
        createDonutProgram,
        type NodeDisplayData,
    } from "../deps/helveg-diagram.ts";

    export let node: HelvegNodeAttributes | undefined = undefined;
    $: renderNode(node);

    let canvas: HTMLCanvasElement;
    const observer = new ResizeObserver(() => resizeCanvas());
    let diagram = getContext<Diagram>("diagram");

    let gl: WebGLRenderingContext | null = null;
    let program: AbstractNodeProgram;

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
        const programType = createDonutProgram(diagram.glyphProgramOptions);
        program = new programType(gl, null, null!);
        program.reallocate(1);
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
            size: 0,
            color: "#000000",
            hidden: false,
            forceLabel: false,
            type: "<invalid>",
            zIndex: 0,
            highlighted: true,
        };
        program.process(0, 0, nodeData);
        program.render({
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
        });
    }
</script>

<canvas bind:this={canvas} style="width: 100%; aspect-ratio: 1/1;"></canvas>
