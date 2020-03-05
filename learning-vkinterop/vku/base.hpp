#pragma once

#include <volk.h>

#define ENSURE(vkFunc, ...) vku::ensure(vkFunc(__VA_ARGS__), #vkFunc)

namespace vku {
void ensure(VkResult result, const char *where);

}
