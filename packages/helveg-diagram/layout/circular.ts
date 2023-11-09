export function wheelOfFortune(pinSize: number, pinCount: number): { radius: number, theta: number } {
    // Based on: https://en.wikipedia.org/wiki/Circular_segment
    // TODO: There's probably a more effective way to compute this.
    const chord = 2 * pinSize;
    const theta = Math.PI * 2 / pinCount;
    const saggita = chord / 2 * Math.tan(theta / 4);
    const radius = saggita / 2 + (chord * chord) / (8 * saggita);
    return { radius, theta };
}
