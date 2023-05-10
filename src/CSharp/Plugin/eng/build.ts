import esbuild from "esbuild";
import esbuildSvelte from "esbuild-svelte";
import sveltePreprocess from "svelte-preprocess";
import yargs from "yargs";
import fs from "fs";
import path from "path";

const args = <any>yargs(process.argv).argv;
const isRelease = args["release"] === true;
let isDebug = args["debug"] === true;
const isWatch = args["watch"] === true;
const outDir = "../obj/esbuild";

if (isRelease && isDebug) {
    throw new Error("A build can't be both --debug and --release.");
}

if (!isRelease && !isDebug) {
    isDebug = true;
}

const context = await esbuild.context({
    entryPoints: ["src/plugin.ts"],
    format: "iife",
    outdir: outDir,
    mainFields: ["svelte", "browser", "module", "main"],
    sourcemap: isDebug,
    define: {
        "DEBUG": isDebug ? "true" : "false"
    },
    splitting: false,
    write: true,
    logLevel: "info",
    bundle: true,
    platform: "browser",
    minify: isRelease,
    tsconfig: "./tsconfig.json",
    plugins: [
        esbuildSvelte({
            preprocess: sveltePreprocess()
        })
    ],
    loader: {
        ".svg": "text",
        ".html": "copy",
        ".vert": "text",
        ".frag": "text"
    },
    metafile: true
})
    .catch(() => process.exit(1));

if (isWatch) {
    await context.watch()
        .catch(() => process.exit(1));
} else {
    await context.rebuild()
        .catch(() => process.exit(1));
    await context.dispose();
}
