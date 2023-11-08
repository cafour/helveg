import { HelvegEvent } from "../common/event.ts";

export enum LogSeverity {
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Success = 4
}

export function getSeverityName(severity: LogSeverity): string | null {
    switch (severity) {
        case LogSeverity.Debug:
            return "debug";
        case LogSeverity.Info:
            return "info";
        case LogSeverity.Warning:
            return "warning";
        case LogSeverity.Error:
            return "error";
        case LogSeverity.Success:
            return "success";
        default:
            return null;
    }
}

export interface LogEntry {
    message: string;
    timestamp: Date;
    severity: LogSeverity;
    category?: string;
}

type LogHandler = (entry: LogEntry) => void;

export interface ILogger {
    get logged(): HelvegEvent<LogEntry>;
    get category(): string;

    log(entry: LogEntry): void;
    debug(message: string): void;
    info(message: string): void;
    warn(message: string): void;
    error(message: string): void;
    success(message: string): void;
}

export class Logger implements ILogger {
    private _logged = new HelvegEvent<LogEntry>("helveg.logger.logged", true);

    constructor(
        public category: string,
        public minSeverity: LogSeverity = LogSeverity.Info,
        defaultLogHandler?: LogHandler) {
        if (defaultLogHandler) {
            this._logged.subscribe(defaultLogHandler);
        }
    }

    get logged(): HelvegEvent<LogEntry> {
        return this._logged;
    }

    log(entry: LogEntry) {
        if (entry.severity >= this.minSeverity) {
            const categorizedEntry = { category: this.category, ...entry };
            this._logged.trigger(categorizedEntry);
        }
    }

    debug(message: string) {
        this.log({ message: message, timestamp: new Date(), severity: LogSeverity.Debug });
    }

    info(message: string) {
        this.log({ message: message, timestamp: new Date(), severity: LogSeverity.Info });
    }

    warn(message: string) {
        this.log({ message: message, timestamp: new Date(), severity: LogSeverity.Warning });
    }

    error(message: string) {
        this.log({ message: message, timestamp: new Date(), severity: LogSeverity.Error });
    }

    success(message: string) {
        this.log({ message: message, timestamp: new Date(), severity: LogSeverity.Success });
    }
}

export class Sublogger implements ILogger {
    private _logged = new HelvegEvent<LogEntry>("helveg.logger.logged", true);

    constructor(
        public parent: ILogger,
        public category: string,
        public minSeverity?: LogSeverity) {
    }
    get logged(): HelvegEvent<LogEntry> {
        return this._logged;
    }

    log(entry: LogEntry): void {
        if (!this.minSeverity || entry.severity >= this.minSeverity) {
            const category = `${this.parent.category}::${entry.category ?? this.category}`
            const categorizedEntry = { ...entry, category: category };
            this._logged.trigger(categorizedEntry);
            this.parent.log(categorizedEntry);
        }
    }
    debug(message: string): void {
        this.log({ message: message, timestamp: new Date(), severity: LogSeverity.Debug });
    }
    info(message: string): void {
        this.log({ message: message, timestamp: new Date(), severity: LogSeverity.Info });
    }
    warn(message: string): void {
        this.log({ message: message, timestamp: new Date(), severity: LogSeverity.Warning });
    }
    error(message: string): void {
        this.log({ message: message, timestamp: new Date(), severity: LogSeverity.Error });
    }
    success(message: string): void {
        this.log({ message: message, timestamp: new Date(), severity: LogSeverity.Success });
    }
}

export function formatEntry(entry: LogEntry): string {
    let result = "";
    if (entry.timestamp) {
        const hour = entry.timestamp.getHours().toString().padStart(2, "0");
        const minute = entry.timestamp.getMinutes().toString().padStart(2, "0");
        const second = entry.timestamp.getSeconds().toString().padStart(2, "0");
        const millisecond = entry.timestamp.getMilliseconds().toString().padStart(3, "0");
        result += `[${hour}:${minute}:${second}.${millisecond}] `;
    }

    let severityName = getSeverityName(entry.severity);
    if (severityName) {
        result += severityName + ": ";
    }

    if (entry.category) {
        result += entry.category + ": ";
    }

    result += entry.message;
    return result;
}

export function logConsole(entry: LogEntry) {
    const formatted = formatEntry(entry);

    switch (entry.severity) {
        case LogSeverity.Error:
            return console.error(formatted);
        case LogSeverity.Warning:
            return console.warn(formatted);
        default:
            return console.log(formatted);
    }
}

export function consoleLogger(
    category: string,
    minSeverity: LogSeverity = LogSeverity.Info) {
    return new Logger(category, minSeverity, logConsole);
}

export function sublogger(
    parent: ILogger,
    category: string,
    minSeverity?: LogSeverity) {
    return new Sublogger(parent, category, minSeverity);
};
