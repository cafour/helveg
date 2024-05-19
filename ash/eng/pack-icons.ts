import * as path from "std/path/mod.ts";
import { encodeBase64 } from "std/encoding/base64.ts";

// TODO: make a JSON schema for these. In the meantime keep it in sync with icons.ts
export type IconFormat = "svg" | "png";

export interface Icon {
    name: string;
    format: IconFormat,
    data: string;
};

export interface IconSet {
    namespace: string;
    icons: Icon[];
}

export async function packIconSet(namespaceDir: string, outDir: string) {
    const set: IconSet = {
        namespace: path.basename(namespaceDir),
        icons: []
    };
    const iconEntries = Deno.readDir(namespaceDir);
    for await (const icon of iconEntries) {
        const iconPath = path.join(namespaceDir, icon.name);
        let contents: string | undefined;
        let format: IconFormat | undefined;

        switch (path.extname(icon.name)) {
            case ".svg":
                format = "svg";
                contents = await Deno.readTextFile(iconPath);
                break;
            case ".png":
                format = "png";
                contents = encodeBase64(await Deno.readFile(iconPath));
                break;
            default:
                console.log(`Ignoring file at '${iconPath}'.`);
                break;
        }
        if (!format || !contents) {
            continue;
        }

        set.icons.push({
            name: icon.name.replace(/\.[^/.]+$/, ""),
            data: contents,
            format: format
        });
    }
    await Deno.writeTextFile(path.join(outDir, `./helveg-icons-${set.namespace}.json`), JSON.stringify(set));
}

export async function packIconSets(dirPath: string, outDir: string) {
    if (!await Deno.lstat(outDir)) {
        await Deno.mkdir(outDir);
    }
    
    const setEntries = Deno.readDir(dirPath);
    for await (const setEntry of setEntries) {
        await packIconSet(path.join(dirPath, setEntry.name), outDir);
    }
}

