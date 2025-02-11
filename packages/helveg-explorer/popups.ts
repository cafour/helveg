export function setPopupPosition(popup: HTMLElement, target: HTMLElement) {
    const rect = target.getBoundingClientRect();

    if (popup.clientHeight > rect.y) {
        popup.classList.add("arrow-top");
        popup.classList.remove("arrow-bottom");
        popup.style.top = rect.y + rect.height + "px";
    } else {
        popup.classList.add("arrow-bottom");
        popup.classList.remove("arrow-top");
        popup.style.top = rect.y - rect.height - popup.offsetHeight + "px";
    }

    const xMid = rect.x + rect.width / 2;
    if (popup.clientWidth / 2 > xMid) {
        popup.style.left = `calc(${xMid}px - 0.5 * var(--arrow-size))`;
        popup.classList.add("point-left");
    } else if (popup.clientWidth / 2 > document.documentElement.clientWidth - xMid) {
        popup.style.left = `calc(${xMid - popup.clientWidth}px + 0.5 * var(--arrow-size))`;
        popup.classList.add("point-right");
    } else {
        popup.style.left = xMid - popup.clientWidth / 2 + "px";
        popup.classList.remove("point-left");
        popup.classList.remove("point-right");
    }

    popup.classList.remove("hidden");
}
