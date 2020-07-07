#include "log.hpp"

#include <vector>

std::vector<std::function<void(vku::LogLevel level, const std::string &message)>> logHandlers
    = std::vector<std::function<void(vku::LogLevel level, const std::string &message)>>();

void vku::onLog(std::function<void(vku::LogLevel level, const std::string &message)> handler)
{
    logHandlers.push_back(handler);
}

void vku::log(vku::LogLevel level, const std::string &message)
{
    for (auto handler : logHandlers) {
        handler(level, message);
    }
}
