#pragma once

#include "vku.hpp"

#include <string>
#include <vector>

class App {
private:
    vku::Window _window;
    vku::Instance _instance;
    vku::Surface _surface;
    vku::PhysicalDevice _physicalDevice;
    vku::Device _device;
    vku::RenderPass _renderPass;
    vku::Swapchain _swapchain;
    vku::Pipeline _pipeline;
    vku::CommandPool _commandPool;
    VkQueue _queue;
    std::vector<VkCommandBuffer> _commandBuffers;

    const std::vector<const char *> DEVICE_EXTENSIONS = {
        VK_KHR_SWAPCHAIN_EXTENSION_NAME
    };

#if NDEBUG
    const bool IS_DEBUG = false;
#else
    const bool IS_DEBUG = true;
#endif

public:
    App(const std::string& name, int width, int height, std::vector<const char *> deviceExtensions)
        : _window(width, height, name.c_str())
        , _instance(name.c_str(), IS_DEBUG)
        , _surface(_instance, _window)
        , _physicalDevice(_instance, _surface)
        , _device(_physicalDevice, DEVICE_EXTENSIONS.data(), DEVICE_EXTENSIONS.size())
        , _renderPass(_device)
        , _swapchain(_renderPass)
        , _pipeline(_renderPass, ) {
    }
};
