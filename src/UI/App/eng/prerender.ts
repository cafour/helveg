import esbuild from "esbuild";
import fs from "fs";
import path from "path";
import { JSDOM } from "jsdom";
import { compile } from "svelte/compiler";

const HelvegPrerenderPluginName = "helveg-prerender";

const DefaultHtmlTemplate = `
<!DOCTYPE html>
<html lang="en">

  <head>
    <meta charset="utf-8" />
    <meta content="width=device-width, initial-scale=1.0" name="viewport" />
  </head>

  <body>
    <div id="app" />
  </body>

</html>
`;

interface HelvegPrerenderOptions {
    outFile?: string,
    appEntryPoint?: string
}

class HelvegPrerenderPlugin {
    options: HelvegPrerenderOptions;

    constructor(options?: HelvegPrerenderOptions) {
        this.options = options ?? {};

        this.options.outFile ??= "output.html";
        this.options.appEntryPoint ??= "./App.svelte";
    }

    setup(build: esbuild.PluginBuild) {
        build.onStart(() => {
            if (!build.initialOptions.metafile) {
                throw new Error("The 'metafile' build option is not enabled.");
            }

            if (!build.initialOptions.outdir) {
                throw new Error("The 'outdir' build option must be set.");
            }
        });

        build.onEnd(async result => {
            if (!result.metafile) {
                throw new Error("The metafile is missing.");
            }

            var outputs = this.getOutputs(result?.metafile);
            if (outputs.length == 0) {
                throw new Error("No JS output produced during build.");
            }

            if (outputs.length > 1) {
                throw new Error("More than one JS output produced during build.");
            }

            var output = outputs[0];
            const dom = new JSDOM(DefaultHtmlTemplate);
            const document = dom.window.document;

            if (output.cssBundle) {
                let styleElement = document.createElement("style");
                styleElement.innerText = await fs.promises.readFile(output.cssBundle, "utf-8");
                document.head.appendChild(styleElement);
            }

            let scriptElement = document.createElement("script");
            scriptElement.type = "text/javascript";
            scriptElement.innerText = await fs.promises.readFile(output.path, "utf-8");
            document.head.appendChild(scriptElement);

            let appSource = await fs.promises.readFile(this.options.appEntryPoint!, "utf-8");
            const wtf = compile(appSource);
            console.log(wtf);
        });
    }

    getOutputs(metafile: esbuild.Metafile) {
        return Object.entries(metafile?.outputs || {})
            .filter(([_, value]) => value.entryPoint != null)
            .map(([key, value]) => {
                return { path: key, ...value };
            });
    }
}

export default function prerenderPlugin(options?: HelvegPrerenderOptions): esbuild.Plugin {
    var plugin = new HelvegPrerenderPlugin(options);
    return {
        name: HelvegPrerenderPluginName,
        setup: plugin.setup.bind(plugin)
    };
}
