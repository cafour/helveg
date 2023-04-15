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

let iconSets: Record<string, IconSet>;

export function getIcon(name: string): Icon {
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

    let icon = iconSet.icons[iconName];
    if (!icon) {
        throw new Error(`Icon '${name}' could not be found.`);
    }
    return icon;
}

function setSvgSize(svgString: string, width: number, height: number, viewBox: string | null = null): string {
    const svg = new DOMParser().parseFromString(svgString, "image/svg+xml");
    svg.documentElement.setAttribute("width", width.toString());
    svg.documentElement.setAttribute("height", height.toString());
    if (viewBox) {
        svg.documentElement.setAttribute("viewBox", viewBox);
    }
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

export function getIconDataUrl(name: string): string {
    let icon = getIcon(name);
    switch(icon.format) {
        case IconFormat.Svg:
            return svgToDataURI(setSvgSize(icon.data, 256, 256));
        case IconFormat.Png:
            return `data:image/png,${icon.data}`;
        default:
            throw new Error(`IconFormat '${icon.format}' is not supported.`);
    }
}
