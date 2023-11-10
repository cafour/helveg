import * as esbuild from "esbuild";
import { copy } from "esbuild-plugin-copy";
import esbuildSvelte from "esbuild-svelte";
import { sassPlugin } from "esbuild-sass-plugin"
import pluginGlobals from "esbuild-plugin-globals";
import sveltePreprocess from "svelte-preprocess";
import yargs from "yargs";
import { hideBin } from "yargs/helpers";
import postcss from "postcss";
import postcssPresetEnv from "postcss-preset-env";
import autoprefixer from "autoprefixer";

const argv = yargs(hideBin(process.argv))
    .options({
        watch: { type: "boolean", default: false },
        serve: { type: "boolean", default: false },
        sourcemap: { type: "boolean", default: false },
        minify: { type: "boolean", default: false },
        diagramPath: { type: "string", default: "../helveg-diagram/dist/helveg-diagram.js" }
    }).parseSync();

const mod = await esbuild.context({
    entryPoints: ["./mod.ts"],
    format: "iife",
    outfile: "./dist/helveg-explorer.js",
    mainFields: ["svelte", "browser", "module", "main"],
    sourcemap: argv.sourcemap,
    splitting: false,
    write: true,
    bundle: true,
    platform: "browser",
    target: "esnext",
    tsconfig: "./tsconfig.json",
    keepNames: false,
    plugins: [
        esbuildSvelte({
            preprocess: sveltePreprocess(),
        }),
        sassPlugin({
            loadPaths: [
                "./node_modules/uniformcss"
            ]
        }),
        pluginGlobals({
            "@cafour/helveg-diagram": "helveg"
        }),
        copy({
            watch: true,
            resolveFrom: "cwd",
            assets: [
                {
                    from: ["./eng/template/*"],
                    to: ["./dist"]
                },
                {
                    from: [argv.diagramPath],
                    to: ["./dist"]
                }
            ],
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

async function main()
{
    if (argv.watch || argv.serve) {
        await mod.watch();
    }
    else {
        await mod.rebuild();
        await mod.dispose();
    }
    
    if (argv.serve) {
        await mod.serve({
            servedir: "./dist/",
            port: 44347,
            host: "0.0.0.0"
        })
    }
}

async function stop()
{
    await mod.dispose();
    process.exit(0);
}

main();
process.on("beforeExit", stop);
process.on("exit", stop);
process.on("SIGINT", stop);
process.on("SIGTERM", stop);
