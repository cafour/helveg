import { PizzaIcons } from "../model/const.ts";
import { MultigraphNode } from "../model/data-model.ts";

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
    Parameter = "Parameter",
}

export const csharpNodeKindOrder: readonly string[] = [
    EntityKind.Solution,
    EntityKind.Project,

    EntityKind.PackageRepository,
    EntityKind.Package,

    EntityKind.Framework,
    EntityKind.Library,

    EntityKind.ExternalDependencySource,

    EntityKind.Assembly,
    EntityKind.Module,

    EntityKind.Namespace,

    EntityKind.Type,
    EntityKind.TypeParameter,

    EntityKind.Field,
    EntityKind.Property,
    EntityKind.Event,
    EntityKind.Method,

    EntityKind.Parameter,
];

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
    Fallback = "Fallback",
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
    Public = "Public",
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
    FunctionPointer = "FunctionPointer",
}

export enum LimitedTypeKind {
    Class = "Class",
    Delegate = "Delegate",
    Enum = "Enum",
    Interface = "Interface",
    Struct = "Struct",
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
    FunctionPointerSignature = "FunctionPointerSignature",
}

export interface NodeColor {
    foreground: string;
    background: string;
}

export interface NodeColorSchema {
    entities: Record<EntityKind, NodeColor>;
    types: Record<LimitedTypeKind, NodeColor>;
}

export interface CSharpNode extends MultigraphNode {
    kind: EntityKind;
    typeKind?: TypeKind;
    accessibility?: MemberAccessibility;
    methodKind?: MethodKind;
    isConst?: boolean;
    isEnumItem?: boolean;
    declaringKind?: EntityKind;
    instanceMemberCount?: number;
    staticMemberCount?: number;
    isStatic?: boolean;
    isSealed?: boolean;
    isVirtual?: boolean;
    isReadOnly?: boolean;
    isWriteOnly?: boolean;
    isAbstract?: boolean;
    isRecord?: boolean;
    isNested?: boolean;
    isVolatile?: boolean;
    isOverride?: boolean;
    isExtern?: boolean;
    isPartial?: boolean;
    isAsync?: boolean;
    isIndexer?: boolean;
    fieldType?: string;
    propertyType?: string;
    eventType?: string;
    returnType?: string;
    parameterType?: string;
    arity?: number;
    parameterCount?: number;
    path?: string;
    diagnosticCount?: number;
    hasDiagnostics?: boolean;
    hasErrors?: boolean;
    errorCount?: number;
    hasWarnings?: boolean;
    warningCount?: number;
    hasComments?: boolean;
    commentCount?: number;
    descendantCount?: number;
    treeHeight?: number;
}

export interface PropCategory {
    name: string;
    hint?: string;
    properties: string[];
}

export const CSHARP_PROP_CATEGORIES: Readonly<PropCategory[]> = [
    {
        name: "Basic",
        properties: ["name", "kind", "declaringKind", "containingType", "childCount", "descendantCount", "treeHeight"],
    },
    {
        name: "Solution and Projects",
        properties: ["isSolution", "isProject", "path"],
    },
    {
        name: "Types",
        properties: [
            "isType",
            "typeKind",
            "baseType",
            "arity",
            "instanceMemberCount",
            "staticMemberCount",
            "isSealed",
            "isRecord",
            "isNested",
            "isAnonymousType",
            "isTupleType",
            "isNativeIntegerType",
            "isUnmanagedType",
            "isRefLikeType",
            "isImplicitClass",
        ],
    },
    {
        name: "Methods",
        properties: [
            "isMethod",
            "methodKind",
            "parameterCount",
            "returnType",
            "isAsync",
            "isExtensionMethod",
            "isInitOnly",
        ],
    },
    {
        name: "Fields",
        properties: ["isField", "fieldType", "isEnumItem", "isConst"],
    },
    {
        name: "Properties",
        properties: ["isProperty", "propertyType", "isIndexer"],
    },
    {
        name: "Events",
        properties: ["isEvent", "eventType"],
    },
    {
        name: "Parameters",
        properties: [
            "isParameter",
            "parameterType",
            "ordinal",
            "isParams",
            "isOptional",
            "isThis",
            "hasExplicitDefaultValue",
            "isDiscard",
        ],
    },
    {
        name: "Modifiers",
        properties: [
            "accessibility",
            "isAbstract",
            "isStatic",
            "isVirtual",
            "isReadOnly",
            "isWriteOnly",
            "isVolatile",
            "isOverride",
            "isExtern",
            "isPartial",
        ],
    },
    {
        name: "Diagnostics",
        properties: ["hasDiagnostics", "diagnosticCount", "hasErrors", "errorCount", "hasWarnings", "warningCount"],
    },
    {
        name: "Comments",
        properties: ["hasComments", "commentCount"],
    },
    {
        name: "Packages and Assemblies",
        properties: ["version", "fileVersion", "cultureName", "publicKeyToken", "targetFramework"],
    },
];

export const CSHARP_CATEGORY_TO_PROP_MAPPING: Readonly<Map<string, { props: string[]; index: number }>> = new Map(
    CSHARP_PROP_CATEGORIES.map((c, i) => [c.name, { props: c.properties, index: i }])
);

export const CSHARP_PROP_TO_CATEGORY_MAPPING: Readonly<Map<string, { category: string; index: number }>> = new Map(
    CSHARP_PROP_CATEGORIES.flatMap((c) => c.properties.map((p, i) => [p, { category: c.name, index: i }]))
);

export const CSHARP_FALLBACK_CATEGORY_NAME = "Miscellaneous";

export function sortProps(props: string[]) {
    const map = props.reduce((m, p) => {
        const category = CSHARP_PROP_TO_CATEGORY_MAPPING.get(p)?.category ?? CSHARP_FALLBACK_CATEGORY_NAME;
        if (!m.has(category)) {
            m.set(category, []);
        }

        m.get(category)!.push(p);
        return m;
    }, new Map<string, string[]>());
    map.entries().forEach(([name, props]) => {
        const canonicalOrder = CSHARP_CATEGORY_TO_PROP_MAPPING.get(name);
        if (canonicalOrder != null) {
            props.sort((a, b) => canonicalOrder.props.indexOf(a) - canonicalOrder.props.indexOf(b));
        } else {
            props.sort((a, b) => a.localeCompare(b));
        }
    });
    const result = [...map.entries()].sort(
        (a, b) =>
            (CSHARP_CATEGORY_TO_PROP_MAPPING.get(a[0])?.index ?? Number.MAX_SAFE_INTEGER) -
            (CSHARP_CATEGORY_TO_PROP_MAPPING.get(b[0])?.index ?? Number.MAX_SAFE_INTEGER)
    );
    return result;
}

export const CSHARP_RELATION_HINTS: Record<string, string> = {
    declares: "An entity declares another. For example: a class type declares a method.",
    inheritsFrom: "A type inherits from another class or implements an interface.",
    typeOf: "A property/field/event/parameter is of a certain type.",
    returns: "A method returns a type.",
    overrides: "A method/property override a base method/property.",
    associatedWith: "A property is associated with its getter and setter, backing field with an auto-property, etc.",
    dependsOn: "A project dependency on another project or an external library.",
    references: "An assembly module references another assembly.",
};

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
    Fallback: PizzaIcons.Cookie,
};
