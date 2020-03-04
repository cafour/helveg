cmake_minimum_required(VERSION 3.10)

project(vulkan-interop)

include(FetchContent)
FetchContent_Declare(volk_FETCH
    GIT_REPOSITORY https://github.com/zeux/volk
)
FetchContent_MakeAvailable(volk_FETCH)
if(WIN32)
    target_compile_definitions(volk INTERFACE VK_USE_PLATFORM_WIN32_KHR)
else()
    target_compile_definitions(volk INTERFACE VK_USE_PLATFORM_XCB_KHR)
endif()

FetchContent_Declare(glfw_FETCH
    URL https://github.com/glfw/glfw/archive/3.3.2.tar.gz
    URL_HASH SHA1=c3eea7eb56b1f316da3d18a907d6fd370cfa8d5d)
FetchContent_MakeAvailable(glfw_FETCH)
option(GLFW_BUILD_DOCS OFF)
option(GLFW_BUILD_TESTS OFF)
option(GLFW_BUILD_EXAMPLES OFF)
option(GLFW_INSTALL OFF)
option(GLFW_VULKAN_STATIC ON)

set(CMAKE_C_VISIBILITY_PRESET hidden)
set(CMAKE_CXX_VISIBILITY_PRESET hidden)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED True)

add_executable(embed embed.cpp)

file(MAKE_DIRECTORY ${CMAKE_SOURCE_DIR}/generated)
function(add_embedded_shader INPUT_GLSL OUTPUT_CPP SHADER_NAME)
    get_filename_component(_SPV ${INPUT_GLSL} NAME)
    set(_SPV generated/${_SPV}.spv)
    add_custom_command(
        OUTPUT ${_SPV}
        COMMAND glslc ${INPUT_GLSL} -o ${_SPV}
        MAIN_DEPENDENCY ${INPUT_GLSL}
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
    )
    add_custom_command(
        OUTPUT ${OUTPUT_CPP}
        COMMAND embed ${_SPV} ${OUTPUT_CPP} ${SHADER_NAME}
        MAIN_DEPENDENCY ${_SPV}
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        DEPENDS embed
    )
endfunction()

add_embedded_shader(${CMAKE_SOURCE_DIR}/shaders/shader.frag
    ${CMAKE_SOURCE_DIR}/generated/shader.frag.cpp
    VERTEX_SHADER)
add_embedded_shader(${CMAKE_SOURCE_DIR}/shaders/shader.vert
    ${CMAKE_SOURCE_DIR}/generated/shader.vert.cpp
    FRAGMENT_SHADER)

add_library(vki SHARED vki.cpp sample.cpp wrapper.cpp generated/shader.frag.cpp generated/shader.vert.cpp)
set_target_properties(vki PROPERTIES PUBLIC_HEADER vki.hpp)
add_dependencies(vki glfw)
target_link_libraries(vki PRIVATE glfw volk)
target_include_directories(vki PRIVATE ${Vulkan_INCLUDE_DIRS})
target_include_directories(vki PRIVATE ${CMAKE_SOURCE_DIR})
