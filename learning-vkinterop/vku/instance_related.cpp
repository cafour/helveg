#include "instance_related.hpp"

#include <iostream>

static VKAPI_ATTR VkBool32 VKAPI_CALL debugCallback(
    VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity,
    VkDebugUtilsMessageTypeFlagsEXT messageType,
    const VkDebugUtilsMessengerCallbackDataEXT *pCallbackData,
    void *pUserData)
{
    (void)messageSeverity;
    (void)messageType;
    (void)pUserData;
    std::cerr << "[" << pCallbackData->pMessageIdName << "] " << pCallbackData->pMessage << std::endl;
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

vku::Surface vku::Surface::glfw(VkInstance instance, GLFWwindow *window)
{
    VkSurfaceKHR raw;
    ENSURE(glfwCreateWindowSurface(instance, window, nullptr, &raw));
    return vku::Surface(instance, raw);
}
