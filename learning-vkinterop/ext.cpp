#include "ext.hpp"

#define PTR(fn) static PFN_##fn pfn_##fn

#define LOAD(instance, fn) \
    pfn_##fn = reinterpret_cast<PFN_##fn>(vkGetInstanceProcAddr((instance), #fn));

PTR(vkCreateDebugUtilsMessengerEXT);
VKAPI_ATTR VkResult VKAPI_CALL vkCreateDebugUtilsMessengerEXT(
    VkInstance instance,
    const VkDebugUtilsMessengerCreateInfoEXT* pCreateInfo,
    const VkAllocationCallbacks* pAllocator,
    VkDebugUtilsMessengerEXT* pMessenger)
{
    return pfn_vkCreateDebugUtilsMessengerEXT(instance, pCreateInfo, pAllocator, pMessenger);
}

PTR(vkDestroyDebugUtilsMessengerEXT);
VKAPI_ATTR void VKAPI_CALL vkDestroyDebugUtilsMessengerEXT(
    VkInstance instance,
    VkDebugUtilsMessengerEXT messenger,
    const VkAllocationCallbacks* pAllocator)
{
    return pfn_vkDestroyDebugUtilsMessengerEXT(instance, messenger, pAllocator);
}

void loadExtFunctions(VkInstance instance)
{
    LOAD(instance, vkCreateDebugUtilsMessengerEXT);
    LOAD(instance, vkDestroyDebugUtilsMessengerEXT);
}
