export function deepCompare(one: any, two: any, maxDepth: number = Number.MAX_VALUE): boolean {
    const oneKeys = Object.keys(one);
    const twoKeys = Object.keys(two);
    if (oneKeys.length !== twoKeys.length) {
        return false;
    }

    for (const key of oneKeys) {
        const isOneNull = one[key] === null || one[key] === undefined;
        const isTwoNull = two[key] === null || two[key] === undefined;
        if (isOneNull != isTwoNull) {
            return false;
        }

        if (isOneNull && isTwoNull) {
            continue;
        }

        if (typeof one[key] !== typeof two[key]) {
            return false;
        }

        if (typeof one[key] === "object"
            && typeof two[key] === "object"
            && maxDepth > 0
            && !deepCompare(one[key], two[key], maxDepth - 1)
        ) {
            return false;
        }
        
        if (one[key] !== two[key]) {
            return false;
        }
    }

    return true;
}
