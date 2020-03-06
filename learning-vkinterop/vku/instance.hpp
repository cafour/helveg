#pragma once

#include <volk.h>

#include <vector>
#include <string>

namespace vku {

class Instance {
private:
    VkInstance _raw;
    VkDebugUtilsMessengerEXT _messenger = nullptr;
    std::vector<std::string> _layers;
    std::vector<std::string> _extensions;
    std::vector<const char *> _layerPtrs;
    std::vector<const char *> _extensionPtrs;

public:
    Instance(
        const char *name,
        bool useDebugMessenger = false,
        const char **layers = nullptr,
        size_t layerCount = 0,
        const char **extensions = nullptr,
        size_t extensionCount = 0);
    ~Instance();

    operator VkInstance() { return _raw; }

    const std::vector<const char *>& layers() const { return _layerPtrs; }
    const std::vector<const char *>& extensions() const { return _extensionPtrs; }
};
}
