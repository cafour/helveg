#pragma once

#include "base.hpp"

#include <algorithm>
#include <utility>
#include <vector>

namespace vku {

class CommandBuffers {
private:
    VkDevice _device = VK_NULL_HANDLE;
    VkCommandPool _commandPool = VK_NULL_HANDLE;
    std::vector<VkCommandBuffer> _raw;

public:
    typedef std::vector<VkCommandBuffer>::value_type value_type;
    typedef std::vector<VkCommandBuffer>::reference reference;
    typedef std::vector<VkCommandBuffer>::const_reference const_reference;
    typedef std::vector<VkCommandBuffer>::iterator iterator;
    typedef std::vector<VkCommandBuffer>::const_iterator const_iterator;
    typedef std::vector<VkCommandBuffer>::difference_type difference_type;
    typedef std::vector<VkCommandBuffer>::size_type size_type;
    typedef std::vector<VkCommandBuffer>::reverse_iterator reverse_iterator;
    typedef std::vector<VkCommandBuffer>::const_reverse_iterator const_reverse_iterator;

    CommandBuffers() = default;
    CommandBuffers(VkDevice device, VkCommandBufferAllocateInfo &allocateInfo)
        : _device(device)
        , _commandPool(allocateInfo.commandPool)
        , _raw(allocateInfo.commandBufferCount)
    {
        ENSURE(vkAllocateCommandBuffers(device, &allocateInfo, _raw.data()));
    }
    ~CommandBuffers()
    {
        if (_device != VK_NULL_HANDLE && _commandPool != VK_NULL_HANDLE) {
            vkFreeCommandBuffers(_device, _commandPool, static_cast<uint32_t>(_raw.size()), _raw.data());
        }
    }
    CommandBuffers(const CommandBuffers &other) = delete;
    CommandBuffers(CommandBuffers &&other) noexcept
        : _device(std::exchange(other._device, static_cast<VkDevice>(VK_NULL_HANDLE)))
        , _commandPool(std::exchange(other._commandPool, static_cast<VkCommandPool>(VK_NULL_HANDLE)))
        , _raw(std::exchange(other._raw, std::vector<VkCommandBuffer>()))
    {}
    CommandBuffers &operator=(const CommandBuffers &other) = delete;
    CommandBuffers &operator=(CommandBuffers &&other) noexcept
    {
        swap(other);
        return *this;
    }

    operator VkCommandBuffer *() { return _raw.data(); }
    VkCommandBuffer *raw() { return _raw.data(); }

    iterator begin() noexcept { return _raw.begin(); }
    iterator end() noexcept { return _raw.end(); }
    const_iterator cbegin() const noexcept { return _raw.cbegin(); }
    const_iterator cend() const noexcept { return _raw.cend(); }
    bool operator==(const CommandBuffers &other)
    {
        return _device == other._device
            && _commandPool == other._commandPool
            && _raw == other._raw;
    }
    void swap(CommandBuffers &other) noexcept
    {
        if (this != &other) {
            std::swap(_device, other._device);
            std::swap(_commandPool, other._commandPool);
            std::swap(_raw, other._raw);
        }
    }
    size_type size() const noexcept { return _raw.size(); }
    size_type max_size() const noexcept { return _raw.max_size(); }
    size_type empty() const noexcept { return _raw.empty(); }

    reference front() { return _raw.front(); }
    const_reference front() const { return _raw.front(); }
    reference back() { return _raw.back(); }
    const_reference back() const { return _raw.back(); }
    reference operator[](size_t i) { return _raw[i]; }
    const_reference operator[](size_t i) const { return _raw[i]; }
    reference at(size_t i) { return _raw.at(i); }
    const_reference at(size_t i) const { return _raw.at(i); }

    reverse_iterator rbegin() noexcept { return _raw.rbegin(); }
    reverse_iterator rend() noexcept { return _raw.rend(); }
    const_reverse_iterator crbegin() const noexcept { return _raw.crbegin(); }
    const_reverse_iterator crend() const noexcept { return _raw.crend(); }

    static VkCommandBufferAllocateInfo allocateInfo()
    {
        VkCommandBufferAllocateInfo info = {};
        info.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
        return info;
    }

    static VkCommandBufferBeginInfo beginInfo()
    {
        VkCommandBufferBeginInfo info = {};
        info.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
        return info;
    }
};

}
