import * as esbuild from "esbuild";
import { denoResolverPlugin, denoLoaderPlugin } from "esbuild_deno_loader";
import { parseArgs } from "std/cli/parse_args.ts";
import { copy } from "npm:esbuild-plugin-copy";
import * as path from "std/path/posix/mod.ts";
import inlineBundlePlugin from "./inline-bundle.ts";
import { packIconSets } from "./pack-icons.ts";
import esbuildSvelte from "npm:esbuild-svelte";
import { sassPlugin } from "npm:esbuild-sass-plugin"
import sveltePreprocess from "npm:svelte-preprocess";

interface Args {
  watch: boolean;
  host: string;
  port: number;
}

const DEFAULT_ARGS: Args = {
  watch: false,
  host: "127.0.0.1",
  port: 44364
};

const args = { ...DEFAULT_ARGS, ...parseArgs<Args>(Deno.args) };

const wgslLoader: esbuild.Plugin = {
  name: 'WgslLoader',
  setup(build) {
    build.onResolve({ filter: /\.wgsl$/ }, (args) => ({
      path: path.join(args.resolveDir, args.path),
      namespace: "WgslLoader",
    }))

    build.onLoad(
      { filter: /\.wgsl$/, namespace: "WgslLoader" },
      async (args) => ({
        contents: await Deno.readTextFile(args.path),
        loader: 'text',
      }),
    )
  },
};

const pngLoader: esbuild.Plugin = {
  name: 'PngLoader',
  setup(build) {
    build.onResolve({ filter: /\.png$/ }, (args) => ({
      path: path.join(args.resolveDir, args.path),
      namespace: "PngLoader",
    }))

    build.onLoad(
      { filter: /\.png$/, namespace: "PngLoader" },
      async (args) => ({
        contents: await Deno.readFile(args.path),
        loader: 'dataurl',
      }),
    )
  },
};

const buildContext = await esbuild.context({
  entryPoints: ["./mod.ts"],
  target: "esnext",
  platform: "browser",
  format: "iife",
  bundle: true,
  outfile: "./build/helveg-ash.js",
  sourcemap: true,
  metafile: true,
  plugins: [
    inlineBundlePlugin(),
    denoResolverPlugin(),
    (esbuildSvelte as any)({
      preprocess: (sveltePreprocess as any)(),
    }),
    sassPlugin({
      loadPaths: [
        "./node_modules/uniformcss"
      ]
    }),
    wgslLoader,
    pngLoader,
    denoLoaderPlugin(),
    ...(args.watch ? [copy({
      watch: true,
      resolveFrom: "cwd",
      assets: {
        from: ["./eng/template/*"],
        to: ["./build"]
      }
    })] : [])
  ]
});

await packIconSets("./icons", "./build");

if (args.watch) {
  await buildContext.watch()

  await buildContext.serve({
    host: args.host,
    servedir: "./build",
    port: args.port
  });
  console.log(`Listening at http://${args.host}:${args.port}`);

  Deno.addSignalListener("SIGINT", async () => {
    await buildContext.dispose();
  })
}
else {
  await buildContext.rebuild();
  await buildContext.dispose();
  esbuild.stop();
}
