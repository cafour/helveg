import esbuild from "esbuild";
import esbuildSvelte from "esbuild-svelte";
import { sassPlugin } from "esbuild-sass-plugin"
import sveltePreprocess from "svelte-preprocess";
// import prerenderPlugin from "./prerender.js";
import inlineBundlePlugin from "./inlineBundle.js";
import yargs from "yargs";
import fs from "fs";
import path from "path";
import postcss from "postcss";
import postcssPresetEnv from "postcss-preset-env";
import autoprefixer from "autoprefixer";

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
    entryPoints: ["src/helveg.ts"],
    format: "iife",
    globalName: "helveg",
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
        inlineBundlePlugin(),
        esbuildSvelte({
            preprocess: sveltePreprocess()
        }),
        sassPlugin({
            async transform(source, _, filePath) {
                const { css } = await postcss([
                    autoprefixer,
                    postcssPresetEnv({ stage: 0 })
                ]).process(source, { from: filePath });
                return css;
            },
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
