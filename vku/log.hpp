#pragma once

#include <string>
#include <functional>

namespace vku
{
    enum LogLevel
    {
        TRACE = 0,
        DEBUG = 1,
        INFORMATION = 2,
        WARNING = 3,
        ERROR = 4,
        CRITICAL = 5,
        NONE = 6
    };

    void onLog(std::function<void (LogLevel level, const std::string& message)> handler);
    void log(LogLevel level, const std::string &message);

    inline void logTrace(const std::string &message)
    {
        log(LogLevel::TRACE, message);
    }

    inline void logDebug(const std::string &message)
    {
        log(LogLevel::DEBUG, message);
    }

    inline void logInformation(const std::string &message)
    {
        log(LogLevel::INFORMATION, message);
    }

    inline void logWarning(const std::string &message)
    {
        log(LogLevel::WARNING, message);
    }

    inline void logError(const std::string &message)
    {
        log(LogLevel::ERROR, message);
    }

    inline void logCritical(const std::string &message)
    {
        log(LogLevel::CRITICAL, message);
    }
}
