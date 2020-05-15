#pragma once

#include <optional>
#include <string>
#include <vector>

#include "instance_related.hpp"
#include "standalone.hpp"

namespace vku {
/**
 * Handles instance creation (including the debug messenger).
 */
class InstanceCore {
private:
    vku::Instance _instance;
    std::optional<vku::DebugMessenger> _debugMessenger;

    static const std::vector<const char *> defaultLayers;
    static const std::vector<const char *> defaultExtensions;

public:
    InstanceCore()
        : InstanceCore("default")
    { }

    InstanceCore(
        const std::string &name,
        bool useGlfw = false,
        bool debug = false)
        : InstanceCore(name, defaultLayers, defaultExtensions, useGlfw, debug)
    { }

    InstanceCore(
        const std::string &name,
        const std::vector<const char *> layers,
        const std::vector<const char *> extensions,
        bool useGlfw = false,
        bool debug = false);

    vku::Instance &instance() { return _instance; }
};
}
