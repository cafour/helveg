import type { Multigraph } from "./multigraph";
import type { DocumentInfo } from "./document";

export interface VisualizationModel
{
    documentInfo: DocumentInfo;
    multigraph: Multigraph;
}
