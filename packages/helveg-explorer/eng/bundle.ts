import * as esbuild from "esbuild";
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
        minify: { type: "boolean", default: false }
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
    plugins: [
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
        }),
        pluginGlobals({
            "@cafour/helveg-diagram": "helveg"
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
  