export function setPopupPosition(popup: HTMLElement, target: HTMLElement) {
    const rect = target.getBoundingClientRect();

    if (popup.offsetHeight > rect.y) {
        popup.classList.add("arrow-top");
        popup.classList.remove("arrow-bottom");
        popup.style.top = rect.y + rect.height + "px";
        popup.style.bottom = "";
    } else {
        popup.classList.add("arrow-bottom");
        popup.classList.remove("arrow-top");
        popup.style.top = rect.y - rect.height - popup.offsetHeight + "px";
        popup.style.bottom = "";
    }

    const xMid = rect.x + rect.width / 2;
    if (popup.offsetWidth / 2 > xMid) {
        popup.style.left = `calc(${xMid}px - 0.5 * var(--arrow-size))`;
        popup.style.right = "";
        popup.classList.add("point-left");
    } else if (popup.offsetWidth / 2 > document.documentElement.offsetWidth - xMid) {
        popup.style.left = `calc(${xMid - popup.offsetWidth}px + 0.5 * var(--arrow-size))`;
        popup.style.right = "";
        popup.classList.add("point-right");
    } else {
        popup.style.left = xMid - popup.offsetWidth / 2 + "px";
        popup.style.right = "";
        popup.classList.remove("point-left");
        popup.classList.remove("point-right");
    }

    popup.classList.remove("hidden");
}
