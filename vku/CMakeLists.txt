cmake_minimum_required(VERSION 3.10)

project(vku)

set(VULKAN_HEADERS_DIR "" CACHE PATH "Path to the Vulkan headers")
include_directories(${VULKAN_HEADERS_DIR})

find_package(Volk REQUIRED)
find_package(GLFW REQUIRED)
find_package(Glm REQUIRED)
add_compile_definitions(GLM_FORCE_RADIANS GLM_FORCE_DEPTH_ZERO_TO_ONE)

include(Embed)


set(VKU_SOURCES
    # headers
    vku.hpp
    base.hpp
    types.hpp
    window.hpp
    standalone.hpp
    instance_related.hpp
    device_related.hpp
    swapchain_env.hpp
    app.hpp
    containers.hpp
    pipeline.hpp
    descriptor_set.hpp
    render_pass.hpp
    vki.hpp
    triangle.hpp
    mesh_render.hpp
    graph_render.hpp

    # sources
    base.cpp
    window.cpp
    standalone.cpp
    instance_related.cpp
    device_related.cpp
    swapchain_env.cpp
    app.cpp
    containers.cpp
    pipeline.cpp
    descriptor_set.cpp
    render_pass.cpp
    vki.cpp
    triangle.cpp
    mesh_render.cpp
    graph_render.cpp)

add_library(${PROJECT_NAME} SHARED ${VKU_SOURCES})
target_link_libraries(${PROJECT_NAME} PUBLIC volk glfw glm)
target_include_directories(${PROJECT_NAME} PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
set_target_properties(${PROJECT_NAME} PROPERTIES
    POSITION_INDEPENDENT_CODE ON
    C_VISIBILITY_PRESET hidden
    CXX_VISIBILITY_PRESET hidden
    CXX_STANDARD 17
    CXX_STANDARD_REQUIRED True)

if(WIN32)
    target_compile_options(${PROJECT_NAME} PRIVATE /W4)
else()
    target_compile_options(${PROJECT_NAME} PRIVATE -Wall -Wextra -pedantic)
endif()

string(LENGTH "${CMAKE_SOURCE_DIR}/" ROOT_PATH_LENGTH)
target_compile_definitions(${PROJECT_NAME} PUBLIC ROOT_PATH_LENGTH=${ROOT_PATH_LENGTH})

target_embed_shader(${PROJECT_NAME} shaders/triangle.frag TRIANGLE_FRAG)
target_embed_shader(${PROJECT_NAME} shaders/triangle.vert TRIANGLE_VERT)
target_embed_shader(${PROJECT_NAME} shaders/mesh.frag MESH_FRAG)
target_embed_shader(${PROJECT_NAME} shaders/mesh.vert MESH_VERT)

add_executable(hello hello.cpp)
target_link_libraries(hello PRIVATE ${PROJECT_NAME})
set_target_properties(hello PROPERTIES
    CXX_STANDARD 17
    CXX_STANDARD_REQUIRED True)
