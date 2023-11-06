import type { Multigraph } from "./multigraph";
import type { DocumentInfo } from "./document";

export interface VisualizationModel
{
    documentInfo: DocumentInfo;
    multigraph: Multigraph;
    isEmpty: boolean;
}

export const EMPTY_MODEL: VisualizationModel = {
    isEmpty: true,
    documentInfo: {
        createdOn: "0000-00-00T00:00:00.000Z",
        helvegVersion: "unknown",
        name: "Empty",
        revision: null
    },
    multigraph: {
        id: "empty",
        nodes: {},
        relations: {}
    }
};
