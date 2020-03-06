#pragma once

#include "vku/vku.hpp"

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

class Sample {
private:
    vku::Window _window;
    vku::Instance _instance;
    VkDebugUtilsMessengerEXT _debugMessenger;
    VkSurfaceKHR _surface;
    QueueIndices _indices;
    SwapchainDetails _swapchainDetails;
    VkPhysicalDevice _physicalDevice;
    VkDevice _device;
    VkSurfaceFormatKHR _surfaceFormat;
    VkFormat _swapchainFormat = VK_FORMAT_UNDEFINED;
    VkExtent2D _swapchainExtent;
    std::vector<VkImage> _swapchainImages;
    VkSwapchainKHR _swapchain;
    std::vector<VkImageView> _swapchainViews;
    VkRenderPass _renderPass;
    VkPipelineLayout _pipelineLayout;
    VkPipeline _pipeline;
    std::vector<VkFramebuffer> _swapchainFramebuffers;
    VkCommandPool _commandPool;
    VkQueue _graphicsQueue;
    VkQueue _presentQueue;
    std::vector<VkCommandBuffer> _commandBuffers;
    std::vector<VkSemaphore> _imageAvailable;
    std::vector<VkSemaphore> _renderFinished;
    std::vector<VkFence> _inflightFences;
    std::vector<VkFence> _imagesInflight;

    const static size_t INFLIGHT_COUNT = 2;

public:
    bool resized = false;

    Sample(int width, int height)
        : _width(width)
        , _height(height)
        , _window(createWindow(width, height))
        , _instance(createInstance())
        , _debugMessenger(createMessenger(_instance))
        , _surface(createSurface(_instance, _window))
        , _physicalDevice(createPhysicalDevice(_instance, _surface, _indices, _swapchainDetails))
        , _device(createDevice(_physicalDevice, _indices))
        , _surfaceFormat(_swapchainDetails.pickFormat())
        , _swapchainFormat(_surfaceFormat.format)
        , _swapchainExtent(chooseExtent())
        , _swapchain(createSwapchain(_device, _surface, _indices, _swapchainDetails, _width,
              _height, _swapchainFormat, _swapchainExtent, _swapchainImages))
        , _swapchainViews(createSwapchainViews(_device, _swapchainImages, _swapchainFormat))
        , _renderPass(createRenderPass(_device, _swapchainFormat))
        , _pipelineLayout(createPipelineLayout(_device))
        , _pipeline(createPipeline(_device, _swapchainExtent, _renderPass, _pipelineLayout))
        , _swapchainFramebuffers(createSwapchainFramebuffers(_device, _swapchainViews, _renderPass,
              _width, _height))
        , _commandPool(createCommandPool(_device, _indices))
        , _commandBuffers(createCommandBuffers(_device, _commandPool, _renderPass,
              _swapchainFramebuffers, _swapchainExtent, _pipeline))
        , _imageAvailable(createSemaphores(_device, INFLIGHT_COUNT))
        , _renderFinished(createSemaphores(_device, INFLIGHT_COUNT))
        , _inflightFences(createFences(_device, INFLIGHT_COUNT))
        , _imagesInflight(_swapchainImages.size(), VK_NULL_HANDLE)
    {
        glfwSetWindowUserPointer(_window, this);
        vkGetDeviceQueue(_device, _indices.graphics, 0, &_graphicsQueue);
        vkGetDeviceQueue(_device, _indices.present, 0, &_presentQueue);
    }

    ~Sample()
    {
        for (size_t i = 0; i < INFLIGHT_COUNT; ++i) {
            vkDestroySemaphore(_device, _imageAvailable[i], nullptr);
            vkDestroySemaphore(_device, _renderFinished[i], nullptr);
            vkDestroyFence(_device, _inflightFences[i], nullptr);
        }
        vkDestroyCommandPool(_device, _commandPool, nullptr);
        destroySwapchain();
        vkDestroyDevice(_device, nullptr);
        vkDestroySurfaceKHR(_instance, _surface, nullptr);
        vkDestroyDebugUtilsMessengerEXT(_instance, _debugMessenger, nullptr);
        vkDestroyInstance(_instance, nullptr);
        glfwDestroyWindow(_window);
        glfwTerminate();
    }

