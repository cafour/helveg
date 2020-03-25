#include "app.hpp"
#include "physical_device.hpp"

#include <algorithm>
#include <vector>

vku::App::App(
    const std::string &name,
    int width,
    int height,
    const std::vector<const char *> *instanceExtensions,
    const std::vector<const char *> *layers,
    const std::vector<const char *> *deviceExtensions)
{
    _window = vku::Window::noApi(width, height, name);
    _instance = vku::Instance::basic(name, true, IS_DEBUG, instanceExtensions, layers, &_debugMessenger);
    _surface = vku::Surface::glfw(_instance, _window);

    size_t extensionCount = 1;
    if (deviceExtensions) {
        extensionCount += deviceExtensions->size();
    }
    std::vector<const char *> actualDeviceExtensions(extensionCount);
    actualDeviceExtensions[0] = VK_KHR_SWAPCHAIN_EXTENSION_NAME;
    if (deviceExtensions) {
        std::copy(deviceExtensions->begin(), deviceExtensions->end(), std::next(actualDeviceExtensions.begin()));
    }

    _physicalDevice = vku::findDevice(_instance, _surface, &_queueIndex, &actualDeviceExtensions);
    _device = vku::Device::basic(_physicalDevice, _queueIndex, &actualDeviceExtensions);
    auto surfaceFormat = vku::findSurfaceFormat(_physicalDevice, _surface);
    _renderPass = vku::RenderPass::basic(_device, surfaceFormat.format);
    _swapchainEnv = std::make_unique<vku::SwapchainEnv>(
        vku::SwapchainEnv::basic(_device, _physicalDevice, _surface, _renderPass));
}
