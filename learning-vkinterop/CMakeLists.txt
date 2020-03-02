cmake_minimum_required(VERSION 3.10)

project(vulkan-interop)

include(ExternalProject)
set(glfw_DIR ${CMAKE_SOURCE_DIR}/external/glfw)
ExternalProject_Add(glfw_DOWNLOAD
    URL https://github.com/glfw/glfw/archive/3.3.2.tar.gz
    URL_HASH SHA1=c3eea7eb56b1f316da3d18a907d6fd370cfa8d5d
    INSTALL_DIR ${glfw_DIR}
    CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${glfw_DIR}
        -DGLFW_BUILD_DOCS=OFF
        -DGLFW_BUILD_TESTS=OFF
        -DGLFW_BUILD_EXAMPLES=OFF
        -DGLFW_VULKAN_STATIC=ON)
add_library(glfw STATIC IMPORTED GLOBAL)
file(MAKE_DIRECTORY ${glfw_DIR}/include)
set_target_properties(glfw PROPERTIES
    IMPORTED_LOCATION ${glfw_DIR}/lib/${CMAKE_STATIC_LIBRARY_PREFIX}glfw3${CMAKE_STATIC_LIBRARY_SUFFIX}
    INTERFACE_INCLUDE_DIRECTORIES ${glfw_DIR}/include)

include(FetchContent)
FetchContent_Declare(volk_DOWNLOAD
    GIT_REPOSITORY https://github.com/zeux/volk
)
FetchContent_MakeAvailable(volk_DOWNLOAD)

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
