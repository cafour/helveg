import esbuild from "esbuild";
import esbuildSvelte from "esbuild-svelte";
import { sassPlugin } from "esbuild-sass-plugin"
import sveltePreprocess from "svelte-preprocess";
// import prerenderPlugin from "./prerender.js";
import inlineBundlePlugin from "./inlineBundle.js";
import yargs from "yargs";
import fs from "fs";
import path from "path";

console.log(process.cwd());

const args = <any>yargs(process.argv).argv;
const isRelease = args["release"] === true;
let isDebug = args["debug"] === true;
const isWatch = args["watch"] === true;
const useTemplate = args["useTemplate"] === true;
const outDir = "../obj/esbuild";

if (isRelease && isDebug) {
    throw new Error("A build can't be both --debug and --release.");
}

if (!isRelease && !isDebug) {
    isDebug = true;
}

if (useTemplate) {
    if (!fs.existsSync(outDir)) {
        fs.mkdirSync(outDir);
    }
    fs.copyFileSync("./template/index.html", path.join(outDir, "index.html"));
    fs.copyFileSync("./template/icons-base.json", path.join(outDir, "icons-base.json"));
    fs.copyFileSync("./template/icons-csharp.json", path.join(outDir, "icons-csharp.json"));
    fs.copyFileSync("./template/helveg-data.json", path.join(outDir, "helveg-data.json"));
}

const context = await esbuild.context({
    entryPoints: ["helveg.ts"],
    format: "iife",
    outdir: outDir,
    mainFields: ["svelte", "browser", "module", "main"],
    sourcemap: isDebug,
    splitting: false,
    write: true,
    logLevel: "info",
    bundle: true,
    platform: "browser",
    minify: isRelease,
    plugins: [
        inlineBundlePlugin(),
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
