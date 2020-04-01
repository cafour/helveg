cmake_minimum_required(VERSION 3.10)

project(vulkan-interop)

set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

set(VULKAN_HEADERS_DIR "" CACHE PATH "Path to the Vulkan headers")
include_directories(${VULKAN_HEADERS_DIR})

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
target_link_libraries(vki PUBLIC vku glm)
target_include_directories(vki PRIVATE ${CMAKE_SOURCE_DIR})
if(WIN32)
    target_compile_options(vki PRIVATE /W4)
    set(DOTNET_RID win-x64)
else()
    target_compile_options(vki PRIVATE -Wall -Wextra -pedantic)
    set(DOTNET_RID linux-x64)
endif()

add_executable(hello hello.cpp)
target_link_libraries(hello PRIVATE vki)

set(DOTNET_CLI dotnet CACHE FILEPATH "Path to the dotnet CLI")
add_executable(dotnet IMPORTED)
set_target_properties(dotnet
    PROPERTIES IMPORTED_LOCATION ${DOTNET_CLI})
message(STATUS "Using dotnet: ${DOTNET_CLI}")

add_custom_target(VkInterop
ALL dotnet build ${CMAKE_SOURCE_DIR} -o ${CMAKE_BINARY_DIR} -r ${DOTNET_RID}
DEPENDS vki)
