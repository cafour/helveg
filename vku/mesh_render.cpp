#include "mesh_render.hpp"
#include "shaders.hpp"

#include <chrono>
#include <glm/gtc/matrix_transform.hpp>

MeshRender::MeshRender(int width, int height, MeshRender::Mesh mesh)
    : vku::App("Hello, MeshRender!", width, height)
    , _mesh(mesh)
{
    auto uboBinding = vku::descriptorBinding(0, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1, VK_SHADER_STAGE_VERTEX_BIT);
    _setLayout = vku::DescriptorSetLayout::basic(device(), &uboBinding, 1);

    _pipelineLayout = vku::PipelineLayout::basic(device(), _setLayout, 1);

    VkVertexInputBindingDescription vertexBindings[2] = {
        vku::vertexInputBinding(0, sizeof(glm::vec3), VK_VERTEX_INPUT_RATE_VERTEX),
        vku::vertexInputBinding(1, sizeof(glm::vec3), VK_VERTEX_INPUT_RATE_VERTEX)
    };

    VkVertexInputAttributeDescription vertexAttributes[] = {
        vku::vertexInputAttribute(0, 0, VK_FORMAT_R32G32B32_SFLOAT, 0),
        vku::vertexInputAttribute(1, 1, VK_FORMAT_R32G32B32_SFLOAT, 0)
    };

    _pipeline = vku::GraphicsPipeline::basic(
        device(),
        _pipelineLayout,
        renderPass(),
        vku::ShaderModule::inlined(device(), MESH_VERT, MESH_VERT_LENGTH),
        vku::ShaderModule::inlined(device(), MESH_FRAG, MESH_FRAG_LENGTH),
        vertexBindings,
        2,
        vertexAttributes,
        2,
        VK_FRONT_FACE_COUNTER_CLOCKWISE);

    size_t verticesSize = mesh.vertexCount * sizeof(glm::vec3);
    auto stagingBuffer = vku::Buffer::exclusive(device(), verticesSize * 2, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    auto stagingMemory = vku::DeviceMemory::hostCoherentBuffer(physicalDevice(), device(), stagingBuffer);
    vku::hostDeviceCopy(device(), mesh.vertices, stagingMemory, verticesSize, 0);
    vku::hostDeviceCopy(device(), mesh.colors, stagingMemory, verticesSize, verticesSize);

    _vertexBuffer = vku::Buffer::exclusive(
        device(),
        verticesSize * 2,
        VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);
    _vertexBufferMemory = vku::DeviceMemory::deviceLocalBuffer(physicalDevice(), device(), _vertexBuffer);
    vku::deviceDeviceCopy(device(), commandPool(), queue(), stagingBuffer, _vertexBuffer, 2 * verticesSize);

    _indexBuffer = vku::Buffer::exclusive(
        device(),
        mesh.indexCount * sizeof(uint32_t),
        VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_INDEX_BUFFER_BIT);
    _indexBufferMemory = vku::DeviceMemory::deviceLocalData(
        physicalDevice(),
        device(),
        commandPool(),
        queue(),
        _indexBuffer,
        mesh.indices,
        mesh.indexCount * sizeof(uint32_t));
}

void MeshRender::recordCommands(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
{
    VkCommandBufferBeginInfo beginInfo = {};
    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
    ENSURE(vkBeginCommandBuffer(commandBuffer, &beginInfo));

    VkRenderPassBeginInfo renderPassInfo = {};
    renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
    renderPassInfo.renderPass = renderPass();
    renderPassInfo.framebuffer = framebuffers()[frame.index];
    renderPassInfo.renderArea.offset = { 0, 0 };

    VkExtent2D extent = swapchainEnv().extent();
    renderPassInfo.renderArea.extent = extent;

    VkClearValue clearValues[2];
    clearValues[0].color = {0.0f, 0.0f, 0.0f, 1.0f};
    clearValues[1].depthStencil = {1.0f, 0};
    renderPassInfo.clearValueCount = 2;
    renderPassInfo.pClearValues = clearValues;

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

    VkBuffer vertexBuffers[] = { _vertexBuffer, _vertexBuffer };
    VkDeviceSize offsets[] = { 0, _mesh.vertexCount * sizeof(glm::vec3) };
    vkCmdBindVertexBuffers(commandBuffer, 0, 2, vertexBuffers, offsets);
    vkCmdBindIndexBuffer(commandBuffer, _indexBuffer, 0, VK_INDEX_TYPE_UINT32);
    vkCmdBindDescriptorSets(
        commandBuffer,
        VK_PIPELINE_BIND_POINT_GRAPHICS,
        _pipelineLayout,
        0, // first set number
        1, // setCount
        &_descriptorSets[frame.index],
        0, // dynamic offset count
        nullptr); // dynamic offsets
    vkCmdDrawIndexed(commandBuffer, _mesh.indexCount, 1, 0, 0, 0);
    vkCmdEndRenderPass(commandBuffer);

    ENSURE(vkEndCommandBuffer(commandBuffer));
}

void MeshRender::prepare()
{
    size_t imageCount = swapchainEnv().frames().size();

    // recreate the uniform buffers as the number of swapchain images could have changed
    _uboBufferMemories.clear();
    _uboBuffers.clear();

    _uboBuffers.resize(imageCount);
    _uboBufferMemories.resize(imageCount);
    for (size_t i = 0; i < imageCount; ++i) {
        _uboBuffers[i] = vku::Buffer::exclusive(device(), sizeof(UBO), VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);
        _uboBufferMemories[i] = vku::DeviceMemory::hostCoherentBuffer(physicalDevice(), device(), _uboBuffers[i]);
    }

    auto poolSize = vku::descriptorPoolSize(VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1);
    _descriptorPool = vku::DescriptorPool::basic(device(), imageCount, &poolSize, 1);
    _descriptorSets = vku::allocateDescriptorSets(device(), _descriptorPool, _setLayout, imageCount);
    for (size_t i = 0; i < imageCount; ++i) {
        vku::updateUboDescriptor(device(), _uboBuffers[i], _descriptorSets[i], 0);
    }

    App::prepare();
}

void MeshRender::update(vku::SwapchainFrame &frame)
{
    static auto start = std::chrono::high_resolution_clock::now();

    auto now = std::chrono::high_resolution_clock::now();
    float time = std::chrono::duration<float, std::chrono::seconds::period>(now - start).count();

    UBO ubo = {};
    ubo.model = glm::rotate(glm::mat4(1.0f), time * glm::radians(90.f), glm::vec3(0.0f, 1.0f, 0.0f));
    ubo.model = glm::scale(ubo.model, glm::vec3(0.05f));
    ubo.view = glm::lookAt(glm::vec3(10.0f, 10.0f, 10.0f), glm::vec3(0.0f, 0.0f, 0.0f), glm::vec3(0.0f, 1.0f, 0.0f));

    auto extent = swapchainEnv().extent();
    ubo.projection = glm::perspective(
        glm::radians(45.0f),
        static_cast<float>(extent.width) / static_cast<float>(extent.height),
        0.1f,
        100.0f);
    ubo.projection[1][1] *= -1;
    // ubo.projection = glm::mat4(1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f, 0.0f, 0.0f, 0.5f, 1.0f)
    //     * ubo.projection;
    vku::hostDeviceCopy(device(), &ubo, _uboBufferMemories[frame.index], sizeof(UBO));
}
