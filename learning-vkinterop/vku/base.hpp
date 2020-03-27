#pragma once

#include <volk.h>

#include <utility>
#include <vector>

#define _FILENAME (static_cast<const char *>(__FILE__) + ROOT_PATH_LENGTH)
#define _STR(what) #what
#define _TOSTR(what) _STR(what)
#define LOG(vkInvocation) vku::log(vkInvocation, _FILENAME, __LINE__, #vkInvocation)
#define ENSURE(vkInvocation) vku::ensure(vkInvocation, _FILENAME, __LINE__, #vkInvocation)

namespace vku {

void log(VkResult result, const char *filename, int line, const char *what);

void ensure(VkResult result, const char *filename, int line, const char *what);

void ensureLayers(const char **layers, size_t length);

const char *resultString(VkResult result);

bool hasExtensionSupport(
    VkPhysicalDevice physicalDevice,
    const std::vector<const char *> &extensions);

VkPhysicalDevice findDevice(
    VkInstance instance,
    VkSurfaceKHR surface,
    uint32_t *queueIndex,
    const std::vector<const char *> *requiredExtensions = nullptr);

VkSurfaceFormatKHR findSurfaceFormat(
    VkPhysicalDevice physicalDevice,
    VkSurfaceKHR surface);

uint32_t findMemoryType(
    VkPhysicalDevice physicalDevice,
    uint32_t allowedTypes,
    VkMemoryPropertyFlags requiredProperties);

void copy(
    VkDevice device,
    VkCommandPool commandPool,
    VkQueue transferQueue,
    VkBuffer src,
    VkBuffer dst,
    VkDeviceSize size);
}
