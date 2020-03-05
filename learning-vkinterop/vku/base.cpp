#include "base.hpp"

#include <sstream>

void vku::ensure(VkResult result, const char *where)
{
    if (result == VK_SUCCESS) {
        return;
    }
    std::stringstream ss;
    ss << where << "["<< result << "]: ";
    std::string message = ss.str();
    throw std::runtime_error(message);
}
