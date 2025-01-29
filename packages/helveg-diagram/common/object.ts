export function provideDefaults(object: any, defaults: any): any {
    for (const prop of Object.getOwnPropertyNames(defaults)) {
        if (object[prop] === undefined) {
            object[prop] = defaults[prop];
        }
    }

    return object;
}
