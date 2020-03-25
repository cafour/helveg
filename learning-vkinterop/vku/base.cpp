#include "base.hpp"

#include <algorithm>
#include <cstring>
#include <iostream>
#include <sstream>
#include <vector>

void vku::log(VkResult result, const char *filename, int line, const char *what)
{
    if (result == VK_SUCCESS) {
        return;
    }
    std::cerr << "[" << resultString(result) << "] "<< filename << ":" << line << ": " << what << std::endl;
}

void vku::ensure(VkResult result, const char *filename, int line, const char *what)
{
    if (result == VK_SUCCESS) {
        return;
    }
    std::stringstream ss;
    ss << "[" << resultString(result) << "] "<< filename << ":" << line << ": " << what;
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
        bool containsLayer = std::any_of(available.begin(), available.end(), [layerName](const auto &layer) {
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

const char *vku::resultString(VkResult result)
{
    switch (result) {
#define RES(r)   \
    case VK_##r: \
        return #r
        RES(SUCCESS);
        RES(NOT_READY);
        RES(TIMEOUT);
        RES(EVENT_SET);
        RES(EVENT_RESET);
        RES(INCOMPLETE);
        RES(ERROR_OUT_OF_HOST_MEMORY);
        RES(ERROR_OUT_OF_DEVICE_MEMORY);
        RES(ERROR_INITIALIZATION_FAILED);
        RES(ERROR_DEVICE_LOST);
        RES(ERROR_MEMORY_MAP_FAILED);
        RES(ERROR_LAYER_NOT_PRESENT);
        RES(ERROR_EXTENSION_NOT_PRESENT);
        RES(ERROR_FEATURE_NOT_PRESENT);
        RES(ERROR_INCOMPATIBLE_DRIVER);
        RES(ERROR_TOO_MANY_OBJECTS);
        RES(ERROR_FORMAT_NOT_SUPPORTED);
        RES(ERROR_FRAGMENTED_POOL);
        RES(ERROR_UNKNOWN);
        RES(ERROR_OUT_OF_POOL_MEMORY);
        RES(ERROR_INVALID_EXTERNAL_HANDLE);
        RES(ERROR_FRAGMENTATION);
        RES(ERROR_INVALID_OPAQUE_CAPTURE_ADDRESS);
        RES(ERROR_SURFACE_LOST_KHR);
        RES(ERROR_NATIVE_WINDOW_IN_USE_KHR);
        RES(SUBOPTIMAL_KHR);
        RES(ERROR_OUT_OF_DATE_KHR);
        RES(ERROR_INCOMPATIBLE_DISPLAY_KHR);
        RES(ERROR_VALIDATION_FAILED_EXT);
        RES(ERROR_INVALID_SHADER_NV);
        RES(ERROR_INCOMPATIBLE_VERSION_KHR);
        RES(ERROR_INVALID_DRM_FORMAT_MODIFIER_PLANE_LAYOUT_EXT);
        RES(ERROR_NOT_PERMITTED_EXT);
        RES(ERROR_FULL_SCREEN_EXCLUSIVE_MODE_LOST_EXT);
        RES(THREAD_IDLE_KHR);
        RES(THREAD_DONE_KHR);
        RES(OPERATION_DEFERRED_KHR);
        RES(OPERATION_NOT_DEFERRED_KHR);
        RES(ERROR_PIPELINE_COMPILE_REQUIRED_EXT);
#undef RES
    default:
        return "#&(%@!";
    }
}
