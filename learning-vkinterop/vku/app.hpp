#pragma once

#include "vku.hpp"

#include <memory>
#include <optional>
#include <string>
#include <vector>

class App {
private:
    std::unique_ptr<vku::Window> _window;
    std::unique_ptr<vku::Instance> _instance;
    std::optional<vku::DebugMessenger> _debugMessenger;
    std::unique_ptr<vku::Surface> _surface;
    uint32_t _queueIndex;
    VkPhysicalDevice _physicalDevice;
    std::unique_ptr<vku::Device> _device;
    std::unique_ptr<vku::RenderPass> _renderPass;
    std::unique_ptr<vku::SwapchainEnv> _swapchainEnv;
    std::unique_ptr<vku::PipelineLayout> _pipelineLayout;
    std::unique_ptr<vku::GraphicsPipeline> _pipeline;
    std::unique_ptr<vku::CommandPool> _commandPool;
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
    App(
        const std::string &name,
        int width,
        int height,
        const std::vector<const char *> *instanceExtensions,
        const std::vector<const char *> *layers,
        const std::vector<const char *> *deviceExtensions)
    {
        _window = std::make_unique<vku::Window>(vku::Window::noApi(width, height, name));
        _instance = std::make_unique<vku::Instance>(
            vku::Instance::basic(name, true, IS_DEBUG, instanceExtensions, layers, &_debugMessenger));
        _surface = std::make_unique<vku::Surface>(vku::Surface::glfw(*_instance, *_window));
        _physicalDevice = vku::findDevice(*_instance, *_surface, &_queueIndex, deviceExtensions);
        _device = std::make_unique<vku::Device>(vku::Device::basic(_physicalDevice, _queueIndex, deviceExtensions));
        auto surfaceFormat = vku::findSurfaceFormat(_physicalDevice, *_surface);
        _renderPass = std::make_unique<vku::RenderPass>(vku::RenderPass::basic(*_device, surfaceFormat.format));
        _swapchainEnv = std::make_unique<vku::SwapchainEnv>(
            vku::SwapchainEnv::basic(*_device, _physicalDevice, *_surface, *_renderPass));
        _pipelineLayout = std::make_unique<vku::PipelineLayout>(vku::PipelineLayout::basic(*_device));
        _pipeline = std::make_unique
    }
};
