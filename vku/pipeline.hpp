#pragma once

#include "base.hpp"
#include "types.hpp"

#include <vector>

namespace vku {

VkVertexInputBindingDescription vertexInputBinding(
    uint32_t binding,
    uint32_t stride,
    VkVertexInputRate inputRate);

VkVertexInputAttributeDescription vertexInputAttribute(
    uint32_t location,
    uint32_t binding,
    VkFormat format,
    uint32_t offset);

VkPipelineVertexInputStateCreateInfo vertexInputState(
    const VkVertexInputBindingDescription *bindings,
    size_t bindingCount,
    const VkVertexInputAttributeDescription *attributes,
    size_t attributeCount);

VkPipelineInputAssemblyStateCreateInfo inputAssemblyState(VkPrimitiveTopology topology);

VkPipelineRasterizationStateCreateInfo rasterizationState(
    VkPolygonMode polygonMode,
    VkCullModeFlags cullMode,
    VkFrontFace frontFace);

VkPipelineMultisampleStateCreateInfo multisampleState(VkSampleCountFlagBits sampleCount);

VkPipelineColorBlendStateCreateInfo colorBlendState(
    const VkPipelineColorBlendAttachmentState *attachments,
    size_t attachmentCount);

VkPipelineColorBlendAttachmentState colorBlendAttachment(
    bool blendEnable,
    VkColorComponentFlags colorWriteMask = VK_COLOR_COMPONENT_R_BIT
        | VK_COLOR_COMPONENT_G_BIT
        | VK_COLOR_COMPONENT_B_BIT
        | VK_COLOR_COMPONENT_A_BIT);

VkPipelineViewportStateCreateInfo viewportState(
    const VkViewport *viewports,
    size_t viewportCount,
    const VkRect2D *scissors,
    size_t scissorCount);

VkPipelineShaderStageCreateInfo shaderStage(
    VkShaderStageFlagBits stage,
    VkShaderModule module,
    const char *entryPoint = "main");

VkPipelineDepthStencilStateCreateInfo depthStencilState(bool useDepthTest);

VkPipelineDynamicStateCreateInfo dynamicState(
    const VkDynamicState *dynamicStates,
    size_t dynamicStateCount);

class PipelineLayout : public DeviceConstructible<
                           VkPipelineLayout,
                           VkPipelineLayoutCreateInfo,
                           &vkCreatePipelineLayout,
                           &vkDestroyPipelineLayout> {
public:
    using DeviceConstructible::DeviceConstructible;
    static PipelineLayout basic(
        VkDevice device,
        const VkDescriptorSetLayout *setLayouts = nullptr,
        size_t setLayoutCount = 0,
        const VkPushConstantRange *pushConstantRanges = nullptr,
        size_t pushConstantRangeCount = 0);
};

class GraphicsPipeline : public DeviceRelated<VkPipeline> {
public:
    using DeviceRelated::DeviceRelated;

    GraphicsPipeline(VkDevice device, VkGraphicsPipelineCreateInfo &createInfo)
        : DeviceRelated(device, VK_NULL_HANDLE)
    {
        ENSURE(vkCreateGraphicsPipelines(device, VK_NULL_HANDLE, 1, &createInfo, nullptr, &_raw));
    }
    ~GraphicsPipeline()
    {
        if (_device != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
            vkDestroyPipeline(_device, _raw, nullptr);
        }
    }
    GraphicsPipeline(GraphicsPipeline &&other) noexcept = default;
    GraphicsPipeline &operator=(GraphicsPipeline &&other) noexcept = default;

    static GraphicsPipeline basic(
        VkDevice device,
        VkPipelineLayout pipelineLayout,
        VkRenderPass renderPass,
        const VkPipelineShaderStageCreateInfo *shaderStages,
        size_t shaderStageCount,
        const VkVertexInputBindingDescription *vertexBindings = nullptr,
        size_t vertexBindingCount = 0,
        const VkVertexInputAttributeDescription *vertexAttributes = nullptr,
        size_t vertexAttributeCount = 0,
        VkFrontFace frontFace = VK_FRONT_FACE_CLOCKWISE,
        VkPrimitiveTopology topology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST,
        bool hasDepthStencil = true,
        bool allowBlending = false);
};

class ComputePipeline : public DeviceRelated<VkPipeline> {
public:
    using DeviceRelated::DeviceRelated;

    ComputePipeline(VkDevice device, VkComputePipelineCreateInfo &createInfo)
        : DeviceRelated(device, VK_NULL_HANDLE)
    {
        ENSURE(vkCreateComputePipelines(device, VK_NULL_HANDLE, 1, &createInfo, nullptr, &_raw));
    }
    ~ComputePipeline()
    {
        if (_device != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
            vkDestroyPipeline(_device, _raw, nullptr);
        }
    }
    ComputePipeline(ComputePipeline &&other) noexcept = default;
    ComputePipeline &operator=(ComputePipeline &&other) noexcept = default;
};

class RayTracingPipeline : public DeviceRelated<VkPipeline> {
public:
    using DeviceRelated::DeviceRelated;

    RayTracingPipeline(VkDevice device, VkRayTracingPipelineCreateInfoKHR &createInfo)
        : DeviceRelated(device, VK_NULL_HANDLE)
    {
        ENSURE(vkCreateRayTracingPipelinesKHR(device, VK_NULL_HANDLE, VK_NULL_HANDLE, 1, &createInfo, nullptr, &_raw));
    }
    ~RayTracingPipeline()
    {
        if (_device != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
            vkDestroyPipeline(_device, _raw, nullptr);
        }
    }
    RayTracingPipeline(RayTracingPipeline &&other) noexcept = default;
    RayTracingPipeline &operator=(RayTracingPipeline &&other) noexcept = default;

    static VkRayTracingPipelineCreateInfoKHR createInfo()
    {
        VkPipelineLibraryCreateInfoKHR libInfo = {};
        libInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LIBRARY_CREATE_INFO_KHR;

        VkRayTracingPipelineCreateInfoKHR info = {};
        info.sType = VK_STRUCTURE_TYPE_RAY_TRACING_PIPELINE_CREATE_INFO_KHR;
        info.pLibraryInfo = &libInfo;

        return info;
    }
};

VkRayTracingShaderGroupCreateInfoKHR rayTracingShaderGroup(
    VkRayTracingShaderGroupTypeKHR type,
    uint32_t generalShader,
    uint32_t closestHitShader = VK_SHADER_UNUSED_KHR,
    uint32_t anyHitShader = VK_SHADER_UNUSED_KHR,
    uint32_t intersectionShader = VK_SHADER_UNUSED_KHR);

}
