cmake_minimum_required(VERSION 3.10)

project(vulkan-interop)

set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

include(FetchContent)
FetchContent_Declare(vulkan_FETCH
    GIT_REPOSITORY https://github.com/KhronosGroup/Vulkan-Headers.git
)
FetchContent_MakeAvailable(vulkan_FETCH)
get_target_property(vulkan_INCLUDE_DIR Vulkan-Headers INTERFACE_INCLUDE_DIRECTORIES)
include_directories(${vulkan_INCLUDE_DIR})

find_package(Volk REQUIRED)
find_package(GLFW REQUIRED)
find_package(Glm REQUIRED)

include(Embed)

set(CMAKE_C_VISIBILITY_PRESET hidden)
set(CMAKE_CXX_VISIBILITY_PRESET hidden)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

string(LENGTH "${CMAKE_SOURCE_DIR}/" ROOT_PATH_LENGTH)
add_compile_definitions(ROOT_PATH_LENGTH=${ROOT_PATH_LENGTH})

add_embedded_shader(${CMAKE_SOURCE_DIR}/shaders/triangle.frag
    ${CMAKE_SOURCE_DIR}/generated/triangle.frag.cpp
    TRIANGLE_FRAG)
add_embedded_shader(${CMAKE_SOURCE_DIR}/shaders/triangle.vert
    ${CMAKE_SOURCE_DIR}/generated/triangle.vert.cpp
    TRIANGLE_VERT)

add_subdirectory(vku)

set (VKI_SOURCES
    # headers
    vki.hpp
    triangle.hpp
    mesh_render.hpp

    # sources
    vki.cpp
    triangle.cpp
    mesh_render.cpp

    # generated files
    generated/triangle.vert.cpp
    generated/triangle.frag.cpp)

add_library(vki SHARED ${VKI_SOURCES})
# set_target_properties(vki PROPERTIES PUBLIC_HEADER vki.hpp)
target_link_libraries(vki PUBLIC vku glm Vulkan-Headers)
target_include_directories(vki PRIVATE ${CMAKE_SOURCE_DIR})
if(WIN32)
    target_compile_options(vki PRIVATE /W4)
else()
    target_compile_options(vki PRIVATE -Wall -Wextra -pedantic)
endif()

add_executable(hello hello.cpp)
target_link_libraries(hello PRIVATE vki)

add_custom_target(VkInterop
ALL dotnet build ${CMAKE_SOURCE_DIR} -o ${CMAKE_BINARY_DIR}
DEPENDS vki)
