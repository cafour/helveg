import * as esbuild from "esbuild";
import { copy } from "esbuild-plugin-copy";
import fs from "fs/promises";
import path from "path";
import inlineBundlePlugin from "./inline-bundle.ts";
import { packIconSets } from "./pack-icons.ts";
import esbuildSvelte from "esbuild-svelte";
import { sassPlugin } from "esbuild-sass-plugin"
import sveltePreprocess from "svelte-preprocess";
import yargs from "yargs";
import { hideBin } from "yargs/helpers";

const args = yargs(hideBin(process.argv))
    .options({
        watch: { type: "boolean", default: false },
        serve: { type: "boolean", default: false },
        sourcemap: { type: "boolean", default: false },
        minify: { type: "boolean", default: false }
    }).parseSync();

const wgslLoader: esbuild.Plugin = {
    name: 'WgslLoader',
    setup(build) {
        build.onResolve({ filter: /\.wgsl$/ }, (args) => ({
            path: path.join(args.resolveDir, args.path),
            namespace: "WgslLoader",
        }))

        build.onLoad(
            { filter: /\.wgsl$/, namespace: "WgslLoader" },
            async (args) => ({
                contents: await fs.readFile(args.path, { encoding: "utf-8", flag: "r" }),
                loader: 'text',
            }),
        )
    },
};

const pngLoader: esbuild.Plugin = {
    name: 'PngLoader',
    setup(build) {
        build.onResolve({ filter: /\.png$/ }, (args) => ({
            path: path.join(args.resolveDir, args.path),
            namespace: "PngLoader",
        }))

        build.onLoad(
            { filter: /\.png$/, namespace: "PngLoader" },
            async (args) => ({
                contents: await fs.readFile(args.path, { encoding: "utf-8", flag: "r" }),
                loader: 'dataurl',
            }),
        )
    },
};

const buildContext = await esbuild.context({
    entryPoints: ["./mod.ts"],
    target: "esnext",
    platform: "browser",
    format: "iife",
    bundle: true,
    outfile: "./build/helveg-ash.js",
    sourcemap: true,
    metafile: true,
    plugins: [
        inlineBundlePlugin(),
        (esbuildSvelte as any)({
            preprocess: (sveltePreprocess as any)(),
        }),
        sassPlugin({
            loadPaths: [
                "./node_modules/uniformcss"
            ]
        }),
        wgslLoader,
        pngLoader,
        ...(args.watch ? [copy({
            watch: true,
            resolveFrom: "cwd",
            assets: {
                from: ["./eng/template/*"],
                to: ["./build"]
            }
        })] : [])
    ]
});

await packIconSets("./icons", "./build");

if (args.watch) {
    await buildContext.watch()

    await buildContext.serve({
        host: "127.0.0.1",
        servedir: "./build",
        port: 44364
    });
    console.log(`Listening at http://${args.host}:${args.port}`);
}
else {
    await buildContext.rebuild();
    await buildContext.dispose();
    esbuild.stop();
}
