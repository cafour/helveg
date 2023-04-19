import { readable, type Readable } from "svelte/store";
import { loadJsonScripts } from "./data";

export enum IconFormat {
    Unknown = 0,
    Svg = 1,
    Png = 2
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
}

// icon by Utkarsh Verma (https://github.com/n3r4zzurr0/svg-spinners)
const fallbackIconSource =
    `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"><circle cx="4" cy="12" r="3" fill="currentColor"><animate id="svgSpinners3DotsBounce0" attributeName="cy" begin="0;svgSpinners3DotsBounce1.end+0.25s" calcMode="spline" dur="0.6s" keySplines=".33,.66,.66,1;.33,0,.66,.33" values="12;6;12"/></circle><circle cx="12" cy="12" r="3" fill="currentColor"><animate attributeName="cy" begin="svgSpinners3DotsBounce0.begin+0.1s" calcMode="spline" dur="0.6s" keySplines=".33,.66,.66,1;.33,0,.66,.33" values="12;6;12"/></circle><circle cx="20" cy="12" r="3" fill="currentColor"><animate id="svgSpinners3DotsBounce1" attributeName="cy" begin="svgSpinners3DotsBounce0.begin+0.2s" calcMode="spline" dur="0.6s" keySplines=".33,.66,.66,1;.33,0,.66,.33" values="12;6;12"/></circle></svg>
`

export const fallbackIcon: Icon = {
    name: "Fallback",
    data: fallbackIconSource,
    format: IconFormat.Svg
};

export async function loadIcons(): Promise<Record<string, IconSet>> {
    let result: Record<string, IconSet> = {};
    let iconSets = await loadJsonScripts<IconSet>("script.helveg-icons");
    iconSets.forEach(s => result[s.namespace] = s);
    return result;
}

export function getIcon(name: string, options?: IconOptions): Icon {
    if (!helveg.iconSets) {
        throw new Error("Icons have not been initialized yet.");
    }

    let segments = name.split(":", 2);
    let namespace = segments[0];
    let iconName = segments[1];
    let iconSet = helveg.iconSets[namespace];
    let icon: Icon | null = null;
    if (!iconSet) {
        console.warn(`Icon set for namespace '${namespace}' could not be found. Using fallback icon.`)
        icon = fallbackIcon;
    }
    else {
        icon = structuredClone(iconSet.icons[iconName]);
        if (!icon) {
            console.warn(`Icon '${name}' could not be found. Using fallback icon.`);
            icon = fallbackIcon;
        }
    }

    if (icon.format === IconFormat.Svg) {
        icon.data = tweakIcon(icon.data, options ?? { removeTitle: true });
    }
    return icon;
}

export function getIconReadable(name: string, options?: IconOptions): Readable<Icon> {
    return readable(getIcon(name, options), set => {
        helveg.iconsLoaded.subscribe(() => set(getIcon(name, options)));
        return () => { };
    });
}

function removeIconTitle(svg: Document) {
    [...svg.getElementsByTagName("title")].forEach(e => e.remove());
}

function setIconSize(svg: Document, options: IconOptions): string {
    if(options.viewBoxOnly) {
        svg.documentElement.removeAttribute("width");
        svg.documentElement.removeAttribute("height");
    }
    
    if (options.width && !svg.documentElement.hasAttribute("width")) {
        svg.documentElement.setAttribute("width", options.width.toString());
    }
    if (options.height && !svg.documentElement.hasAttribute("height")) {
        svg.documentElement.setAttribute("height", options.height.toString());
    }
    if (options.viewBox && !svg.documentElement.hasAttribute("viewBox")) {
        svg.documentElement.setAttribute("viewBox", options.viewBox);
    }
    return new XMLSerializer().serializeToString(svg);
}

function tweakIcon(svgString: string, options: IconOptions): string {
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

export function getIconDataUrl(name: string, options?: IconOptions): string {
    let icon = getIcon(name, options);
    switch (icon.format) {
        case IconFormat.Svg:
            return svgToDataURI(icon.data);
        case IconFormat.Png:
            return `data:image/png,${icon.data}`;
        default:
            throw new Error(`IconFormat '${icon.format}' is not supported.`);
    }
}
