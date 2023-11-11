export async function loadScript(element: Element): Promise<string>;
export async function loadScript(selector: string): Promise<string>;
export async function loadScript(object: Element | string): Promise<string>
{
    if (typeof(object) == "string") {
        const element = document.querySelector(object);
        if (!element) {
            throw new Error(`Selector '${object}' matched no elements.`);
        }
        object = element;
    }
    
    let src = object.getAttribute('src');
    if (src === null && object.textContent === null) {
        throw new Error(`Script element '${object}' does not have a 'src' attribute or text content.`);
    }

    return src === null
        ? object.textContent!
        : (await fetch(src)).text();
}

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
