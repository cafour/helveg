import { AppIcons, AppPanels } from "./const.ts";

export interface TutorialPosition {
    elementQuery?: string;
    top?: string;
    left?: string;
    right?: string;
    bottom?: string;
}

export interface TutorialMessage {
    header?: string;
    icon?: string;
    selectedPanel?: string;
    message: string;
    position: TutorialPosition;
}

export const TUTORIAL_MESSAGES: TutorialMessage[] = [
    {
        header: "Controls: Inspect nodes",
        message: "Left click on nodes to show their properties.",
        position: {
            bottom: "6rem",
            left: "4rem",
        },
    },
    {
        header: "Controls: Expand and collapse nodes",
        message: "Double click on nodes to expand or collapse them.",
        position: {
            bottom: "6rem",
            left: "4rem",
        },
    },
    {
        header: "Controls: Move nodes",
        message: "Drag nodes around with the left mouse button while holding Shift.",
        position: {
            bottom: "6rem",
            left: "4rem",
        },
    },
    {
        header: "Controls: Remove nodes",
        message: "Left click on nodes while holding Alt to remove them from the current graph.",
        position: {
            bottom: "6rem",
            left: "4rem",
        },
    },
    {
        header: "Collapsed node indicator",
        message: "Notice that nodes that are collapsed and may be expanded have a shadow below them.",
        position: {
            bottom: "6rem",
            left: "4rem",
        },
    },
    {
        header: "Toolbar",
        message:
            "The Toolbar contains basic graph operations. Try them out! Note that Refresh can be used to restore nodes that you previously deleted.",
        position: {
            elementQuery: ".toolbar",
        },
    },
    {
        header: "Quick Start Panel",
        icon: AppIcons.QuickStartPanel,
        selectedPanel: AppPanels.QuickStart,
        message: "The Quick Start panel allows you to set most of Helveg's options at once based on your use case.",
        position: {
            elementQuery: "#dock-panels"
        },
    },
    {
        header: "Search Panel",
        icon: AppIcons.SearchPanel,
        selectedPanel: AppPanels.Search,
        message: "The Search panel allows you to filter the graph using a full-text search or various filters.",
        position: {
            elementQuery: "#dock-panels"
        },
    },
    {
        header: "Layout Panel",
        icon: AppIcons.LayoutPanel,
        selectedPanel: AppPanels.Layout,
        message: "The Layout panel controls which nodes and relations are visualized and laid out.",
        position: {
            elementQuery: "#dock-panels"
        },
    },
    {
        header: "Appearance Panel",
        icon: AppIcons.AppearancePanel,
        selectedPanel: AppPanels.Appearance,
        message:
            "The Appearance panel can tweak colors and other visual elements. Especially take a look at the Node color preset.",
        position: {
            elementQuery: "#dock-panels"
        },
    },
    {
        header: "Tools Panel",
        icon: AppIcons.ToolsPanel,
        selectedPanel: AppPanels.Tools,
        message:
            "The Tools panel controls the behavior of various tools. You can for example use it to highlight entire node subtrees.",
        position: {
            elementQuery: "#dock-panels"
        },
    },
    {
        header: "Properties Panel",
        icon: AppIcons.PropertiesPanel,
        selectedPanel: AppPanels.Properties,
        message: "The Properties panel shows the properties of the currently selected node. It also shows its preview.",
        position: {
            elementQuery: "#dock-panels"
        },
    },
    {
        header: "Document Panel",
        icon: AppIcons.DocumentPanel,
        selectedPanel: AppPanels.Document,
        message:
            "In the Document panel, you may Reset options of the entire application, or export the graph as an image.",
        position: {
            elementQuery: "#dock-panels"
        },
    },
    {
        header: "Hints",
        icon: "vscode:question",
        selectedPanel: AppPanels.Layout,
        message:
            "There are various Hints throughout the panels. To see them hover your mouse over buttons tiny questionmark icons.",
        position: {
            elementQuery: "#dock-panels"
        },
    },
    {
        header: "Tree View",
        message:
            "On the left side of the screen there is the TreeView. It shows a more traditional view of the codebase's hierarchy.",
        position: {
            elementQuery: "#tree-view-button"
        },
    },
    {
        header: "Cheat Sheet",
        icon: "vscode:question",
        message:
            "Finally, in the lower left corner, there is the Cheat Sheet button. Use it to open up an overview of all the controls, glyphs features, panels, and so on. Good luck!",
        position: {
            elementQuery: ".cheatsheet-button"
        },
    },
];
