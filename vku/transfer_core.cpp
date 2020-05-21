#include "transfer_core.hpp"

vku::TransferCore::TransferCore(VkPhysicalDevice physical, VkDevice device)
    : _physical(physical)
    , _device(device)
{
    _transferQueueFamily = vku::findQueueFamily(_physical, VK_QUEUE_TRANSFER_BIT);
    if (_transferQueueFamily == static_cast<uint32_t>(-1)) {
        throw std::runtime_error("failed to find a transfer queue");
    }
    vkGetDeviceQueue(device, _transferQueueFamily, 0, &_transferQueue);
    _transferPool = vku::CommandPool::basic(_device, _transferQueueFamily);
}
