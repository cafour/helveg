export interface TutorialPosition {
    elementQuery?: string;
    top?: string;
    left?: string;
    right?: string;
    bottom?: string;
}

export interface TutorialMessage {
    header?: string;
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
            left: "4rem"
        }
    },
    {
        header: "Controls: Move nodes",
        message: "Drag nodes around with the left mouse button while holding Shift.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Controls: Remove nodes",
        message: "Left click on nodes while holding Alt to remove them from the current graph.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Collapsed node indicator",
        message: "Notice that nodes that are collapsed and may be expanded have a crescent shadow below them.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Toolbar",
        message: "The Toolbar contains basic graph operations. Try them out!",
        position: {
            // elementQuery: ".toolbar"
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Search Panel",
        message: "The Search panel allows you to filter the graph using a full-text search or various filters.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Layout Panel",
        message: "The Layout panel controls which nodes and relations are visualized and laid out.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Appearance Panel",
        message: "The Appearance panel can tweak colors and other visual elements. Especially take a look at the Node color preset.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Tools Panel",
        message: "The Tools panel controls the behavior of various tools. You can for example use it to highlight entire node subtrees.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Properties Panel",
        message: "The Properties panel shows the properties of the currently selected node. It also shows its preview.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Document Panel",
        message: "In the Document panel, you may Reset options of the entire application, or export the graph as an image.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Hints",
        message: "There are various Hints throughout the panels. To see them hover your mouse over buttons tiny questionmark icons.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Tree View",
        message: "On the left side of the screen there is the TreeView. It shows a more traditional view of the codebase's hierarchy.",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
    {
        header: "Cheat Sheet",
        message: "Finally, in the lower left corner, there is the Cheat Sheet button. Use it to open up an overview of all the controls, glyphs features, panels, and so on. Good luck!",
        position: {
            bottom: "6rem",
            left: "4rem"
        }
    },
];
