import fs from "fs/promises";
import { existsSync } from "fs";
import path from "path";

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
    const iconEntries = await fs.readdir(namespaceDir, { withFileTypes: true });
    for (let icon of iconEntries) {
        const iconPath = path.join(icon.path, icon.name);
        let contents: string | undefined;
        let format: IconFormat | undefined;

        switch (path.extname(icon.name)) {
            case ".svg":
                format = "svg";
                contents = await fs.readFile(iconPath, {
                    encoding: "utf-8",
                    flag: "r"
                });
                break;
            case ".png":
                format = "png";
                let buffer = await fs.readFile(iconPath, {
                    flag: "r"
                })
                contents = buffer.toString("base64");
                break;
            default:
                console.log(`Ignoring file '${iconPath}'.`);
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
    await fs.writeFile(path.join(outDir, `./helveg-icons-${set.namespace}.json`), JSON.stringify(set));
}

export async function packIconSets(dirPath: string, outDir: string) {
    if (!existsSync(outDir)) {
        await fs.mkdir(outDir);
    }

    const setEntries = await fs.readdir(dirPath, { recursive: false, withFileTypes: true });
    for (const setEntry of setEntries) {
        await packIconSet(path.join(dirPath, setEntry.name), outDir);
    }
}
