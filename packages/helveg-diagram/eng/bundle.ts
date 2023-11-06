import * as esbuild from "esbuild";
import inlineBundlePlugin from "./inlineBundle.ts";

const context = await esbuild.context({
  entryPoints: ["mod.ts"],
  outfile: "./dist/helveg-diagram.js",
  platform: "browser",
  target: "esnext",
  format: "esm",
  tsconfig: "./tsconfig.json",
  bundle: true,
  plugins: [
    inlineBundlePlugin()
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
