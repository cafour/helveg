import esbuild from "esbuild";

const InlineBundlePluginName = "inline-bundle";
const InlineBundleFilter = /^inline-bundle:/;

interface InlineBundlePluginOptions {
    outFile?: string,
    appEntryPoint?: string
}

export class InlineBundlePlugin
{
    constructor(public options: InlineBundlePluginOptions) {
    }
    
    setup(build: esbuild.PluginBuild) {
        build.onResolve({ filter: InlineBundleFilter }, async args => {
            let path = args.path.replace(InlineBundleFilter, "");
            const resolved = await build.resolve(path, args);
            return {
                path: resolved.path,
                namespace: InlineBundlePluginName
            };
        });
        
        build.onLoad({filter: InlineBundleFilter, namespace: InlineBundlePluginName}, async args => {
            const inlineBundleCode = await esbuild.build({
                format: "iife",
                platform: "browser",
                bundle: true,
                write: false,
                entryPoints: [ args.path ]
            });
            return {
                contents: inlineBundleCode.outputFiles[0].text,
                loader: "text"
            };
        });
    }
}

export default function inlineBundlePlugin(options?: InlineBundlePluginOptions) {
    var plugin = new InlineBundlePlugin(options ?? {});
    return {
        name: InlineBundlePluginName,
        setup: plugin.setup.bind(plugin)
    };
}
