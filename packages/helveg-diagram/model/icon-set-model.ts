/**
 * Icons used by Helveg, a software visualization toolkit.
 */
export interface IconSetModel {
    icons: IconModel[];
    /**
     * The name of the set that will become the namespace of each of its icons.
     */
    namespace: string;
    [property: string]: any;
}

/**
 * An icon.
 */
export interface IconModel {
    data:   string;
    format: Format;
    name:   string;
    [property: string]: any;
}

export type Format = "svg" | "png";
