import * as esbuild from "esbuild";
import { denoPlugins } from "esbuild_deno_loader";

const context = await esbuild.context({
  entryPoints: ["mod.ts"],
  outfile: "./dist/mod.esm.js",
  platform: "browser",
  target: "esnext",
  format: "esm",
  bundle: true,
  plugins: [
    ...denoPlugins()
  ],
  loader: {
    ".svg": "text",
    ".html": "copy",
    ".vert": "text",
    ".frag": "text"
  },
});

await context.rebuild();
await context.dispose();

esbuild.stop();
