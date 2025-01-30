import { DiagramStatus } from "@cafour/helveg-diagram";
import { OperationScope, type GlobalOperation } from "./executor";

export const OP_LAYOUT: GlobalOperation = {
    id: "layout",
    name: "Layout",
    scope: OperationScope.GLOBAL,
    hint: "Runs or stop a continuous layout algorithm.",
    icon: "vscode:play",
    shortcut: {
        key: "Space"
    },
    async beginExecute(state) {
        if (state.diagram.status === DiagramStatus.Stopped) {
            await state.diagram.runLayout(false);
        } else {
            await state.diagram.stopLayout();
        }
    }
}
