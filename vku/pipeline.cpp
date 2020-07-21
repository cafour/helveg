#include "pipeline.hpp"

VkVertexInputBindingDescription vku::vertexInputBinding(
    uint32_t binding,
    uint32_t stride,
    VkVertexInputRate inputRate)
{
    VkVertexInputBindingDescription description = {};
    description.binding = binding;
    description.stride = stride;
    description.inputRate = inputRate;
    return description;
}

VkVertexInputAttributeDescription vku::vertexInputAttribute(
    uint32_t location,
    uint32_t binding,
    VkFormat format,
    uint32_t offset)
{
    VkVertexInputAttributeDescription description = {};
    description.location = location;
    description.binding = binding;
    description.format = format;
    description.offset = offset;
    return description;
}

VkPipelineVertexInputStateCreateInfo vku::vertexInputState(
    const VkVertexInputBindingDescription *bindings,
    size_t bindingCount,
    const VkVertexInputAttributeDescription *attributes,
    size_t attributeCount)
{
    VkPipelineVertexInputStateCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
    if (bindings) {
        createInfo.vertexBindingDescriptionCount = static_cast<uint32_t>(bindingCount);
        createInfo.pVertexBindingDescriptions = bindings;
    }
    if (attributes) {
        createInfo.vertexAttributeDescriptionCount = static_cast<uint32_t>(attributeCount);
        createInfo.pVertexAttributeDescriptions = attributes;
    }
    return createInfo;
}

VkPipelineInputAssemblyStateCreateInfo vku::inputAssemblyState(VkPrimitiveTopology topology)
{
    VkPipelineInputAssemblyStateCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
    createInfo.topology = topology;
    return createInfo;
}

VkPipelineRasterizationStateCreateInfo vku::rasterizationState(
    VkPolygonMode polygonMode,
    VkCullModeFlags cullMode,
    VkFrontFace frontFace)
{
    VkPipelineRasterizationStateCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
    createInfo.lineWidth = 1.0f;
    createInfo.polygonMode = polygonMode;
    createInfo.cullMode = cullMode;
    createInfo.frontFace = frontFace;
    return createInfo;
}

VkPipelineMultisampleStateCreateInfo vku::multisampleState(VkSampleCountFlagBits sampleCount)
{
    VkPipelineMultisampleStateCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
    createInfo.rasterizationSamples = sampleCount;
    return createInfo;
}

VkPipelineColorBlendAttachmentState vku::colorBlendAttachment(
    bool blendEnable,
    VkColorComponentFlags colorWriteMask)
{
    // TODO: Create a better abstraction. This is too specific.
    VkPipelineColorBlendAttachmentState attachment = {};
    attachment.blendEnable = blendEnable;
    attachment.colorWriteMask = colorWriteMask;
    attachment.srcColorBlendFactor = VK_BLEND_FACTOR_SRC_ALPHA;
    attachment.dstColorBlendFactor = VK_BLEND_FACTOR_ONE;
    attachment.colorBlendOp = VK_BLEND_OP_ADD;
    attachment.srcAlphaBlendFactor = VK_BLEND_FACTOR_ONE;
    attachment.dstAlphaBlendFactor = VK_BLEND_FACTOR_ZERO;
    attachment.alphaBlendOp = VK_BLEND_OP_ADD;
    return attachment;
}

VkPipelineColorBlendStateCreateInfo vku::colorBlendState(
    const VkPipelineColorBlendAttachmentState *attachments,
    size_t attachmentCount)
{
    VkPipelineColorBlendStateCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
    createInfo.attachmentCount = static_cast<uint32_t>(attachmentCount);
    createInfo.pAttachments = attachments;
    return createInfo;
}

VkPipelineViewportStateCreateInfo vku::viewportState(
    const VkViewport *viewports,
    size_t viewportCount,
    const VkRect2D *scissors,
    size_t scissorCount)
{
    VkPipelineViewportStateCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;
    createInfo.pViewports = viewports;
    createInfo.viewportCount = static_cast<uint32_t>(viewportCount);
    createInfo.pScissors = scissors;
    createInfo.scissorCount = static_cast<uint32_t>(scissorCount);
    return createInfo;
}

