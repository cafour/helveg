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

export class Logger {
    private _logged = new HelvegEvent<LogEntry>("helveg.logger.logged", true);

    constructor(public minSeverity: LogSeverity = LogSeverity.Info, public category?: string) {
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

export function formatEntry(entry: LogEntry): string {
    let result = "";
    if (entry.timestamp) {
        result += '[' + entry.timestamp.toTimeString() + "] ";
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
