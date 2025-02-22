export function setPopupPosition(popup: HTMLElement, target: HTMLElement) {
    const rect = target.getBoundingClientRect();

    popup.classList.remove(
        "arrow-top",
        "arrow-right",
        "arrow-bottom",
        "arrow-left",
        "point-top",
        "point-right",
        "point-bottom",
        "point-left"
    );

    // first try to position the popup above or below the target
    if (rect.height + popup.offsetHeight < document.documentElement.offsetHeight) {
        if (popup.offsetHeight > rect.y) {
            popup.classList.add("arrow-top");
            popup.style.top = rect.y + rect.height + "px";
            popup.style.bottom = "";
        } else {
            popup.classList.add("arrow-bottom");
            popup.style.top = `calc(${rect.y - popup.offsetHeight + "px"} - var(--arrow-size))`;
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
        }
    } else {
        // position the popup to the left or right of the target

        if (popup.offsetWidth > rect.x) {
            popup.classList.add("arrow-left");
            popup.style.left = rect.x + rect.width + "px";
            popup.style.right = "";
        } else {
            popup.classList.add("arrow-right");
            popup.style.left = `calc(${rect.x - popup.offsetWidth + "px"} - var(--arrow-size))`;
            popup.style.right = "";
        }

        const yMid = rect.y + rect.height / 2;
        if (popup.offsetHeight / 2 > yMid) {
            popup.style.top = `calc(${yMid}px - 0.5 * var(--arrow-size))`;
            popup.style.bottom = "";
            popup.classList.add("point-top");
        } else if (popup.offsetHeight / 2 > document.documentElement.offsetHeight - yMid) {
            popup.style.top = `calc(${yMid - popup.offsetHeight}px + 0.5 * var(--arrow-size))`;
            popup.style.bottom = "";
            popup.classList.add("point-bottom");
        } else {
            popup.style.top = yMid - popup.offsetHeight / 2 + "px";
            popup.style.bottom = "";
        }
    }

    popup.classList.remove("hidden");
}
