/**
 * Icons used by Helveg, a software visualization toolkit.
 */
export interface IconSetModel {
    icons: Icon[];
    /**
     * The name of the set that will become the namespace of each of its icons.
     */
    namespace: string;
    [property: string]: any;
}

/**
 * An icon.
 */
export interface Icon {
    data:   string;
    format: Format;
    name:   string;
    [property: string]: any;
}

export type Format = "svg" | "png";
