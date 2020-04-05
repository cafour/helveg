#pragma once

#include "containers.hpp"
#include "device_related.hpp"
#include "instance_related.hpp"
#include "render_pass.hpp"
#include "standalone.hpp"
#include "swapchain_env.hpp"
#include "window.hpp"

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
    VkQueue _queue;
    vku::CommandPool _commandPool;
    VkFormat _depthFormat;
    vku::RenderPass _renderPass;
    vku::SwapchainEnv _swapchainEnv;
    vku::Image _depthImage;
    vku::DeviceMemory _depthImageMemory;
    vku::ImageView _depthImageView;
    std::vector<vku::Framebuffer> _framebuffers;
    vku::CommandBuffers _commandBuffers;

#if NDEBUG
    const bool IS_DEBUG = false;
#else
    const bool IS_DEBUG = true;
#endif

    void resize();
    void step();

public:
    App(
        const std::string &name,
        int width,
        int height,
        const std::vector<const char *> *instanceExtensions = nullptr,
        const std::vector<const char *> *layers = nullptr,
        const std::vector<const char *> *deviceExtensions = nullptr);
    virtual ~App() = default;

    vku::Window &window() { return _window; }
    vku::Instance &instance() { return _instance; }
    vku::Surface &surface() { return _surface; }
    uint32_t queueIndex() { return _queueIndex; }
    VkPhysicalDevice physicalDevice() { return _physicalDevice; }
    vku::Device &device() { return _device; }
    vku::RenderPass &renderPass() { return _renderPass; }
    vku::SwapchainEnv &swapchainEnv() { return _swapchainEnv; };
    vku::CommandPool &commandPool() { return _commandPool; }
    vku::CommandBuffers &commandBuffers() { return _commandBuffers; }
    std::vector<vku::Framebuffer> &framebuffers() { return _framebuffers; }
    VkQueue queue() { return _queue; }

    void run();
    virtual void prepare() {};
    virtual void update(vku::SwapchainFrame &frame) { (void)frame; }
    virtual void recordCommands(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame) = 0;
};

}
