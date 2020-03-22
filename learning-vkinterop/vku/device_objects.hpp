#pragma once

#include "base.hpp"

#include <volk.h>

namespace vku {

template <typename T, typename TCreateInfo>
using DeviceObjectConstructor = VkResult (**)(
    VkDevice device,
    const TCreateInfo *pCreateInfo,
    const VkAllocationCallbacks *pAllocator,
    T *pDeviceObject);

template <typename T>
using DeviceObjectDestructor = void (**)(
    VkDevice device,
    T deviceObject,
    const VkAllocationCallbacks *pAllocator);

template <
    typename T,
    typename TCreateInfo,
    DeviceObjectConstructor<T, TCreateInfo> vkConstructor,
    DeviceObjectDestructor<T> vkDestructor>
class DeviceObject {
protected:
    VkDevice _device;
    T _raw;

public:
    DeviceObject(VkDevice device, TCreateInfo &createInfo)
        : _device(device)
    {
        ENSURE((*vkConstructor)(device, &createInfo, nullptr, &_raw));
    }
    DeviceObject(VkDevice device, T raw)
        : _device(device)
        , _raw(raw)
    {
    }
    ~DeviceObject()
    {
        if (_device != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
            (*vkDestructor)(_device, _raw, nullptr);
        }
    }
    DeviceObject(const DeviceObject &other) = delete;
    DeviceObject(DeviceObject &&other) noexcept
        : _device(std::exchange(other._device, nullptr))
        , _raw(std::exchange(other._raw, nullptr))
    {
    }
    DeviceObject &operator=(const DeviceObject &other) = delete;
    DeviceObject &operator=(DeviceObject &&other) noexcept
    {
        if (this != &other) {
            std::swap(_device, other._device);
            std::swap(_raw, other._raw);
        }
    }

    operator T() { return _raw; }
    T raw() { return _raw; }
    VkDevice device() { return _device; }
};

class Semaphore : public DeviceObject<VkSemaphore, VkSemaphoreCreateInfo, &vkCreateSemaphore, &vkDestroySemaphore> {
public:
    using DeviceObject::DeviceObject;
    static Semaphore basic(VkDevice device);
};

class Fence : public DeviceObject<VkFence, VkFenceCreateInfo, &vkCreateFence, &vkDestroyFence> {
public:
    using DeviceObject::DeviceObject;
    static Fence basic(VkDevice device);
};
}
