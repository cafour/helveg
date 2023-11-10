import * as esbuild from "esbuild";
import inlineBundlePlugin from "./inlineBundle.ts";
import { copy } from "esbuild-plugin-copy";
import yargs from "yargs";
import { hideBin } from "yargs/helpers";

const argv = yargs(hideBin(process.argv))
  .options({
    watch: { type: "boolean", default: false },
    serve: { type: "boolean", default: false },
    sourcemap: { type: "boolean", default: false }
  }).parseSync();

const mod = await esbuild.context({
  entryPoints: ["./mod.ts"],
  outfile: "./dist/helveg-diagram.js",
  platform: "browser",
  target: "esnext",
  format: "iife",
  tsconfig: "./tsconfig.json",
  sourcemap: argv.sourcemap,
  bundle: true,
  plugins: [
    inlineBundlePlugin(),
    ...(argv.serve ? [copy({
      watch: true,
      resolveFrom: "cwd",
      assets: {
        from: ["./eng/template/*"],
        to: ["./dist"]
      }
    })] : [])
  ],
  loader: {
    ".svg": "text",
    ".html": "copy",
    ".vert": "text",
    ".frag": "text"
  },
});

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
    port: 44342,
    host: "0.0.0.0"
  })
}
