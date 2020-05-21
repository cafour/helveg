#pragma once

#include "wrapper.hpp"

#include <stdexcept>

namespace vku {
class TransferCore {
private:
    VkPhysicalDevice _physical;
    VkDevice _device;
    uint32_t _transferQueueFamily;
    VkQueue _transferQueue;
    vku::CommandPool _transferPool;

public:
    TransferCore(VkPhysicalDevice physical, VkDevice device);

    VkPhysicalDevice physicalDevice() { return _physical; }
    VkDevice device() { return _device; }
    uint32_t transferQueueFamily() { return _transferQueueFamily; }
    VkQueue transferQueue() { return _transferQueue; }
    vku::CommandPool &transferPool() { return _transferPool; }
};
}
