#include "debug_messenger.hpp"
#include "base.hpp"

#include <iostream>
#include <utility>

vku::DebugMessenger::DebugMessenger(VkInstance instance, VkDebugUtilsMessengerEXT raw)
    : _instance(instance)
    , _raw(raw)
{
}

vku::DebugMessenger::DebugMessenger(VkInstance instance, VkDebugUtilsMessengerCreateInfoEXT &createInfo)
    : _instance(instance)
{
    ENSURE(vkCreateDebugUtilsMessengerEXT(instance, &createInfo, nullptr, &_raw));
}

vku::DebugMessenger::~DebugMessenger()
{
    vkDestroyDebugUtilsMessengerEXT(_instance, _raw, nullptr);
}

vku::DebugMessenger::DebugMessenger(vku::DebugMessenger &&other)
    : _instance(std::exchange(other._instance, nullptr))
    , _raw(std::exchange(other._raw, nullptr))
{
}

vku::DebugMessenger &vku::DebugMessenger::operator=(vku::DebugMessenger &&other)
{
    if (this != &other) {
        _instance = std::exchange(other._instance, nullptr);
        _raw = std::exchange(other._raw, nullptr);
    }
    return *this;
}

static VKAPI_ATTR VkBool32 VKAPI_CALL debugCallback(
    VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity,
    VkDebugUtilsMessageTypeFlagsEXT messageType,
    const VkDebugUtilsMessengerCallbackDataEXT *pCallbackData,
    void *pUserData)
{
    (void)messageSeverity;
    (void)messageType;
    (void)pUserData;
    std::cerr << pCallbackData->pMessageIdName << ": " << pCallbackData->pMessage << std::endl;
    return VK_FALSE;
}

VkDebugUtilsMessengerCreateInfoEXT vku::DebugMessenger::cerrCreateInfo()
{
    VkDebugUtilsMessengerCreateInfoEXT createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
    createInfo.messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT
        | VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT
        | VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;
    createInfo.messageType = VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT
        | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT
        | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
    createInfo.pfnUserCallback = debugCallback;
    return createInfo;
}

vku::DebugMessenger vku::DebugMessenger::cerr(VkInstance instance)
{
    auto createInfo = cerrCreateInfo();
    return vku::DebugMessenger(instance, createInfo);
}
