import type { ComponentType, SvelteComponentTyped } from "svelte";
import type { AppPanels } from "./const";

export interface UIExtension {
    targetPanel: AppPanels;
    component: ComponentType<SvelteComponentTyped>;
}

export class UIExtensionRegistry {
    private extensions = new Map<string, UIExtension>();

    register(name: string, extension: UIExtension): void {
        if (this.extensions.has(name)) {
            throw new Error(`The registry already contains an extension named '${name}'.`);
        }

        this.extensions.set(name, extension);
    }

    get(name: string): UIExtension | null {
        let extension = this.extensions.get(name);
        if (!extension) {
            DEBUG && console.log(`Could not find the '${name}' extension.`);
            return null;
        }

        return extension;
    }

    for(panel: AppPanels): UIExtension[] {
        return [...this.extensions.values()].filter(e => e.targetPanel === panel);
    }
}
