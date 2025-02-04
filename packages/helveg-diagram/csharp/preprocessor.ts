import { DataModel } from "../model/data-model.ts";
import { CSharpNode } from "./model.ts";

export function preprocessModel(model: DataModel): DataModel {
    if (model.data == null) {
        return model;
    }

    return model;
}

function visitDescendantCount(model: DataModel, node: string) {
    
}
