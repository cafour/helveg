import { HelvegEvent } from "../common/event.ts";
import { ILogger } from "./logger.ts";

export enum IconFormat {
    Unknown = "Unknown",
    Svg = "Svg",
    Png = "Png"
}

export interface Icon {
    name: string;
    format: IconFormat,
    data: string;
};

export interface IconSet {
    namespace: string;
    icons: Record<string, Icon>;
}

export interface IconOptions {
    width?: number;
    height?: number;
    viewBox?: string;
    removeTitle?: boolean;
    viewBoxOnly?: boolean;
    overrideExisting?: boolean;
}

export const DEFAULT_ICON_OPTIONS: IconOptions = {
    removeTitle: true
}

// icon by Utkarsh Verma (https://github.com/n3r4zzurr0/svg-spinners)
const FALLBACK_ICON_SOURCE =
    `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"><circle cx="4" cy="12" r="3" fill="currentColor"><animate id="svgSpinners3DotsBounce0" attributeName="cy" begin="0;svgSpinners3DotsBounce1.end+0.25s" calcMode="spline" dur="0.6s" keySplines=".33,.66,.66,1;.33,0,.66,.33" values="12;6;12"/></circle><circle cx="12" cy="12" r="3" fill="currentColor"><animate attributeName="cy" begin="svgSpinners3DotsBounce0.begin+0.1s" calcMode="spline" dur="0.6s" keySplines=".33,.66,.66,1;.33,0,.66,.33" values="12;6;12"/></circle><circle cx="20" cy="12" r="3" fill="currentColor"><animate id="svgSpinners3DotsBounce1" attributeName="cy" begin="svgSpinners3DotsBounce0.begin+0.2s" calcMode="spline" dur="0.6s" keySplines=".33,.66,.66,1;.33,0,.66,.33" values="12;6;12"/></circle></svg>
`

export const FALLBACK_ICON: Icon = {
    name: "Fallback",
    data: FALLBACK_ICON_SOURCE,
    format: IconFormat.Svg
};

export class IconRegistry {
    private sets: Record<string, IconSet> = {};
    private _setAdded = new HelvegEvent<string>("helveg.IconRegistry.setAdded", true);
    
    constructor(public logger?: ILogger) {
    }

    register(set: IconSet) {
        if (this.sets[set.namespace]) {
            throw new Error(`Icon set with namespace '${set.namespace}' already registered.`);
        }
        this.sets[set.namespace] = set;
        this.setAdded.trigger(set.namespace);
    }

    get(name: string, options?: IconOptions): Icon {
        options = { ...DEFAULT_ICON_OPTIONS, ...options };

        if (name === FALLBACK_ICON.name) {
            return cloneIcon(FALLBACK_ICON, options);
        }

        const segments = name.split(":", 2);
        const namespace = segments[0];
        const iconName = segments[1];
        const iconSet = this.sets[namespace];
        let icon: Icon | null = null;
        if (!iconSet) {
            this.logger?.warn(`Icon set for namespace '${namespace}' could not be found. Using fallback icon.`)
            icon = FALLBACK_ICON;
        }
        else {
            icon = structuredClone(iconSet.icons[iconName]);
            if (!icon) {
                this.logger?.warn(`Icon '${name}' could not be found. Using fallback icon.`);
                icon = FALLBACK_ICON;
            }
        }

        if (icon.format === IconFormat.Svg) {
            icon.data = cloneSvgIcon(icon.data, options ?? { removeTitle: true });
        }
        return icon;
    }

    getIconDataUrl(name: string, options?: IconOptions): string {
        let icon = this.get(name, options);
        switch (icon.format) {
            case IconFormat.Svg:
                return svgToDataURI(icon.data);
            case IconFormat.Png:
                return `data:image/png;base64,${icon.data}`;
            default:
                throw new Error(`IconFormat '${icon.format}' is not supported.`);
        }
    }
    
    get setAdded() {
        return this._setAdded;
    }
}

export const EMPTY_ICON_REGISTRY: Readonly<IconRegistry> = new IconRegistry();

function removeIconTitle(svg: Document) {
    [...svg.getElementsByTagName("title")].forEach(e => e.remove());
}

function setIconSize(svg: Document, options: IconOptions): string {
    if (options.viewBoxOnly) {
        svg.documentElement.removeAttribute("width");
        svg.documentElement.removeAttribute("height");
    }

    if (options.width && (options.overrideExisting || !svg.documentElement.hasAttribute("width"))) {
        svg.documentElement.setAttribute("width", options.width.toString());
    }
    if (options.height && (options.overrideExisting || !svg.documentElement.hasAttribute("height"))) {
        svg.documentElement.setAttribute("height", options.height.toString());
    }
    if (options.viewBox && (options.overrideExisting || !svg.documentElement.hasAttribute("viewBox"))) {
        svg.documentElement.setAttribute("viewBox", options.viewBox);
    }
    return new XMLSerializer().serializeToString(svg);
}

function cloneIcon(icon: Icon, options: IconOptions): Icon {
    let clone = structuredClone(icon);
    if (clone.format === IconFormat.Svg) {
        clone.data = cloneSvgIcon(clone.data, options);
    }
    return clone;
}

function cloneSvgIcon(svgString: string, options: IconOptions): string {
    const svg = new DOMParser().parseFromString(svgString, "image/svg+xml");
    if (options.removeTitle) {
        removeIconTitle(svg);
    }
    setIconSize(svg, options);
    return new XMLSerializer().serializeToString(svg);
}

// Based on https://github.com/heyallan/svg-to-data-uri
function svgToDataURI(svg: string): string {
    svg = svg.trim();
    // remove xml, doctype, generator...
    svg = svg.slice(svg.indexOf('<svg'));
    // soft validate
    if (!svg.startsWith('<svg') || !svg.endsWith('svg>'))
        throw new Error("The string is not an svg.");
    // add namespace if necessary
    if (!svg.includes('http://www.w3.org/2000/svg'))
        svg = svg.replace(/<svg/g, `<svg xmlns='http://www.w3.org/2000/svg'`);
    // remove comments
    svg = svg.replace(/<!--.{1,}-->/g, '');
    // remove unnecessary attributes
    svg = svg.replace(/version=[\"\'](.{0,}?)[\"\'](?=[\s>])/g, '');
    // svg = svg.replace(/id=[\"\'](.{0,}?)[\"\'](?=[\s>])/g, '');
    // svg = svg.replace(/class=[\"\'](.{0,}?)[\"\'](?=[\s>])/g, '');
    // replace nested quotes
    svg = svg.replace(/"'(.{1,})'"/g, '\'$1\'');
    // replace double quotes
    svg = svg.replace(/"/g, '\'');
    // remove empty spaces between tags
    svg = svg.replace(/>\s{1,}</g, '><');
    // remove duplicate spaces
    svg = svg.replace(/\s{2,}/g, ' ');
    // trim again
    svg = svg.trim();
    // soft validate again
    if (!(svg.startsWith('<svg')) || !(svg.endsWith('svg>')))
        throw new Error("The string is not an svg.");
    // replace ampersand
    svg = svg.replace(/&/g, '&amp;');
    // encode only unsafe symbols
    svg = svg.replace(/[%#<>?\[\\\]^`{|}]/g, encodeURIComponent);
    // build data uri
    svg = `data:image/svg+xml,${svg}`;
    // ok, ship it!
    return svg;
}

