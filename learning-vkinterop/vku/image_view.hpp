#pragma once

#include <volk.h>

namespace vku {

class ImageView {
private:
    VkDevice _device;
    VkImageView _raw;

public:
    ImageView(VkDevice device, VkImageView raw);
    ImageView(VkDevice device, VkImageViewCreateInfo &createInfo);
    ~ImageView();
    ImageView(const ImageView &other) = delete;
    ImageView(ImageView &&other) noexcept;
    ImageView &operator=(const ImageView &other) = delete;
    ImageView &operator=(ImageView &&other) noexcept;

    operator VkImageView() { return _raw; }
    VkImageView raw() { return _raw; }
    VkDevice device() { return _device; }

    static ImageView basic(VkDevice device, VkImage image, VkFormat format);
};
}
