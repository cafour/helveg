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

struct StructureHeader {
    VkStructureType sType;
    void *pNext;
};

void log(VkResult result, const char *filename, int line, const char *what);

void ensure(VkResult result, const char *filename, int line, const char *what);

void ensureLayers(const char **layers, size_t length);

const char *resultString(VkResult result);

bool hasExtensionSupport(
    VkPhysicalDevice physicalDevice,
    const std::vector<const char *> &extensions);

uint32_t findQueueFamily(
    VkPhysicalDevice physical,
    VkQueueFlags requiredFlags,
    VkQueueFamilyProperties *queueFamily = nullptr);

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

void deviceDeviceCopy(
    VkDevice device,
    VkCommandPool commandPool,
    VkQueue transferQueue,
    VkBuffer src,
    VkBuffer dst,
    VkDeviceSize size);

void hostDeviceCopy(
    VkDevice device,
    const void *src,
    VkDeviceMemory dst,
    size_t size,
    size_t offset = 0);

void fillBuffer(
    VkDevice device,
    VkCommandPool commandPool,
    VkQueue transferQueue,
    VkBuffer dst,
    VkDeviceSize size,
    uint32_t data);

VkFormat findSupportedFormat(
    VkPhysicalDevice physicalDevice,
    const std::vector<VkFormat> &candidates,
    VkImageTiling tiling,
    VkFormatFeatureFlags features);

template <typename Properties>
Properties findProperties(
    VkPhysicalDevice physicalDevice,
    VkStructureType propertiesType)
{
    Properties result = {};
    result.sType = propertiesType;
    VkPhysicalDeviceProperties2 properties = {};
    properties.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_PROPERTIES_2;
    properties.pNext = &result;
    vkGetPhysicalDeviceProperties2(physicalDevice, &properties);
    return result;
}

VkDeviceAddress getBufferAddress(VkDevice device, VkBuffer buffer);

VkDeviceOrHostAddressConstKHR addressConst(VkDeviceAddress deviceAddress);

VkDeviceOrHostAddressConstKHR addressConst(const void *hostAddress);

void recordImageLayoutChange(
    VkCommandBuffer cb,
    VkImage image,
    VkImageSubresourceRange subresourceRange,
    VkImageLayout oldImageLayout,
    VkImageLayout newImageLayout,
    VkPipelineStageFlags srcStageMask = VK_PIPELINE_STAGE_ALL_COMMANDS_BIT,
    VkPipelineStageFlags dstStageMask = VK_PIPELINE_STAGE_ALL_COMMANDS_BIT);

void copyToImage(
    VkPhysicalDevice physicalDevice,
    VkDevice device,
    VkCommandPool commandPool,
    VkQueue queue,
    VkImage image,
    uint32_t width,
    uint32_t height,
    uint8_t *data);

}
