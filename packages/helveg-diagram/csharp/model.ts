import { PizzaIcons } from "../model/const.ts";
import { MultigraphNode } from "../model/data-model.ts";
import { NodeStyle, FireStatus } from "../model/style.ts";

export enum EntityKind {
    Solution = "Solution",
    Project = "Project",
    ExternalDependencySource = "ExternalDependencySource",
    Framework = "Framework",
    PackageRepository = "PackageRepository",
    Package = "Package",
    Library = "Library",
    Assembly = "Assembly",
    Module = "Module",
    Namespace = "Namespace",
    Type = "Type",
    Field = "Field",
    Property = "Property",
    Event = "Event",
    Method = "Method",
    TypeParameter = "TypeParameter",
    Parameter = "Parameter"
}

export enum DefaultIcons {
    Solution = "csharp:Solution",
    Project = "csharp:CSProjectNode",
    ExternalDependencySource = "csharp:ReferenceGroup",
    Framework = "csharp:Framework",
    PackageRepository = "csharp:NuGet",
    Package = "csharp:Package",
    Library = "csharp:Library",
    Assembly = "csharp:Assembly",
    Module = "csharp:Module",
    Namespace = "csharp:Namespace",
    Type = "csharp:Class",
    TypeParameter = "csharp:Type",
    Field = "csharp:Field",
    Method = "csharp:Method",
    Property = "csharp:Property",
    Event = "csharp:Event",
    Parameter = "csharp:LocalVariable",
}

export enum IconableEntities {
    Solution = "Solution",
    Project = "Project",
    Framework = "Framework",
    ExternalDependencySource = "ExternalDependencySource",
    PackageRepository = "PackageRepository",
    Package = "Package",
    Library = "Library",
    Assembly = "Assembly",
    Module = "Module",
    Namespace = "Namespace",
    Type = "Type",
    Class = "Class",
    Interface = "Interface",
    Enum = "Enum",
    Struct = "Struct",
    Delegate = "Delegate",
    Field = "Field",
    EnumItem = "EnumItem",
    Const = "Const",
    Method = "Method",
    Property = "Property",
    Event = "Event",
    Parameter = "Parameter",
    Fallback = "Fallback"
}

export enum Relations {
    Declares = "declares",
    InheritsFrom = "inheritsFrom",
    TypeOf = "typeOf",
    Returns = "returns",
    Overrides = "overrides",
    AssociatedWith = "associatedWith",
    DependsOn = "dependsOn",
    References = "references",
}

export enum DefaultRelationColors {
    Declares = "#dfdfdf",
    InheritsFrom = "#c3e5de",
    TypeOf = "#dcdcaa",
    Returns = "#d1c3e5",
    Overrides = "#e5c3c8",
    AssociatedWith = "#e5ccb7",
    DependsOn = "#c3d6e5",
    References = "#c3d6e5",
}

export enum MemberAccessibility {
    Invalid = "Invalid",
    Private = "Private",
    ProtectedAndInternal = "ProtectedAndInternal",
    Protected = "Protected",
    Internal = "Internal",
    ProtectedOrInternal = "ProtectedOrInternal",
    Public = "Public"
}

export enum TypeKind {
    Unknown = "Unknown",
    Array = "Array",
    Class = "Class",
    Delegate = "Delegate",
    Dynamic = "Dynamic",
    Enum = "Enum",
    Error = "Error",
    Interface = "Interface",
    Module = "Module",
    Pointer = "Pointer",
    Struct = "Struct",
    TypeParameter = "TypeParameter",
    Submission = "Submission",
    FunctionPointer = "FunctionPointer"
}

export enum MethodKind {
    Invalid = "Invalid",
    AnonymousFunction = "AnonymousFunction",
    Constructor = "Constructor",
    Conversion = "Conversion",
    DelegateInvoke = "DelegateInvoke",
    Destructor = "Destructor",
    EventAdd = "EventAdd",
    EventRaise = "EventRaise",
    EventRemove = "EventRemove",
    ExplicitInterfaceImplementation = "ExplicitInterfaceImplementation",
    UserDefinedOperator = "UserDefinedOperator",
    Ordinary = "Ordinary",
    PropertyGet = "PropertyGet",
    PropertySet = "PropertySet",
    ReducedExtension = "ReducedExtension",
    StaticConstructor = "StaticConstructor",
    BuiltinOperator = "BuiltinOperator",
    DeclareMethod = "DeclareMethod",
    LocalFunction = "LocalFunction",
    FunctionPointerSignature = "FunctionPointerSignature"
}

export enum VSColor {
    DarkGray = "#212121",
    DarkPurple = "#68217a",
    Purple = "#6936aa",
    DarkYellow = "#996f00",
    Blue = "#005dba",
    NuGetBlue = "#004880"
}

export interface CSharpNode extends MultigraphNode {
    kind: EntityKind,
    typeKind?: TypeKind,
    accessibility?: MemberAccessibility,
    methodKind?: MethodKind,
    isConst?: boolean,
    isEnumItem?: boolean,
    declaringKind?: EntityKind,
    instanceMemberCount?: number,
    staticMemberCount?: number,
    isStatic?: boolean
}

export const FALLBACK_STYLE: NodeStyle = {
    icon: "csharp:ExplodedDoughnutChart",
    color: VSColor.DarkGray,
    size: 5,
    outlines: [],
    fire: FireStatus.None
};

export interface CSharpDataOptions {
    includedKinds: string[];
    autoExpandedKinds: string[];
}

export const DEFAULT_CSHARP_DATA_OPTIONS: CSharpDataOptions = {
    includedKinds: [
        EntityKind.Solution,
        EntityKind.Project,
        EntityKind.Namespace,
        EntityKind.Type,
        EntityKind.Field,
        EntityKind.Method,
        EntityKind.Property,
        EntityKind.Event
    ],
    autoExpandedKinds: [
        EntityKind.Solution,
        EntityKind.Project,
        EntityKind.Namespace
    ]
}

export const DEFAULT_CSHARP_PIZZA_TOPPINGS: Record<keyof typeof IconableEntities, PizzaIcons> = {
    Solution: PizzaIcons.Bacon,
    Project: PizzaIcons.Mozzarella,
    Framework: PizzaIcons.Basil,
    ExternalDependencySource: PizzaIcons.Basil,
    PackageRepository: PizzaIcons.Basil,
    Package: PizzaIcons.Mozzarella,
    Library: PizzaIcons.Egg,
    Assembly: PizzaIcons.Chicken,
    Module: PizzaIcons.Ham,
    Namespace: PizzaIcons.Salami,
    Type: PizzaIcons.OlomoucCheese,
    Class: PizzaIcons.Pineapple,
    Interface: PizzaIcons.Shrimp,
    Enum: PizzaIcons.Olive,
    Struct: PizzaIcons.Eidam,
    Delegate: PizzaIcons.Tomato,
    Field: PizzaIcons.Fries,
    EnumItem: PizzaIcons.Pickle,
    Const: PizzaIcons.Meatball,
    Method: PizzaIcons.CherryTomato,
    Property: PizzaIcons.Jalapeno,
    Event: PizzaIcons.Chilli,
    Parameter: PizzaIcons.Corn,
    Fallback: PizzaIcons.Cookie
};