    void run()
    {
        size_t currentFrame = 0;
        while (!glfwWindowShouldClose(_window)) {
            glfwPollEvents();

            vkWaitForFences(_device, 1, &_inflightFences[currentFrame], VK_TRUE, UINT64_MAX);

            uint32_t imageIndex;
            VkResult result = vkAcquireNextImageKHR(_device, _swapchain, UINT64_MAX,
                _imageAvailable[currentFrame], VK_NULL_HANDLE, &imageIndex);
            if (resized || result == VK_ERROR_OUT_OF_DATE_KHR) {
                resized = false;
                recreateSwapchain();
                continue;
            } else if (result != VK_SUCCESS && result != VK_SUBOPTIMAL_KHR) {
                throw std::runtime_error("failed to acquire a swapchain image");
            }

            if (_imagesInflight[imageIndex] != VK_NULL_HANDLE) {
                vkWaitForFences(_device, 1, &_imagesInflight[imageIndex], VK_TRUE, UINT64_MAX);
            }
            _imagesInflight[imageIndex] = _inflightFences[currentFrame];

            VkSubmitInfo submitInfo = {};
            submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
            VkSemaphore waitSemaphores[] = { _imageAvailable[currentFrame] };
            VkPipelineStageFlags waitStages[] = { VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT };
            submitInfo.waitSemaphoreCount = 1;
            submitInfo.pWaitSemaphores = waitSemaphores;
            submitInfo.pWaitDstStageMask = waitStages;
            submitInfo.commandBufferCount = 1;
            submitInfo.pCommandBuffers = &_commandBuffers[imageIndex];
            submitInfo.signalSemaphoreCount = 1;
            VkSemaphore signalSemaphores[] = { _renderFinished[currentFrame] };
            submitInfo.pSignalSemaphores = signalSemaphores;
            vkResetFences(_device, 1, &_inflightFences[currentFrame]);
            if (vkQueueSubmit(_graphicsQueue, 1, &submitInfo, _inflightFences[currentFrame]) != VK_SUCCESS) {
                throw std::runtime_error("failed to submit a command buffer");
            }

            VkPresentInfoKHR presentInfo = {};
            presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
            presentInfo.waitSemaphoreCount = 1;
            presentInfo.pWaitSemaphores = signalSemaphores;
            presentInfo.swapchainCount = 1;
            presentInfo.pSwapchains = &_swapchain;
            presentInfo.pImageIndices = &imageIndex;
            result = vkQueuePresentKHR(_presentQueue, &presentInfo);
            if (result == VK_ERROR_OUT_OF_DATE_KHR) {
                recreateSwapchain();
                continue;
            } else if (result != VK_SUCCESS && result != VK_SUBOPTIMAL_KHR) {
                throw std::runtime_error("failed to acquire a swapchain image");
            }

            currentFrame = (currentFrame + 1) % 2;
        }

        vkDeviceWaitIdle(_device);
    }

    void destroySwapchain()
    {
        for (auto framebuffer : _swapchainFramebuffers) {
            vkDestroyFramebuffer(_device, framebuffer, nullptr);
        }
        vkFreeCommandBuffers(_device, _commandPool, static_cast<uint32_t>(_commandBuffers.size()), _commandBuffers.data());
        vkDestroyPipeline(_device, _pipeline, nullptr);
        vkDestroyPipelineLayout(_device, _pipelineLayout, nullptr);
        vkDestroyRenderPass(_device, _renderPass, nullptr);
        for (auto view : _swapchainViews) {
            vkDestroyImageView(_device, view, nullptr);
        }
        vkDestroySwapchainKHR(_device, _swapchain, nullptr);
    }

