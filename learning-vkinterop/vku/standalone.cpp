#include "standalone.hpp"
#include "instance_related.hpp"

#include <GLFW/glfw3.h>

#include <utility>

vku::Instance vku::Instance::basic(
    const std::string &appName,
    bool usesGlfw,
    bool isDebug,
    const std::vector<const char *> *extensions,
    const std::vector<const char *> *layers,
    std::optional<vku::DebugMessenger> *messenger)
{
    VkApplicationInfo appInfo = {};
    appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
    appInfo.pApplicationName = appName.c_str();
    appInfo.applicationVersion = VK_MAKE_VERSION(0, 0, 0);
    appInfo.engineVersion = VK_MAKE_VERSION(0, 0, 0);
    appInfo.apiVersion = VK_API_VERSION_1_2;

    VkInstanceCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
    createInfo.pApplicationInfo = &appInfo;

    std::vector<const char *> extensionNames;
    if (extensions) {
        for (auto extension : *extensions) {
            extensionNames.push_back(extension);
        }
    }
    if (usesGlfw) {
        uint32_t glfwExtensionCount = 0;
        const char **glfwExtensions = glfwGetRequiredInstanceExtensions(&glfwExtensionCount);
        for (size_t i = 0; i < glfwExtensionCount; ++i) {
            extensionNames.push_back(glfwExtensions[i]);
        }
    }

    std::vector<const char *> layerNames;
    if (layers) {
        for (auto layer : *layers) {
            layerNames.push_back(layer);
        }
    }

    VkDebugUtilsMessengerCreateInfoEXT messengerCreateInfo;
    if (isDebug) {
        extensionNames.push_back(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
        layerNames.push_back("VK_LAYER_KHRONOS_validation");
        messengerCreateInfo = vku::DebugMessenger::cerrCreateInfo();
        createInfo.pNext = &messengerCreateInfo;
    }

    ensureLayers(layerNames.data(), layerNames.size());

    createInfo.enabledLayerCount = static_cast<uint32_t>(layerNames.size());
    createInfo.ppEnabledLayerNames = layerNames.data();

    createInfo.enabledExtensionCount = static_cast<uint32_t>(extensionNames.size());
    createInfo.ppEnabledExtensionNames = extensionNames.data();

    auto instance = vku::Instance(createInfo);
    if (isDebug && messenger) {
        *messenger = vku::DebugMessenger::cerr(instance);
    }
    return instance;
}

vku::Device vku::Device::basic(
    VkPhysicalDevice physicalDevice,
    uint32_t queueIndex,
    const std::vector<const char *> *extensions)
{
    float queuePriority = 1.0f;
    VkDeviceQueueCreateInfo queueCreateInfo = {};
    queueCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
    queueCreateInfo.queueFamilyIndex = queueIndex;
    queueCreateInfo.queueCount = 1;
    queueCreateInfo.pQueuePriorities = &queuePriority;

    VkPhysicalDeviceFeatures features = {};

    VkDeviceCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
    createInfo.pQueueCreateInfos = &queueCreateInfo;
    createInfo.queueCreateInfoCount = 1;
    createInfo.pEnabledFeatures = &features;
    if (extensions) {
        createInfo.enabledExtensionCount = static_cast<uint32_t>(extensions->size());
        createInfo.ppEnabledExtensionNames = extensions->data();
    }

    return vku::Device(physicalDevice, createInfo);
}
