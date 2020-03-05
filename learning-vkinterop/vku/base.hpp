#pragma once

#include <volk.h>

#define ENSURE(vkFunc, ...) vku::ensure(vkFunc(__VA_ARGS__), #vkFunc)

namespace vku {

void ensure(VkResult result, const char *where);
void ensureLayers(const char **layers, size_t length);
}
