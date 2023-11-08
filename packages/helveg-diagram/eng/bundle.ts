import * as esbuild from "esbuild";
import inlineBundlePlugin from "./inlineBundle.ts";
import { copy } from "esbuild-plugin-copy";
import yargs from "yargs";
import { hideBin } from "yargs/helpers";

const argv = yargs(hideBin(process.argv))
  .options({
    watch: { type: "boolean", default: false }
  }).parseSync();

const mod = await esbuild.context({
  entryPoints: ["./mod.ts"],
  outfile: "./dist/helveg-diagram.js",
  platform: "browser",
  target: "esnext",
  format: "iife",
  tsconfig: "./tsconfig.json",
  bundle: true,
  plugins: [
    inlineBundlePlugin(),
    copy({
      watch: true,
      resolveFrom: "cwd",
      assets: {
        from: "./eng/template.html",
        to: "./dist/index.html"
      }
    })
  ],
  loader: {
    ".svg": "text",
    ".html": "copy",
    ".vert": "text",
    ".frag": "text"
  },
});

if (argv.watch) {
  await mod.watch();
}
else {
  await mod.rebuild();
  await mod.dispose();
}
