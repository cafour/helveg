import * as esbuild from "esbuild";
import inlineBundlePlugin from "./inlineBundle.ts";

const mod = await esbuild.context({
  entryPoints: ["./mod.ts"],
  globalName: "helveg",
  outfile: "./dist/helveg-diagram.js",
  platform: "browser",
  target: "esnext",
  format: "iife",
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

await mod.rebuild();
await mod.dispose();
