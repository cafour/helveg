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
    (void)messageSeverity;
    (void)messageType;
    (void)pUserData;
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
        _layers.emplace_back(layers[i]);
    }

    uint32_t glfwExtensionCount = 0;
    const char **glfwExtensions = glfwGetRequiredInstanceExtensions(&glfwExtensionCount);
    for (size_t i = 0; i < glfwExtensionCount; ++i) {
        _extensions.emplace_back(glfwExtensions[i]);
    }

    for (size_t i = 0; i < extensionCount; ++i) {
        _extensions.emplace_back(extensions[i]);
    }

    VkDebugUtilsMessengerCreateInfoEXT messengerCreateInfo;
    if (useDebugMessenger) {
        _extensions.emplace_back(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
        _layers.emplace_back("VK_LAYER_KHRONOS_validation");
        // create a messenger for calls to vkCreateInstance and vkDestroyInstance
        messengerCreateInfo = getMessengerCreateInfo();
        createInfo.pNext = &messengerCreateInfo;
    }

    _layerPtrs.resize(_layers.size());
    for (size_t i = 0; i < _layers.size(); ++i) {
        _layerPtrs[i] = _layers[i].c_str();
    }
    _extensionPtrs.resize(_extensions.size());
    for (size_t i = 0; i < _extensions.size(); ++i) {
        _extensionPtrs[i] = _extensions[i].c_str();
    }

    ensureLayers(_layerPtrs.data(), _layerPtrs.size());
    createInfo.enabledLayerCount = static_cast<uint32_t>(_layerPtrs.size());
    createInfo.ppEnabledLayerNames = _layerPtrs.data();

    createInfo.enabledExtensionCount = static_cast<uint32_t>(_extensionPtrs.size());
    createInfo.ppEnabledExtensionNames = _extensionPtrs.data();

    ENSURE(vkCreateInstance(&createInfo, nullptr, &_raw));
    volkLoadInstance(_raw);

    if (useDebugMessenger) {
        // create another messenger for all other calls
        ENSURE(vkCreateDebugUtilsMessengerEXT(_raw, &messengerCreateInfo, nullptr, &_messenger));
    }
}

vku::Instance::~Instance()
{
    if (_messenger) {
        vkDestroyDebugUtilsMessengerEXT(_raw, _messenger, nullptr);
    }
    vkDestroyInstance(_raw, nullptr);
}
