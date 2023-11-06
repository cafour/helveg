import * as esbuild from "esbuild";
const context = await esbuild.context({
    entryPoints: ["mod.ts"],
    outfile: "./dist/helveg-diagram.js",
    platform: "browser",
    target: "esnext",
    format: "esm",
    bundle: true,
    plugins: [
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
