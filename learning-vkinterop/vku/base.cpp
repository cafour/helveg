#include "base.hpp"

#include <algorithm>
#include <sstream>
#include <vector>

void vku::ensure(VkResult result, const char *where)
{
    if (result == VK_SUCCESS) {
        return;
    }
    std::stringstream ss;
    ss << where << "[" << result << "]: ";
    std::string message = ss.str();
    throw std::runtime_error(message);
}

void vku::ensureLayers(const char **layers, size_t length)
{
    uint32_t layerCount = 0;
    vkEnumerateInstanceLayerProperties(&layerCount, nullptr);

    std::vector<VkLayerProperties> available(layerCount);
    vkEnumerateInstanceLayerProperties(&layerCount, available.data());

    for (size_t i = 0; i < length; ++i) {
        const char *layerName = layers[i];
        bool containsLayer = std::any_of(available.begin(), available.end(),
            [layerName](const auto &layer) {
                return !strcmp(layerName, layer.layerName);
            });
        if (!containsLayer) {
            std::stringstream ss;
            ss << "could not load the '" << layerName << "' layer";
            std::string message = ss.str();
            throw std::runtime_error(message);
        }
    }
}
