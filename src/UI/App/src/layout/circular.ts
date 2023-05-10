export function wheellOfFortune(pinSize, pinCount): { radius: number, theta: number } {
    // Based on: https://en.wikipedia.org/wiki/Circular_segment
    // TODO: There's probably a more effective way to compute this.
    let chord = 2 * pinSize;
    let theta = Math.PI * 2 / pinCount;
    let saggita = chord / 2 * Math.tan(theta / 4);
    let radius = saggita / 2 + (chord * chord) / (8 * saggita);
    return { radius, theta };
}
