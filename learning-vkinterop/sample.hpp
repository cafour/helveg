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

class Sample {
private:
    vku::Window _window;
    vku::Instance _instance;
    vku::Surface _surface;
    vku::PhysicalDevice _physicalDevice;
    vku::Device _device;
    vku::RenderPass _renderPass;
    vku::Swapchain _swapchain;
    vku::Pipeline _pipeline;
    VkCommandPool _commandPool;
    VkQueue _graphicsQueue;
    VkQueue _presentQueue;
    std::vector<VkCommandBuffer> _commandBuffers;
    std::vector<VkSemaphore> _imageAvailable;
    std::vector<VkSemaphore> _renderFinished;
    std::vector<VkFence> _inflightFences;
    std::vector<VkFence> _imagesInflight;

    const static size_t INFLIGHT_COUNT = 2;

    static void onResize(vku::Window &window, void *userData)
    {
        auto sample = reinterpret_cast<Sample *>(userData);
        sample->resized = true;
    }

public:
    bool resized = false;

    Sample(int width, int height)
        : _window(width, height, "Vulkan", &onResize, this)
        , _instance("Sample", ENABLE_VALIDATION)
        , _surface(_instance, _window)
        , _physicalDevice(_instance, _surface)
        , _device(_physicalDevice)
        , _renderPass(_device)
        , _swapchain(_renderPass)
        , _pipeline(_renderPass,
              vku::Shader(_device, VERTEX_SHADER, VERTEX_SHADER_LENGTH),
              vku::Shader(_device, FRAGMENT_SHADER, FRAGMENT_SHADER_LENGTH))
        , _commandPool(createCommandPool(_device, _physicalDevice.queueIndices()))
        , _commandBuffers(createCommandBuffers(_device, _commandPool, _renderPass,
              _swapchain.framebuffers(), _swapchain.extent(), _pipeline))
        , _imageAvailable(createSemaphores(_device, INFLIGHT_COUNT))
        , _renderFinished(createSemaphores(_device, INFLIGHT_COUNT))
        , _inflightFences(createFences(_device, INFLIGHT_COUNT))
        , _imagesInflight(_swapchain.images().size(), VK_NULL_HANDLE)
    {
        glfwSetWindowUserPointer(_window, this);
        vkGetDeviceQueue(_device, _physicalDevice.queueIndices().graphics, 0, &_graphicsQueue);
        vkGetDeviceQueue(_device, _physicalDevice.queueIndices().present, 0, &_presentQueue);
    }

    ~Sample()
    {
        for (size_t i = 0; i < INFLIGHT_COUNT; ++i) {
            vkDestroySemaphore(_device, _imageAvailable[i], nullptr);
            vkDestroySemaphore(_device, _renderFinished[i], nullptr);
            vkDestroyFence(_device, _inflightFences[i], nullptr);
        }
        vkDestroyCommandPool(_device, _commandPool, nullptr);
        vkFreeCommandBuffers(_device, _commandPool, static_cast<uint32_t>(_commandBuffers.size()), _commandBuffers.data());
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
            VkSwapchainKHR swapchain = _swapchain;
            presentInfo.pSwapchains = &swapchain;
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

    void recreateSwapchain()
    {
        int width = 0, height = 0;
        glfwGetFramebufferSize(_window, &width, &height);
        while (width == 0 || height == 0) {
            glfwGetFramebufferSize(_window, &width, &height);
            glfwWaitEvents();
        }
        vkDeviceWaitIdle(_device);
        _swapchain = vku::Swapchain(_renderPass);
        _commandBuffers = createCommandBuffers(_device, _commandPool, _renderPass, _swapchain.framebuffers(),
            _swapchain.extent(), _pipeline);
    }

    VkExtent2D chooseExtent()
    {
        if (_physicalDevice.swapchainDetails().capabilities.currentExtent.width != UINT32_MAX) {
            return _physicalDevice.swapchainDetails().capabilities.currentExtent;
        }
        int width;
        int height;
        glfwGetFramebufferSize(_window, &width, &height);
        return { static_cast<uint32_t>(width), static_cast<uint32_t>(height) };
    }

private:
    static VkCommandPool createCommandPool(vku::Device &device, vku::QueueIndices &indices);
    static std::vector<VkCommandBuffer> createCommandBuffers(
        vku::Device &device,
        VkCommandPool commandPool,
        vku::RenderPass &renderPass,
        std::vector<VkFramebuffer> &framebuffers,
        VkExtent2D extent,
        vku::Pipeline &pipeline);
    static std::vector<VkSemaphore> createSemaphores(VkDevice device, size_t count);
    static std::vector<VkFence> createFences(VkDevice device, size_t count);
};
