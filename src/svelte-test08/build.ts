import esbuild from "esbuild";
import esbuildSvelte from "esbuild-svelte";
import sveltePreprocess from "svelte-preprocess";

await esbuild.build({
    entryPoints: ["main.ts"],
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
        })
    ],
    loader: {
        ".svg": "text"
    }
})
.catch(() => process.exit(1));
