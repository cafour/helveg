#pragma once

#include <volk.h>

#include <vector>
#include <string>
#include <optional>

namespace vku {

class DebugMessenger;

class Instance {
private:
    VkInstance _raw;

public:
    Instance(VkInstance raw);
    Instance(VkInstanceCreateInfo &createInfo);
    ~Instance();
    Instance(const Instance &other) = delete;
    Instance(Instance &&other) noexcept;
    Instance &operator=(const Instance &other) = delete;
    Instance &operator=(Instance &&other) noexcept;

    operator VkInstance() { return _raw; }
    VkInstance raw() { return _raw; }

    static Instance basic(
        const std::string &appName,
        bool usesGlfw = true,
        bool isDebug = false,
        const std::vector<const char *> *extensions = nullptr,
        const std::vector<const char *> *layers = nullptr,
        std::optional<vku::DebugMessenger> *messenger = nullptr);
};
}
