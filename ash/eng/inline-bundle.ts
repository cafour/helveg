import * as esbuild from "esbuild";
import { denoResolverPlugin, denoLoaderPlugin } from "esbuild_deno_loader";

const InlineBundlePluginName = "inline-bundle";
const InlineBundleFilter = /^inline-bundle:/;

interface InlineBundlePluginOptions {
    outFile?: string,
    appEntryPoint?: string
}

export class InlineBundlePlugin {
    constructor(public options: InlineBundlePluginOptions) {
    }

    setup(build: esbuild.PluginBuild) {
        build.onResolve({ filter: InlineBundleFilter }, async args => {
            const path = args.path.replace(InlineBundleFilter, "");
            const resolved = await build.resolve(path, {
                pluginName: InlineBundlePluginName,
                importer: args.importer,
                kind: args.kind,
                namespace: args.namespace,
                pluginData: args.pluginData,
                resolveDir: args.resolveDir
            });
            return {
                path: resolved.path,
                namespace: InlineBundlePluginName
            };
        });

        build.onLoad({ filter: /.*/, namespace: InlineBundlePluginName }, async args => {
            const inlineBundleCode = await esbuild.build({
                format: "iife",
                platform: "browser",
                bundle: true,
                write: false,
                entryPoints: [`file://${args.path}`],
                plugins: [
                    denoResolverPlugin(),
                    denoLoaderPlugin(),
                ]
            });
            return {
                contents: inlineBundleCode.outputFiles[0].text,
                loader: "text"
            };
        });
    }
}

export default function inlineBundlePlugin(options?: InlineBundlePluginOptions) {
    const plugin = new InlineBundlePlugin(options ?? {});
    return {
        name: InlineBundlePluginName,
        setup: plugin.setup.bind(plugin)
    };
}