    void recreateSwapchain()
    {
        int width = 0, height = 0;
        glfwGetFramebufferSize(_window, &width, &height);
        while (width == 0 || height == 0) {
            glfwGetFramebufferSize(_window, &width, &height);
            glfwWaitEvents();
        }
        vkDeviceWaitIdle(_device);
        destroySwapchain();

        _swapchainDetails = getSwapchainDetails(_physicalDevice, _surface);
        _swapchainExtent = chooseExtent();
        _width = _swapchainExtent.width;
        _height = _swapchainExtent.height;
        _swapchain = createSwapchain(_device, _surface, _indices, _swapchainDetails,
            _width, _height, _swapchainFormat, _swapchainExtent, _swapchainImages);
        _swapchainViews = createSwapchainViews(_device, _swapchainImages, _swapchainFormat);
        _renderPass = createRenderPass(_device, _swapchainFormat);
        _pipelineLayout = createPipelineLayout(_device);
        _pipeline = createPipeline(_device, _swapchainExtent, _renderPass, _pipelineLayout);
        _swapchainFramebuffers = createSwapchainFramebuffers(_device, _swapchainViews, _renderPass, _width, _height);
        _commandBuffers = createCommandBuffers(_device, _commandPool, _renderPass, _swapchainFramebuffers,
            _swapchainExtent, _pipeline);
    }

    VkExtent2D chooseExtent()
    {
        if (_swapchainDetails.capabilities.currentExtent.width != UINT32_MAX) {
            return _swapchainDetails.capabilities.currentExtent;
        }
        int width;
        int height;
        glfwGetFramebufferSize(_window, &width, &height);
        return { static_cast<uint32_t>(width), static_cast<uint32_t>(height) };
    }

private:
    static GLFWwindow *createWindow(int width, int height);
    static VkInstance createInstance();
    static VkDebugUtilsMessengerEXT createMessenger(VkInstance instance);
    static VkSurfaceKHR createSurface(VkInstance instance, GLFWwindow *window);
    static VkPhysicalDevice createPhysicalDevice(VkInstance instance,
        VkSurfaceKHR surface,
        QueueIndices &indices,
        SwapchainDetails &details);
    static VkDevice createDevice(VkPhysicalDevice physicalDevice, QueueIndices indices);
    static VkSwapchainKHR createSwapchain(VkDevice device,
        VkSurfaceKHR surface,
        QueueIndices &indices,
        SwapchainDetails &details,
        uint32_t width,
        uint32_t height,
        VkFormat format,
        VkExtent2D extent,
        std::vector<VkImage> &swapchainImages);
    static std::vector<VkImageView> createSwapchainViews(VkDevice device,
        const std::vector<VkImage> &swapchainImages,
        VkFormat format);
    static VkRenderPass createRenderPass(VkDevice device, VkFormat swapchainFormat);
    static VkPipelineLayout createPipelineLayout(VkDevice device);
    static VkPipeline createPipeline(VkDevice device,
        VkExtent2D swapchainExtent,
        VkRenderPass renderPass,
        VkPipelineLayout layout);
    static std::vector<VkFramebuffer> createSwapchainFramebuffers(VkDevice device,
        const std::vector<VkImageView> &swapchainViews,
        VkRenderPass renderPass,
        uint32_t width,
        uint32_t height);
    static VkCommandPool createCommandPool(VkDevice device, QueueIndices &indices);
    static std::vector<VkCommandBuffer> createCommandBuffers(VkDevice device,
        VkCommandPool commandPool,
        VkRenderPass renderPass,
        std::vector<VkFramebuffer> &framebuffers,
        VkExtent2D extent,
        VkPipeline pipeline);
    static std::vector<VkSemaphore> createSemaphores(VkDevice device, size_t count);
    static std::vector<VkFence> createFences(VkDevice device, size_t count);
};
