export interface TutorialPosition {
    elementId?: string;
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
            bottom: "4rem",
            left: "4rem",
        },
    },
    {
        header: "Controls: Expand and collapse nodes",
        message: "Double click on nodes to expand or collapse them.",
        position: {
            bottom: "4rem",
            left: "4rem"
        }
    },
    {
        header: "Controls: Move nodes",
        message: "Drag nodes around with the left mouse button while holding Shift.",
        position: {
            bottom: "4rem",
            left: "4rem"
        }
    },
    {
        header: "Controls: Remove nodes",
        message: "Left click on nodes while holding Alt to remove them from the current graph.",
        position: {
            bottom: "4rem",
            left: "4rem"
        }
    }
];
