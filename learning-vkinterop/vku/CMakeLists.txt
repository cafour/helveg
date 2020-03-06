cmake_minimum_required(VERSION 3.10)

project(vku LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

set(VKU_SOURCES
    # headers
    vku.hpp
    base.hpp
    shader.hpp
    instance.hpp
    physical_device.hpp
    window.hpp
    surface.hpp

    # sources
    base.cpp
    shader.cpp
    instance.cpp
    physical_device.cpp
    window.cpp
    surface.cpp)

add_library(${PROJECT_NAME} STATIC ${VKU_SOURCES})
target_link_libraries(${PROJECT_NAME} volk glfw)
