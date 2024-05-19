declare module "graphology-layout-forceatlas2/helpers"
{
    import type { ForceAtlas2Settings } from "graphology-layout-forceatlas2";

    export function assign<T>(a: T, b: T, c: T): T;
    
    export function validateSettings(settings: ForceAtlas2Settings): string;
}


declare module "graphology-layout-forceatlas2/iterate"
{
    import type { ForceAtlas2Settings } from "graphology-layout-forceatlas2";

    export default function iterate(settings: ForceAtlas2Settings, nodes: Float32Array, edges: Float32Array): void;
}
