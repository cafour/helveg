cmake_minimum_required(VERSION 3.10)
project(vku)

set(VULKAN_HEADERS_DIR "" CACHE PATH "Path to the Vulkan headers")
include_directories(${VULKAN_HEADERS_DIR})

find_package(VulkanHeaders REQUIRED)
find_package(Volk REQUIRED)
find_package(GLFW REQUIRED)
find_package(Glm REQUIRED)

include(Embed)

set(VKU_SOURCES
    # interop
    interop.hpp
    interop.cpp
    data.hpp

    # wrappers & utils
    base.hpp
    base.cpp
    types.hpp
    window.hpp
    window.cpp
    standalone.hpp
    standalone.cpp
    instance_related.hpp
    instance_related.cpp
    device_related.hpp
    device_related.cpp
    containers.hpp
    containers.cpp
    pipeline.hpp
    pipeline.cpp
    descriptor_set.hpp
    descriptor_set.cpp
    render_pass.hpp
    render_pass.cpp
    wrapper.hpp
    log.hpp
    log.cpp
    
    # cores
    cores.hpp
    instance_core.hpp
    instance_core.cpp
    display_core.hpp
    display_core.cpp
    swapchain_core.hpp
    swapchain_core.cpp
    render_core.hpp
    render_core.cpp
    depth_core.hpp
    depth_core.cpp
    transfer_core.hpp
    transfer_core.cpp
    inline_mesh_core.hpp
    inline_mesh_core.cpp
    inline_cube.cpp
    mesh_core.hpp
    mesh_core.cpp
    camera_core.hpp
    camera_core.cpp
    
    # renders
    triangle_render.hpp
    triangle_render.cpp
    mesh_render.hpp
    mesh_render.cpp
    graph_render.hpp
    graph_render.cpp
    chunk_render.hpp
    chunk_render.cpp
    world_render.hpp
    world_render.cpp)

add_library(${PROJECT_NAME} SHARED ${VKU_SOURCES})
target_link_libraries(${PROJECT_NAME} PUBLIC volk glfw glm)
target_include_directories(${PROJECT_NAME} PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
set_target_properties(${PROJECT_NAME} PROPERTIES
    POSITION_INDEPENDENT_CODE ON
    C_VISIBILITY_PRESET hidden
    CXX_VISIBILITY_PRESET hidden
    CXX_STANDARD 17
    CXX_STANDARD_REQUIRED True)

if(MSVC)
    set(CMAKE_C_FLAGS "/DWIN32 /D_WINDOWS /W4")
    set(CMAKE_C_FLAGS_DEBUG "/D_DEBUG /MTd /Zi /Ob0 /Od /RTC1")
    set(CMAKE_C_FLAGS_RELEASE "/D NDEBUG /MT /O2 /Ob2")
    set(CMAKE_CXX_FLAGS "/DWIN32 /D_WINDOWS /W4 /GR /EHsc")
    set(CMAKE_CXX_FLAGS_DEBUG "/D_DEBUG /MTd /Zi /Ob0 /Od /RTC1")
    set(CMAKE_CXX_FLAGS_RELEASE "/D NDEBUG /MT /O2 /Ob2")
else()
    target_compile_options(${PROJECT_NAME} PRIVATE -Wall -Wextra -pedantic)
endif()

string(LENGTH "${CMAKE_SOURCE_DIR}/" ROOT_PATH_LENGTH)
target_compile_definitions(${PROJECT_NAME} PUBLIC ROOT_PATH_LENGTH=${ROOT_PATH_LENGTH})
target_compile_definitions(${PROJECT_NAME} PUBLIC _USE_MATH_DEFINES)
target_compile_definitions(${PROJECT_NAME} PRIVATE GLM_FORCE_RADIANS GLM_FORCE_DEPTH_ZERO_TO_ONE)

target_embed_shader(${PROJECT_NAME} shaders/triangle.frag TRIANGLE_FRAG)
target_embed_shader(${PROJECT_NAME} shaders/triangle.vert TRIANGLE_VERT)
target_embed_shader(${PROJECT_NAME} shaders/mesh.frag MESH_FRAG)
target_embed_shader(${PROJECT_NAME} shaders/mesh.vert MESH_VERT)
target_embed_shader(${PROJECT_NAME} shaders/node.vert NODE_VERT)
target_embed_shader(${PROJECT_NAME} shaders/node.geom NODE_GEOM)
target_embed_shader(${PROJECT_NAME} shaders/node.frag NODE_FRAG)
target_embed_shader(${PROJECT_NAME} shaders/edge.vert EDGE_VERT)
target_embed_shader(${PROJECT_NAME} shaders/edge.frag EDGE_FRAG)
target_embed_shader(${PROJECT_NAME} shaders/chunk.vert CHUNK_VERT)
target_embed_shader(${PROJECT_NAME} shaders/chunk.frag CHUNK_FRAG)
target_embed_shader(${PROJECT_NAME} shaders/world.vert WORLD_VERT)
target_embed_shader(${PROJECT_NAME} shaders/world.frag WORLD_FRAG)

add_executable(hello hello.cpp)
target_link_libraries(hello PRIVATE ${PROJECT_NAME})
set_target_properties(hello PROPERTIES
    CXX_STANDARD 17
    CXX_STANDARD_REQUIRED True)
