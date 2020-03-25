#pragma once

#include <volk.h>

#include <utility>

#define _FILENAME (static_cast<const char*>(__FILE__) + ROOT_PATH_LENGTH)
#define _STR(what) #what
#define _TOSTR(what) _STR(what)
#define LOG(vkInvocation) vku::log(vkInvocation, _FILENAME, __LINE__, #vkInvocation)
#define ENSURE(vkInvocation) vku::ensure(vkInvocation, _FILENAME, __LINE__, #vkInvocation)

namespace vku {

void log(VkResult result, const char *filename, int line, const char *what);
void ensure(VkResult result, const char *filename, int line, const char *what);
void ensureLayers(const char **layers, size_t length);
const char *resultString(VkResult result);
}
