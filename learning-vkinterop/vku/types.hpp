#pragma once

#include "base.hpp"

#include <volk.h>

#include <utility>

namespace vku {

template <typename T, typename TCreateInfo>
using StandaloneConstructor = VkResult (*)(
    const TCreateInfo *pCreateInfo,
    const VkAllocationCallbacks *pAllocator,
    T *pDeviceObject);

template <typename T>
using StandaloneDestructor = void (*)(T standalone, const VkAllocationCallbacks *pAllocator);

template <typename TParent, typename T, typename TCreateInfo>
using RelatedConstructor = VkResult (*)(
    TParent parent,
    const TCreateInfo *pCreateInfo,
    const VkAllocationCallbacks *pAllocator,
    T *pRelated);

template <typename TParent, typename T>
using RelatedDestructor = void (*)(
    TParent parent,
    T related,
    const VkAllocationCallbacks *pAllocator);

template <typename T>
class Standalone {
protected:
    T _raw;

public:
    Standalone()
        : _raw(VK_NULL_HANDLE)
    {}
    Standalone(T raw)
        : _raw(raw)
    {}
    Standalone(const Standalone &other) = delete;
    Standalone(Standalone &&other) noexcept
        : _raw(std::exchange(other._raw, nullptr))
    {}
    Standalone &operator=(const Standalone &other) = delete;
    Standalone &operator=(Standalone &&other) noexcept
    {
        if (this != &other) {
            std::swap(_raw, other._raw);
        }
        return *this;
    }

    operator T() { return _raw; }
    T raw() { return _raw; }
};

template <
    typename T,
    typename TCreateInfo,
    StandaloneConstructor<T, TCreateInfo> *vkCreate,
    StandaloneDestructor<T> *vkDestroy>
class StandaloneConstructible : public Standalone<T> {
protected:
    using Standalone<T>::_raw;

public:
    using Standalone<T>::Standalone;
    StandaloneConstructible(TCreateInfo &createInfo)
    {
        ENSURE((*vkCreate)(&createInfo, nullptr, &_raw));
    }
    ~StandaloneConstructible()
    {
        if (_raw != VK_NULL_HANDLE) {
            (*vkDestroy)(_raw, nullptr);
        }
    }
    StandaloneConstructible(StandaloneConstructible &&other) noexcept = default;
    StandaloneConstructible& operator=(StandaloneConstructible &&other) noexcept = default;
};

template <typename T>
class InstanceRelated : public Standalone<T> {
protected:
    VkInstance _instance;

public:
    InstanceRelated(VkInstance Instance, T raw)
        : Standalone<T>(raw)
        , _instance(Instance)
    {}
    InstanceRelated(const InstanceRelated &other) = delete;
    InstanceRelated(InstanceRelated &&other) noexcept
        : Standalone<T>(std::move(other))
        , _instance(std::exchange(other._instance, nullptr))
    {}
    InstanceRelated &operator=(const InstanceRelated &other) = delete;
    InstanceRelated &operator=(InstanceRelated &&other) noexcept
    {
        if (this != &other) {
            std::swap(_instance, other._instance);
        }
        Standalone<T>::operator=(std::move(other));
        return *this;
    }

    VkInstance Instance() { return _instance; }
};

template <
    typename T,
    typename TCreateInfo,
    RelatedConstructor<VkInstance, T, TCreateInfo> *vkCreate,
    RelatedDestructor<VkInstance, T> *vkDestroy>
class InstanceConstructible : public InstanceRelated<T> {
protected:
    using InstanceRelated<T>::_instance;
    using InstanceRelated<T>::_raw;

public:
    using InstanceRelated<T>::InstanceRelated;
    InstanceConstructible(VkInstance instance, TCreateInfo &createInfo)
        : InstanceRelated<T>(instance, VK_NULL_HANDLE)
    {
        ENSURE((*vkCreate)(instance, &createInfo, nullptr, &_raw));
    }
    ~InstanceConstructible()
    {
        if (_instance != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
            (*vkDestroy)(_instance, _raw, nullptr);
        }
    }
    InstanceConstructible(InstanceConstructible &&other) noexcept = default;
    InstanceConstructible &operator=(InstanceConstructible &&other) noexcept = default;
};

template <typename T>
class DeviceRelated : public Standalone<T> {
protected:
    VkDevice _device;

public:
    DeviceRelated(VkDevice device, T raw)
        : Standalone<T>(raw)
        , _device(device)
    {}
    DeviceRelated(DeviceRelated &&other) noexcept
        : Standalone<T>(std::move(other))
        , _device(std::exchange(other._device, nullptr))
    {}
    DeviceRelated &operator=(DeviceRelated &&other) noexcept
    {
        if (this != &other) {
            std::swap(_device, other._device);
        }
        Standalone<T>::operator=(std::move(other));
        return *this;
    }

    VkDevice device() { return _device; }
};

template <
    typename T,
    typename TCreateInfo,
    RelatedConstructor<VkDevice, T, TCreateInfo> *vkCreate,
    RelatedDestructor<VkDevice, T> *vkDestroy>
class DeviceConstructible : public DeviceRelated<T> {
protected:
    using DeviceRelated<T>::_device;
    using DeviceRelated<T>::_raw;

public:
    using DeviceRelated<T>::DeviceRelated;
    DeviceConstructible(VkDevice device, TCreateInfo &createInfo)
        : DeviceRelated<T>(device, VK_NULL_HANDLE)
    {
        ENSURE((*vkCreate)(device, &createInfo, nullptr, &_raw));
    }
    ~DeviceConstructible()
    {
        if (_device != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
            (*vkDestroy)(_device, _raw, nullptr);
        }
    }
    DeviceConstructible(DeviceConstructible &&other) noexcept = default;
    DeviceConstructible &operator=(DeviceConstructible &&other) noexcept = default;
};

}
