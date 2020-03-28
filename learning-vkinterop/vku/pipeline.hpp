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
    VkVertexInputBindingDescription *bindings,
    size_t bindingCount,
    VkVertexInputAttributeDescription *attributes,
    size_t attributeCount);

VkPipelineInputAssemblyStateCreateInfo inputAssemblyState(VkPrimitiveTopology topology);

VkPipelineRasterizationStateCreateInfo rasterizationState(
    VkPolygonMode polygonMode,
    VkCullModeFlags cullMode,
    VkFrontFace frontFace);

VkPipelineMultisampleStateCreateInfo multisampleState(VkSampleCountFlagBits sampleCount);

VkPipelineColorBlendStateCreateInfo colorBlendState(
    VkPipelineColorBlendAttachmentState *attachments,
    size_t attachmentCount);

VkPipelineColorBlendAttachmentState colorBlendAttachment(
    bool blendEnable,
    VkColorComponentFlags colorWriteMask = VK_COLOR_COMPONENT_R_BIT
        | VK_COLOR_COMPONENT_G_BIT
        | VK_COLOR_COMPONENT_B_BIT
        | VK_COLOR_COMPONENT_A_BIT);

VkPipelineViewportStateCreateInfo viewportState(
    VkViewport *viewports,
    size_t viewportCount,
    VkRect2D *scissors,
    size_t scissorCount);

VkPipelineShaderStageCreateInfo shaderStage(
    VkShaderStageFlagBits stage,
    VkShaderModule module,
    const char *entryPoint = "main");

VkPipelineDynamicStateCreateInfo dynamicState(
    VkDynamicState *dynamicStates,
    size_t dynamicStateCount);

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

    static GraphicsPipeline basic(VkDevice device,
        VkPipelineLayout pipelineLayout,
        VkRenderPass renderPass,
        VkShaderModule vertexShader,
        VkShaderModule fragmentShader,
        VkVertexInputBindingDescription *vertexBindings = nullptr,
        size_t vertexBindingCount = 0,
        VkVertexInputAttributeDescription *vertexAttributes = nullptr,
        size_t vertexAttributeCount = 0);

    struct CreateInfo {
        VkPipelineVertexInputStateCreateInfo vertexInputState = {};
        VkPipelineInputAssemblyStateCreateInfo inputAssemblyState = {};
        VkPipelineTessellationStateCreateInfo tessellationState = {};
        VkPipelineViewportStateCreateInfo viewportState = {};
        VkPipelineRasterizationStateCreateInfo rasterizationState = {};
        VkPipelineMultisampleStateCreateInfo multisampleState = {};
        VkPipelineDepthStencilStateCreateInfo depthStencilState = {};
        VkPipelineColorBlendStateCreateInfo colorBlendState = {};
        VkPipelineDynamicStateCreateInfo dynamicState = {};

        VkGraphicsPipelineCreateInfo raw = {};

        CreateInfo()
        {
            vertexInputState.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
            inputAssemblyState.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
            tessellationState.sType = VK_STRUCTURE_TYPE_PIPELINE_TESSELLATION_STATE_CREATE_INFO;
            viewportState.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;
            rasterizationState.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
            multisampleState.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
            depthStencilState.sType = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO;
            colorBlendState.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
            dynamicState.sType = VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO;
            raw.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;
            raw.pVertexInputState = &vertexInputState;
            raw.pInputAssemblyState = &inputAssemblyState;
            raw.pTessellationState = &tessellationState;
            raw.pViewportState = &viewportState;
            raw.pRasterizationState = &rasterizationState;
            raw.pMultisampleState = &multisampleState;
            raw.pDepthStencilState = &depthStencilState;
            raw.pColorBlendState = &colorBlendState;
            raw.pDynamicState = &dynamicState;
        }

        operator VkGraphicsPipelineCreateInfo &() { return raw; }
    };
};
}
