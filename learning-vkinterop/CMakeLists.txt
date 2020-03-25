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

set(GLFW_BUILD_DOCS OFF CACHE BOOL "" FORCE)
set(GLFW_BUILD_TESTS OFF CACHE BOOL "" FORCE)
set(GLFW_BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
set(GLFW_INSTALL OFF CACHE BOOL "" FORCE)
set(GLFW_VULKAN_STATIC OFF CACHE BOOL "" FORCE)
FetchContent_Declare(glfw_FETCH
    URL https://github.com/glfw/glfw/archive/3.3.2.tar.gz
    URL_HASH SHA1=c3eea7eb56b1f316da3d18a907d6fd370cfa8d5d)
FetchContent_MakeAvailable(glfw_FETCH)

set(CMAKE_C_VISIBILITY_PRESET hidden)
set(CMAKE_CXX_VISIBILITY_PRESET hidden)

set(CMAKE_CXX_STANDARD 17)
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

string(LENGTH "${CMAKE_SOURCE_DIR}/" ROOT_PATH_LENGTH)
add_compile_definitions(ROOT_PATH_LENGTH=${ROOT_PATH_LENGTH})

add_embedded_shader(${CMAKE_SOURCE_DIR}/shaders/shader.frag
    ${CMAKE_SOURCE_DIR}/generated/shader.frag.cpp
    FRAGMENT_SHADER)
add_embedded_shader(${CMAKE_SOURCE_DIR}/shaders/shader.vert
    ${CMAKE_SOURCE_DIR}/generated/shader.vert.cpp
    VERTEX_SHADER)

add_subdirectory(vku)

set (VKI_SOURCES
    # headers
    vki.hpp
    triangle.hpp

    # sources
    vki.cpp
    triangle.cpp

    # generated files
    generated/shader.vert.cpp
    generated/shader.frag.cpp)

add_library(vki SHARED ${VKI_SOURCES})
set_target_properties(vki PROPERTIES PUBLIC_HEADER vki.hpp)
add_dependencies(vki glfw)
target_link_libraries(vki PRIVATE glfw vku)
target_include_directories(vki PRIVATE ${Vulkan_INCLUDE_DIRS})
target_include_directories(vki PRIVATE ${CMAKE_SOURCE_DIR})
if(WIN32)
    target_compile_options(vki PRIVATE /W4)
else()
    target_compile_options(vki PRIVATE -Wall -Wextra -pedantic)
endif()

add_executable(hello hello.cpp)
target_link_libraries(hello vki)

add_custom_target(VkInterop
ALL dotnet build ${CMAKE_SOURCE_DIR} -o ${CMAKE_BINARY_DIR}
DEPENDS vki)