VkPipelineShaderStageCreateInfo vku::shaderStage(
    VkShaderStageFlagBits stage,
    VkShaderModule module,
    const char *entryPoint)
{
    VkPipelineShaderStageCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
    createInfo.stage = stage;
    createInfo.module = module;
    createInfo.pName = entryPoint;
    return createInfo;
}

VkPipelineDynamicStateCreateInfo vku::dynamicState(
    const VkDynamicState *dynamicStates,
    size_t dynamicStateCount)
{
    VkPipelineDynamicStateCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO;
    createInfo.pDynamicStates = dynamicStates;
    createInfo.dynamicStateCount = static_cast<uint32_t>(dynamicStateCount);
    return createInfo;
}

VkPipelineDepthStencilStateCreateInfo vku::depthStencilState(bool useDepthTest)
{
    VkPipelineDepthStencilStateCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO;
    if (useDepthTest) {
        createInfo.depthTestEnable = true;
        createInfo.depthWriteEnable = true;
        createInfo.depthCompareOp = VK_COMPARE_OP_LESS;
    }
    return createInfo;
}

vku::PipelineLayout vku::PipelineLayout::basic(
    VkDevice device,
    const VkDescriptorSetLayout *setLayouts,
    size_t setLayoutCount,
    const VkPushConstantRange *pushConstantRanges,
    size_t pushConstantRangeCount)
{
    VkPipelineLayoutCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
    if (setLayouts) {
        createInfo.pSetLayouts = setLayouts;
        createInfo.setLayoutCount = static_cast<uint32_t>(setLayoutCount);
    }
    if (pushConstantRanges) {
        createInfo.pPushConstantRanges = pushConstantRanges;
        createInfo.pushConstantRangeCount = static_cast<uint32_t>(pushConstantRangeCount);
    }
    return vku::PipelineLayout(device, createInfo);
}

vku::GraphicsPipeline vku::GraphicsPipeline::basic(
    VkDevice device,
    VkPipelineLayout pipelineLayout,
    VkRenderPass renderPass,
    const VkPipelineShaderStageCreateInfo *shaderStages,
    size_t shaderStageCount,
    const VkVertexInputBindingDescription *vertexBindings,
    size_t vertexBindingCount,
    const VkVertexInputAttributeDescription *vertexAttributes,
    size_t vertexAttributeCount,
    VkFrontFace frontFace,
    VkPrimitiveTopology topology,
    bool hasDepthStencil,
    bool allowBlending)
{
    VkGraphicsPipelineCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;

    auto vertexInputState = vku::vertexInputState(
        vertexBindings,
        vertexBindingCount,
        vertexAttributes,
        vertexAttributeCount);
    createInfo.pVertexInputState = &vertexInputState;

    auto inputAssemblyState = vku::inputAssemblyState(topology);
    createInfo.pInputAssemblyState = &inputAssemblyState;

    auto rasterizationState = vku::rasterizationState(
        VK_POLYGON_MODE_FILL,
        VK_CULL_MODE_BACK_BIT,
        frontFace);
    createInfo.pRasterizationState = &rasterizationState;

    auto multisampleState = vku::multisampleState(VK_SAMPLE_COUNT_1_BIT);
    createInfo.pMultisampleState = &multisampleState;

    auto colorBlendAttachment = vku::colorBlendAttachment(allowBlending);
    auto colorBlendState = vku::colorBlendState(&colorBlendAttachment, 1);
    createInfo.pColorBlendState = &colorBlendState;

    auto viewportState = vku::viewportState(nullptr, 1, nullptr, 1);
    createInfo.pViewportState = &viewportState;

    VkPipelineDepthStencilStateCreateInfo depthStencil;
    if (hasDepthStencil) {
        depthStencil = vku::depthStencilState(true);
        createInfo.pDepthStencilState = &depthStencil;
    }

    createInfo.pStages = shaderStages;
    createInfo.stageCount = static_cast<uint32_t>(shaderStageCount);

    VkDynamicState dynamicStates[] = { VK_DYNAMIC_STATE_VIEWPORT, VK_DYNAMIC_STATE_SCISSOR };
    auto dynamicState = vku::dynamicState(dynamicStates, 2);
    createInfo.pDynamicState = &dynamicState;

    createInfo.layout = pipelineLayout;
    createInfo.renderPass = renderPass;
    createInfo.subpass = 0;
    return vku::GraphicsPipeline(device, createInfo);
}
