import esbuild from "esbuild";
import esbuildSvelte from "esbuild-svelte";
import { sassPlugin } from "esbuild-sass-plugin"
import stylePlugin from "esbuild-style-plugin";
import postcss from "postcss";
import postcssImport from "postcss-import";
import tailwindcss from "tailwindcss";
import autoprefixer from "autoprefixer";
import sveltePreprocess from "svelte-preprocess";
import prerenderPlugin from "./prerender.js";
import yargs from "yargs";
import { copy } from 'esbuild-plugin-copy';
import path from "path";
import skeletonTailwindPlugin from "@skeletonlabs/skeleton/tailwind/skeleton.cjs";
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);

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

const skeletonPath = require.resolve("@skeletonlabs/skeleton");

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
        stylePlugin({
            postcss: {
                plugins: [
                    postcssImport(),
                    tailwindcss({
                        content: [
                            "./**/*.{html,js,svelte,ts}",
                            path.join(skeletonPath, "../**/*.{html,js,svelte,ts}")
                        ],
                        darkMode: "class",
                        plugins: [
                            ...skeletonTailwindPlugin()
                        ]
                    }),
                    autoprefixer
                ]
            }
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
