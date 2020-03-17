#pragma once

#include "device.hpp"

#include <volk.h>

namespace vku {

class CommandPool {
private:
    VkCommandPool _raw;
    Device &_device;

public:
    CommandPool(Device &device);
    ~CommandPool();

    operator VkCommandPool() { return _raw; }
    VkCommandPool *raw() { return &_raw; }

    Device &device() { return _device; }
};
}
