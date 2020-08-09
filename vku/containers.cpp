#include "containers.hpp"

#include "device_related.hpp"

#include <cstdint>

void vku::CommandBuffers::submitSingle(vku::CommandBuffers &commandBuffers, VkQueue queue)
{
    VkSubmitInfo submitInfo = {};
    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = commandBuffers;
    auto fence = vku::Fence::basic(commandBuffers._device);
    ENSURE(vkResetFences(commandBuffers._device, 1, fence));
    ENSURE(vkQueueSubmit(queue, 1, &submitInfo, fence));
    ENSURE(vkWaitForFences(commandBuffers._device, 1, fence, VK_TRUE, UINT64_MAX));
}
