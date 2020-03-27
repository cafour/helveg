#pragma once

#include "device_related.hpp"
#include "instance_related.hpp"
#include "standalone.hpp"
#include "swapchain_env.hpp"
#include "window.hpp"
#include "containers.hpp"

#include <memory>
#include <optional>
#include <string>
#include <vector>

namespace vku {

class App {
private:
    vku::Window _window;
    vku::Instance _instance;
    std::optional<vku::DebugMessenger> _debugMessenger;
    vku::Surface _surface;
    uint32_t _queueIndex;
    VkPhysicalDevice _physicalDevice;
    vku::Device _device;
    vku::RenderPass _renderPass;
    std::optional<vku::SwapchainEnv> _swapchainEnv;
    vku::CommandPool _commandPool;
    vku::CommandBuffers _commandBuffers;
    VkQueue _queue;

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
        const std::vector<const char *> *instanceExtensions = nullptr,
        const std::vector<const char *> *layers = nullptr,
        const std::vector<const char *> *deviceExtensions = nullptr);

    vku::Window &window() { return _window; }
    vku::Instance &instance() { return _instance; }
    vku::Surface &surface() { return _surface; }
    uint32_t queueIndex() { return _queueIndex; }
    VkPhysicalDevice physicalDevice() { return _physicalDevice; }
    vku::Device &device() { return _device; }
    vku::RenderPass &renderPass() { return _renderPass; }
    vku::SwapchainEnv &swapchainEnv() { return _swapchainEnv.value(); };
    vku::CommandPool &commandPool() { return _commandPool; }
    vku::CommandBuffers &commandBuffers() { return _commandBuffers; }
    VkQueue queue() { return _queue; }

    virtual void prepare();
    virtual void run();
    virtual void step();
    virtual void recordCommands(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame) = 0;
};

}
