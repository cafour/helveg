import esbuild from "esbuild";
import esbuildSvelte from "esbuild-svelte";
import { sassPlugin } from "esbuild-sass-plugin"
import sveltePreprocess from "svelte-preprocess";
// import prerenderPlugin from "./prerender.js";
import yargs from "yargs";

const args = <any>yargs(process.argv).argv;
const isRelease = args["release"] === true;
let isDebug = args["debug"] === true;
const isWatch = args["watch"] === true;

if (isRelease && isDebug) {
    throw new Error("A build can't be both --debug and --release.");
}

if (!isRelease && !isDebug) {
    isDebug = true;
}

const context = await esbuild.context({
    entryPoints: ["helveg.ts", "index.html"],
    format: "iife",
    outdir: "../obj/esbuild",
    mainFields: ["svelte", "browser", "module", "main"],
    sourcemap: isDebug,
    splitting: false,
    write: true,
    logLevel: "info",
    bundle: true,
    minify: isRelease,
    plugins: [
        esbuildSvelte({
            preprocess: sveltePreprocess()
        }),
        sassPlugin({
            loadPaths: [
                "./node_modules/uniformcss"
            ]
        })
    ],
    loader: {
        ".svg": "text",
        ".html": "copy"
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
