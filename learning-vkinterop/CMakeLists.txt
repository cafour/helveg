cmake_minimum_required(VERSION 3.10)

project(vulkan-interop)

include(ExternalProject)
set(glfw_DIR ${CMAKE_SOURCE_DIR}/external/glfw)
ExternalProject_Add(glfw_DOWNLOAD
    URL https://github.com/glfw/glfw/archive/3.3.2.tar.gz
    URL_HASH SHA1=c3eea7eb56b1f316da3d18a907d6fd370cfa8d5d
    INSTALL_DIR ${glfw_DIR}
    CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${glfw_DIR}
        -DGLFW_BUILD_DOCS="OFF"
        -DGLFW_BUILD_TESTS="OFF"
        -DGLFW_BUILD_EXAMPLES="OFF")
add_library(glfw STATIC IMPORTED GLOBAL)
file(MAKE_DIRECTORY ${glfw_DIR}/include)
set_target_properties(glfw PROPERTIES
    IMPORTED_LOCATION ${glfw_DIR}/lib/${CMAKE_STATIC_LIBRARY_PREFIX}glfw3${CMAKE_STATIC_LIBRARY_SUFFIX}
    INTERFACE_INCLUDE_DIRECTORIES ${glfw_DIR}/include)

set(CMAKE_C_VISIBILITY_PRESET hidden)
set(CMAKE_CXX_VISIBILITY_PRESET hidden)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED True)

add_executable(embed embed.cpp)

add_library(vki SHARED vki.cpp)
set_target_properties(vki PROPERTIES PUBLIC_HEADER vki.hpp)
add_dependencies(vki glfw embed)
target_link_libraries(vki glfw)
