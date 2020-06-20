include(FetchContent)

FetchContent_Declare(fetch_vulkan_headers
GIT_REPOSITORY https://github.com/KhronosGroup/Vulkan-Headers
)
FetchContent_GetProperties(fetch_vulkan_headers)
if(NOT fetch_vulkan_headers_POPULATED)
FetchContent_Populate(fetch_vulkan_headers)
add_subdirectory(${fetch_vulkan_headers_SOURCE_DIR} ${fetch_vulkan_headers_BINARY_DIR} EXCLUDE_FROM_ALL)
endif()

get_target_property(VULKAN_INCLUDE_DIR Vulkan::Headers INTERFACE_INCLUDE_DIRECTORIES)
include_directories(${VULKAN_INCLUDE_DIR})
