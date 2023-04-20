/// Inspired by Guillame Plique's sigma-experiments
/// https://github.com/Yomguithereal/sigma-experiments/blob/master/renderers/src/node/node.pictogram.ts

import { HelvegEvent } from "common/event";
import { getIconDataUrl } from "model/icons";

export interface IconAtlasOptions {
    iconSize: number;
}

export const DEFAULT_OPTIONS: IconAtlasOptions = {
    iconSize: 256
};

const MAX_CANVAS_WIDTH = 3072;

export enum IconEntryStatus {
    Created,
    Loaded,
    Rendered,
    Failed
}

export interface IconAtlasEntry {
    status: IconEntryStatus;
    iconName: string;
    x: number;
    y: number;
}

interface PendingImage {
    element: HTMLImageElement;
    entry: IconAtlasEntry;
}

export class IconAtlas {
    private static atlasId = 0;

    id: number;
    width: number;
    height: number;
    texture: ImageData;
    redrawn: HelvegEvent<IconAtlas> = new HelvegEvent("helveg.IconAtlas.redrawn");
    entries: Record<string, IconAtlasEntry> = {};

    get iconSize() {
        return this.options.iconSize;
    }

    private options: IconAtlasOptions;
    private writePositionX = 0;
    private writePositionY = 0;
    private pendingImages: PendingImage[] = [];

    constructor(options?: IconAtlasOptions) {
        this.options = { ...DEFAULT_OPTIONS, ...options };
        this.id = ++IconAtlas.atlasId;
        this.width = this.options.iconSize;
        this.height = this.options.iconSize;
        this.texture = new ImageData(this.width, this.height);
    }

    addIcon(name: string) {
        if (this.entries[name]) {
            return;
        }

        let entry = this.entries[name] = <IconAtlasEntry>{
            status: IconEntryStatus.Created,
            iconName: name,
            x: 0,
            y: 0
        };

        const image = new Image(this.options.iconSize, this.options.iconSize);
        image.id = `IconAtlas-${this.id}-${name}`;
        image.addEventListener("load", () => {
            entry.status = IconEntryStatus.Loaded;
            this.pendingImages.push({
                element: image,
                entry: entry
            });
            requestAnimationFrame(() => this.redraw());
        })
        image.addEventListener("error", () => {
            entry.status = IconEntryStatus.Failed;
            console.error(`Failed to load ${image.id}.`);
        });
        image.setAttribute("crossorigin", "");
        image.src = getIconDataUrl(name, { width: this.options.iconSize });
    }

    private redraw() {
        const canvas = document.createElement("canvas");
        canvas.width = this.width;
        canvas.height = this.height;
        const ctx = canvas.getContext("2d", { willReadFrequently: true });
        if (!ctx) {
            throw new Error("Unable to get 2d context for an IconAtlas canvas.");
        }

        // redraw the atlas so far
        ctx.putImageData(this.texture, 0, 0);

        if (this.pendingImages.length === 0) {
            return;
        }

        this.pendingImages.forEach(image => {
            let { element, entry } = image;
            if (this.writePositionX + this.options.iconSize > MAX_CANVAS_WIDTH) {
                // the row is full
                this.writePositionX = 0;
                this.writePositionY += this.options.iconSize;
            }
            else {
                this.writePositionX += this.options.iconSize;
            }
            this.width = Math.max(this.width, this.writePositionX + this.options.iconSize);
            this.height = Math.max(this.height, this.writePositionY + this.options.iconSize);

            if (this.width !== canvas.width || this.height !== canvas.height) {
                // resize the canvas
                canvas.width = this.width;
                canvas.height = this.height;

                // redraw the atlas so far
                ctx.putImageData(this.texture, 0, 0);
            }

            ctx.drawImage(
                element,
                0,
                0,
                element.width,
                element.height,
                this.writePositionX,
                this.writePositionY,
                this.options.iconSize,
                this.options.iconSize
            );
            entry.x = this.writePositionX;
            entry.y = this.writePositionY;
            entry.status = IconEntryStatus.Rendered;
        });
        this.pendingImages = [];
        this.texture = ctx.getImageData(0, 0, canvas.width, canvas.height);
        this.redrawn.trigger(this);
    }
}
