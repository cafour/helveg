import esbuild from "esbuild";
import esbuildSvelte from "esbuild-svelte";
import sveltePreprocess from "svelte-preprocess";
import prerenderPlugin from "./prerender.js";

await esbuild.build({
    entryPoints: ["main.client.ts"],
    format: "iife",
    outdir: "./dist",
    mainFields: ["svelte", "browser", "module", "main"],
    sourcemap: "inline",
    splitting: false,
    write: true,
    logLevel: "info",
    bundle: true,
    plugins: [
        esbuildSvelte({
            preprocess: sveltePreprocess()
        }),
        prerenderPlugin()
    ],
    loader: {
        ".svg": "text"
    },
    metafile: true
})
// .catch(() => process.exit(1));
