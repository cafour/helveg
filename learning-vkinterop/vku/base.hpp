#pragma once

#include <volk.h>


#define _TOSTR(what) #what
#define LOG(vkInvocation) vku::log(vkInvocation, __FILE__ ":" _TOSTR(__LINE__) #vkInvocation)
#define ENSURE(vkInvocation) vku::ensure(vkInvocation, __FILE__ ":" _TOSTR(__LINE__) #vkInvocation)

namespace vku {

void log(VkResult result, const char *where);
void ensure(VkResult result, const char *where);
void ensureLayers(const char **layers, size_t length);
}
