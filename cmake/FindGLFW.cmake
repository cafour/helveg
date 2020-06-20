include(FetchContent)
set(GLFW_BUILD_DOCS OFF CACHE BOOL "" FORCE)
set(GLFW_BUILD_TESTS OFF CACHE BOOL "" FORCE)
set(GLFW_BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
set(GLFW_INSTALL OFF CACHE BOOL "" FORCE)
set(GLFW_VULKAN_STATIC OFF CACHE BOOL "" FORCE)
FetchContent_Declare(fetch_glfw
    URL https://github.com/glfw/glfw/archive/3.3.2.tar.gz
    URL_HASH SHA1=c3eea7eb56b1f316da3d18a907d6fd370cfa8d5d)
FetchContent_GetProperties(fetch_glfw)
if(NOT fetch_glfw_POPULATED)
    FetchContent_Populate(fetch_glfw)
    add_subdirectory(${fetch_glfw_SOURCE_DIR} ${fetch_glfw_BINARY_DIR})
endif()
