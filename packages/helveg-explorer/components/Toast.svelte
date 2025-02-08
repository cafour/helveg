<!-- Based on https://www.skeleton.dev/utilities/toasts -->
<script lang="ts" context="module">
    let counter = 0;

    export interface ToastSlice {
        text: string;
        title: string;
        autohide: boolean;
        timeout: number;
        id: number;
        timeoutId?: number;
    }

    export const DEFAULT_TOAST_SLICE: ToastSlice = {
        text: "Missing Message",
        title: "",
        autohide: true,
        timeout: 5000,
        id: 0,
    };

    class Toaster implements Writable<ToastSlice[]> {
        private store = writable<ToastSlice[]>([]);

        set(value: ToastSlice[]): void {
            this.store.set(value);
        }

        update(updater: Updater<ToastSlice[]>): void {
            this.store.update(updater);
        }

        subscribe(
            run: Subscriber<ToastSlice[]>,
            invalidate?:
                | ((value?: ToastSlice[] | undefined) => void)
                | undefined
        ): Unsubscriber {
            return this.store.subscribe(run, invalidate);
        }

        putIn(slice: Partial<ToastSlice>): void {
            let id = ++counter;
            let fullSlice = { ...DEFAULT_TOAST_SLICE, ...slice, id };

            if (fullSlice.autohide) {
                fullSlice.timeoutId = window.setTimeout(() => {
                    this.popOut(id);
                }, fullSlice.timeout);
            }

            this.update((slices) => {
                slices.push(fullSlice);
                return slices;
            });
        }

        popOut(id: number): void {
            this.update((slices) => {
                let index = slices.findIndex((x) => x.id === id);
                if (index > -1) {
                    let slice = slices[index];
                    if (slice.timeoutId) {
                        clearTimeout(slice.timeoutId);
                    }

                    slices.splice(index, 1);
                }
                return slices;
            });
        }

        clear(): void {
            this.set([]);
        }
    }

    export const toaster = new Toaster();
</script>

<script lang="ts">
    import { crossfade } from "svelte/transition";
    import { cubicInOut } from "svelte/easing";
    import { flip } from "svelte/animate";

    import {
        writable,
        type Unsubscriber,
        type Writable,
        type Subscriber,
        type Updater,
    } from "svelte/store";
    import { getContext } from "svelte";
    import { LogSeverity, type Diagram, type LogEntry } from "../deps/helveg-diagram.ts";
    import { onDestroy } from "svelte";
    import { AppIcons } from "../const.ts";
    import Icon from "./Icon.svelte";

    function onLogged(entry: LogEntry) {
        toaster.putIn({
            text: entry.message,
            title: entry.severity,
            autohide: entry.severity !== LogSeverity.Error
        });
    }

    let diagram = getContext<Diagram>("diagram");
    diagram.logger.logged.subscribe(onLogged);

    export let max = 3;
    export let duration = 250;
    export let buttonDismissLabel = "✕";

    $: readySlices = Array.from($toaster).slice(0, max);

    // Crossfade animation for Toasts
    let animAxis = { x: 100, y: 0 };
    const [send, receive] = crossfade({
        duration: (d) => Math.sqrt(d * duration),

        fallback(node) {
            const style = getComputedStyle(node);
            const transform = style.transform === "none" ? "" : style.transform;

            return {
                duration,
                easing: cubicInOut,
                css: (t, u) => `
                    transform: ${transform} scale(${t}) translate(${
                    u * animAxis.x
                }%, ${u * animAxis.y}%);
                    opacity: ${t}
                `,
            };
        },
    });

    onDestroy(() => {
        diagram.logger.logged.unsubscribe(onLogged);
    });

    function getToastIcon(title: string | null): string {
        switch (title) {
            case LogSeverity.Debug:
                return AppIcons.DebugToast;
            case LogSeverity.Info:
                return AppIcons.InfoToast;
            case LogSeverity.Warning:
                return AppIcons.WarningToast;
            case LogSeverity.Error:
                return AppIcons.ErrorToast;
            case LogSeverity.Success:
                return AppIcons.SuccessToast;
            default:
                return AppIcons.MessageToast;
        }
    }
    export let style: string | undefined = undefined;
</script>

<div class="toast-wrapper" {style}>
    {#each readySlices as slice, i (slice.id)}
        <div
            class="toast"
            animate:flip={{ duration }}
            in:receive={{ key: slice.id }}
            out:send={{ key: slice.id }}
        >
            <div class="toast-header">
                <Icon name={getToastIcon(slice.title)} />
                <span class="title">{slice.title}</span>
                <button
                    on:click={() => toaster.popOut(slice.id)}
                    class="button-icon primary"
                >
                    {buttonDismissLabel}
                </button>
            </div>
            <div class="toast-body">
                {@html slice.text}
            </div>
        </div>
    {/each}
</div>
