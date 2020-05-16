#include "display_core.hpp"

const std::vector<const char *> vku::DisplayCore::defaultExtensions = std::vector<const char *> { VK_KHR_SWAPCHAIN_EXTENSION_NAME };

vku::DisplayCore::DisplayCore(
    VkInstance instance,
    int width,
    int height,
    const std::string &name,
    const std::vector<const char *> extensions,
    const VkPhysicalDeviceFeatures *features)
    : _instance(instance)
{
    _window = vku::Window::noApi(width, height, name);
    _surface = vku::Surface::glfw(_instance, _window);
    _physicalDevice = vku::findDevice(_instance, _surface, &_queueIndex, &extensions);
    _device = vku::Device::basic(_physicalDevice, _queueIndex, &extensions, features);
    vkGetDeviceQueue(_device, _queueIndex, 0, &_queue);
    _surfaceFormat = vku::findSurfaceFormat(physicalDevice(), surface());
    const std::vector<VkFormat> depthFormats {
        VK_FORMAT_D32_SFLOAT_S8_UINT,
        VK_FORMAT_D24_UNORM_S8_UINT
    };
}
