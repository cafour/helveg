import esbuild from "esbuild";
import { sassPlugin } from "esbuild-sass-plugin";
import yargs from "yargs";

const args = yargs(process.argv).argv;

const isDebug = args.config === "Debug";
const isRelease = args.config === "Release";
const isWatch = args.watch === true;

await esbuild.build({
    entryPoints: ["Scripts/Index.ts"],
    format: "iife",
    bundle: true,
    outfile: "obj/esbuild/helveg.js",
    minify: isRelease,
    incremental: false,
    sourcemap: isDebug,
    target: ["es2020"],
    treeShaking: true,
    watch: isWatch,
    plugins: [
        sassPlugin()
    ]
});
