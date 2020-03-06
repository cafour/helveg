#include "instance.hpp"
#include "base.hpp"

#include <GLFW/glfw3.h>

#include <iostream>
#include <stdexcept>
#include <vector>

static VKAPI_ATTR VkBool32 VKAPI_CALL debugCallback(
    VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity,
    VkDebugUtilsMessageTypeFlagsEXT messageType,
    const VkDebugUtilsMessengerCallbackDataEXT *pCallbackData,
    void *pUserData)
{
    std::cerr << pCallbackData->pMessageIdName << ": " << pCallbackData->pMessage << std::endl;
    return VK_FALSE;
}

static VkDebugUtilsMessengerCreateInfoEXT getMessengerCreateInfo()
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

vku::Instance::Instance(
    const char *name,
    bool useDebugMessenger,
    const char **layers,
    size_t layerCount,
    const char **extensions,
    size_t extensionCount)
{
    VkApplicationInfo appInfo = {};
    appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
    appInfo.pApplicationName = name;
    appInfo.applicationVersion = VK_MAKE_VERSION(0, 0, 0);
    appInfo.engineVersion = VK_MAKE_VERSION(0, 0, 0);
    appInfo.apiVersion = VK_API_VERSION_1_2;

    VkInstanceCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
    createInfo.pApplicationInfo = &appInfo;

    for (size_t i = 0; i < layerCount; ++i) {
        auto &layerName = _layers.emplace_back(layers[i]);
        _layerPtrs.push_back(layerName.c_str());
    }

    uint32_t glfwExtensionCount = 0;
    const char **glfwExtensions = glfwGetRequiredInstanceExtensions(&glfwExtensionCount);
    for (size_t i = 0; i < glfwExtensionCount; ++i) {
        auto &extensionName = _extensions.emplace_back(glfwExtensions[i]);
        _extensionPtrs.push_back(extensionName.c_str());
    }

    for (size_t i = 0; i < extensionCount; ++i) {
        auto &extensionName = _extensions.emplace_back(extensions[i]);
        _extensionPtrs.push_back(extensionName.c_str());
    }

    VkDebugUtilsMessengerCreateInfoEXT messengerCreateInfo;
    if (useDebugMessenger) {
        _extensionPtrs.push_back(_extensions.emplace_back(VK_EXT_DEBUG_UTILS_EXTENSION_NAME).c_str());
        _layerPtrs.push_back(_layers.emplace_back("VK_LAYER_KHRONOS_validation").c_str());
        // create a messenger for calls to vkCreateInstance and vkDestroyInstance
        messengerCreateInfo = getMessengerCreateInfo();
        createInfo.pNext = &messengerCreateInfo;
    }

    ensureLayers(_layerPtrs.data(), _layerPtrs.size());
    createInfo.enabledLayerCount = static_cast<uint32_t>(_layerPtrs.size());
    createInfo.ppEnabledLayerNames = _layerPtrs.data();

    createInfo.enabledExtensionCount = static_cast<uint32_t>(_extensionPtrs.size());
    createInfo.ppEnabledExtensionNames = _extensionPtrs.data();

    ENSURE(vkCreateInstance, &createInfo, nullptr, &_raw);
    volkLoadInstance(_raw);

    if (useDebugMessenger) {
        // create another messenger for all other calls
        ENSURE(vkCreateDebugUtilsMessengerEXT, _raw, &messengerCreateInfo, nullptr, &_messenger);
    }
}

vku::Instance::~Instance()
{
    if (_messenger) {
        vkDestroyDebugUtilsMessengerEXT(_raw, _messenger, nullptr);
    }
    vkDestroyInstance(_raw, nullptr);
}
