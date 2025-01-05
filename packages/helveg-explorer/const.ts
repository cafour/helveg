export enum AppIcons {
    LayoutPanel = "vscode:type-hierarchy",
    AppearancePanel = "vscode:symbol-color",
    ToolsPanel = "vscode:tools",
    PropertiesPanel = "vscode:note",
    DocumentPanel = "vscode:symbol-file",
    GuidePanel = "vscode:question",
    SearchPanel = "vscode:search",

    ContainsMode = "vscode:preserve-case",
    RegexMode = "vscode:regex",
    JavaScriptMode = "vs:JSFileNode",

    DebugToast = "vs:StatusRunning",
    InfoToast = "vs:StatusInformation",
    WarningToast = "vs:StatusWarning",
    ErrorToast = "vs:StatusError",
    SuccessToast = "vs:StatusSuccess",
    MessageToast = "vs:Message",

    MoveTool = "vscode:move",
    ShowPropertiesTool = "vscode:note",
    ToggleTool = "vscode:source-control",
    CutTool = "vscode:trash",

    UnknownDiagnostic = "vs:StatusHelp",
    HiddenDiagnostic = "vs:StatusHidden",
    InfoDiagnostic = "vs:StatusInformation",
    WarningDiagnostic = "vs:StatusWarning",
    ErrorDiagnostic = "vs:StatusError",
    
    Hint = "vscode:question",
}

export enum AppPanels {
    Unknown = "unknown-panel",
    Data = "data-panel",
    Layout = "layout-panel",
    Appearance = "appearance-panel",
    Tools = "tools-panel",
    Properties = "properties-panel",
    Document = "document-panel",
    Guide = "guide-panel",
    Search = "search-panel"
}

export enum AppTools {
    Move = "move",
    ShowProperties = "showProperties",
    Toggle = "expand",
    Cut = "cut",
}
