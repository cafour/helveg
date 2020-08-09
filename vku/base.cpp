#include "base.hpp"

#include "containers.hpp"
#include "device_related.hpp"
#include "log.hpp"

#include <algorithm>
#include <cstring>
#include <iostream>
#include <limits>
#include <sstream>
#include <vector>

void vku::log(VkResult result, const char *filename, int line, const char *what)
{
    if (result == VK_SUCCESS) {
        return;
    }
    std::stringstream ss;
    ss << "[" << resultString(result) << "] " << filename << ":" << line << ": " << what;
    logError(ss.str());
}

void vku::ensure(VkResult result, const char *filename, int line, const char *what)
{
    if (result == VK_SUCCESS) {
        return;
    }
    std::stringstream ss;
    ss << "[" << resultString(result) << "] " << filename << ":" << line << ": " << what;
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

bool vku::hasExtensionSupport(
    VkPhysicalDevice physicalDevice,
    const std::vector<const char *> &extensions)
{
    uint32_t extensionCount;
    vkEnumerateDeviceExtensionProperties(physicalDevice, nullptr, &extensionCount, nullptr);

    std::vector<VkExtensionProperties> available(extensionCount);
    vkEnumerateDeviceExtensionProperties(physicalDevice, nullptr, &extensionCount, available.data());

    for (auto extensionName : extensions) {
        bool containsExtension = std::any_of(available.begin(), available.end(), [extensionName](const auto &extension) {
            return !strcmp(extensionName, extension.extensionName);
        });
        if (!containsExtension) {
            return false;
        }
    }

    return true;
}

uint32_t vku::findQueueFamily(
    VkPhysicalDevice physical,
    VkQueueFlags requiredFlags,
    VkQueueFamilyProperties *queueFamily)
{
    uint32_t queueFamilyCount = 0;
    vkGetPhysicalDeviceQueueFamilyProperties(physical, &queueFamilyCount, nullptr);
    std::vector<VkQueueFamilyProperties> queueFamilies(queueFamilyCount);
    vkGetPhysicalDeviceQueueFamilyProperties(physical, &queueFamilyCount, queueFamilies.data());

    for (uint32_t i = 0; i < queueFamilyCount; ++i) {
        if (queueFamilies[i].queueFlags & requiredFlags) {
            if (queueFamily) {
                *queueFamily = queueFamilies[i];
            }
            return i;
        }
    }
    return std::numeric_limits<uint32_t>::max();
}

VkPhysicalDevice vku::findDevice(
    VkInstance instance,
    VkSurfaceKHR surface,
    uint32_t *queueIndex,
    const std::vector<const char *> *requiredExtensions)
{
    uint32_t deviceCount = 0;
    ENSURE(vkEnumeratePhysicalDevices(instance, &deviceCount, nullptr));
    if (deviceCount == 0) {
        throw std::runtime_error("A GPU with Vulkan support could not be found.");
    }

    std::vector<VkPhysicalDevice> devices(deviceCount);
    ENSURE(vkEnumeratePhysicalDevices(instance, &deviceCount, devices.data()));
    std::vector<std::pair<VkPhysicalDevice, VkPhysicalDeviceProperties>> props(deviceCount);
    std::transform(devices.begin(), devices.end(), props.begin(), [](auto d) {
        VkPhysicalDeviceProperties properties;
        vkGetPhysicalDeviceProperties(d, &properties);
        return std::make_pair(d, properties);
    });

    std::sort(props.begin(), props.end(), [](const auto &left, const auto &right) {
        const VkPhysicalDeviceProperties &leftProps = std::get<1>(left);
        const VkPhysicalDeviceProperties &rightProps = std::get<1>(right);
        return leftProps.deviceType == VK_PHYSICAL_DEVICE_TYPE_DISCRETE_GPU
            && rightProps.deviceType != VK_PHYSICAL_DEVICE_TYPE_DISCRETE_GPU;
    });

    VkPhysicalDevice chosenDevice = VK_NULL_HANDLE;
    for (auto pair : props) {
        auto device = std::get<0>(pair);
        if (requiredExtensions && !hasExtensionSupport(device, *requiredExtensions)) {
            continue;
        }

        VkQueueFamilyProperties queueFamily;
        uint32_t queue = vku::findQueueFamily(device, VK_QUEUE_GRAPHICS_BIT, &queueFamily);
        if (queue == static_cast<uint32_t>(-1)) {
            continue;
        }
        VkBool32 isPresentSupported = false;
        vkGetPhysicalDeviceSurfaceSupportKHR(device, queue, surface, &isPresentSupported);
        if (!isPresentSupported) {
            continue;
        }
        if (queueIndex) {
            *queueIndex = queue;
        }
        chosenDevice = device;
        break;
    }
    if (chosenDevice == VK_NULL_HANDLE) {
        throw std::runtime_error("A suitable physical device could not be found.");
    }
    return chosenDevice;
}

VkSurfaceFormatKHR vku::findSurfaceFormat(
    VkPhysicalDevice physicalDevice,
    VkSurfaceKHR surface)
{
    uint32_t formatCount;
    vkGetPhysicalDeviceSurfaceFormatsKHR(physicalDevice, surface, &formatCount, nullptr);
    std::vector<VkSurfaceFormatKHR> formats(formatCount);
    vkGetPhysicalDeviceSurfaceFormatsKHR(physicalDevice, surface, &formatCount, formats.data());

    VkSurfaceFormatKHR chosenFormat;
    if (formatCount == 1 && formats[0].format == VK_FORMAT_UNDEFINED) {
        chosenFormat = formats[0];
        chosenFormat.format = VK_FORMAT_B8G8R8A8_UNORM;
        return chosenFormat;
    }

    if (formatCount == 0) {
        throw std::runtime_error("Could not find a suitable surface format as the surface has no formats.");
    }

    chosenFormat.format = VK_FORMAT_UNDEFINED;
    for (auto &format : formats) {
        switch (format.format) {
        case VK_FORMAT_B8G8R8A8_UNORM:
            chosenFormat = format;
            break;

        default:
            break;
        }

        if (chosenFormat.format != VK_FORMAT_UNDEFINED) {
            break;
        }
    }

    if (chosenFormat.format == VK_FORMAT_UNDEFINED) {
        chosenFormat = formats[0];
    }

    return chosenFormat;
}

uint32_t vku::findMemoryType(
    VkPhysicalDevice physicalDevice,
    uint32_t allowedTypes,
    VkMemoryPropertyFlags requiredProperties)
{
    VkPhysicalDeviceMemoryProperties memoryProperties;
    vkGetPhysicalDeviceMemoryProperties(physicalDevice, &memoryProperties);

    for (uint32_t i = 0; i < memoryProperties.memoryTypeCount; ++i) {
        if (allowedTypes & (1 << i)
            && (memoryProperties.memoryTypes[i].propertyFlags & requiredProperties) == requiredProperties) {
            return i;
        }
    }
    throw std::runtime_error("A suitable memory type could not be found.");
}

void vku::deviceDeviceCopy(
    VkDevice device,
    VkCommandPool commandPool,
    VkQueue transferQueue,
    VkBuffer src,
    VkBuffer dst,
    VkDeviceSize size)
{
    VkCommandBufferAllocateInfo allocateInfo = {};
    allocateInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    allocateInfo.commandPool = commandPool;
    allocateInfo.commandBufferCount = 1;
    allocateInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    vku::CommandBuffers commandBuffers(device, allocateInfo);

    VkCommandBufferBeginInfo beginInfo = {};
    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
    beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
    ENSURE(vkBeginCommandBuffer(commandBuffers[0], &beginInfo));

    VkBufferCopy copy = {};
    copy.size = size;
    vkCmdCopyBuffer(commandBuffers[0], src, dst, 1, &copy);

    ENSURE(vkEndCommandBuffer(commandBuffers[0]));

    VkSubmitInfo submitInfo = {};
    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = commandBuffers;
    ENSURE(vkQueueSubmit(transferQueue, 1, &submitInfo, VK_NULL_HANDLE));

    ENSURE(vkQueueWaitIdle(transferQueue));
}

VkDeviceAddress vku::getBufferAddress(VkDevice device, VkBuffer buffer)
{
    VkBufferDeviceAddressInfo addressInfo = {};
    addressInfo.sType = VK_STRUCTURE_TYPE_BUFFER_DEVICE_ADDRESS_INFO;
    addressInfo.buffer = buffer;
    return vkGetBufferDeviceAddress(device, &addressInfo);
}

void vku::fillBuffer(
    VkDevice device,
    VkCommandPool commandPool,
    VkQueue transferQueue,
    VkBuffer dst,
    VkDeviceSize size,
    uint32_t data)
{
    auto allocateInfo = vku::CommandBuffers::allocateInfo();
    allocateInfo.commandPool = commandPool;
    allocateInfo.commandBufferCount = 1;
    allocateInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    vku::CommandBuffers commandBuffers(device, allocateInfo);

    auto beginInfo = vku::CommandBuffers::beginInfo();
    beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
    ENSURE(vkBeginCommandBuffer(commandBuffers[0], &beginInfo));

    vkCmdFillBuffer(commandBuffers[0], dst, 0, size, data);

    ENSURE(vkEndCommandBuffer(commandBuffers[0]));

    VkSubmitInfo submitInfo = {};
    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = commandBuffers;
    ENSURE(vkQueueSubmit(transferQueue, 1, &submitInfo, VK_NULL_HANDLE));

    ENSURE(vkQueueWaitIdle(transferQueue));
}

void vku::hostDeviceCopy(
    VkDevice device,
    const void *src,
    VkDeviceMemory dst,
    size_t size,
    size_t offset)
{
    void *data;
    ENSURE(vkMapMemory(device, dst, offset, size, 0, &data));
    memcpy(data, src, size);
    vkUnmapMemory(device, dst);
}

VkFormat vku::findSupportedFormat(
    VkPhysicalDevice physicalDevice,
    const std::vector<VkFormat> &candidates,
    VkImageTiling tiling,
    VkFormatFeatureFlags features)
{
    if (tiling != VK_IMAGE_TILING_OPTIMAL && tiling != VK_IMAGE_TILING_LINEAR) {
        throw std::runtime_error("this function supports only 'optimal' and 'linear' image tiling");
    }

    for (VkFormat format : candidates) {
        VkFormatProperties properties;
        vkGetPhysicalDeviceFormatProperties(physicalDevice, format, &properties);
        auto flags = tiling == VK_IMAGE_TILING_OPTIMAL
            ? properties.optimalTilingFeatures
            : properties.linearTilingFeatures;
        if ((flags & features) == features) {
            return format;
        }
    }

    throw std::runtime_error("Could not find a supported format.");
}

static VkAccessFlags getLayoutAccessMask(VkImageLayout imageLayout, bool isDst)
{
    switch (imageLayout) {
    case VK_IMAGE_LAYOUT_UNDEFINED:
        return 0;
    case VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL:
        return VK_ACCESS_TRANSFER_READ_BIT;
    case VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL:
        return VK_ACCESS_TRANSFER_WRITE_BIT;
    case VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL:
        return isDst
            ? VK_ACCESS_HOST_WRITE_BIT | VK_ACCESS_TRANSFER_WRITE_BIT
            : VK_ACCESS_SHADER_READ_BIT;
    default:
        throw std::runtime_error("Access mask for an image layout is missing.");
    }
}

// Based on https://github.com/SaschaWillems/Vulkan/blob/master/base/VulkanTools.cpp
void vku::recordImageLayoutChange(
    VkCommandBuffer cb,
    VkImage image,
    VkImageSubresourceRange subresourceRange,
    VkImageLayout oldImageLayout,
    VkImageLayout newImageLayout,
    VkPipelineStageFlags srcStageMask,
    VkPipelineStageFlags dstStageMask)
{
    // Create an image barrier object
    VkImageMemoryBarrier barrier = {};
    barrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
    barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    barrier.oldLayout = oldImageLayout;
    barrier.newLayout = newImageLayout;
    barrier.subresourceRange = subresourceRange;
    barrier.image = image;
    barrier.srcAccessMask = getLayoutAccessMask(oldImageLayout, false);
    barrier.dstAccessMask = getLayoutAccessMask(newImageLayout, true);

    vkCmdPipelineBarrier(
        cb,
        srcStageMask,
        dstStageMask,
        0,
        0,
        nullptr,
        0,
        nullptr,
        1,
        &barrier);
}

// Based on https://github.com/SaschaWillems/Vulkan/blob/master/base/VulkanTools.cpp
void vku::copyToImage(
    VkPhysicalDevice physicalDevice,
    VkDevice device,
    VkCommandPool commandPool,
    VkQueue queue,
    VkImage image,
    uint32_t width,
    uint32_t height,
    uint8_t *data)
{
    VkMemoryRequirements memoryReqs;
    vkGetImageMemoryRequirements(device, image, &memoryReqs);
    auto stagingBuffer = vku::Buffer::exclusive(device, memoryReqs.size, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    auto stagingMemory = vku::DeviceMemory::hostCoherentBuffer(physicalDevice, device, stagingBuffer);

    auto allocateInfo = vku::CommandBuffers::allocateInfo();
    allocateInfo.commandPool = commandPool;
    allocateInfo.commandBufferCount = 1;
    allocateInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    vku::CommandBuffers commandBuffers(device, allocateInfo);
    VkCommandBuffer cb = commandBuffers.front();

    vku::hostDeviceCopy(device, data, stagingMemory, width * height);

    auto beginInfo = vku::CommandBuffers::beginInfo();
    beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
    ENSURE(vkBeginCommandBuffer(cb, &beginInfo));

    VkImageSubresourceRange subresourceRange = {};
    subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    subresourceRange.baseMipLevel = 0;
    subresourceRange.levelCount = 1;
    subresourceRange.layerCount = 1;

    vku::recordImageLayoutChange(
        cb,
        image,
        subresourceRange,
        VK_IMAGE_LAYOUT_UNDEFINED,
        VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);

    VkBufferImageCopy bufferCopyRegion = {};
    bufferCopyRegion.imageSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    bufferCopyRegion.imageSubresource.mipLevel = 0;
    bufferCopyRegion.imageSubresource.layerCount = 1;
    bufferCopyRegion.imageExtent.width = width;
    bufferCopyRegion.imageExtent.height = height;
    bufferCopyRegion.imageExtent.depth = 1;

    vkCmdCopyBufferToImage(
        cb, // command buffer
        stagingBuffer, // src
        image, // dst
        VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, // dstLayout
        1, // regionCount
        &bufferCopyRegion // regions
    );

    vku::recordImageLayoutChange(
        cb,
        image,
        subresourceRange,
        VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
        VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);

    ENSURE(vkEndCommandBuffer(cb));

    VkSubmitInfo submitInfo = {};
    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = &cb;

    ENSURE(vkQueueSubmit(queue, 1, &submitInfo, VK_NULL_HANDLE));
    ENSURE(vkQueueWaitIdle(queue));
}
