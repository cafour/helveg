export async function loadJsonScripts<T>(selector: string): Promise<T[]> {
    let results: T[] = [];
    let scripts = [...document.querySelectorAll(selector)];
    for (let script of scripts) {
        let src = script.getAttribute('src');
        if (src === null && script.textContent === null) {
            console.warn(`Script element '${script}' does not have a 'src' attribute or text content. Ignoring.`);
            continue;
        }
        let result = src === null
            ? <T>JSON.parse(script.textContent!)
            : <T>await (await fetch(src)).json();
        results.push(result);
    }
    return results;
}
