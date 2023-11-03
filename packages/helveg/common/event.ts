class HelvegEventHandler<T> {
    constructor(public handler: (f: T) => void, public isOneTime: boolean) {
    }
}

/**
 * An event that isn't tied to the DOM and supports triggering of missed events on handler subscribe.
 * 
 * Based on DotvvmEvent from DotVVM.
 */
export class HelvegEvent<T> {
    private handlers: Array<HelvegEventHandler<T>> = [];
    private history: T[] = [];

    constructor(
        public readonly name?: string,
        private readonly triggerMissedEventsOnSubscribe: boolean = false) {
    }

    public subscribe(handler: (data: T) => void) {
        this.handlers.push(new HelvegEventHandler<T>(handler, false));

        if (this.triggerMissedEventsOnSubscribe) {
            for (const h of this.history) {
                handler(h);
            }
        }
    }

    public subscribeOnce(handler: (data: T) => void) {
        this.handlers.push(new HelvegEventHandler<T>(handler, true));
    }

    public unsubscribe(handler: (data: T) => void) {
        for (let i = 0; i < this.handlers.length; i++) {
            if (this.handlers[i].handler === handler) {
                this.handlers.splice(i, 1);
                return;
            }
        }
    }

    public trigger(data: T): void {
        for (let i = 0; i < this.handlers.length; i++) {
            this.handlers[i].handler(data);
            if (this.handlers[i].isOneTime) {
                this.handlers.splice(i, 1);
                i--;
            }
        }

        if (this.triggerMissedEventsOnSubscribe) {
            this.history.push(data);
        }
    }
}
