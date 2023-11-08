/// Inspired by Guillame Plique's sigma-experiments
/// https://github.com/Yomguithereal/sigma-experiments/blob/master/renderers/src/node/node.pictogram.ts

import { HelvegEvent } from "../common/event.ts";
import { EMPTY_ICON_REGISTRY, IconRegistry } from "../model/icons.ts";

export interface IconAtlasOptions {
    iconSize: number;
}

export const DEFAULT_ICON_ATLAS_OPTIONS: IconAtlasOptions = {
    iconSize: 256
};

const MAX_CANVAS_WIDTH = 3072;

export enum IconAtlasEntryStatus {
    Created,
    Loaded,
    Rendered,
    Failed
}

export interface IconAtlasEntry {
    status: IconAtlasEntryStatus;
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

    constructor(private iconRegistry: IconRegistry, options?: IconAtlasOptions) {
        this.options = { ...DEFAULT_ICON_ATLAS_OPTIONS, ...options };
        this.id = ++IconAtlas.atlasId;
        this.width = this.options.iconSize;
        this.height = this.options.iconSize;
        this.texture = new ImageData(this.width, this.height);
    }

    /**
     * Attempts to add an icon into the atlas so that it can be rendered once it is loaded (asynchronously).
     * @returns true, if the icon has been added succesfully, false if it already exists in the atlas.
     */
    tryAddIcon(name: string): boolean {
        if (this.entries[name]) {
            return false;
        }

        let entry = this.entries[name] = <IconAtlasEntry>{
            status: IconAtlasEntryStatus.Created,
            iconName: name,
            x: 0,
            y: 0
        };

        const image = new Image(this.options.iconSize, this.options.iconSize);
        image.id = `IconAtlas-${this.id}-${name}`;
        image.addEventListener("load", () => {
            entry.status = IconAtlasEntryStatus.Loaded;
            if (this.pendingImages.length === 0) {
                requestAnimationFrame(() => this.redraw());
            }
            this.pendingImages.push({
                element: image,
                entry: entry
            });
        })
        image.addEventListener("error", e => {
            entry.status = IconAtlasEntryStatus.Failed;
            console.error(`Failed to load ${image.id} with: ${e.error}`);
        });
        image.setAttribute("crossorigin", "");
        image.src = this.iconRegistry.getIconDataUrl(name, {
            width: this.options.iconSize,
            height: this.options.iconSize,
            overrideExisting: true
        });
        return true;
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
            if (this.writePositionX + this.options.iconSize > MAX_CANVAS_WIDTH) {
                // the row is full
                this.writePositionX = 0;
                this.writePositionY += this.options.iconSize;
            }
            this.width = Math.max(this.width, this.writePositionX + this.options.iconSize);
            this.height = Math.max(this.height, this.writePositionY + this.options.iconSize);

            if (this.width !== canvas.width || this.height !== canvas.height) {
                this.texture = ctx.getImageData(0, 0, canvas.width, canvas.height);

                // resize the canvas
                canvas.width = this.width;
                canvas.height = this.height;

                // redraw the atlas so far
                ctx.putImageData(this.texture, 0, 0);
            }

            ctx.drawImage(
                image.element,
                0,
                0,
                image.element.naturalWidth,
                image.element.naturalHeight,
                this.writePositionX,
                this.writePositionY,
                this.options.iconSize,
                this.options.iconSize
            );
            image.entry.x = this.writePositionX;
            image.entry.y = this.writePositionY;
            image.entry.status = IconAtlasEntryStatus.Rendered;

            this.writePositionX += this.options.iconSize;
        });
        this.pendingImages = [];
        this.texture = ctx.getImageData(0, 0, canvas.width, canvas.height);
        this.redrawn.trigger(this);
    }
}

export const EMPTY_ICON_ATLAS: Readonly<IconAtlas> = new IconAtlas(EMPTY_ICON_REGISTRY as IconRegistry);
