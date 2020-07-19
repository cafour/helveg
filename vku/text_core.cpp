#include "text_core.hpp"

#include "transfer_core.hpp"
#include "wrapper.hpp"

#include "stb_font_consolas_24_usascii.inl"

#include <vector>

stb_fontchar fontData[STB_FONT_consolas_24_usascii_NUM_CHARS];

void vku::TextCore::createFontImage(vku::TransferCore &transfer)
{
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
    _fontImage = vku::Image(transfer.device(), imageInfo);

    _fontImageMemory = vku::DeviceMemory::forImage(
        transfer.physicalDevice(),
        transfer.device(),
        _fontImage,
        VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);

    vku::copyToImage(
        transfer.physicalDevice(),
        transfer.device(),
        transfer.transferPool(),
        transfer.transferQueue(),
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
    _fontImageView = vku::ImageView(transfer.device(), viewInfo);

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
    _fontSampler = vku::Sampler(transfer.device(), samplerInfo);
}

vku::TextCore::TextCore(vku::TransferCore &transfer)
{
    createFontImage(transfer);
}
