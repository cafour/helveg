import type { Multigraph } from "./multigraph";
import type { DocumentInfo } from "./document";

export interface VisualizationModel
{
    documentInfo: DocumentInfo;
    multigraph: Multigraph;
}

export const empty: VisualizationModel = {
    documentInfo: {
        createdOn: "0000-00-00T00:00:00.000Z",
        helvegVersion: "unknown",
        name: "Empty",
        revision: null
    },
    multigraph: {
        id: "empty",
        label: "Empty",
        nodes: {},
        relations: {}
    }
};
