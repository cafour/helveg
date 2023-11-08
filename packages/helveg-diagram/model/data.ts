export async function loadJsonScripts<T>(selector: string): Promise<T[]> {
    let results: T[] = [];
    let scripts = [...document.querySelectorAll(selector)];
    for (let script of scripts) {
        let result = await loadJsonScript<T>(script);
        if (!result) {
            continue;
        }
        results.push(result);
    }
    return results;
}


export async function loadJsonScript<T>(script: Element): Promise<T | null> {
    let src = script.getAttribute('src');
    if (src === null && script.textContent === null) {
        console.warn(`Script element '${script}' does not have a 'src' attribute or text content. Ignoring.`);
        return null;
    }
    return src === null
        ? <T>JSON.parse(script.textContent!)
        : <T>await (await fetch(src)).json();
}

export async function requireJsonScript<T>(script: Element): Promise<T> {
    const result = await loadJsonScript<T>(script);
    if (!result) {
        throw new Error(`No JSON could be read from element ${script}.`);
    }

    return result;
}
