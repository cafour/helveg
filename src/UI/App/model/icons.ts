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
