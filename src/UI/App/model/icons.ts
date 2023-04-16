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
}

let iconSets: Record<string, IconSet>;

export function getIcon(name: string, options?: IconOptions): Icon {
    if (!iconSets) {
        iconSets = {};
        let iconScripts = document.getElementsByClassName("helveg-icons");
        for (let iconScript of iconScripts) {
            let iconSet = <IconSet>JSON.parse(iconScript.textContent!);
            iconSets[iconSet.namespace] = iconSet;
        }
    }

    let segments = name.split(":", 2);
    let namespace = segments[0];
    let iconName = segments[1];
    let iconSet = iconSets[namespace];
    if (!iconSet) {
        throw new Error(
            `Icon set for namespace '${namespace}' could not be found.`
        );
    }

    let icon = structuredClone(iconSet.icons[iconName]);
    if (!icon) {
        console.error(`Icon '${name}' could not be found. Using fallback icon.`);
        return getIcon("base:PolarChart", options);
    }

    if (icon.format === IconFormat.Svg) {
        icon.data = tweakIcon(icon.data, options ?? { removeTitle: true });
    }
    return icon;
}

function removeIconTitle(svg: Document) {
    [...svg.getElementsByTagName("title")].forEach(e => e.remove());
}

function setIconSize(svg: Document, width: number | null, height: number | null, viewBox: string | null = null): string {
    if (width && !svg.documentElement.hasAttribute("width")) {
        svg.documentElement.setAttribute("width", width.toString());
    }
    if (height && !svg.documentElement.hasAttribute("height")) {
        svg.documentElement.setAttribute("height", height.toString());
    }
    if (viewBox && !svg.documentElement.hasAttribute("viewBox")) {
        svg.documentElement.setAttribute("viewBox", viewBox);
    }
    return new XMLSerializer().serializeToString(svg);
}

function tweakIcon(svgString: string, options: IconOptions): string {
    const svg = new DOMParser().parseFromString(svgString, "image/svg+xml");
    if (options.removeTitle) {
        removeIconTitle(svg);
    }
    setIconSize(svg, options.width ?? null, options.height ?? null, options.viewBox);
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
