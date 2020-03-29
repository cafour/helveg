#include "triangle.hpp"
#include "shaders.hpp"

#include <chrono>
#include <glm/gtc/matrix_transform.hpp>

Triangle::Triangle(int width, int height)
    : vku::App("Hello, Triangle!", width, height)
{
    auto uboBinding = vku::descriptorBinding(0, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1, VK_SHADER_STAGE_VERTEX_BIT);
    _setLayout = vku::DescriptorSetLayout::basic(device(), &uboBinding, 1);

    _pipelineLayout = vku::PipelineLayout::basic(device(), _setLayout, 1);

    auto vertexBinding = vku::vertexInputBinding(0, sizeof(Vertex), VK_VERTEX_INPUT_RATE_VERTEX);

    VkVertexInputAttributeDescription vertexAttributes[2] = {
        vku::vertexInputAttribute(0, 0, VK_FORMAT_R32G32_SFLOAT, offsetof(Vertex, position)),
        vku::vertexInputAttribute(1, 0, VK_FORMAT_R32G32B32_SFLOAT, offsetof(Vertex, color))
    };

    _pipeline = vku::GraphicsPipeline::basic(
        device(),
        _pipelineLayout,
        renderPass(),
        vku::ShaderModule::inlined(device(), TRIANGLE_VERT, TRIANGLE_VERT_LENGTH),
        vku::ShaderModule::inlined(device(), TRIANGLE_FRAG, TRIANGLE_FRAG_LENGTH),
        &vertexBinding,
        1,
        vertexAttributes,
        2);

    _vertexBuffer = vku::Buffer::exclusive(
        device(),
        vertices.size() * sizeof(Vertex),
        VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);

    _vertexBufferMemory = vku::DeviceMemory::deviceLocalData(
        physicalDevice(),
        device(),
        commandPool(),
        queue(),
        _vertexBuffer,
        vertices.data(),
        vertices.size() * sizeof(Vertex));
}

void Triangle::recordCommands(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
{
    VkCommandBufferBeginInfo beginInfo = {};
    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
    ENSURE(vkBeginCommandBuffer(commandBuffer, &beginInfo));

    VkRenderPassBeginInfo renderPassInfo = {};
    renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
    renderPassInfo.renderPass = renderPass();
    renderPassInfo.framebuffer = frame.framebuffer;
    renderPassInfo.renderArea.offset = { 0, 0 };

    VkExtent2D extent = swapchainEnv().extent();
    renderPassInfo.renderArea.extent = extent;

    VkClearValue clearColor = { 0.0f, 0.0f, 0.0f, 1.0f };
    renderPassInfo.clearValueCount = 1;
    renderPassInfo.pClearValues = &clearColor;

    VkViewport viewport = {};
    viewport.width = extent.width;
    viewport.height = extent.height;
    viewport.maxDepth = 1.0f;

    VkRect2D scissor = {};
    scissor.extent = extent;

    vkCmdBeginRenderPass(commandBuffer, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
    vkCmdBindPipeline(commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, _pipeline);
    vkCmdSetViewport(commandBuffer, 0, 1, &viewport);
    vkCmdSetScissor(commandBuffer, 0, 1, &scissor);
    VkDeviceSize offset = 0;
    vkCmdBindVertexBuffers(commandBuffer, 0, 1, _vertexBuffer, &offset);
    vkCmdBindDescriptorSets(
        commandBuffer,
        VK_PIPELINE_BIND_POINT_GRAPHICS,
        _pipelineLayout,
        0, // first set number
        1, // setCount
        &_descriptorSets[frame.index],
        0, // dynamic offset count
        nullptr); // dynamic offsets
    vkCmdDraw(commandBuffer, static_cast<uint32_t>(vertices.size()), 1, 0, 0);
    vkCmdEndRenderPass(commandBuffer);

    ENSURE(vkEndCommandBuffer(commandBuffer));
}

void Triangle::prepare()
{
    size_t imageCount = swapchainEnv().frames().size();

    // recreate the uniform buffers as the number of swapchain images could have changed
    _uboBufferMemories.clear();
    _uboBuffers.clear();

    _uboBuffers.resize(imageCount);
    _uboBufferMemories.resize(imageCount);
    for (size_t i = 0; i < imageCount; ++i) {
        _uboBuffers[i] = vku::Buffer::exclusive(device(), sizeof(UBO), VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);
        _uboBufferMemories[i] = vku::DeviceMemory::host(physicalDevice(), device(), _uboBuffers[i]);
    }

    auto poolSize = vku::descriptorPoolSize(VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1);
    _descriptorPool = vku::DescriptorPool::basic(device(), imageCount, &poolSize, 1);
    _descriptorSets = vku::allocateDescriptorSets(device(), _descriptorPool, _setLayout, imageCount);
    for (size_t i = 0; i < imageCount; ++i) {
        vku::updateUboDescriptor(device(), _uboBuffers[i], _descriptorSets[i], 0);
    }

    App::prepare();
}

void Triangle::update(vku::SwapchainFrame &frame)
{
    static auto start = std::chrono::high_resolution_clock::now();

    auto now = std::chrono::high_resolution_clock::now();
    float time = std::chrono::duration<float, std::chrono::seconds::period>(now - start).count();

    UBO ubo = {};
    ubo.model = glm::rotate(glm::mat4(1.0f), time * glm::radians(90.f), glm::vec3(0.0f, 0.0f, 1.0f));
    ubo.view = glm::lookAt(glm::vec3(2.0f, 2.0f, 2.0f), glm::vec3(0.0f, 0.0f, 0.0f), glm::vec3(0.0f, 0.0f, 1.0f));

    auto extent = swapchainEnv().extent();
    ubo.projection = glm::perspective(
        glm::radians(45.0f),
        static_cast<float>(extent.width) / static_cast<float>(extent.height),
        0.1f,
        10.0f);
    ubo.projection[1][1] *= -1; // vulkan has flipped y compared to opengl
    vku::hostDeviceCopy(device(), &ubo, _uboBufferMemories[frame.index], sizeof(UBO));
}
