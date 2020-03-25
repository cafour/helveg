#include "instance.hpp"
#include "base.hpp"
#include "debug_messenger.hpp"

#include <GLFW/glfw3.h>
#include <utility>

vku::Instance::Instance(VkInstance raw)
    : _raw(raw)
{
}

vku::Instance::Instance(VkInstanceCreateInfo &createInfo)
{
    ENSURE(vkCreateInstance(&createInfo, nullptr, &_raw));
    volkLoadInstance(_raw);
}

vku::Instance::~Instance()
{
    if (_raw != VK_NULL_HANDLE) {
        vkDestroyInstance(_raw, nullptr);
    }
}

vku::Instance::Instance(vku::Instance &&other) noexcept
    : _raw(std::exchange(other._raw, nullptr))
{
}

vku::Instance &vku::Instance::operator=(vku::Instance &&other) noexcept
{
    if (this != &other) {
        std::swap(_raw, other._raw);
    }
    return *this;
}

