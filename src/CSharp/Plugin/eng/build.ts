import esbuild from "esbuild";
import esbuildSvelte from "esbuild-svelte";
import sveltePreprocess from "svelte-preprocess";
import yargs from "yargs";
import globalsPlugin from "esbuild-plugin-globals";

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

// const helvegPlugin: esbuild.Plugin = {
//     name: "helveg-global",
//     setup(build) {
//         build.onResolve({ filter: /^helveg$/ }, _ => {
//             return { path: "helveg", namespace: "helveg" };
//         });
//         build.onLoad({ filter: /^helveg$/, namespace: "helveg" }, args => {
//             return { contents: `export default window.helveg[${JSON.stringify(args.)}];` };
//         });
//         build.
//     }
// };

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
        globalsPlugin({
            helveg: "window.helveg"
        }),
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
