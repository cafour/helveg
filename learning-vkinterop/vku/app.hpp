#pragma once

#include "device_related.hpp"
#include "instance_related.hpp"
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
    vku::RenderPass _renderPass;
    std::unique_ptr<vku::SwapchainEnv> _swapchainEnv;

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
};

}
