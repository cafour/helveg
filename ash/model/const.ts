import { DataModel } from "./data-model.ts";
import { IconSetModel } from "./icon-set-model.ts";

export enum Colors {
    DarkGray = "#202020",
    White = "#ffffff"
}

export enum PizzaIcons {
    Bacon = "pizza:Bacon",
    Mozzarella = "pizza:Mozzarella",
    Basil = "pizza:Basil",
    CherryTomato = "pizza:CherryTomato",
    Corn = "pizza:Corn",
    Egg = "pizza:Egg",
    Ham = "pizza:Ham",
    Jalapeno = "pizza:Jalapeno",
    Pineapple = "pizza:Pineapple",
    Salami = "pizza:Salami",
    Olive = "pizza:Olive",
    OlomoucCheese = "pizza:OlomoucCheese",
    Eidam = "pizza:Eidam",
    Shrimp = "pizza:Shrimp",
    Chicken = "pizza:Chicken",
    Pickle = "pizza:Pickle",
    Fries = "pizza:Fries",
    Cookie = "pizza:Cookie",
    Meatball = "pizza:Meatball",
    Chilli = "pizza:Chilli",
    Tomato = "pizza:Tomato"
}

export const FALLBACK_PIZZA_ICON = PizzaIcons.Cookie;

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
