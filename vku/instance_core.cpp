#include "instance_core.hpp"

const std::vector<const char *> vku::InstanceCore::defaultLayers = std::vector<const char *>();
const std::vector<const char *> vku::InstanceCore::defaultExtensions = std::vector<const char *>();

vku::InstanceCore::InstanceCore(
    const std::string &name,
    const std::vector<const char *> layers,
    const std::vector<const char *> extensions,
    bool useGlfw,
    bool debug)
{
    _instance = vku::Instance::basic(name, useGlfw, debug, extensions, layers, &_debugMessenger);
}
