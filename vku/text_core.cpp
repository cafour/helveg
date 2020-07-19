#include "text_core.hpp"

#include "render_core.hpp"
#include "shaders.hpp"
#include "wrapper.hpp"

#include "stb_font_consolas_24_usascii.inl"

#include <vector>

stb_fontchar fontData[STB_FONT_consolas_24_usascii_NUM_CHARS];

vku::TextCore::TextCore(vku::RenderCore &render, VkBuffer cameraBuffer)
    : _render(render)
    , _cameraBuffer(cameraBuffer)
{
    createFontImage();
}

void vku::TextCore::createFontImage()
{
    VkPhysicalDevice physicalDevice = _render.displayCore().physicalDevice();
    VkDevice device = _render.displayCore().device();

    const uint32_t width = STB_FONT_consolas_24_usascii_BITMAP_WIDTH;
    const uint32_t height = STB_FONT_consolas_24_usascii_BITMAP_HEIGHT;

    static unsigned char pixels[height][width];
    stb_font_consolas_24_usascii(fontData, pixels, height);

    auto imageInfo = vku::Image::createInfo();
    imageInfo.imageType = VK_IMAGE_TYPE_2D;
    imageInfo.format = VK_FORMAT_R8_UNORM;
    imageInfo.extent.width = width;
    imageInfo.extent.height = height;
    imageInfo.extent.depth = 1;
    imageInfo.mipLevels = 1;
    imageInfo.arrayLayers = 1;
    imageInfo.samples = VK_SAMPLE_COUNT_1_BIT;
    imageInfo.tiling = VK_IMAGE_TILING_OPTIMAL;
    imageInfo.usage = VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK_IMAGE_USAGE_SAMPLED_BIT;
    imageInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
    imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
    _fontImage = vku::Image(device, imageInfo);

    _fontImageMemory = vku::DeviceMemory::forImage(
        physicalDevice,
        device,
        _fontImage,
        VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);

    vku::copyToImage(
        physicalDevice,
        device,
        _render.commandPool(),
        _render.displayCore().queue(),
        _fontImage,
        width,
        height,
        &pixels[0][0]);

    VkImageViewCreateInfo viewInfo = vku::ImageView::createInfo();
    viewInfo.image = _fontImage;
    viewInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
    viewInfo.format = imageInfo.format;
    viewInfo.components = { VK_COMPONENT_SWIZZLE_R, VK_COMPONENT_SWIZZLE_G, VK_COMPONENT_SWIZZLE_B, VK_COMPONENT_SWIZZLE_A };
    viewInfo.subresourceRange = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1 };
    _fontImageView = vku::ImageView(device, viewInfo);

    // Sampler
    VkSamplerCreateInfo samplerInfo = vku::Sampler::createInfo();
    samplerInfo.maxAnisotropy = 1.0f;
    samplerInfo.magFilter = VK_FILTER_LINEAR;
    samplerInfo.minFilter = VK_FILTER_LINEAR;
    samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
    samplerInfo.addressModeU = VK_SAMPLER_ADDRESS_MODE_REPEAT;
    samplerInfo.addressModeV = VK_SAMPLER_ADDRESS_MODE_REPEAT;
    samplerInfo.addressModeW = VK_SAMPLER_ADDRESS_MODE_REPEAT;
    samplerInfo.mipLodBias = 0.0f;
    samplerInfo.compareOp = VK_COMPARE_OP_NEVER;
    samplerInfo.minLod = 0.0f;
    samplerInfo.maxLod = 1.0f;
    samplerInfo.borderColor = VK_BORDER_COLOR_FLOAT_OPAQUE_WHITE;
    _fontSampler = vku::Sampler(device, samplerInfo);
}

void vku::TextCore::createTextGP()
{
    VkDevice device = _render.displayCore().device();

    // create descriptor pool
    auto poolSizes = {
        vku::descriptorPoolSize(VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1),
        vku::descriptorPoolSize(VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, 1),
    };
    _textDP = vku::DescriptorPool::basic(device, 1, poolSizes.begin(), poolSizes.size());

    // create a descriptor set layout
    auto bindings = {
        vku::descriptorBinding(
            0,
            VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,
            1,
            VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT),
        vku::descriptorBinding(
            1,
            VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER,
            1,
            VK_SHADER_STAGE_FRAGMENT_BIT),
    };
    _textDSL = vku::DescriptorSetLayout::basic(device, bindings.begin(), bindings.size());
    auto setLayouts = { _textDSL.raw() };

    // create a pipeline layout
    _textPL = vku::PipelineLayout::basic(
        _render.displayCore().device(),
        setLayouts.begin(),
        setLayouts.size());

    // create a descriptor set
    _textDS = vku::allocateDescriptorSets(device, _textDP, _textDSL, 1)[0];
    VkDescriptorImageInfo descriptorInfo = {};
    descriptorInfo.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
    descriptorInfo.imageView = _fontImageView;
    descriptorInfo.sampler = _fontSampler;
    vku::writeWholeBufferDescriptor(
        device,
        VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,
        _cameraBuffer,
        _textDS,
        0);
    vku::writeImageDescriptor(
        device,
        _textDS,
        VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER,
        &descriptorInfo,
        1);

    auto vertexBindings = {
        vku::vertexInputBinding(0, sizeof(glm::vec3), VK_VERTEX_INPUT_RATE_VERTEX), // position
        vku::vertexInputBinding(1, sizeof(glm::vec2), VK_VERTEX_INPUT_RATE_VERTEX), // uv
    };

    auto vertexAttributes = {
        vku::vertexInputAttribute(0, 0, VK_FORMAT_R32G32B32_SFLOAT, 0),
        vku::vertexInputAttribute(1, 1, VK_FORMAT_R32G32_SFLOAT, 0)
    };

    auto vertexShader = vku::ShaderModule::inlined(_render.displayCore().device(), TEXT_VERT, TEXT_VERT_LENGTH);
    auto fragmentShader = vku::ShaderModule::inlined(_render.displayCore().device(), TEXT_FRAG, TEXT_FRAG_LENGTH);
    auto shaderStages = {
        vku::shaderStage(VK_SHADER_STAGE_VERTEX_BIT, vertexShader),
        vku::shaderStage(VK_SHADER_STAGE_FRAGMENT_BIT, fragmentShader)
    };

    _textGP = vku::GraphicsPipeline::basic(
        _render.displayCore().device(),
        _textPL,
        _renderPass,
        shaderStages.begin(),
        shaderStages.size(),
        vertexBindings.begin(),
        vertexBindings.size(),
        vertexAttributes.begin(),
        vertexAttributes.size(),
        VK_FRONT_FACE_COUNTER_CLOCKWISE);
}
