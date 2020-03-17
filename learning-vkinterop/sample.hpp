#pragma once

#include "shaders.hpp"
#include "vku.hpp"

#include <algorithm>
#include <functional>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

#ifdef NDEBUG
const bool ENABLE_VALIDATION = false;
#else
const bool ENABLE_VALIDATION = true;
#endif

const std::vector<const char *> DEVICE_EXTENSIONS = {
    VK_KHR_SWAPCHAIN_EXTENSION_NAME
};

class Sample {
private:
    vku::Window _window;
    vku::Instance _instance;
    vku::Surface _surface;
    vku::PhysicalDevice _physicalDevice;
    vku::Device _device;
    vku::RenderPass _renderPass;
    vku::Swapchain _swapchain;
    vku::SwapchainFrame _swapchainFrame;
    vku::Pipeline _pipeline;
    vku::CommandPool _commandPool;
    VkQueue _queue;
    std::vector<VkCommandBuffer> _commandBuffers;

    static void onResize(vku::Window &window, void *userData)
    {
        (void)window;
        auto sample = reinterpret_cast<Sample *>(userData);
        sample->resized = true;
    }
    void recordCommands();

public:
    bool resized = false;

    Sample(int width, int height)
        : _window(width, height, "Vulkan", &onResize, this)
        , _instance("Sample", ENABLE_VALIDATION)
        , _surface(_instance, _window)
        , _physicalDevice(_instance, _surface)
        , _device(_physicalDevice, DEVICE_EXTENSIONS.data(), DEVICE_EXTENSIONS.size())
        , _renderPass(_device)
        , _swapchain(_renderPass)
        , _pipeline(_renderPass,
              vku::Shader(_device, VERTEX_SHADER, VERTEX_SHADER_LENGTH),
              vku::Shader(_device, FRAGMENT_SHADER, FRAGMENT_SHADER_LENGTH))
        , _commandPool(_device)
    {
        vkGetDeviceQueue(_device, _physicalDevice.queueIndex(), 0, &_queue);
        recordCommands();
    }

    ~Sample()
    {
        vkFreeCommandBuffers(
            _device,
            _commandPool,
            static_cast<uint32_t>(_commandBuffers.size()),
            _commandBuffers.data());
    }

    void step();
    void run();
};
