import { DataModel } from "./data-model.ts";
import { IconSetModel } from "./icon-set-model.ts";

export enum Colors {
    DarkGray = "#202020",
    White = "#ffffff"
}

export enum PizzaIcons {
    Bacon = "base:Bacon",
    Mozzarella = "base:Mozzarella",
    Basil = "base:Basil",
    CherryTomato = "base:CherryTomato",
    Corn = "base:Corn",
    Egg = "base:Egg",
    Ham = "base:Ham",
    Jalapeno = "base:Jalapeno",
    Pineapple = "base:Pineapple",
    Salami = "base:Salami",
    Olive = "base:Olive",
    OlomoucCheese = "base:OlomoucCheese",
    Eidam = "base:Eidam",
    Shrimp = "base:Shrimp",
    Chicken = "base:Chicken",
    Pickle = "base:Pickle",
    Fries = "base:Fries",
    Cookie = "base:Cookie",
    Meatball = "base:Meatball",
    Chilli = "base:Chilli",
    Tomato = "base:Tomato"
}

export const EMPTY_DATA_MODEL: DataModel = {
    name: "Empty",
    createdOn: new Date("0000-00-00T00:00:00.000Z"),
    analyzer: {
        version: "unknown",
        name: "unknown"
    },
    data: undefined
};

export const EMPTY_ICON_SET_MODEL: IconSetModel = {
    namespace: "unknown",
    icons: []
};
