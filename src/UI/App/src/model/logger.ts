import { HelvegEvent } from "common/event";

export enum LogSeverity {
    Debug = "debug",
    Info = "info",
    Warning = "warning",
    Error = "error",
    Success = "success"
}

export interface LogEntry {
    message: string;
    timestamp: Date;
    severity: LogSeverity;
}

export class Logger {
    private _logged = new HelvegEvent<LogEntry>("helveg.Logger.logged", true);
    
    get logged(): HelvegEvent<LogEntry> {
        return this._logged;
    }

    log(entry: LogEntry) {
        this._logged.trigger(entry);
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
